$ErrorActionPreference = 'Stop'
$health = $null
try { $health = Invoke-RestMethod 'http://127.0.0.1:8787/health' -TimeoutSec 2 } catch { }
if ($health) {
    if ($health.service -ne 'dogracer-ai') { throw 'Port 8787 belongs to a different service.' }
    $pidPath = Join-Path $PSScriptRoot '.local/backend.pid'
    if (-not (Test-Path -LiteralPath $pidPath)) { throw 'Backend PID file not found; refusing to stop an unknown process.' }
    $backendProcessId = 0
    if (-not [int]::TryParse((Get-Content -LiteralPath $pidPath -Raw).Trim(), [ref]$backendProcessId)) { throw 'Invalid backend PID.' }
    $owners = @(Get-NetTCPConnection -LocalPort 8787 -State Listen -ErrorAction Stop | Select-Object -ExpandProperty OwningProcess)
    $backendProcess = Get-Process -Id $backendProcessId -ErrorAction Stop
    if ($owners -notcontains $backendProcessId -or $backendProcess.ProcessName -ne 'node') {
        throw 'Listener does not match the DogRacer backend PID; refusing to stop it.'
    }
    Stop-Process -Id $backendProcessId
    Start-Sleep -Milliseconds 250
}
& (Join-Path $PSScriptRoot 'Start-DogRacerAI.ps1') -NonInteractive
