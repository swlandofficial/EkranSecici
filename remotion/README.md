# Ekran Seçici — Tanıtım Videosu (Remotion)

Ekran Seçici'nin Türkçe seslendirmeli ve altyazılı tanıtım videosu. [Remotion](https://www.remotion.dev/)
(React + TypeScript) ile hazırlanır; seslendirme [Piper TTS](https://github.com/OHF-Voice/piper1-gpl)
(`tr_TR-dfki-medium` sesi) ile üretilir.

| Kompozisyon | Boyut | Kullanım |
|---|---|---|
| `EkranSeciciYatay` | 1920×1080, 30 fps | YouTube, web sitesi |
| `EkranSeciciDikey` | 1080×1920, 30 fps | Reels / TikTok / Shorts |
| `YoutubeKapak` | 1280×720 (resim) | YouTube kapak resmi |

Süre ≈ 58 sn (seslendirmeye göre otomatik hesaplanır).

## Kurulum

Gereksinimler: Node.js 18+, Python 3.9+, ffmpeg gerekmez (Remotion kendi getirir).

```bash
cd remotion
npm install

# Seslendirme için (sanal ortam önerilir)
python3 -m venv .venv
source .venv/bin/activate          # Windows: .venv\Scripts\activate
pip install piper-tts
```

### Tarayıcı (Chromium)

Remotion kareleri Chromium ile çizer. `remotion.config.ts` hazır kurulu tarayıcıyı kullanır, **indirme yapmaz**:

1. `REMOTION_BROWSER_EXECUTABLE` ortam değişkeni (tam yol), yoksa
2. Playwright'ın Chromium'u (`$PLAYWRIGHT_BROWSERS_PATH`, varsayılan `/opt/pw-browsers`).

Kendi bilgisayarınızda Chrome/Chromium yolunu verin, örn. Windows'ta:

```powershell
$env:REMOTION_BROWSER_EXECUTABLE = "C:\Program Files\Google\Chrome\Application\chrome.exe"
```

Hiçbiri bulunamazsa Remotion varsayılan davranışına döner (kendi tarayıcısını indirir).

## Kullanım

```bash
npm run voice            # Seslendirme + altyazı zamanlaması üret (model yoksa models/ klasörüne indirir)
npm run studio           # Remotion Studio: sahneleri tarayıcıda canlı düzenle
npm run preview          # Yatay önizleme, yarım çözünürlük → out/preview.mp4
npm run render           # Yatay 1080p → out/ekransecici-1080p.mp4
npm run render:vertical  # Dikey 1080x1920 → out/ekransecici-dikey.mp4
npm run thumbnail        # YouTube kapak resmi → out/youtube-kapak.png
npm run typecheck        # TypeScript kontrolü
```

`npm run voice` çalıştırılmadan da stüdyo açılır: süreler metin uzunluğundan tahmin edilir, ses çalınmaz.
Seslendirmeyi `.venv` içinde kurduysanız `npm run voice`'tan önce ortamı etkinleştirin
(veya `.venv/bin/python scripts/generate_voice.py`). Okuma hızı: `python3 scripts/generate_voice.py --length-scale 1.05`.

## Seslendirme ve altyazı nasıl çalışır?

1. **Metin:** [`scripts/narration.json`](scripts/narration.json) — bölümler ve cümleler. `text` altyazıda görünen
   yazımdır. `pauseAfter` cümleden sonraki sessizliği (sn) ayarlar.
2. **Okunuş sözlüğü:** [`scripts/pronunciations.json`](scripts/pronunciations.json) — farklı okunan kelimeler
   (marka adları, İngilizce terimler) seslendirmeye fonetik yazımla verilir, altyazıda doğru yazım kalır.
   Örn. `Firefox → Fayırfoks`, `Kiosk → Kiyosk`, `Domain → Domeyn`. Ekler korunur (`Domain'deki → Domeyndeki`).
   Tek bir cümleye özel okunuş için cümleye `"say": "..."` alanı eklenebilir.
3. **Betik:** [`scripts/generate_voice.py`](scripts/generate_voice.py) her cümleyi ayrı sentezler, bölüm başına
   `public/voice/<bölüm>.wav` yazar ve cümle başlangıç/bitiş zamanlarını `public/voice/timings.json`'a kaydeder.
4. **Video:** Sahne süreleri `timings.json`'dan hesaplanır (`src/timeline.ts`): her sahne = kısa giriş + konuşma +
   kuyruk sessizliği. Altyazı (`src/components/Subtitles.tsx`) o anda okunan cümleyi alt kısımda yarı saydam
   kutuda gösterir; dikeyde alttan 280 px yukarıda durur (platform arayüzünün altında kalmaz).

Metni değiştirdikten sonra `npm run voice` ile sesi yeniden üretin; sahneler yeni sürelere kendiliğinden uyar.

## Yapı

```
remotion/
  remotion.config.ts        # Tarayıcı seçimi (indirme yok), çıktı ayarları
  scripts/                  # Seslendirme metni, okunuş sözlüğü, Piper betiği
  public/logo.png           # Uygulama ikonundan (src/EkranSecici/Resources/app.ico)
  src/
    theme.ts                # Marka renkleri + yazı tipi (Inter, Segoe UI'ya en yakın Google Fonts)
    timeline.ts             # timings.json → sahne süreleri, cümle kareleri
    layout.ts               # Yatay/dikey yerleşim ölçüleri, altyazı güvenli alanı
    Video.tsx               # Sahne dizisi, geçişler, ses, altyazı
    Thumbnail.tsx           # YouTube kapak resmi
    scenes/                 # Intro, Reveal, Monitors, Autostart, Watchdog, Firefox, NoAdmin, Outro
    components/             # Monitör çizimleri, sanal pencereler, kart/rozet, altyazı
```

Sahneler yatay ve dikeyde aynıdır; `Split` bileşeni panelleri yatayda yan yana, dikeyde alt alta dizer.

## Git'e eklenmeyenler

`node_modules/`, `out/` (render çıktıları), `models/` (Piper modeli, ~63 MB), `public/voice/` (üretilen sesler),
`.venv/`.
