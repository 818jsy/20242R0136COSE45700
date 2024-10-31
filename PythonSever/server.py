from flask import Flask, request, send_file, jsonify
import os
import subprocess
import librosa
import numpy as np
from spleeter.separator import Separator

app = Flask(__name__)

# 유튜브 오디오 다운로드 함수
def download_audio_yt_dlp(url):
    audio_file = "youtube_audio.mp3"
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
    # 1. 오디오 다운로드
    audio_path = download_audio_yt_dlp(youtube_url)
    if not os.path.exists(audio_path):
        raise FileNotFoundError("Audio file not found after download.")

    # 2. Spleeter를 사용한 음성 분리
    separator = Separator('spleeter:2stems')
    output_path = './audio_output'
    separator.separate_to_file(audio_path, output_path)

    # 분리된 음성 파일의 경로
    vocal_path = os.path.join(output_path, 'youtube_audio', 'vocals.wav')
    if not os.path.exists(vocal_path):
        raise FileNotFoundError("Vocal track not found after separation.")

    # 3. Librosa를 사용한 Onset Detection
    y, sr = librosa.load(vocal_path, sr=None)
    onset_frames = librosa.onset.onset_detect(y=y, sr=sr, backtrack=True, delta=0.1)
    onset_times = librosa.frames_to_time(onset_frames, sr=sr)

    # 4. 결과를 텍스트 파일로 저장
    output_file = "vocal_onset_times.txt"
    with open(output_file, "w") as f:
        for onset_time in onset_times:
            f.write(f"{onset_time:.2f}\n")
    return output_file

@app.route('/process-youtube', methods=['POST'])
def process_youtube():
    data = request.json
    youtube_url = data.get("youtube_url")
    if not youtube_url:
        return jsonify({"error": "No YouTube URL provided"}), 400

    try:
        output_file = process_audio(youtube_url)
        return send_file(output_file, as_attachment=True)
    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
