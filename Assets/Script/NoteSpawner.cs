using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;  // 노트 프리팹
    public Transform[] spawnPositions;  // Q, W, E, R 바의 위치
    public float yOffset = 10f;  // y축 오프셋 (인스펙터에서 조정 가능)
    public float noteTravelTime = 2f;  // 노트가 목적지까지 도달하는 데 걸리는 시간

    private List<(int, float)> noteData;  // 바 번호와 타이밍 정보를 저장할 리스트
    private int currentNoteIndex = 0;
    private float timeElapsed = 0f;
    private bool isSpawningActive = false;  // 노트 스포닝 활성화 여부

    void Start()
    {
        // 노트 데이터 로드
        noteData = new List<(int, float)>();

        string[] lines = File.ReadAllLines(Application.dataPath + "/Resources/note_data.txt");
        foreach (string line in lines)
        {
            var splitLine = line.Split(',');
            int bar = int.Parse(splitLine[0]);
            float timing = float.Parse(splitLine[1]);
            noteData.Add((bar, timing));
        }
    }

    void Update()
    {
        // 스페이스 바가 눌리면 노트 스포닝 시작
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isSpawningActive = true;
            timeElapsed = 0f;  // 타이머 리셋
            currentNoteIndex = 0;  // 노트 인덱스 리셋
        }

        // 노트 스포닝이 활성화된 경우에만 실행
        if (isSpawningActive)
        {
            timeElapsed += Time.deltaTime;

            // 타이밍에 맞춰 노트 생성
            if (currentNoteIndex < noteData.Count)
            {
                float targetTime = noteData[currentNoteIndex].Item2;
                float spawnTime = targetTime - noteTravelTime;

                if (timeElapsed >= spawnTime)
                {
                    int bar = noteData[currentNoteIndex].Item1;
                    GenerateNote(bar - 1);
                    currentNoteIndex++;
                }
            }
        }
    }

    void GenerateNote(int barIndex)
    {
        // 지정된 바의 위치에서 y축으로 yOffset만큼 위에 노트 생성
        Vector3 spawnPosition = spawnPositions[barIndex].position + new Vector3(0, yOffset, 0);
        Vector3 targetPosition = spawnPositions[barIndex].position;
        GameObject note = Instantiate(notePrefab, spawnPosition, Quaternion.identity);

        // 이동 거리 계산
        float distance = Vector3.Distance(spawnPosition, targetPosition);

        // 이동 속도 계산 (거리 / 시간)
        float speed = distance / noteTravelTime;

        // 노트에 속도를 설정하여 바까지 이동하게 함
        NoteMovement noteMovement = note.GetComponent<NoteMovement>();
        noteMovement.SetTarget(targetPosition, speed);
        Debug.Log(speed);
    }
}
