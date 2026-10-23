# Ekran Seçici

Çift (veya çoklu) monitörlü Windows bilgisayarlarda seçtiğiniz programların **hangi monitörde** ve **hangi modda**
(normal pencere / ekranı kapla / tam ekran) açılacağını belirler ve bilgisayar açıldığında bunu **otomatik** yapar.

**Örnek:** Personel 1. monitörde kendi programlarını kullanırken, dışarıdaki 2. ekranda (TV/monitör) yalnızca
Firefox'taki **hasta çağrı ekranı** tam ekran açık kalır. PC açılınca çağrı ekranı kendiliğinden doğru ekrana gelir.

## Özellikler

- Sağ alt köşede (bildirim alanı) simge olarak arka planda çalışır; **tıklayınca** ayar ekranı açılır.
- Her program için ayrı **profil**: program yolu, parametreler, hedef monitör, pencere modu.
- **Pencere modları:** Normal pencere · Ekranı kapla (maximize) · Tam ekran (kenarlıksız veya F11).
- **Windows açılışında otomatik başlatma** (hem Ekran Seçici hem de seçilen profiller için ayrı ayrı).
- **Monitörde tut:** Pencere yanlışlıkla diğer ekrana sürüklenirse geri getirilir.
- **Kapanırsa yeniden başlat:** Çağrı ekranı kapanırsa tekrar açılır.
- **Dış ekran sonradan açılırsa** (ör. TV PC'den sonra açıldı): bağlanmasını bekler, bağlanınca pencereyi oraya taşır.
- **Monitörleri tanımla:** Her ekranda büyük numara göstererek hangi monitörün hangisi olduğunu gösterir.
- **Hazır Firefox hasta çağrı ekranı şablonu** (tek tıkla).
- **Yönetici yetkisi gerektirmez:** `%APPDATA%\EkranSecici` klasörüne kurulur, domain'deki ve yetkisiz PC'lerde çalışır.
- .NET kurulumu gerektirmez (runtime exe'nin içindedir).

## Kurulum

1. [Releases](../../releases) sayfasından (veya GitHub Actions → *Build* → artifact'tan) `EkranSecici-Setup-x.y.z.exe`
   dosyasını indirin.
2. Çalıştırın. Yönetici şifresi **istemez**; program şuraya kurulur:
   `C:\Users\<kullanıcı>\AppData\Roaming\EkranSecici`
3. Kurulumda **"Windows açılışında otomatik başlat"** seçeneği işaretli kalsın.

> Kurulum paketi istemiyorsanız `EkranSecici.exe`'yi tek başına da (taşınabilir) çalıştırabilirsiniz.
> Ayarlar yine `%APPDATA%\EkranSecici\config.json` dosyasına yazılır.

Kurulum kullanıcı bazlıdır: aynı PC'de başka bir kullanıcı oturum açıyorsa o kullanıcı için de kurulmalıdır.

## Hasta çağrı ekranı kurulumu (adım adım)

1. Sağ alttaki **Ekran Seçici** simgesine tıklayın.
2. **Monitörleri tanımla** ile dış ekranın numarasını görün (ör. *2*).
3. **Yeni profil** → **Firefox hasta çağrı ekranı şablonu** düğmesine basın, çağrı ekranı adresini (URL) girin.
4. **Monitör** olarak dış ekranı seçin, **Pencere modu: Tam ekran** olsun.
5. **Test et** ile deneyin, ardından **Kaydet**.

Şablon şu ayarları yapar:

| Alan | Değer |
|---|---|
| Program | `C:\Program Files\Mozilla Firefox\firefox.exe` (otomatik bulunur) |
| Parametreler | `-no-remote -profile "%APPDATA%\EkranSecici\FirefoxCagri" --kiosk "https://adres"` |
| İşlem adı | `firefox` |
| Pencere modu | Tam ekran |
| Açılışta başlat / Monitörde tut / Kapanırsa yeniden başlat | Açık |

- `--kiosk`: Firefox adres çubuğu ve sekmeler olmadan tam ekran açılır.
- `-no-remote -profile ...`: Çağrı ekranı personelin kendi Firefox'undan **ayrı** çalışır. Personel kendi Firefox'unu
  açıp kapatsa da çağrı ekranı etkilenmez.
- İlk açılışta Firefox "hoş geldiniz" sayfası gösterirse, profili bir kez `--kiosk` olmadan açıp kapatın.
- Kiosk modunda pencere ana monitörde kalıyorsa: parametrelerden `--kiosk`'u kaldırıp
  **"Tam ekran için F11 tuşu gönder"** seçeneğini işaretleyin.

## Ayar ekranı

| Ayar | Açıklama |
|---|---|
| Windows açılışında Ekran Seçici'yi otomatik başlat | `HKCU\...\Run` kaydı (yetki gerektirmez). |
| Açılışta programları başlatmadan önce bekle | Ağın ve masaüstünün hazır olması için (varsayılan 10 sn). |
| Hedef monitör bağlı değilse bekle | Dış ekran geç açılıyorsa bekleme süresi (varsayılan 60 sn). Süre dolarsa program ana monitörde **normal pencere** açılır, ekran bağlanınca oraya taşınır. |

Profil seçenekleri:

| Seçenek | Açıklama |
|---|---|
| Monitör | Hedef ekran. 1 = ana monitör, diğerleri soldan sağa. |
| Pencere modu | Normal pencere / Ekranı kapla / Tam ekran |
| F11 gönder | Tam ekranı programın kendi F11 kısayoluyla yapar (tarayıcılar). |
| Her zaman üstte | Pencere diğer pencerelerin üstünde kalır. |
| Windows açılışında otomatik başlat | Profil açılışta başlatılır. |
| Monitörde ve modda tut | Pencere başka ekrana taşınırsa/küçültülürse geri getirilir. |
| Kapanırsa yeniden başlat | Pencere kapanırsa birkaç saniye içinde yeniden açılır. |
| İşlem adı | Pencere farklı bir işleme aitse (ör. UWP'de `ApplicationFrameHost`). |
| Başlık içerir | Yalnızca başlığı bu metni içeren pencere taşınır. |

## Dosyalar

| Yol | İçerik |
|---|---|
| `%APPDATA%\EkranSecici\EkranSecici.exe` | Program |
| `%APPDATA%\EkranSecici\config.json` | Profiller ve ayarlar (başka PC'ye kopyalanabilir) |
| `%APPDATA%\EkranSecici\logs\` | Günlükler (7 gün saklanır) |
| `%APPDATA%\EkranSecici\FirefoxCagri\` | Çağrı ekranı için ayrı Firefox profili |

Aynı ayarları birden çok PC'ye dağıtmak için bir PC'de hazırlanan `config.json`'u diğerlerine kopyalayın.
(Monitör aygıt adı farklıysa monitör numarasına göre eşleştirilir.)

Komut satırı:

| Komut | Açıklama |
|---|---|
| `EkranSecici.exe` | Başlatır (zaten çalışıyorsa ayar ekranını öne getirir). |
| `EkranSecici.exe --autostart` | Açılış modu: bekler ve profilleri başlatır. |
| `EkranSecici.exe --exit` | Çalışan örneği kapatır. |

## Kaldırma

*Ayarlar → Uygulamalar → Ekran Seçici → Kaldır* veya Başlat menüsündeki **Ekran Seçici Kaldır**.
Kaldırırken profillerin de silinip silinmeyeceği sorulur.

## Sorun giderme

- **Pencere taşınmıyor:** Hedef program yönetici olarak çalışıyorsa Windows, normal yetkili bir programın onu taşımasına
  izin vermez. Programı normal yetkiyle çalıştırın.
- **Pencere bulunamadı uyarısı:** *İşlem adı* alanına pencereyi açan işlemin adını (Görev Yöneticisi → Ayrıntılar)
  `.exe` olmadan yazın; gerekirse *Başlık içerir* filtresini kullanın veya bekleme süresini artırın.
- **Program açılışta başlamıyor:** Ayar ekranında "Windows açılışında ... otomatik başlat" işaretli mi kontrol edin;
  Görev Yöneticisi → Başlangıç sekmesinde *EkranSecici* etkin olmalı.
- **AppLocker / yazılım kısıtlama ilkesi** `AppData` altından exe çalıştırmayı engelliyorsa BT biriminin
  `%APPDATA%\EkranSecici\EkranSecici.exe` için istisna tanımlaması gerekir.
- Ayrıntı için `%APPDATA%\EkranSecici\logs` klasöründeki günlüğe bakın.

## Neden Windows Hizmeti değil?

Windows Hizmetleri ayrı bir oturumda (*Session 0*) çalışır; kullanıcının masaüstüne pencere açamaz/taşıyamaz ve
kurulumu yönetici yetkisi gerektirir. Ekran Seçici bu yüzden kullanıcı oturumunda, oturum açılışında otomatik başlayan
bir **arka plan (tepsi) uygulaması** olarak çalışır — hizmet gibi görünmez çalışır ama pencerelere erişebilir.

## Geliştirme

Gereksinim: .NET 8 SDK, (kurulum paketi için) [Inno Setup 6](https://jrsoftware.org/isinfo.php).

```powershell
# Windows'ta derleme + kurulum paketi -> dist\EkranSecici-Setup-<sürüm>.exe
./build.ps1 -Version 1.0.0

# Yalnızca derleme
dotnet build -c Release
```

- `main`'e her push'ta GitHub Actions kurulum paketini artifact olarak üretir.
- `v1.2.3` biçiminde bir etiket push'landığında kurulum paketi GitHub Release'e eklenir.
- Mimari ve kodlama kuralları için [CLAUDE.md](CLAUDE.md) dosyasına bakın.
- İkonu yeniden üretmek için: `python3 tools/make_icon.py` (Pillow gerekir).
