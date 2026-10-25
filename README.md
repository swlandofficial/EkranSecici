# Ekran Seçici

Çift (veya çoklu) monitörlü Windows bilgisayarlarda seçtiğiniz programların **hangi monitörde** ve **hangi modda**
(normal pencere / ekranı kapla / tam ekran) açılacağını belirler ve bilgisayar açıldığında bunu **otomatik** yapar.

**Örnek:** Personel 1. monitörde kendi programlarını kullanırken, dışarıdaki 2. ekranda (TV/monitör) yalnızca
Firefox'taki **hasta çağrı ekranı** tam ekran açık kalır. PC açılınca çağrı ekranı kendiliğinden doğru ekrana gelir.

## Özellikler

- Sağ alt köşede (bildirim alanı) simge olarak arka planda çalışır; **tıklayınca** ayar ekranı açılır.
- Her program için ayrı **profil**: program yolu, parametreler, hedef monitör, pencere modu.
- Profiller **aktif/pasif** yapılabilir: pasif profil silinmeden devre dışı kalır.
- **Pencere modları:** Normal pencere · Ekranı kapla (maximize) · Tam ekran (kenarlıksız veya F11).
- **Windows açılışında otomatik başlatma** (hem Ekran Seçici hem de seçilen profiller için ayrı ayrı).
- **Monitörde tut:** Pencere yanlışlıkla diğer ekrana sürüklenirse geri getirilir.
- **Kapanırsa yeniden başlat:** Çağrı ekranı kapanırsa tekrar açılır.
- **Dış ekran sonradan açılırsa** (ör. TV PC'den sonra açıldı): bağlanmasını bekler, bağlanınca pencereyi oraya taşır.
- **Monitörleri tanımla:** Her ekranda büyük numara göstererek hangi monitörün hangisi olduğunu gösterir.
- **Hazır Firefox hasta çağrı ekranı şablonu** (tek tıkla).
- **Yönetici yetkisi gerektirmez:** `%APPDATA%\EkranSecici` klasörüne kurulur, domain'deki ve yetkisiz PC'lerde çalışır.
- .NET kurulumu gerektirmez (runtime exe'nin içindedir).

## Tanıtım ve indirme sayfası

[`docs/index.html`](docs/index.html): tanıtım, özellikler ve indirme düğmesi içeren tek dosyalık sayfa.
Kendi sitenize olduğu gibi yükleyebilir veya GitHub Pages'te (*Settings → Pages → Branch: main, /docs*) yayınlayabilirsiniz.
İndirme düğmeleri her zaman son sürümün kurulum dosyasına bağlanır:
`https://github.com/swlandofficial/EkranSecici/releases/latest/download/EkranSecici-Setup.exe`
(bu yüzden Release'e yüklenen dosyanın adı her sürümde aynen `EkranSecici-Setup.exe` olmalıdır).

## Tanıtım videosu

[`remotion/`](remotion/) klasöründe Remotion ile hazırlanan Türkçe seslendirmeli tanıtım videosu (yatay + dikey)
ve YouTube kapak resmi bulunur. Ayrıntılar: [remotion/README.md](remotion/README.md).

## Kurulum

1. Son sürümün kurulum dosyasını indirin:
   [EkranSecici-Setup.exe](https://github.com/swlandofficial/EkranSecici/releases/latest/download/EkranSecici-Setup.exe)
   ([tüm sürümler](https://github.com/swlandofficial/EkranSecici/releases)).
2. Çalıştırın. Yönetici şifresi **istemez**; program şuraya kurulur:
   `C:\Users\<kullanıcı>\AppData\Roaming\EkranSecici`
3. Kurulumda **"Windows açılışında otomatik başlat"** seçeneği işaretli kalsın.

> Kurulum paketi istemiyorsanız `EkranSecici.exe`'yi tek başına da (taşınabilir) çalıştırabilirsiniz.
> Ayarlar yine `%APPDATA%\EkranSecici\config.json` dosyasına yazılır.

Program **Ayarlar → Uygulamalar** (veya Denetim Masası → Programlar ve Özellikler) listesinde *Ekran Seçici* olarak
görünür ve oradan kaldırılabilir.

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
| Profil aktif | İşaretsizse profil pasiftir: açılışta, "Tümünü başlat"ta ve bekçide atlanır; silinmez. Listede soldaki kutu veya **Aktif / Pasif** düğmesiyle de değiştirilir. Pasif yapılan profilin açık penceresi kapatılmaz, yalnızca izlenmez. |
| Monitör | Hedef ekran. 1 = ana monitör, diğerleri soldan sağa. |
| Pencere modu | Normal pencere / Ekranı kapla / Tam ekran |
| F11 gönder | Tam ekranı programın kendi F11 kısayoluyla yapar (tarayıcılar). |
| Her zaman üstte | Pencere diğer pencerelerin üstünde kalır; kapanıp yeniden açıldığında da en üste gelir. |
| Windows açılışında otomatik başlat | Profil açılışta başlatılır. |
| Monitörde ve modda tut | Pencere başka ekrana taşınırsa/küçültülürse geri getirilir. |
| Program elle açılsa da bu monitöre taşı | Program Ekran Seçici dışında (masaüstü kısayolu vb.) açılsa da penceresi yakalanıp hedef monitöre taşınır. Firefox şablonunda kapalıdır (personelin kendi Firefox pencereleri taşınmasın). |
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
# Inno Setup bir kez kurulur
winget install JRSoftware.InnoSetup

# Derleme + kurulum paketi -> build\installer\EkranSecici-Setup.exe
powershell -ExecutionPolicy Bypass -File installer\windows\build.ps1
# Sürümü elle vermek için (varsayılan: csproj'daki <Version>)
powershell -ExecutionPolicy Bypass -File installer\windows\build.ps1 -Version 1.1.0

# Yalnızca derleme
dotnet build -c Release
```

### Sürüm yayınlama (GitHub Releases)

Sayfadaki indirme düğmeleri Release olmadan **404** verir. İlk sürüm için:

1. `winget install JRSoftware.InnoSetup` ile Inno Setup'ı kurun.
2. Repo klasöründe `powershell -ExecutionPolicy Bypass -File installer\windows\build.ps1` çalıştırın.
3. Oluşan `build\installer\EkranSecici-Setup.exe` dosyasını GitHub'da **Releases → Draft a new release** ile
   yükleyin. Etiket `v1.0.0` olsun, dosya adı aynen `EkranSecici-Setup.exe` kalmalı.
4. **Publish release** deyin.

Alternatif (otomatik): `git tag v1.0.0 && git push origin v1.0.0` — GitHub Actions kurulum paketini Windows'ta
derleyip aynı adla Release'e ekler.

Yeni sürümde `src/EkranSecici/EkranSecici.csproj` içindeki `<Version>` değerini artırın ve aynı adımları yeni etiketle
(`v1.1.0` …) tekrarlayın.

- `main`'e her push'ta GitHub Actions kurulum paketini artifact olarak üretir.
- `v1.2.3` biçiminde bir etiket push'landığında kurulum paketi GitHub Release'e eklenir.
- Mimari ve kodlama kuralları için [CLAUDE.md](CLAUDE.md) dosyasına bakın.
- İkonu yeniden üretmek için: `python3 tools/make_icon.py` (Pillow gerekir).
