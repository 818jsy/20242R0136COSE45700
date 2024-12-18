using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement; // 씬 전환을 위해 필요
using TMPro;
using Newtonsoft.Json.Linq;

public class YouTubeAudioRequester : MonoBehaviour
{
    public string serverUrl = "http://127.0.0.1:5000/process-youtube"; // Flask 서버 URL
    public TextMeshProUGUI logText; // 로그 출력을 위한 TextMeshProUGUI
    public bool isTest = false; // 인스펙터에서 체크할 수 있는 테스트 변수

    void Start()
    {
        if (isTest)
        {
            // 테스트 모드일 경우 5초 후 씬 전환
            StartCoroutine(TransitionToSceneAfterDelay(5f));
        }
        else
        {
            // PlayerPrefs에서 URL 불러오기
            string videoUrl = PlayerPrefs.GetString("YouTubeVideoUrl", "");
            if (!string.IsNullOrEmpty(videoUrl) && videoUrl != "URL이 없습니다.")
            {
                Debug.Log($"URL found in PlayerPrefs: {videoUrl}");
                StartCoroutine(SendRequestToServer(videoUrl)); // URL이 있으면 자동으로 서버 요청 시작
            }
            else
            {
                Debug.Log("No URL found in PlayerPrefs.");
                logText.text = "No URL found in PlayerPrefs.";
            }
        }
    }

    // 서버에 요청을 보내고 결과 파일을 받는 코루틴
    IEnumerator SendRequestToServer(string youtubeUrl)
    {
        // JSON 형식의 요청 데이터 생성
        string jsonData = "{\"youtube_url\": \"" + youtubeUrl + "\"}";
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(serverUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Sending request to server...");

        // 서버 요청 전송
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Request failed. Error: " + request.error);
            logText.text = "Error: " + request.error;
        }
        else
        {
            // 서버 응답 데이터 파싱
            string jsonResponse = request.downloadHandler.text;
            JObject json = JObject.Parse(jsonResponse);
            string audioFilePath = json["audio_file"].ToString();
            string textFilePath = json["onset_file"].ToString();

            // PlayerPrefs에 파일 경로 저장
            PlayerPrefs.SetString("YouTubeAudioPath", audioFilePath);
            PlayerPrefs.SetString("OnsetTimesPath", textFilePath);
            PlayerPrefs.Save();

            // 로그에 출력
            logText.text = $"Download complete! Audio: {audioFilePath}, Text: {textFilePath}";
            Debug.Log($"Download complete! Audio: {audioFilePath}, Text: {textFilePath}");

            // 모든 요청이 완료된 후 SampleScene으로 씬 전환
            SceneManager.LoadScene("SampleScene");
        }
    }

    // 5초 후에 씬 전환하는 코루틴
    IEnumerator TransitionToSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("SampleScene");
    }
}
