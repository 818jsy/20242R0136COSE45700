using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListItem : MonoBehaviour
{
    public TMP_Text titleText;       // 제목 텍스트를 직접 참조
    public Image thumbnailImage;     // 썸네일 이미지를 직접 참조

    // 초기 로딩 상태 텍스트 설정
    public void SetLoadingState()
    {
        titleText.text = "로딩 중...";  // 썸네일 로드 전 로딩 텍스트 설정
    }

    // 최종 텍스트 설정
    public void SetTitle(string title)
    {
        titleText.text = title;  // 로드 완료 후 제목 설정
    }
}