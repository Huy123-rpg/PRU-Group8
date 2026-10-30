param([switch]$NonInteractive)
$ErrorActionPreference = 'Stop'
$localDir = Join-Path $PSScriptRoot '.local'
New-Item -ItemType Directory -Force -Path $localDir | Out-Null
try {
    $health = Invoke-RestMethod 'http://127.0.0.1:8787/health' -TimeoutSec 2
    if ($health.service -eq 'dogracer-ai' -and $health.configured) { Write-Output 'DogRacer AI is already running.'; exit 0 }
    throw 'Port 8787 is occupied by an unconfigured or unrelated service.'
} catch {
    if ($_.Exception.Message -like 'Port 8787*') { throw }
}
$nodeCommand = Get-Command node.exe -ErrorAction SilentlyContinue
$nodePath = if ($nodeCommand) { $nodeCommand.Source } else { Join-Path $env:ProgramFiles 'nodejs/node.exe' }
if (-not (Test-Path -LiteralPath $nodePath)) { throw 'Install Node.js 20 or newer.' }
$oldKey = $env:GEMINI_API_KEY
try {
    if ([string]::IsNullOrWhiteSpace($env:GEMINI_API_KEY)) {
        $secretPath = Join-Path $localDir 'gemini-key.dpapi'
        if (-not (Test-Path -LiteralPath $secretPath)) {
            if ($NonInteractive) { throw 'Run Configure-Key.ps1 first.' }
            & (Join-Path $PSScriptRoot 'Configure-Key.ps1')
        }
        $encryptedKey = Get-Content -LiteralPath $secretPath -Raw
        $secureKey = ConvertTo-SecureString $encryptedKey.Trim()
        $env:GEMINI_API_KEY = [System.Net.NetworkCredential]::new('', $secureKey).Password
    }
    $process = Start-Process -FilePath $nodePath -ArgumentList 'server.mjs' -WorkingDirectory $PSScriptRoot -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput (Join-Path $localDir 'backend.log') -RedirectStandardError (Join-Path $localDir 'backend.err.log')
    Set-Content -LiteralPath (Join-Path $localDir 'backend.pid') -Value $process.Id
    for ($attempt = 0; $attempt -lt 20; $attempt++) {
        Start-Sleep -Milliseconds 250
        try {
            $health = Invoke-RestMethod 'http://127.0.0.1:8787/health' -TimeoutSec 1
            if ($health.service -eq 'dogracer-ai' -and $health.configured) { Write-Output 'DogRacer AI started.'; exit 0 }
        } catch { }
        if ($process.HasExited) { throw 'Backend stopped. Check .local/backend.err.log.' }
    }
    throw 'Backend startup timed out.'
} finally { $env:GEMINI_API_KEY = $oldKey }
