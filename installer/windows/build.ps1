<#
.SYNOPSIS
  Ekran Seçici kurulum paketini üretir: build\installer\EkranSecici-Setup.exe

.DESCRIPTION
  1) dotnet publish (win-x64, self-contained, tek dosya) -> build\publish\EkranSecici.exe
  2) Inno Setup (ISCC)                                    -> build\installer\EkranSecici-Setup.exe

  Gereksinimler: .NET 8 SDK, Inno Setup 6 (winget install JRSoftware.InnoSetup)

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File installer\windows\build.ps1
  powershell -ExecutionPolicy Bypass -File installer\windows\build.ps1 -Version 1.2.0
#>
param(
    # Boşsa src\EkranSecici\EkranSecici.csproj içindeki <Version> kullanılır.
    [string]$Version = ""
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$csproj = Join-Path $root "src\EkranSecici\EkranSecici.csproj"
$publish = Join-Path $root "build\publish"
$installerOut = Join-Path $root "build\installer"

if (-not $Version) {
    $Version = ([xml](Get-Content $csproj -Raw)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    if (-not $Version) { $Version = "1.0.0" }
}

# publish klasöründen çalışan örnek exe'yi kilitler ve publish üzerine yazamaz: önce kapat.
$running = Get-Process -Name EkranSecici -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -and $_.Path.StartsWith($publish, [StringComparison]::OrdinalIgnoreCase) }
if ($running) {
    Write-Host "==> build\publish klasöründen çalışan Ekran Seçici kapatılıyor"
    & (Join-Path $publish "EkranSecici.exe") --exit
    $running | ForEach-Object { if (-not $_.WaitForExit(5000)) { Stop-Process -Id $_.Id -Force } }
}

Write-Host "==> Publish (win-x64, self-contained, tek dosya) - sürüm $Version"
dotnet publish $csproj -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true `
    -p:DebugType=none -p:Version=$Version -o $publish
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish başarısız. Yukarıdaki kırmızı 'error' satırına bakın (EkranSecici.exe başka bir yerden açık olabilir)."
}

$iscc = @(
    (Get-Command iscc -ErrorAction SilentlyContinue).Source,
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1

if (-not $iscc) {
    throw "Inno Setup (ISCC.exe) bulunamadı. Kurmak için: winget install JRSoftware.InnoSetup"
}

Write-Host "==> Kurulum paketi ($iscc)"
& $iscc "/DAppVersion=$Version" (Join-Path $PSScriptRoot "EkranSecici.iss")
if ($LASTEXITCODE -ne 0) { throw "ISCC başarısız" }

$setup = Join-Path $installerOut "EkranSecici-Setup.exe"
Write-Host ""
Write-Host "==> Hazır: $setup"
Write-Host "    GitHub Releases'e bu adla (EkranSecici-Setup.exe) yükleyin; etiket v$Version olsun."
