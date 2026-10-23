# CLAUDE.md — Ekran Seçici (EkranSecici)

Bu dosya projenin "beyni"dir. Koda dokunmadan önce oku; mimari bir karar değiştiğinde bu dosyayı da güncelle.

## Amaç

Çift (veya çoklu) monitörlü Windows PC'lerde seçilen programların **belirli bir monitörde**, **belirli bir pencere
modunda** (normal / ekranı kapla / tam ekran) açılmasını sağlar ve bunu **PC açılışında otomatik** yapar.

Ana senaryo: Poliklinik/bekleme salonu. Personel 1. monitörde kendi işini yapar; dışarıdaki 2. ekranda (TV/monitör)
yalnızca Firefox'ta açılan **hasta çağrı ekranı** tam ekran durur. PC açıldığında kimse uğraşmadan çağrı ekranı doğru
monitörde açılmalıdır.

## Temel kararlar (değiştirmeden önce düşün)

| Karar | Neden |
|---|---|
| **C# / .NET 8, WinForms** (`net8.0-windows`) | Hedef Windows; tepsi ikonu + basit ayar ekranı için en hafif yol. |
| **Gerçek Windows Hizmeti DEĞİL, kullanıcı oturumunda arka plan (tepsi) işlemi** | Windows Hizmetleri *Session 0*'da çalışır, masaüstüne pencere açamaz/taşıyamaz ve kurulumu yönetici yetkisi ister. Pencere yönetimi ancak kullanıcının oturumunda çalışan bir işlemle yapılabilir. "Hizmet gibi" davranış: HKCU `Run` anahtarıyla oturum açılışında `--autostart` ile başlar, görev çubuğunda görünmez, sağ altta ikon olarak yaşar. |
| **Kurulum yeri: `%APPDATA%\EkranSecici`** (ör. `C:\Users\selcukdogan\AppData\Roaming\EkranSecici`) | Yönetici yetkisi olmayan ve domain'deki PC'lerde kurulabilsin. Hiçbir adım admin istemez (`PrivilegesRequired=lowest`, HKCU kayıtları, kullanıcı Başlat menüsü). |
| **Self-contained, tek dosya exe (win-x64)** | Hedef makinede .NET Runtime kurulu olmayabilir ve kullanıcı kuramaz (admin ister). Runtime exe'nin içinde gelir. |
| **Kurulum paketi: Inno Setup** (`installer/EkranSecici.iss`) | Per-user kurulum, Türkçe sihirbaz, kaldırma kaydı (HKCU), otomatik başlatma seçeneği. |
| **Per-Monitor V2 DPI farkındalığı** | Farklı ölçekli monitörlerde (ör. %100 + %150) koordinatların fiziksel piksel olması ve pencerenin doğru boyutlanması için. |
| **Ayarlar JSON** (`%APPDATA%\EkranSecici\config.json`) | Elle düzenlenebilir, yedeklenebilir, başka PC'ye kopyalanabilir. |

## Dizin yapısı

```
EkranSecici.sln
src/EkranSecici/
  EkranSecici.csproj        # net8.0-windows, WinForms, PerMonitorV2, ikon gömülü
  app.manifest              # asInvoker (asla admin isteme)
  Program.cs                # Giriş: tek örnek (mutex), --autostart / --exit argümanları
  AppInfo.cs                # Uygulama adı, sürüm, veri klasörü yolları
  Models/
    AppConfig.cs            # Genel ayarlar + LaunchProfile listesi
    LaunchProfile.cs        # Bir programın nerede/nasıl açılacağı
  Services/
    ConfigStore.cs          # config.json oku/yaz (atomik yazma, bozuk dosya yedekleme)
    Log.cs                  # %APPDATA%\EkranSecici\logs\log-YYYYMMDD.txt (7 gün saklanır)
    AutoStartService.cs     # HKCU\...\CurrentVersion\Run kaydı
    MonitorService.cs       # Monitör listesi, profilin hedef monitörünü çözme
    WindowFinder.cs         # EnumWindows ile üst seviye pencereleri listeleme
    WindowPlacer.cs         # Pencereyi monitöre taşı + Normal / Maximize / Tam ekran uygula
    AppLauncher.cs          # Programı başlat, yeni penceresini bul, yerleştir, takip et
  Native/NativeMethods.cs   # user32 / dwmapi P/Invoke tanımları
  UI/
    TrayApplicationContext.cs # Tepsi ikonu, menü, açılış akışı, bekçi (watchdog) zamanlayıcısı
    MainForm.cs               # Profil listesi + genel ayarlar (kapatınca tepsiye gizlenir)
    ProfileEditorForm.cs      # Profil ekle/düzenle, "Firefox hasta çağrı şablonu"
    MonitorIdentifyForm.cs    # Her monitörde büyük numara gösterir
    AppIcons.cs               # Gömülü ikonun yüklenmesi
  Resources/app.ico         # tools/make_icon.py ile üretilir
installer/EkranSecici.iss   # Inno Setup betiği (per-user, Türkçe)
build.ps1                   # Windows'ta yerel derleme + kurulum paketi
.github/workflows/build.yml # CI: publish + Inno Setup -> EkranSecici-Setup.exe (artifact / release)
tools/make_icon.py          # İkon üretici (Pillow)
```

