; Ekran Seçici — Inno Setup betiği
; Yönetici yetkisi İSTEMEZ: %APPDATA%\EkranSecici klasörüne, yalnızca o kullanıcı için kurar.
; Derleme: powershell -ExecutionPolicy Bypass -File installer\windows\build.ps1
;          (publish + bu betik -> build\installer\EkranSecici-Setup.exe)
; Kurulum "Programlar ve Özellikler" / "Ayarlar > Uygulamalar" listesine kullanıcı bazında (HKCU) eklenir.

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\..\build\publish"
#endif

#define AppName "Ekran Seçici"
#define AppExe "EkranSecici.exe"

[Setup]
AppId={{6F1B2E4A-8C3D-4F57-9A2E-3B7D5C1E9F40}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=SWLand
AppPublisherURL=https://swlandofficial.github.io/EkranSecici/
AppSupportURL=https://github.com/swlandofficial/EkranSecici/issues
AppUpdatesURL=https://github.com/swlandofficial/EkranSecici/releases/latest
AppComments=Programları seçilen monitörde ve pencere modunda otomatik açar.
VersionInfoVersion={#AppVersion}
VersionInfoProductName={#AppName}
VersionInfoDescription={#AppName} Kurulum
DefaultDirName={userappdata}\EkranSecici
DisableDirPage=yes
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
; Sabit dosya adı: Releases'teki "latest/download/EkranSecici-Setup.exe" bağlantısı her sürümde çalışsın.
OutputDir=..\..\build\installer
OutputBaseFilename=EkranSecici-Setup
SetupIconFile=..\..\src\EkranSecici\Resources\app.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "autostart"; Description: "Windows açılışında otomatik başlat (önerilir)"; GroupDescription: "Başlangıç:"
Name: "desktopicon"; Description: "Masaüstü kısayolu oluştur"; GroupDescription: "Kısayollar:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{userprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{userprograms}\{#AppName} Kaldır"; Filename: "{uninstallexe}"
Name: "{userdesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "EkranSecici"; \
  ValueData: """{app}\{#AppExe}"" --autostart"; Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\{#AppExe}"; Description: "{#AppName} uygulamasını şimdi başlat"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\{#AppExe}"; Parameters: "--exit"; Flags: runhidden waituntilterminated; RunOnceId: "StopEkranSecici"

[UninstallDelete]
Type: filesandordirs; Name: "{app}\logs"

[Code]
// Güncelleme sırasında çalışan örneği kapat (exe kilitli kalmasın).
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
  Exe: String;
begin
  Result := '';
  Exe := ExpandConstant('{app}\{#AppExe}');
  if FileExists(Exe) then
  begin
    Exec(Exe, '--exit', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(1500);
  end;
end;

// Kaldırırken ayarları silmek isteyip istemediğini sor.
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  // Otomatik başlatma kaydını her durumda sil (uygulama ayarlarından da açılmış olabilir).
  if CurUninstallStep = usUninstall then
    RegDeleteValue(HKEY_CURRENT_USER, 'Software\Microsoft\Windows\CurrentVersion\Run', 'EkranSecici');

  if CurUninstallStep = usPostUninstall then
  begin
    if DirExists(ExpandConstant('{app}')) and
       (SuppressibleMsgBox('Ayarlar (profiller) da silinsin mi?', mbConfirmation, MB_YESNO, IDNO) = IDYES) then
      DelTree(ExpandConstant('{app}'), True, True, True);
  end;
end;
