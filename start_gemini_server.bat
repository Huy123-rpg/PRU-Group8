@echo off
rem =====================================================================
rem   AURELIA ACADEMY - PHYSICS 9 GEMINI AI TUTOR BACKEND LAUNCHER
rem =====================================================================
chcp 65001 >nul
title Aurelia Physics 9 - Gemini AI Server (Port 5050)
cls

echo =====================================================================
echo    AURELIA ACADEMY - PHYSICS 9 GEMINI AI TUTOR BACKEND SERVER
echo =====================================================================
echo.

:: 1. Kiem tra xem port 5050 co dang duoc su dung khong
netstat -ano | findstr :5050 | findstr LISTENING >nul
if %errorlevel% == 0 goto port_in_use

goto start_server

:port_in_use
:: 2. Port 5050 dang mo - Kiem tra xem co dung la Backend Aurelia khong
echo [KIEM TRA] Phat hien port 5050 dang mo. Dang xac minh backend Aurelia...
curl.exe -s -m 4 http://127.0.0.1:5050/health 2>nul | findstr /i "Aurelia" >nul
if %errorlevel% == 0 goto backend_already_running

:: Truong hop port 5050 mo nhung khong phai backend Aurelia
echo.
echo =====================================================================
echo [CANH BAO] Port 5050 dang mo nhung KHONG PHAI Backend Aurelia Physics 9!
echo Tien trinh khac co the dang chiem giu port nay.
echo.
echo Danh sach tien trinh dang lang nghe tren port 5050:
netstat -ano | findstr :5050 | findstr LISTENING
echo.
echo Vui long tat tien trinh chiem port hoac doi port trong Backend va Unity.
echo =====================================================================
echo.
pause
exit /b 1

:backend_already_running
echo.
echo =====================================================================
echo [THONG BAO] Backend Aurelia Physics 9 DA DANG CHAY tren port 5050!
echo Backend da san sang de Unity Game 2 ket noi.
echo Giu nguyen server hien tai, khong khoi dong ban trung lap.
echo =====================================================================
echo.
echo Chi tiet trang thai he thong (GET http://127.0.0.1:5050/health):
echo ---------------------------------------------------------------------
curl.exe -s -m 4 http://127.0.0.1:5050/health
echo.
echo ---------------------------------------------------------------------
echo.
echo Ban co the thu nho cua so nay hoac dong lai. Server van tiep tuc hoat dong.
echo.
pause
exit /b 0

:start_server
:: 3. Kiem tra Python tren he thong
where python >nul 2>nul
if %errorlevel% neq 0 (
    echo [LOI] Khong tim thay Python trong bien moi truong PATH.
    echo Vui long cai dat Python 3.9 tro len va tick chon "Add Python to PATH"!
    echo.
    pause
    exit /b 1
)

:: 4. Kiem tra file ma nguon server
if not exist "Backend\PhysicsGeminiServer\physics_gemini_server.py" (
    echo [LOI] Khong tim thay file: Backend\PhysicsGeminiServer\physics_gemini_server.py
    echo Vui long chay file BAT tu thu muc goc cua du an PRU-Group8!
    echo.
    pause
    exit /b 1
)

:: 5. Kiem tra file cau hinh .env
if not exist "Backend\PhysicsGeminiServer\.env" (
    echo [CANH BAO] Khong tim thay file Backend\PhysicsGeminiServer\.env!
    echo Server se khoi dong nhung khong the goi live Gemini API neu thieu Key.
    echo.
)

echo Dang khoi dong Python Backend tai Backend\PhysicsGeminiServer\physics_gemini_server.py ...
echo Port: 5050 - Endpoint cham: /api/grade_physics
echo Ban co the thu nho cua so nay, khong tat khi dang choi Unity Game 2.
echo.
echo ---------------------------------------------------------------------

python -u -X utf8 Backend\PhysicsGeminiServer\physics_gemini_server.py

set PY_EXIT=%errorlevel%
echo.
echo ---------------------------------------------------------------------
if %PY_EXIT% neq 0 (
    echo.
    echo =====================================================================
    echo [LOI] Backend Server da dung dot ngot voi ma loi: %PY_EXIT%
    echo Vui long kiem tra cac thong bao loi cua Python o phia tren.
    echo =====================================================================
    echo.
) else (
    echo [THONG BAO] Backend Server da dung hoat dong binh thuong.
)
pause
exit /b %PY_EXIT%