UI tamamen kodla kuruluyor (Designer dosyası yok). Yeni form eklerken aynı stili koru: `TableLayoutPanel` /
`FlowLayoutPanel`, `AutoScaleMode.Dpi`, sabit piksel konumlandırmadan kaçın.

## Çalışma akışı

1. **Oturum açılışı:** `Run` kaydı → `EkranSecici.exe --autostart`.
2. `Program.Main`: mutex ile tek örnek. İkinci örnek açılırsa ilkine "pencereni göster" sinyali (`EventWaitHandle`)
   gönderip çıkar. `--exit` ilk örneği kapatır (kurulum/kaldırma bunu kullanır).
3. `TrayApplicationContext`: ayarları yükler, `Run` kaydını ayarla senkronlar, tepsi ikonunu kurar.
   `--autostart` ise `StartupDelaySeconds` bekler, sonra `AutoStart=true` olan profilleri sırayla başlatır.
   Hedef monitör henüz bağlı değilse `MonitorWaitSeconds` kadar bekler.
4. `AppLauncher.StartOrPlaceAsync`:
   - Başlatmadan önce mevcut pencerelerin anlık görüntüsünü alır.
   - `Process.Start` (ShellExecute) ile programı başlatır.
   - Süre dolana kadar (`WindowWaitSeconds`) **yeni** görünen, işlem adı (veya PID) eşleşen, varsa başlık filtresine
     uyan üst seviye pencereyi arar. (Firefox gibi programlarda başlatılan PID ile pencere sahibi PID farklıdır;
     bu yüzden işlem adı ile eşleştirme esastır.)
   - Bulunan pencereyi `WindowPlacer.EnsurePlaced` ile yerleştirir; bazı programlar açıldıktan sonra kendi kayıtlı
     konumuna döndüğü için kısa süre sonra bir kez daha uygular.
5. **Bekçi (her 5 sn):** `KeepOnMonitor` profillerinde pencere başka monitöre kaydıysa geri taşır;
   `RestartIfClosed` profillerinde pencere kapandıysa (15 sn bekleme ile) yeniden başlatır;
   `AlwaysOnTop` profillerinde `HWND_TOPMOST` her turda yeniden uygulanır (bayrak varken bile — başka en-üstte
   pencereler sırayı değiştirebilir). (Yeniden) başlatılan `AlwaysOnTop` penceresi ayrıca öne getirilir ve odak
   personelin penceresine iade edilir; aksi halde Windows yeni pencereyi arkada açabilir.
6. **Pasif profiller** (`Enabled=false`): açılışta, "Tümünü başlat"ta, bekçide ve ekran değişikliğinde atlanır;
   `StartOrPlaceAsync` hata mesajı döner. Pasif yapılınca açık pencere kapatılmaz, `AppLauncher.Forget` ile izleme
   bırakılır. Ayar ekranındaki "Test et" pasif profilde de çalışır.
7. **Ekran ayarı değişince** (`SystemEvents.DisplaySettingsChanged`, ör. dış ekran sonradan açıldı): takip edilen tüm
   pencereler yeniden yerleştirilir.

## Pencere yerleştirme kuralları (`WindowPlacer`)

- `EnsurePlaced` **idempotent** olmalı: bekçi her 5 sn çağırır. Zaten doğru yerdeyse hiçbir şey yapmamalı
  (özellikle F11 gönderimi iki kez yapılırsa tam ekran kapanır!).
