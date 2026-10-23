; Ekran Seçici — Inno Setup betiği
; Yönetici yetkisi İSTEMEZ: %APPDATA%\EkranSecici klasörüne, yalnızca o kullanıcı için kurar.
; Derleme: iscc /DAppVersion=1.0.0 installer\EkranSecici.iss   (önce publish klasörü oluşturulmalı)

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\publish"
#endif

#define AppName "Ekran Seçici"
#define AppExe "EkranSecici.exe"

[Setup]
AppId={{6F1B2E4A-8C3D-4F57-9A2E-3B7D5C1E9F40}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=SWLand
DefaultDirName={userappdata}\EkranSecici
DisableDirPage=yes
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\dist
OutputBaseFilename=EkranSecici-Setup-{#AppVersion}
SetupIconFile=..\src\EkranSecici\Resources\app.ico
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
  if CurUninstallStep = usPostUninstall then
  begin
    if DirExists(ExpandConstant('{app}')) and
       (SuppressibleMsgBox('Ayarlar (profiller) da silinsin mi?', mbConfirmation, MB_YESNO, IDNO) = IDYES) then
      DelTree(ExpandConstant('{app}'), True, True, True);
  end;
end;
