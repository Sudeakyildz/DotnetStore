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
Write-Host 'API baslatiliyor: http://localhost:5198' -ForegroundColor Cyan
Write-Host 'Not: Eski DB ile migration cakisirsa (Development) DB bir kez silinip yeniden kurulur; Vite proxy icin API bu portta ayakta olmali.' -ForegroundColor DarkGray
dotnet run --launch-profile http @args
