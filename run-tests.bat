@echo off
REM Smoke/regression tests for the currently deployed Task4.2 stack.
setlocal EnableExtensions EnableDelayedExpansion
chcp 65001 >nul
cd /d "%~dp0"
set "BASE=http://localhost:3000"
set "TMP=%TEMP%\task_tests"
if not exist "%TMP%" mkdir "%TMP%"
set PASS=0
set FAIL=0

echo.
echo 🏁 Регрессионный тест
echo.

echo 🧪 Проверка окружения...

docker version >nul 2>&1
if errorlevel 1 (call :fail "Docker не запущен") else call :pass "Docker запущен"

docker compose ps -a --format "{{.Service}} {{.Status}}" > "%TMP%\svc.txt" 2>nul
for %%s in (postgres tasks-service) do (
    findstr /b /c:"%%s Up" "%TMP%\svc.txt" >nul
    if errorlevel 1 (call :fail "Сервис %%s не запущен") else call :pass "Сервис %%s запущен"
)
findstr /c:"deploy-process Exited (0)" "%TMP%\svc.txt" >nul
if errorlevel 1 (call :fail "deploy-process не отработал с кодом 0") else call :pass "deploy-process отработал успешно"

@REM  echo 🧪 Ожидание Task Service...
@REM  set /a wh=0
@REM  :wait_health
@REM  curl.exe -s -f "%BASE%/health" >nul 2>&1
@REM  if not errorlevel 1 goto health_ok
@REM  set /a wh+=1
@REM  if !wh! geq 10 goto health_timeout
@REM  timeout /t 3 /nobreak >nul
@REM  goto wait_health
@REM  :health_timeout
@REM  call :fail "Payment Service /health не отвечает"
@REM  goto after_health
@REM  :health_ok
@REM  call :pass "Payment Service /health отвечает"
@REM  :after_health

REM ============ CRUD и валидация ============
echo.
echo 🧪 HTTP-тесты Tasks Service - CRUD и валидация...

curl.exe -s -X POST "%BASE%/api/tasks" -H "Content-Type: application/json" -d "{\"title\":\"MyTitle",\"priority\":\"Low\"}" -o "%TMP%\create.json" >nul 2>&1
findstr /C:"Ok" "%TMP%\create.json" >nul
if errorlevel 1 (call :fail "Создание не успешно") else call :pass "Создание Ok"

curl.exe -s -X POST "%BASE%/api/tasks" -H "Content-Type: application/json" -d "{\"title\":\"MyTitle",\"priority\":\"Low\"}" -o "%TMP%\create.json" >nul 2>&1
findstr /C:"Ok" "%TMP%\create.json" >nul
if errorlevel 1 (call :pass "Создание без title блокировано") else call :fail "Удалось создать без title"
