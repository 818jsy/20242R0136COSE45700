using UnityEngine;

public class Note : MonoBehaviour
{
    public KeyCode activationKey;

    void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            // 노트가 특정 범위에 있을 때 터치 가능하도록 처리
            // 이 범위는 게임 디자인에 따라 조정 가능
            if (Mathf.Abs(transform.position.y) < 0.5f)
            {
                Destroy(gameObject); // 노트 제거
            }
        }

    }
}