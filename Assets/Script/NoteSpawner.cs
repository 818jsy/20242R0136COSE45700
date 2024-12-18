using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
    public Transform[] spawnPositions;
    public float yOffset = 10f;
    public float noteTravelTime = 2f;
    public AudioSource audioSource;

    // 인스펙터에서 직접 텍스트 파일과 오디오 파일을 할당받을 변수들
    public TextAsset noteDataTextAsset;  // TXT 파일을 인스펙터에서 할당
    public AudioClip audioClip;  // MP3 파일을 인스펙터에서 할당

    private List<(int, float)> noteData;
    private int currentNoteIndex = 0;
    private float timeElapsed = 0f;
    private bool isSpawningActive = false;
    private float loadingCompleteTime; // 로딩 완료 시간 저장

    void Start()
    {
        StartCoroutine(LoadDataAndAudio());
    }

    IEnumerator LoadDataAndAudio()
    {
        // 텍스트 파일이 인스펙터에서 할당되었으면 그걸 사용, 아니면 기본 경로로 파일 읽기
        if (noteDataTextAsset == null)
        {
            Debug.LogWarning("Note data file not assigned in inspector. Using default file path.");
            string projectRootPath = Directory.GetParent(Application.dataPath).FullName;
            string resultsFolderPath = Path.Combine(projectRootPath, "PythonServer", "results");
            string noteDataPath = Path.Combine(resultsFolderPath, "vocal_onset_times.txt");

            if (!File.Exists(noteDataPath))
            {
                Debug.LogWarning($"Note data file not found at path: {noteDataPath}");
                yield break;
            }

            string[] lines = File.ReadAllLines(noteDataPath);
            ParseNoteData(lines);
        }
        else
        {
            // 인스펙터에서 할당된 TextAsset을 사용할 경우
            string[] lines = noteDataTextAsset.text.Split('\n');
            ParseNoteData(lines);
        }

        // 오디오 파일이 인스펙터에서 할당되었으면 그걸 사용, 아니면 기본 경로로 파일 로드
        if (audioClip == null)
        {
            Debug.LogWarning("Audio file not assigned in inspector. Using default file path.");
            string projectRootPath = Directory.GetParent(Application.dataPath).FullName;
            string resultsFolderPath = Path.Combine(projectRootPath, "PythonServer", "results");
            string audioFilePath = Path.Combine(resultsFolderPath, "youtube_audio.mp3");

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + audioFilePath, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                }
                else
                {
                    Debug.LogError("Audio file loading failed: " + www.error);
                    yield break;
                }
            }
        }
        else
        {
            audioSource.clip = audioClip;
        }

        loadingCompleteTime = Time.time;
        Debug.Log("Loading complete. Time elapsed since start: " + loadingCompleteTime);

        StartSpawningAndPlayAudio();
    }

    // 노트 데이터 파싱 함수
    void ParseNoteData(string[] lines)
    {
        noteData = new List<(int, float)>();
        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            var splitLine = line.Split(',');
            if (splitLine.Length == 2)
            {
                int bar = int.Parse(splitLine[0]);
                float timing = float.Parse(splitLine[1]);
                noteData.Add((bar, timing));
            }
        }
    }

    void StartSpawningAndPlayAudio()
    {
        isSpawningActive = true;
    
        // 음악을 2초 지연시키기 위해 2초 대기 후 재생
        StartCoroutine(PlayAudioWithDelay());
    
        // 로딩 완료 이후부터 카운트를 시작하기 위해 timeElapsed 초기화
        timeElapsed = 0f;
        loadingCompleteTime = Time.time;

        Debug.Log("Audio will start after 2 seconds of delay.");
        currentNoteIndex = 0;
    }

    IEnumerator PlayAudioWithDelay()
    {
        yield return new WaitForSeconds(1.6f);  // 2초 대기
        audioSource.Play();  // 음악 재생
    }

    void Update()
    {
        if (!isSpawningActive) return; // 로딩 완료 후에만 Update 실행

        timeElapsed += Time.deltaTime;

        if (currentNoteIndex < noteData.Count)
        {
            float targetTime = noteData[currentNoteIndex].Item2;

            // 이전 노트와의 시간 차이가 0.1초 이하이면 생략
            if (currentNoteIndex > 0)
            {
                float previousTargetTime = noteData[currentNoteIndex - 1].Item2;
                if (targetTime - previousTargetTime <= 0.15f)
                {
                    Debug.Log(targetTime + " "+ (previousTargetTime));
                    currentNoteIndex++;
                    return;  // 노트를 생략하고 다음 노트로 넘어감
                }
            }

            // 노트 생성
            if (timeElapsed >= targetTime)
            {
                int bar = noteData[currentNoteIndex].Item1;
                GenerateNote(bar - 1);

                Debug.Log((currentNoteIndex + 1) + " Spawned after " + timeElapsed + " seconds from loading completion.");

                currentNoteIndex++;
            }
        }
    }


    void GenerateNote(int barIndex)
    {
        Vector3 spawnPosition = spawnPositions[barIndex].position + new Vector3(0, yOffset + 2.5f, 0);
        Vector3 targetPosition = spawnPositions[barIndex].position + new Vector3(0, 2.5f, 0);;

        Debug.Log($"Spawn Position: {spawnPosition}, Target Position: {targetPosition}");

        GameObject note = Instantiate(notePrefab, spawnPosition, Quaternion.identity);

        float distance = Vector3.Distance(spawnPosition, targetPosition);
        float speed = distance / noteTravelTime;

        NoteMovement noteMovement = note.GetComponent<NoteMovement>();
        noteMovement.SetTarget(targetPosition, speed);
    }
}
