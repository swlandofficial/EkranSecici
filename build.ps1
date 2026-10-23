<#
.SYNOPSIS
  Ekran Seçici'yi derler ve (Inno Setup kuruluysa) kurulum paketini üretir.
.EXAMPLE
  ./build.ps1
  ./build.ps1 -Version 1.2.0
#>
param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$publish = Join-Path $root "publish"

Write-Host "==> Publish (win-x64, self-contained, tek dosya) - sürüm $Version"
dotnet publish "$root/src/EkranSecici/EkranSecici.csproj" -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true `
    -p:DebugType=none -p:Version=$Version -o $publish
if ($LASTEXITCODE -ne 0) { throw "dotnet publish başarısız" }

$iscc = @(
    (Get-Command iscc -ErrorAction SilentlyContinue).Source,
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1

if (-not $iscc) {
    Write-Warning "Inno Setup (ISCC.exe) bulunamadı. Kurulum paketi üretilmedi; taşınabilir exe: $publish\EkranSecici.exe"
    Write-Warning "Kurmak için: winget install JRSoftware.InnoSetup"
    exit 0
}

Write-Host "==> Kurulum paketi ($iscc)"
& $iscc "/DAppVersion=$Version" "$root/installer/EkranSecici.iss"
if ($LASTEXITCODE -ne 0) { throw "ISCC başarısız" }
Write-Host "==> Hazır: $root\dist\EkranSecici-Setup-$Version.exe"
