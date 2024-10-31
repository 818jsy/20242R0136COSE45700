using UnityEngine;
using System.Collections;

public class Bar : MonoBehaviour
{
    public KeyCode activationKey; // Q, W, E, R 키 중 하나

    public Material redMaterial;    // Bar의 평상시 기본 Material
    public Material yellowMaterial; // 특정 거리 범위에서 사용할 Material
    public Material greenMaterial;  // 절반 이하의 거리에서 사용할 Material
    public Material noteYellowMaterial; // 노트의 Yellow Material
    public Material noteGreenMaterial;  // 노트의 Green Material

    private SpriteRenderer spriteRenderer;
    private bool isColliding = false;
    private GameObject currentNote;
    private SpriteRenderer noteSpriteRenderer;
    private float distanceThreshold; // 노트와의 초기 중앙 간 거리 (n 값)

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.material = redMaterial; // Bar의 기본 Material로 설정
    }

    void Update()
    {
        if (isColliding && Input.GetKeyDown(activationKey))
        {
            float distance = Vector2.Distance(transform.position, currentNote.transform.position);
            float n = distanceThreshold;

            // 거리 범위에 따라 Bar의 색상 변경 및 노트의 색상 변경
            if (distance > n / 2 && distance <= n)
            {
                StartCoroutine(ChangeMaterialTemporarily(yellowMaterial)); // Bar를 yellowMaterial로 0.2초간 변경
                ChangeNoteMaterial(noteYellowMaterial); // 노트의 Material을 노란색으로 변경
            }
            else if (distance > 0 && distance <= n / 2)
            {
                StartCoroutine(ChangeMaterialTemporarily(greenMaterial)); // Bar를 greenMaterial로 0.2초간 변경
                ChangeNoteMaterial(noteGreenMaterial); // 노트의 Material을 초록색으로 변경
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Note"))
        {
            isColliding = true;
            currentNote = other.gameObject;
            noteSpriteRenderer = currentNote.GetComponent<SpriteRenderer>(); // 노트의 SpriteRenderer 가져오기

            // 충돌 시점에서의 두 중앙 간 거리 계산
            distanceThreshold = Vector2.Distance(transform.position, currentNote.transform.position);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Note"))
        {
            isColliding = false;
            currentNote = null;
            noteSpriteRenderer = null;
            spriteRenderer.material = redMaterial; // Bar의 기본 상태로 되돌림
        }
    }

    private IEnumerator ChangeMaterialTemporarily(Material newMaterial)
    {
        spriteRenderer.material = newMaterial; // Bar의 Material을 변경
        yield return new WaitForSeconds(0.2f); // 0.2초 동안 유지
        spriteRenderer.material = redMaterial; // Bar의 기본 Material로 복원
    }

    private void ChangeNoteMaterial(Material newMaterial)
    {
        if (noteSpriteRenderer != null)
        {
            noteSpriteRenderer.material = newMaterial; // 노트의 Material을 변경
        }
    }
}
