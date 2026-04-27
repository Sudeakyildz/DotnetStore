# 5198 portunu ve DotnetStore.Api surecini temizler, sonra API'yi baslatir.
$ErrorActionPreference = 'SilentlyContinue'

Get-Process -Name 'DotnetStore.Api' | Stop-Process -Force

# Port 5198'i dinleyen surec (ikinci dotnet run / unutulmus instance)
$listeners = Get-NetTCPConnection -LocalPort 5198 -State Listen -ErrorAction SilentlyContinue
foreach ($l in $listeners) {
    if ($l.OwningProcess -gt 0) {
        Stop-Process -Id $l.OwningProcess -Force -ErrorAction SilentlyContinue
    }
}

Start-Sleep -Milliseconds 400

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

# Eski hedef cerceve klasorleri (or. net9.0) kaldiysa yanlis exe calismasin diye temizle
$debugOut = Join-Path $PSScriptRoot 'bin\Debug'
if (Test-Path $debugOut) {
    Get-ChildItem $debugOut -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match '^net\d' -and $_.Name -ne 'net8.0' } |
        ForEach-Object {
            Write-Host "Eski derleme siliniyor: $($_.FullName)" -ForegroundColor DarkYellow
            Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
        }
}

Write-Host 'API baslatiliyor: http://localhost:5198' -ForegroundColor Cyan
Write-Host 'Not: Eski DB ile migration cakisirsa (Development) DB bir kez silinip yeniden kurulur; Vite proxy icin API bu portta ayakta olmali.' -ForegroundColor DarkGray
dotnet run --launch-profile http @args
