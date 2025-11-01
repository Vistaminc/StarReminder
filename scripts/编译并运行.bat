@echo off
chcp 65001 > nul
echo ========================================
echo   StarReminder 编译并运行
echo ========================================
echo.

cd /d "%~dp0.."

echo [步骤 1/4] 清理旧文件...
rmdir /s /q obj 2>nul
rmdir /s /q bin 2>nul
rmdir /s /q ToastNotifier\obj 2>nul
rmdir /s /q ToastNotifier\bin 2>nul
echo ✓ 清理完成
echo.

echo [步骤 2/4] 编译 ToastNotifier...
cd ToastNotifier
dotnet build -c Debug --nologo
if %ERRORLEVEL% NEQ 0 exit /b 1
cd ..
echo ✓ ToastNotifier 编译成功
echo.

echo [步骤 3/4] 编译主程序...
dotnet build -c Debug --nologo
if %ERRORLEVEL% NEQ 0 exit /b 1
echo ✓ 主程序编译成功
echo.

echo [步骤 4/4] 部署文件...
set SOURCE_DIR=ToastNotifier\bin\Debug\net8.0-windows10.0.17763.0
set TARGET_DIR=bin\Debug\net8.0-windows10.0.17763.0

copy "%SOURCE_DIR%\ToastNotifier.exe" "%TARGET_DIR%\" /Y >nul
copy "%SOURCE_DIR%\ToastNotifier.dll" "%TARGET_DIR%\" /Y >nul
copy "%SOURCE_DIR%\ToastNotifier.runtimeconfig.json" "%TARGET_DIR%\" /Y >nul
copy "%SOURCE_DIR%\ToastNotifier.deps.json" "%TARGET_DIR%\" /Y >nul
copy "defender.png" "%TARGET_DIR%\" /Y >nul
copy "logo.png" "%TARGET_DIR%\" /Y >nul
echo ✓ 文件部署完成
echo.

echo ========================================
echo   启动 StarReminder...
echo ========================================
echo.

start "" "%TARGET_DIR%\StarReminder.exe"

echo ✓ 程序已启动！
echo.
pause






