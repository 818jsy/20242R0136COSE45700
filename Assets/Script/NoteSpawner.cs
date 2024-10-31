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
        string projectRootPath = Directory.GetParent(Application.dataPath).FullName;
        string resultsFolderPath = Path.Combine(projectRootPath, "PythonServer", "results");
        string noteDataPath = Path.Combine(resultsFolderPath, "vocal_onset_times.txt");
        string audioFilePath = Path.Combine(resultsFolderPath, "youtube_audio.mp3");

        if (!File.Exists(noteDataPath))
        {
            Debug.LogWarning($"Note data file not found at path: {noteDataPath}");
            yield break;
        }

        noteData = new List<(int, float)>();
        string[] lines = File.ReadAllLines(noteDataPath);
        foreach (string line in lines)
        {
            var splitLine = line.Split(',');
            int bar = int.Parse(splitLine[0]);
            float timing = float.Parse(splitLine[1]);
            noteData.Add((bar, timing));
        }

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + audioFilePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
            }
            else
            {
                Debug.LogError("Audio file loading failed: " + www.error);
                yield break;
            }
        }

        loadingCompleteTime = Time.time;
        Debug.Log("Loading complete. Time elapsed since start: " + loadingCompleteTime);

        StartSpawningAndPlayAudio();
    }

    void StartSpawningAndPlayAudio()
    {
        isSpawningActive = true;
        audioSource.Play();
        
        // 로딩 완료 이후부터 카운트를 시작하기 위해 timeElapsed 초기화
        timeElapsed = 0f;
        loadingCompleteTime = Time.time;

        Debug.Log("Audio started after " + (Time.time - loadingCompleteTime) + " seconds from loading completion.");
        currentNoteIndex = 0;
    }

    void Update()
    {
        if (!isSpawningActive) return; // 로딩 완료 후에만 Update 실행

        timeElapsed += Time.deltaTime;
        Debug.Log(timeElapsed);

        if (currentNoteIndex < noteData.Count)
        {
            float targetTime = noteData[currentNoteIndex].Item2;
            float spawnTime = targetTime - noteTravelTime;

            if (timeElapsed - 0.8 >= spawnTime)
            {
                int bar = noteData[currentNoteIndex].Item1;
                GenerateNote(bar - 1);

                if (currentNoteIndex == 0)
                {
                    float firstNoteTime = Time.time - loadingCompleteTime;
                    Debug.Log("First note spawned after " + firstNoteTime + " seconds from loading completion.");
                }

                currentNoteIndex++;
            }
        }
    }

    void GenerateNote(int barIndex)
    {
        Vector3 spawnPosition = spawnPositions[barIndex].position + new Vector3(0, yOffset, 0);
        Vector3 targetPosition = spawnPositions[barIndex].position;

        Debug.Log($"Spawn Position: {spawnPosition}, Target Position: {targetPosition}");

        GameObject note = Instantiate(notePrefab, spawnPosition, Quaternion.identity);

        float distance = Vector3.Distance(spawnPosition, targetPosition);
        float speed = distance / noteTravelTime;

        NoteMovement noteMovement = note.GetComponent<NoteMovement>();
        noteMovement.SetTarget(targetPosition, speed);
    }
}
