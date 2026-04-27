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

Write-Host "Tarayici: http://localhost:5173  |  Giris: admin / admin123" -ForegroundColor Yellow
