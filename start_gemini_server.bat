@echo off
chcp 65001 >nul
title Aurelia Physics 9 - Gemini AI Server (Port 5050)
cls
echo =====================================================================
echo    AURELIA ACADEMY - PHYSICS 9 GEMINI AI TUTOR BACKEND SERVER
echo =====================================================================
echo.

:: 1. Kiem tra xem port 5050 da co tien trinh chay chua
netstat -ano | findstr :5050 | findstr LISTENING >nul
if %errorlevel% == 0 (
    echo [THONG BAO] Server DA DANG CHAY tren port 5050!
    echo Backend da san sang de Unity Game 2 ket noi.
    echo.
    goto :health_check
)

:: 2. Tim Python tren he thong
where python >nul 2>nul
if %errorlevel% neq 0 (
    echo [LOI] Khong tim thay Python trong bien moi truong PATH.
    echo Vui long cai dat Python 3.9 tro len de chay server!
    echo.
    pause
    exit /b 1
)

echo Dang khoi dong Python Backend tai Backend/PhysicsGeminiServer/physics_gemini_server.py ...
echo Ban co the thu nho cua so nay, khong can tat khi dang chay Unity.
echo.
python -u -X utf8 Backend/PhysicsGeminiServer/physics_gemini_server.py
pause
exit /b 0

:health_check
echo Dang kiem tra trang thai Backend qua http://127.0.0.1:5050/health ...
curl -s http://127.0.0.1:5050/health
echo.
echo.
echo =====================================================================
echo Server dang hoat dong binh thuong. Ban co the giu cua so nay hoac dong lai.
echo =====================================================================
pause
