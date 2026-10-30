$ErrorActionPreference = 'Stop'
$localDir = Join-Path $PSScriptRoot '.local'
New-Item -ItemType Directory -Force -Path $localDir | Out-Null
$secret = Read-Host 'Gemini API key (hidden)' -AsSecureString
if ($secret.Length -eq 0) { throw 'API key is empty.' }
$secret | ConvertFrom-SecureString | Set-Content -LiteralPath (Join-Path $localDir 'gemini-key.dpapi')
Write-Output 'Key saved with Windows user encryption. It is excluded from Git.'
