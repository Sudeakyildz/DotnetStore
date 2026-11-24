# E-ticaret: once 5198'i bosaltir, sonra API + React (iki pencere).
$root = $PSScriptRoot
$apiScript = Join-Path $root 'backend\DotnetStore.Api\dev-run.ps1'

Start-Process powershell -ArgumentList @(
    '-NoExit',
    '-ExecutionPolicy', 'Bypass',
    '-File', $apiScript
)

Start-Sleep -Seconds 4

Start-Process powershell -ArgumentList @(
    '-NoExit',
    '-Command',
    "Set-Location '$root\Frontend'; Write-Host 'Panel: http://localhost:5173' -ForegroundColor Green; npm run dev"
)

Write-Host "Magaza: http://localhost:5173  |  Yonetim: http://localhost:5173/yonetim (admin@dotnetstore.local / Admin123!)" -ForegroundColor Yellow
