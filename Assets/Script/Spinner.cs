using UnityEngine;

public class Spinner : MonoBehaviour
{
    public float baseSpeed = 50f;       // 기본 회전 속도
    public float speedMultiplier = 100f; // 속도 변동 폭
    public float speedFrequency = 2f;   // 속도가 변하는 주기 (sin 함수의 주기)

    private float timeElapsed = 0f;

    void Update()
    {
        // 경과 시간에 따라 sin 함수를 사용해 회전 속도를 계산
        timeElapsed += Time.deltaTime;

        // sin 함수로 주기적으로 증가와 감소하는 속도 계산 (0에서 1로 변동)
        float speedVariation = Mathf.Sin(timeElapsed * speedFrequency) * 0.5f + 0.5f;

        // 기본 속도에 변동 속도를 더하여 최종 회전 속도 계산
        float finalSpeed = baseSpeed + (speedVariation * speedMultiplier);

        // Z축을 기준으로 회전
        transform.Rotate(Vector3.forward, finalSpeed * Time.deltaTime);
    }
}