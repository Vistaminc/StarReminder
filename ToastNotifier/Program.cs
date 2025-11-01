using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.IO;
using System.Linq;
using Windows.UI.Notifications;

namespace ToastNotifier
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("[ToastNotifier] 启动子程序");
                Console.WriteLine($"[ToastNotifier] 参数数量: {args.Length}");
                
                // 解析命令行参数
                var parsedArgs = ParseArguments(args);
                
                if (!parsedArgs.ContainsKey("title") || !parsedArgs.ContainsKey("message"))
                {
                    Console.WriteLine("[ToastNotifier] ❌ 缺少必需参数: title 和 message");
                    Console.WriteLine("用法: ToastNotifier.exe -title \"标题\" -message \"消息\" [-appid \"应用ID\"] [-appname \"应用名称\"] [-icon \"图标路径\"]");
                    return 1;
                }
                
                string title = parsedArgs["title"];
                string message = parsedArgs["message"];
                string appId = parsedArgs.GetValueOrDefault("appid", "StarReminder.ProcessMonitor");
                string appName = parsedArgs.GetValueOrDefault("appname", "Windows 安全中心");
                string iconPath = parsedArgs.GetValueOrDefault("icon", "");
                
                Console.WriteLine($"[ToastNotifier] 标题: {title}");
                Console.WriteLine($"[ToastNotifier] 消息: {message}");
                Console.WriteLine($"[ToastNotifier] AppID: {appId}");
                Console.WriteLine($"[ToastNotifier] 应用名称: {appName}");
                Console.WriteLine($"[ToastNotifier] 图标路径: {iconPath}");
                
                // 发送Toast通知
                bool success = SendToastNotification(title, message, appId, appName, iconPath);
                
                if (success)
                {
                    Console.WriteLine("[ToastNotifier] ✓ 通知发送成功");
                    return 0;
                }
                else
                {
                    Console.WriteLine("[ToastNotifier] ❌ 通知发送失败");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ToastNotifier] ❌ 异常: {ex.Message}");
                Console.WriteLine($"[ToastNotifier] 堆栈: {ex.StackTrace}");
                return 1;
            }
        }
        
        static Dictionary<string, string> ParseArguments(string[] args)
        {
            var result = new Dictionary<string, string>();
            
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-") && i + 1 < args.Length)
                {
                    string key = args[i].TrimStart('-').ToLower();
                    string value = args[i + 1].Trim('"');
                    result[key] = value;
                    i++; // 跳过值
                }
            }
            
            return result;
        }
        
        static bool SendToastNotification(string title, string message, string appId, string appName, string iconPath)
        {
            try
            {
                // 注册应用程序到通知系统
                RegisterApp(appId, appName);
                
                // 构建Toast内容
                var builder = new ToastContentBuilder();
                
                // 设置应用信息
                builder.AddArgument("action", "viewNotification");
                
                // 添加应用标识文本（显示在通知顶部的小字）
                builder.AddAttributionText(appName);
                
                // 添加标题
                builder.AddText(title);
                
                // 添加消息内容
                builder.AddText(message);
                
                // 如果提供了图标，添加应用logo
                if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                {
                    Console.WriteLine($"[ToastNotifier] ✓ 使用图标: {iconPath}");
                    
                    // 转换为绝对路径
                    string absoluteIconPath = Path.GetFullPath(iconPath);
                    
                    // 添加应用logo（显示在通知右侧）
                    builder.AddAppLogoOverride(new Uri($"file:///{absoluteIconPath.Replace("\\", "/")}"), ToastGenericAppLogoCrop.Default);
                }
                else
                {
                    Console.WriteLine($"[ToastNotifier] ⚠ 图标文件不存在或未指定: {iconPath}");
                }
                
                // 发送通知
                var toastContent = builder.GetToastContent();
                var toastNotification = new ToastNotification(toastContent.GetXml());
                
                // 使用指定的AppId发送
                var toastNotifier = ToastNotificationManager.CreateToastNotifier(appId);
                toastNotifier.Show(toastNotification);
                
                Console.WriteLine("[ToastNotifier] ✓ Toast通知已发送");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ToastNotifier] ❌ 发送Toast通知失败: {ex.Message}");
                Console.WriteLine($"[ToastNotifier] 堆栈: {ex.StackTrace}");
                return false;
            }
        }
        
        static void RegisterApp(string appId, string appName)
        {
            try
            {
                // 在Windows注册表中注册应用程序
                // 这样可以自定义应用名称在通知中心显示
                string regPath = $@"SOFTWARE\Classes\AppUserModelId\{appId}";
                
                using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(regPath))
                {
                    if (key != null)
                    {
                        key.SetValue("DisplayName", appName);
                        Console.WriteLine($"[ToastNotifier] ✓ 已注册应用: {appName} (ID: {appId})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ToastNotifier] ⚠ 注册应用失败 (非致命错误): {ex.Message}");
            }
        }
    }
}






