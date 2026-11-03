# Script khoi dong Gemini Backend cho PowerShell
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host "   AURELIA ACADEMY - PHYSICS 9 GEMINI AI TUTOR BACKEND SERVER" -ForegroundColor Cyan
Write-Host "=====================================================================" -ForegroundColor Cyan

$conn = Get-NetTCPConnection -LocalPort 5050 -ErrorAction SilentlyContinue
if ($conn) {
    Write-Host "[THONG BAO] Server da dang chay tren port 5050 (PID: $($conn[0].OwningProcess))!" -ForegroundColor Green
    try {
        $res = Invoke-RestMethod -Uri "http://127.0.0.1:5050/health" -TimeoutSec 3
        Write-Host "Trang thai: $($res.status) | Model: $($res.model)" -ForegroundColor Green
    } catch {
        Write-Host "Kiem tra /health that bai: $_" -ForegroundColor Yellow
    }
    exit 0
}

Write-Host "Dang khoi dong Python Backend tren port 5050..." -ForegroundColor Yellow
python -u -X utf8 Backend/PhysicsGeminiServer/physics_gemini_server.py
