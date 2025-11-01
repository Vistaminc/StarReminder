@echo off
chcp 65001 > nul
echo ========================================
echo   StarReminder 一键编译脚本
echo ========================================
echo.

cd /d "%~dp0.."

echo [步骤 1/6] 清理旧的编译文件...
echo.
rmdir /s /q obj 2>nul
rmdir /s /q bin 2>nul
rmdir /s /q ToastNotifier\obj 2>nul
rmdir /s /q ToastNotifier\bin 2>nul
echo ✓ 清理完成
echo.

echo [步骤 2/6] 编译 ToastNotifier 子程序...
echo.
cd ToastNotifier
dotnet build -c Debug --nologo

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ ToastNotifier 编译失败！
    echo.
    pause
    exit /b 1
)

echo ✓ ToastNotifier 编译成功
echo.
cd ..

echo [步骤 3/6] 编译主程序 StarReminder...
echo.
dotnet build -c Debug --nologo

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ 主程序编译失败！
    echo.
    pause
    exit /b 1
)

echo ✓ 主程序编译成功
echo.

echo [步骤 4/6] 部署 ToastNotifier 文件...
echo.

set SOURCE_DIR=ToastNotifier\bin\Debug\net8.0-windows10.0.17763.0
set TARGET_DIR=bin\Debug\net8.0-windows10.0.17763.0

if not exist "%TARGET_DIR%" (
    echo ❌ 输出目录不存在：%TARGET_DIR%
    pause
    exit /b 1
)

copy "%SOURCE_DIR%\ToastNotifier.exe" "%TARGET_DIR%\" /Y >nul
copy "%SOURCE_DIR%\ToastNotifier.dll" "%TARGET_DIR%\" /Y >nul
copy "%SOURCE_DIR%\ToastNotifier.runtimeconfig.json" "%TARGET_DIR%\" /Y >nul
copy "%SOURCE_DIR%\ToastNotifier.deps.json" "%TARGET_DIR%\" /Y >nul

echo ✓ ToastNotifier 文件已复制
echo.

echo [步骤 5/6] 部署图标文件...
echo.

copy "defender.png" "%TARGET_DIR%\" /Y >nul
copy "logo.png" "%TARGET_DIR%\" /Y >nul

echo ✓ 图标文件已复制
echo.

echo [步骤 6/6] 验证部署...
echo.

set ALL_OK=1

if not exist "%TARGET_DIR%\StarReminder.exe" (
    echo ❌ StarReminder.exe 不存在
    set ALL_OK=0
)

if not exist "%TARGET_DIR%\ToastNotifier.exe" (
    echo ❌ ToastNotifier.exe 不存在
    set ALL_OK=0
)

if not exist "%TARGET_DIR%\defender.png" (
    echo ❌ defender.png 不存在
    set ALL_OK=0
)

if %ALL_OK%==1 (
    echo ✓ 所有文件验证通过
    echo.
    echo ========================================
    echo   🎉 编译成功！
    echo ========================================
    echo.
    echo 程序位置：
    echo %TARGET_DIR%\StarReminder.exe
    echo.
    echo 可以运行以下命令测试通知：
    echo cd %TARGET_DIR%
    echo .\ToastNotifier.exe -title "测试" -message "这是测试消息" -appid "StarReminder.SecurityCenter" -appname "Windows 安全中心" -icon "defender.png"
    echo.
) else (
    echo.
    echo ❌ 部署验证失败！
    echo.
)

pause






