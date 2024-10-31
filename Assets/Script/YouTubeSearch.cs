using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class YouTubeSearch : MonoBehaviour
{
    public TMP_InputField searchInputField; // 검색어 입력 필드
    public Button searchButton;             // 검색 버튼
    public Transform contentPanel;          // 아이템이 추가될 Content Panel
    public GameObject listItemPrefab;       // 아이템 프리팹

    private string apiKey = "AIzaSyDLnGN5d4mY0wKyYcouaGzQwGc-hBZ30lY"; // YouTube API 키 입력
    private string baseUrl = "https://www.googleapis.com/youtube/v3/search";

    void Start()
    {
        searchButton.onClick.AddListener(OnSearchButtonClicked);
    }

    void OnSearchButtonClicked()
    {
        string query = searchInputField.text;
        if (!string.IsNullOrEmpty(query))
        {
            StartCoroutine(SearchYouTube(query));
        }
    }

    IEnumerator SearchYouTube(string query)
    {
        string url = $"{baseUrl}?part=snippet&type=video&maxResults=10&q={UnityWebRequest.EscapeURL(query)}&key={apiKey}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                ClearResults(); // 오류 발생 시 이전 결과 제거
            }
            else
            {
                ParseAndDisplayResults(www.downloadHandler.text);
            }
        }
    }

    void ParseAndDisplayResults(string jsonResponse)
    {
        // 이전 결과를 삭제합니다
        ClearResults();

        // JsonUtility를 사용하여 JSON 응답을 파싱
        YouTubeSearchResult searchResult = JsonUtility.FromJson<YouTubeSearchResult>(jsonResponse);

        int index = 1; // 순서 번호

        foreach (var item in searchResult.items)
        {
            string title = $"{index}. {item.snippet.title}"; // 제목에 순서 번호 추가
            string thumbnailUrl = item.snippet.thumbnails.high.url;
            string videoId = item.id.videoId; // 비디오 ID 추가

            // 비디오 ID를 전달하여 아이템 생성
            AddResultItem(title, thumbnailUrl, videoId);
            index++; // 순서 번호 증가
        }
    }

    // 아이템 추가 메서드
    void AddResultItem(string title, string thumbnailUrl, string videoId)
    {
        GameObject newItem = Instantiate(listItemPrefab, contentPanel);

        // TMP_Text는 프리팹 내에서 자식이든 본인이든 상관없이 하나만 있을 경우 가져와도 문제없음
        TMP_Text titleText = newItem.GetComponentInChildren<TMP_Text>();

        // 본인을 제외하고 자식에서만 Image를 찾기
        Image thumbnailImage = null;
        Image[] images = newItem.GetComponentsInChildren<Image>();
        
        Button button = newItem.GetComponentInChildren<Button>();
        
        foreach (var img in images)
        {
            if (img.gameObject != newItem)
            {
                thumbnailImage = img;
                break;
            }
        }

        // 초기 로딩 상태 설정
        titleText.text = "로딩 중...";

        // 썸네일 로드와 제목 업데이트
        StartCoroutine(LoadThumbnail(thumbnailUrl, thumbnailImage, titleText, title, button));

        // 클릭 이벤트 추가
        string videoUrl = $"https://www.youtube.com/watch?v={videoId}";
        newItem.GetComponent<Button>().onClick.AddListener(() => OnItemClicked(videoUrl));
    }


    void OnItemClicked(string videoUrl)
    {
        
        // PlayerPrefs에 URL 저장
        PlayerPrefs.SetString("YouTubeVideoUrl", videoUrl);
        Debug.Log(videoUrl);

        // 다음 씬으로 이동
        SceneManager.LoadScene("APIScene"); // 다음 씬 이름을 "NextScene"으로 가정
    }

    // 썸네일 이미지를 다운로드하여 Image 컴포넌트에 설정
    IEnumerator LoadThumbnail(string url, Image image, TMP_Text titleText, string title, Button button)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

                // 이미지 활성화
                image.enabled = true;

                // 로드 완료 후 제목 설정
                titleText.text = title;

                titleText.enabled = true;

                button.enabled = true;
            }
            else
            {
                Debug.LogError("Failed to load thumbnail: " + www.error);
                titleText.text = "썸네일 로드 실패";
            }
        }
    }

    // 기존 아이템 제거 메서드
    void ClearResults()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
    }
}
