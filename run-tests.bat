@echo off
REM Smoke/regression tests for the currently deployed Task4.2 stack.
setlocal EnableExtensions EnableDelayedExpansion
chcp 65001 >nul
cd /d "%~dp0"
set "BASE=http://localhost:3000"
set "TMP=%TEMP%\task_tests_%RANDOM%"
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
for %%s in (postgres-t tasks-service) do (
    findstr /b /c:"%%s Up" "%TMP%\svc.txt" >nul
    if errorlevel 1 (call :fail "Сервис %%s не запущен") else call :pass "Сервис %%s запущен"
)

echo 🧪 Ожидание Task Service...
set /a wh=0
:wait_health
curl.exe -s -f "%BASE%/health" >nul 2>&1
if not errorlevel 1 goto health_ok
set /a wh+=1
if !wh! geq 10 goto health_timeout
timeout /t 3 /nobreak >nul
goto wait_health
:health_timeout
call :fail "Tasks Service /health не отвечает"
goto after_health
:health_ok
call :pass "Tasks Service /health отвечает"
:after_health

REM ============ CRUD и валидация ============
echo.
echo 🧪 HTTP-тесты Tasks Service - CRUD и валидация...

REM Уникальный title, чтобы тест не зависел от данных прошлых запусков.
set "TITLE=MyTitle_!RANDOM!"

REM 1. POST /api/tasks - создание задачи (ожидаем HTTP 2xx).
REM priority передаём числом: 1 = Low (сервис использует числовое перечисление Priority).
echo {"title":"!TITLE!","priority":1} > "%TMP%\body_create.json"
curl.exe -s -X POST "%BASE%/api/tasks" -H "Content-Type: application/json" -d @"%TMP%\body_create.json" -o "%TMP%\create.json" -w "%%{http_code}" > "%TMP%\create.code" 2>nul
findstr /B /C:"2" "%TMP%\create.code" >nul
if errorlevel 1 (call :fail "Создание не успешно - ожидался HTTP 2xx") else call :pass "Создание Ok"
findstr /C:"!TITLE!" "%TMP%\create.json" >nul
if errorlevel 1 (call :fail "В теле ответа нет созданной задачи") else call :pass "Создание вернуло задачу с title"

REM 2. POST /api/tasks без title (ожидаем отказ - не HTTP 2xx).
echo {"priority":1} > "%TMP%\body_notitle.json"
curl.exe -s -X POST "%BASE%/api/tasks" -H "Content-Type: application/json" -d @"%TMP%\body_notitle.json" -o "%TMP%\create_notitle.json" -w "%%{http_code}" > "%TMP%\create_notitle.code" 2>nul
findstr /B /C:"2" "%TMP%\create_notitle.code" >nul
if not errorlevel 1 (call :fail "Удалось создать задачу без title") else call :pass "Создание без title блокировано"

REM 3. GET /api/tasks - получение всех задач (ожидаем HTTP 2xx + JSON-массив с созданной задачей).
curl.exe -s "%BASE%/api/tasks" -o "%TMP%\getall.json" -w "%%{http_code}" > "%TMP%\getall.code" 2>nul
findstr /B /C:"2" "%TMP%\getall.code" >nul
if errorlevel 1 (call :fail "GET /api/tasks не успешен - ожидался HTTP 2xx") else call :pass "GET /api/tasks Ok"
findstr /C:"[{" "%TMP%\getall.json" >nul
if errorlevel 1 (call :fail "GET /api/tasks вернул не JSON-массив") else call :pass "GET /api/tasks вернул JSON-массив"
findstr /C:"!TITLE!" "%TMP%\getall.json" >nul
if errorlevel 1 (call :fail "Созданная задача не найдена в списке") else call :pass "Созданная задача присутствует в списке"

rmdir /s /q "%TMP%" >nul 2>&1

echo.
echo ==========================================
echo Результаты: PASS=!PASS! FAIL=!FAIL!
echo ==========================================
if !FAIL! gtr 0 (
    echo ❌ Регрессионный тест НЕ ПРОЙДЕН
    exit /b 1
)
echo ✅ Регрессионный тест ПРОЙДЕН
exit /b 0

:pass
echo   ✔ %~1
set /a PASS+=1
goto :eof

:fail
echo   ✘ %~1
set /a FAIL+=1
goto :eof