- **Normal:** Pencere hedef monitörün çalışma alanında ortalanır; sığmıyorsa çalışma alanının %80'i.
- **Ekranı kapla (Maximized):** Önce hedef monitöre taşınır, sonra `SW_MAXIMIZE`.
- **Tam ekran:**
  - Varsayılan: *kenarlıksız tam ekran* — başlık/kenar stilleri kaldırılır, pencere monitörün tüm alanına
    (`Bounds`, görev çubuğu dahil) oturtulur.
  - `SendF11=true`: pencere hedef monitöre taşınır, öne getirilir ve F11 gönderilir (tarayıcılar için),
    sonra odak önceki pencereye iade edilir.
  - Firefox için önerilen: `--kiosk` parametresi + Tam ekran modu.
- Hedef monitör bulunamazsa pencere ana monitörde **Normal** modda bırakılır (personelin ekranını tam ekranla
  kapatmamak için); monitör bağlanınca bekçi/ekran-değişikliği olayı doğru yere taşır.
- Monitör eşleştirme sırası: `MonitorDeviceName` (ör. `\\.\DISPLAY2`) → `MonitorIndex` (1 = ana monitör, sonra
  soldan sağa) → bulunamadı.

## Firefox hasta çağrı ekranı notları

- Şablon parametreleri: `-no-remote -profile "%APPDATA%\EkranSecici\FirefoxCagri" --kiosk "https://..."`
- `-no-remote` + ayrı profil: Personelin kendi Firefox'u açık olsa bile çağrı ekranı **ayrı bir işlem** olarak açılır;
  aksi halde komut mevcut Firefox'a devredilir ve `--kiosk` yok sayılır.
- Profil klasörü yoksa `AppLauncher` başlatmadan önce oluşturur.
- Argümanlardaki `%ORTAM_DEGISKENI%` ifadeleri başlatmadan önce genişletilir.

## Derleme

Windows'ta:
```powershell
./build.ps1              # publish + (Inno Setup kuruluysa) dist\EkranSecici-Setup-<sürüm>.exe
./build.ps1 -Version 1.2.0
```
Elle:
```bash
dotnet publish src/EkranSecici/EkranSecici.csproj -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true \
  -o publish
iscc /DAppVersion=1.0.0 installer/EkranSecici.iss
```
Linux/macOS'ta derleme doğrulaması: `dotnet build -c Release` (csproj'da `EnableWindowsTargeting=true` var);
çalıştırma yalnızca Windows'ta mümkündür.

CI: `main`'e push ve `v*` etiketlerinde GitHub Actions kurulum paketini üretir; etikette Release'e ekler.

## Kodlama kuralları

- Kullanıcıya görünen tüm metinler **Türkçe**. Kod tanımlayıcıları İngilizce, yorumlar Türkçe ve kısa.
- Hiçbir özellik admin yetkisi gerektirmemeli: HKLM'ye yazma, `Program Files`'a yazma, hizmet kurma YOK.
- Win32 çağrıları yalnızca `Native/NativeMethods.cs` içinde tanımlanır.
- Hatalar kullanıcıyı engellememeli: arka planda `Log`'a yaz, elle tetiklenen işlemlerde `MessageBox`/balon göster.
- `config.json` şemasına alan eklerken varsayılan değer ver (eski dosyalar sorunsuz okunmalı); alan silme/yeniden
  adlandırma yapma.
- Başlatılan programları uygulama kapanırken **öldürme**; Ekran Seçici yalnızca açar ve konumlar.

## Bilinen sınırlamalar / dikkat

- AppLocker/Yazılım Kısıtlama İlkesi `%APPDATA%` altından exe çalıştırmayı engelliyorsa BT'nin yol/imza istisnası
  eklemesi gerekir.
- Yönetici olarak çalışan (yükseltilmiş) bir programın penceresi, normal yetkili Ekran Seçici tarafından taşınamaz
  (UIPI). Hedef programı normal yetkiyle çalıştırın.
- UWP/Store uygulamalarında pencere sahibi `ApplicationFrameHost` olduğu için `ProcessName` alanına
  `ApplicationFrameHost` ve `WindowTitleContains` filtresi girilmelidir.
