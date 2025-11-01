@echo off
chcp 65001 > nul
echo ========================================
echo   测试 StarReminder 通知功能
echo ========================================
echo.

cd /d "%~dp0..\bin\Debug\net8.0-windows10.0.17763.0"

if not exist "ToastNotifier.exe" (
    echo ❌ ToastNotifier.exe 不存在！
    echo.
    echo 请先运行编译脚本：
    echo    scripts\一键编译.bat
    echo.
    pause
    exit /b 1
)

if not exist "defender.png" (
    echo ❌ defender.png 不存在！
    echo.
    echo 请先运行编译脚本：
    echo    scripts\一键编译.bat
    echo.
    pause
    exit /b 1
)

echo 当前目录: %CD%
echo.

echo [测试 1/3] Defender样式通知（完整版）
echo.
.\ToastNotifier.exe -title "Windows Defender 扫描结果" -message "Microsoft Defender 防病毒已于 %TIME% 检测到 测试程序 启动。 正在使用：摄像头、麦克风。" -appid "StarReminder.SecurityCenter" -appname "Windows 安全中心" -icon "defender.png"

echo.
echo 请检查是否弹出通知...
timeout /t 5 /nobreak >nul
echo.

echo [测试 2/3] Defender样式通知（简洁版）
echo.
.\ToastNotifier.exe -title "进程监控提醒" -message "检测到希沃录屏启动" -appid "StarReminder.SecurityCenter" -appname "Windows 安全中心" -icon "defender.png"

echo.
echo 请检查是否弹出通知...
timeout /t 5 /nobreak >nul
echo.

echo [测试 3/3] 无图标通知
echo.
.\ToastNotifier.exe -title "测试通知" -message "这是一个测试消息，不带图标" -appid "StarReminder.SecurityCenter" -appname "Windows 安全中心"

echo.
echo.
echo ========================================
echo   测试完成！
echo ========================================
echo.
echo 如果看到了3个通知，说明功能正常。
echo.
echo 通知应该显示为：
echo - 应用名称: Windows 安全中心
echo - 图标: Defender 盾牌（前两个测试）
echo - 出现在通知中心
echo.

pause






