using UnityEngine;
using TMPro;

public class DisplayVideoUrl : MonoBehaviour
{
    public TMP_Text displayText; // 텍스트 UI로 URL 표시 (선택 사항)

    void Start()
    {
        // PlayerPrefs에서 URL 불러오기
        string videoUrl = PlayerPrefs.GetString("YouTubeVideoUrl", "URL이 없습니다.");

        // 디버그로 출력
        Debug.Log("불러온 URL: " + videoUrl);

        // 텍스트 UI에 표시 (선택 사항)
        if (displayText != null)
        {
            displayText.text = videoUrl;
        }
    }
}