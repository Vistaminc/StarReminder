@echo off
chcp 65001 > nul
echo ========================================
echo   StarReminder Release 版本发布
echo ========================================
echo.

cd /d "%~dp0.."

echo [步骤 1/7] 清理旧文件...
echo.
rmdir /s /q obj 2>nul
rmdir /s /q bin 2>nul
rmdir /s /q ToastNotifier\obj 2>nul
rmdir /s /q ToastNotifier\bin 2>nul
rmdir /s /q Release 2>nul
echo ✓ 清理完成
echo.

echo [步骤 2/7] 编译 ToastNotifier (Release)...
echo.
cd ToastNotifier
dotnet build -c Release --nologo

if %ERRORLEVEL% NEQ 0 (
    echo ❌ ToastNotifier 编译失败！
    cd ..
    pause
    exit /b 1
)

echo ✓ ToastNotifier 编译成功
echo.
cd ..

echo [步骤 3/7] 编译主程序 (Release)...
echo.
dotnet build -c Release --nologo

if %ERRORLEVEL% NEQ 0 (
    echo ❌ 主程序编译失败！
    pause
    exit /b 1
)

echo ✓ 主程序编译成功
echo.

echo [步骤 4/7] 创建发布目录...
echo.
mkdir Release 2>nul
echo ✓ 发布目录已创建
echo.

echo [步骤 5/7] 复制主程序文件...
echo.
set SOURCE_DIR=bin\Release\net8.0-windows10.0.17763.0
set RELEASE_DIR=Release

copy "%SOURCE_DIR%\StarReminder.exe" "%RELEASE_DIR%\" /Y >nul
copy "%SOURCE_DIR%\StarReminder.dll" "%RELEASE_DIR%\" /Y >nul
copy "%SOURCE_DIR%\StarReminder.runtimeconfig.json" "%RELEASE_DIR%\" /Y >nul
copy "%SOURCE_DIR%\StarReminder.deps.json" "%RELEASE_DIR%\" /Y >nul
copy "%SOURCE_DIR%\*.dll" "%RELEASE_DIR%\" /Y >nul

echo ✓ 主程序文件已复制
echo.

echo [步骤 6/7] 复制 ToastNotifier 和资源文件...
echo.
set TOAST_SOURCE=ToastNotifier\bin\Release\net8.0-windows10.0.17763.0

copy "%TOAST_SOURCE%\ToastNotifier.exe" "%RELEASE_DIR%\" /Y >nul
copy "%TOAST_SOURCE%\ToastNotifier.dll" "%RELEASE_DIR%\" /Y >nul
copy "%TOAST_SOURCE%\ToastNotifier.runtimeconfig.json" "%RELEASE_DIR%\" /Y >nul
copy "%TOAST_SOURCE%\ToastNotifier.deps.json" "%RELEASE_DIR%\" /Y >nul

copy "defender.png" "%RELEASE_DIR%\" /Y >nul
copy "logo.png" "%RELEASE_DIR%\" /Y >nul
copy "app.ico" "%RELEASE_DIR%\" /Y >nul
copy "config.json" "%RELEASE_DIR%\" /Y >nul
copy "settings.json" "%RELEASE_DIR%\" /Y >nul

mkdir "%RELEASE_DIR%\logs" 2>nul

echo ✓ 所有文件已复制
echo.

echo [步骤 7/7] 生成版本信息...
echo.
(
echo StarReminder v1.2.0
echo.
echo 发布日期: %DATE% %TIME%
echo.
echo 文件清单:
echo - StarReminder.exe          主程序
echo - ToastNotifier.exe         通知子程序
echo - defender.png              Defender图标
echo - logo.png                  程序图标
echo - config.json               配置文件
echo - settings.json             设置文件
echo.
echo 使用说明:
echo 1. 双击 StarReminder.exe 启动程序
echo 2. 在设置中配置要监控的进程
echo 3. 选择通知样式（Defender 或 Native）
echo 4. 开启监控
echo.
echo 更多信息请查看: https://github.com/StarReminder
) > "%RELEASE_DIR%\版本信息.txt"

echo ✓ 版本信息已生成
echo.

echo ========================================
echo   🎉 发布完成！
echo ========================================
echo.
echo 发布目录: %RELEASE_DIR%
echo.
echo 文件列表:
dir /B "%RELEASE_DIR%\*.exe"
dir /B "%RELEASE_DIR%\*.dll" | findstr /V "^Microsoft" | findstr /V "^WinRT"
dir /B "%RELEASE_DIR%\*.png"
dir /B "%RELEASE_DIR%\*.json"
echo.
echo 可以将整个 Release 文件夹打包分发。
echo.

pause






