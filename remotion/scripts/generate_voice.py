#!/usr/bin/env python3
"""Piper TTS ile Türkçe seslendirme ve altyazı zamanlaması üretir.

Girdi : scripts/narration.json, scripts/pronunciations.json
Çıktı : public/voice/<bölüm>.wav  ve  public/voice/timings.json
        (sahne süreleri Remotion tarafında timings.json'dan hesaplanır)

Kullanım:
    pip install piper-tts
    python3 scripts/generate_voice.py            # model yoksa models/ klasörüne indirir
    python3 scripts/generate_voice.py --length-scale 1.05   # daha yavaş okuma
"""
from __future__ import annotations

import argparse
import json
import re
import sys
import urllib.request
import wave
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SCRIPTS = ROOT / "scripts"
MODELS = ROOT / "models"
OUT = ROOT / "public" / "voice"

MODEL = "tr_TR-dfki-medium"
MODEL_URL = "https://huggingface.co/rhasspy/piper-voices/resolve/main/tr/tr_TR/dfki/medium/"

DEFAULT_PAUSE = 0.35  # cümleler arası sessizlik (sn)


def ensure_model() -> Path:
    MODELS.mkdir(exist_ok=True)
    onnx = MODELS / f"{MODEL}.onnx"
    for name in (f"{MODEL}.onnx", f"{MODEL}.onnx.json"):
        target = MODELS / name
        if not target.exists():
            print(f"Model indiriliyor: {name}")
            urllib.request.urlretrieve(MODEL_URL + name, target)
    return onnx


def load_pronunciations() -> list[tuple[re.Pattern[str], str]]:
    data = json.loads((SCRIPTS / "pronunciations.json").read_text(encoding="utf-8"))
    rules = []
    # Uzun ifadeler önce (ör. ".NET" kısa kelimelerden önce)
    for written, spoken in sorted(data.items(), key=lambda kv: -len(kv[0])):
        if written.startswith("_"):
            continue
        rules.append((re.compile(r"(?<!\w)" + re.escape(written), re.IGNORECASE), spoken))
    return rules


def to_spoken(text: str, rules) -> str:
    spoken = text
    for pattern, replacement in rules:
        spoken = pattern.sub(replacement, spoken)
    # Kesme işareti okunuşu bozmasın: Seçici'yi → Seçiciyi
    return spoken.replace("'", "").replace("’", "")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--length-scale", type=float, default=1.0, help=">1 daha yavaş, <1 daha hızlı")
    args = parser.parse_args()

    try:
        from piper import PiperVoice
        from piper.config import SynthesisConfig
    except ImportError:
        print("piper-tts kurulu değil: pip install piper-tts", file=sys.stderr)
        return 1

    voice = PiperVoice.load(str(ensure_model()))
    syn = SynthesisConfig(length_scale=args.length_scale)
    rules = load_pronunciations()
    narration = json.loads((SCRIPTS / "narration.json").read_text(encoding="utf-8"))

    OUT.mkdir(parents=True, exist_ok=True)
    rate = voice.config.sample_rate
    timings = {"sampleRate": rate, "sections": []}

    for section in narration["sections"]:
        frames = bytearray()
        sentences = []
        for i, sentence in enumerate(section["sentences"]):
            text = sentence["text"]
            spoken = sentence.get("say") or to_spoken(text, rules)
            audio = bytearray()
            for chunk in voice.synthesize(spoken, syn_config=syn):
                audio += chunk.audio_int16_bytes

            start = len(frames) / 2 / rate
            frames += audio
            end = len(frames) / 2 / rate
            sentences.append({"text": text, "spoken": spoken, "start": round(start, 3), "end": round(end, 3)})

            if i < len(section["sentences"]) - 1:
                pause = sentence.get("pauseAfter", DEFAULT_PAUSE)
                frames += b"\x00\x00" * int(pause * rate)

        path = OUT / f"{section['id']}.wav"
        with wave.open(str(path), "wb") as wf:
            wf.setnchannels(1)
            wf.setsampwidth(2)
            wf.setframerate(rate)
            wf.writeframes(bytes(frames))

        duration = len(frames) / 2 / rate
        timings["sections"].append(
            {"id": section["id"], "file": f"voice/{section['id']}.wav", "duration": round(duration, 3), "sentences": sentences}
        )
        print(f"{section['id']:<10} {duration:5.2f} sn  ({len(sentences)} cümle)")

    (OUT / "timings.json").write_text(json.dumps(timings, ensure_ascii=False, indent=2), encoding="utf-8")
    total = sum(s["duration"] for s in timings["sections"])
    print(f"Toplam konuşma: {total:.1f} sn → {OUT / 'timings.json'}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
