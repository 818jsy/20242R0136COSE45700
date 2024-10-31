from flask import Flask, request, send_file, jsonify
import os
import subprocess
import librosa
import random
from spleeter.separator import Separator

app = Flask(__name__)


# 폴더 초기화 함수
def clear_results_folder(folder_path):
    # 폴더 내 파일을 삭제
    for filename in os.listdir(folder_path):
        file_path = os.path.join(folder_path, filename)
        try:
            if os.path.isfile(file_path) or os.path.islink(file_path):
                os.unlink(file_path)  # 파일 또는 심볼릭 링크 삭제
            elif os.path.isdir(file_path):
                os.rmdir(file_path)  # 하위 폴더가 있는 경우 삭제
        except Exception as e:
            print(f'Failed to delete {file_path}. Reason: {e}')


# 유튜브 오디오 다운로드 함수
def download_audio_ytdlp(url, output_path):
    audio_file = os.path.join(output_path, "youtube_audio.mp3")
    result = subprocess.run(
        ["yt-dlp", "-f", "bestaudio", "--extract-audio", "--audio-format", "mp3", "-o", audio_file, url],
        capture_output=True,
        text=True
    )
    if result.returncode != 0:
        raise RuntimeError(f"yt-dlp failed: {result.stderr}")
    return audio_file


# 음성 분리 및 온셋 탐지 함수
def process_audio(youtube_url):
    results_folder = './results'
    os.makedirs(results_folder, exist_ok=True)

    # 기존 파일 삭제
    clear_results_folder(results_folder)

    # 1. 오디오 다운로드
    audio_path = download_audio_ytdlp(youtube_url, results_folder)
    if not os.path.exists(audio_path):
        raise FileNotFoundError("Audio file not found after download.")

    # 2. Spleeter를 사용한 음성 분리
    separator = Separator('spleeter:2stems', multiprocess=False)
    output_path = os.path.join(results_folder, 'audio_output')
    separator.separate_to_file(audio_path, output_path)

    # 분리된 음성 파일의 경로
    vocal_path = os.path.join(output_path, 'youtube_audio', 'vocals.wav')
    if not os.path.exists(vocal_path):
        raise FileNotFoundError("Vocal track not found after separation.")

    # 3. Librosa를 사용한 Onset Detection
    y, sr = librosa.load(vocal_path, sr=None)
    onset_frames = librosa.onset.onset_detect(y=y, sr=sr, backtrack=True, delta=0.1)
    onset_times = librosa.frames_to_time(onset_frames, sr=sr)

    # 4. 결과를 텍스트 파일로 저장, 각 시간 앞에 1-4의 랜덤 숫자 추가
    output_file = os.path.join(results_folder, "vocal_onset_times.txt")
    with open(output_file, "w") as f:
        for onset_time in onset_times:
            random_number = random.randint(1, 4)
            f.write(f"{random_number},{onset_time:.2f}\n")

    return audio_path, output_file


@app.route('/process-youtube', methods=['POST'])
def process_youtube():
    data = request.json
    youtube_url = data.get("youtube_url")
    if not youtube_url:
        return jsonify({"error": "No YouTube URL provided"}), 400

    try:
        audio_file, onset_file = process_audio(youtube_url)
        # JSON 형태로 응답을 보냅니다.
        return jsonify({
            "audio_file": audio_file,
            "onset_file": onset_file
        })
    except Exception as e:
        return jsonify({"error": str(e)}), 500


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
