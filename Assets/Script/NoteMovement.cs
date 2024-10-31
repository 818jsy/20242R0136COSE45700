using UnityEngine;

public class NoteMovement : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed;
    private bool isMoving = false;
    public GameObject targetObject; // 충돌할 특정 오브젝트 참조

    public void SetTarget(Vector3 target, float speed)
    {
        targetPosition = target;
        this.speed = speed;
        isMoving = true;
    }

    void Update()
    {
        if (isMoving)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition + new Vector3(0, -10, 0), step);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 참조된 특정 오브젝트에 닿으면 사라짐
        if (other.gameObject == targetObject)
        {
            Destroy(gameObject); // 이 오브젝트를 삭제
        }
    }
}