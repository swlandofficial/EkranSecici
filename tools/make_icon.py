"""Uygulama ikonunu (Resources/app.ico) üretir: iki monitör, sağdaki vurgulu.

Kullanım: python3 tools/make_icon.py
"""
from pathlib import Path
from PIL import Image, ImageDraw

OUT = Path(__file__).resolve().parent.parent / "src" / "EkranSecici" / "Resources" / "app.ico"
S = 256


def draw_monitor(d, x, y, w, h, body, screen):
    r = max(2, w // 12)
    d.rounded_rectangle([x, y, x + w, y + h], radius=r, fill=body)
    pad = max(2, w // 14)
    d.rectangle([x + pad, y + pad, x + w - pad, y + h - pad], fill=screen)
    sw = w // 4
    d.rectangle([x + w // 2 - sw // 6, y + h, x + w // 2 + sw // 6, y + h + h // 6], fill=body)
    d.rounded_rectangle([x + w // 2 - sw // 2, y + h + h // 6, x + w // 2 + sw // 2, y + h + h // 6 + h // 10],
                        radius=r // 2, fill=body)


img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
d = ImageDraw.Draw(img)
draw_monitor(d, 8, 52, 128, 96, (55, 65, 81, 255), (148, 163, 184, 255))
draw_monitor(d, 108, 30, 140, 112, (17, 94, 89, 255), (45, 212, 191, 255))
# Hedef ekrandaki "pencere" işareti
d.rectangle([132, 54, 224, 118], fill=(255, 255, 255, 255))
d.rectangle([132, 54, 224, 66], fill=(13, 148, 136, 255))
# Seçim oku
d.polygon([(60, 200), (150, 200), (150, 180), (196, 214), (150, 248), (150, 228), (60, 228)], fill=(13, 148, 136, 255))

OUT.parent.mkdir(parents=True, exist_ok=True)
img.save(OUT, sizes=[(16, 16), (20, 20), (24, 24), (32, 32), (40, 40), (48, 48), (64, 64), (128, 128), (256, 256)])
print(f"Yazıldı: {OUT}")
