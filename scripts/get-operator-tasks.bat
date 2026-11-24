@echo off
setlocal

REM Step 1 & 2: Get JWT token for operator user and extract access_token
echo [1/2] Obtaining JWT token for operator_user...
for /f "usebackq tokens=*" %%T in (`powershell -NoProfile -Command "(curl.exe -s -X POST http://localhost:8081/realms/tasks-realm/protocol/openid-connect/token -H 'Content-Type: application/x-www-form-urlencoded' -d 'client_id=tasksservice' -d 'grant_type=password' -d 'username=operator_user' -d 'password=operator123' | ConvertFrom-Json).access_token"`) do set "JWT_TOKEN=%%T"

if "%JWT_TOKEN%"=="" (
    echo [ERROR] Failed to obtain JWT token.
    exit /b 1
)

echo [OK] JWT token received.

REM Step 3: Call GET /api/tasks with JWT in Authorization header
echo.
echo [2/2] Fetching tasks from TasksService...
curl.exe -i -X GET http://localhost:8080/api/tasks ^
  -H "Authorization: Bearer %JWT_TOKEN%"

echo.
endlocal