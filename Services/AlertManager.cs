using System;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using MediaDetectionSystem.Models;
using Application = System.Windows.Application;
using Window = System.Windows.Window;
using WindowStyle = System.Windows.WindowStyle;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using SystemParameters = System.Windows.SystemParameters;
using ResizeMode = System.Windows.ResizeMode;
using Thickness = System.Windows.Thickness;
using Color = System.Windows.Media.Color;
using Microsoft.Toolkit.Uwp.Notifications;

namespace MediaDetectionSystem.Services
{
    public class AlertManager
    {
        private readonly AlertConfig _config;
        private Window? _watermarkWindow;

        public AlertManager(AlertConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// 显示进程启动时的提醒
        /// </summary>
        public void ShowProcessStartAlert(string processName, string displayName, MediaDeviceUsage? mediaUsage = null)
        {
            if (!_config.EnableStartupNotification)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    switch (_config.NotificationType)
                    {
                        case "Defender":
                            ShowDefenderStyleNotification(processName, displayName, mediaUsage);
                            break;
                        case "Native":
                            ShowNativeToastNotification(processName, displayName, mediaUsage);
                            break;
                        default:
                            ShowDefenderStyleNotification(processName, displayName, mediaUsage);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"显示启动提醒失败: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 显示持续提醒 - Windows激活水印样式
        /// </summary>
        public void ShowContinuousAlert()
        {
            if (!_config.EnableContinuousAlert)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    ShowWatermark();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"显示持续提醒失败: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 隐藏持续提醒
        /// </summary>
        public void HideContinuousAlert()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    HideWatermark();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"隐藏持续提醒失败: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 替换消息模板中的变量
        /// </summary>
        private string ReplaceMessageVariables(string template, string processName, string displayName, MediaDeviceUsage? mediaUsage)
        {
            string result = template;
            
            // 基本变量替换
            result = result.Replace("{ProcessName}", processName);
            result = result.Replace("{DisplayName}", displayName);
            
            // 媒体设备变量替换
            if (mediaUsage != null)
            {
                var devices = new List<string>();
                if (mediaUsage.IsCameraInUse) devices.Add("📷 摄像头");
                if (mediaUsage.IsMicrophoneInUse) devices.Add("🎤 麦克风");
                
                string cameraStatus = mediaUsage.IsCameraInUse ? "📷 摄像头" : "";
                string microphoneStatus = mediaUsage.IsMicrophoneInUse ? "🎤 麦克风" : "";
                string devicesText = devices.Count > 0 ? string.Join(" 和 ", devices) : "";
                
                result = result.Replace("{Camera}", cameraStatus);
                result = result.Replace("{Microphone}", microphoneStatus);
                result = result.Replace("{Devices}", devicesText);
            }
            else
            {
                result = result.Replace("{Camera}", "");
                result = result.Replace("{Microphone}", "");
                result = result.Replace("{Devices}", "");
            }
            
            return result;
        }

        /// <summary>
        /// 显示Windows Defender样式通知
        /// </summary>
        private void ShowDefenderStyleNotification(string processName, string displayName, MediaDeviceUsage? mediaUsage = null)
        {
            try
            {
                // 使用自定义标题和消息
                string title = ReplaceMessageVariables(_config.NotificationTitle, processName, displayName, mediaUsage);
                string message = ReplaceMessageVariables(_config.NotificationMessage, processName, displayName, mediaUsage);
                
                // 调用 ToastNotifier.exe 显示 Windows 安全中心样式的 Toast 通知
                var toastNotifierPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ToastNotifier.exe");
                var defenderIconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "defender.png");
                
                if (!System.IO.File.Exists(toastNotifierPath))
                {
                    System.Diagnostics.Debug.WriteLine($"ToastNotifier.exe 不存在: {toastNotifierPath}");
                    return;
                }
                
                // 构建命令行参数
                var arguments = $"-title \"{title}\" " +
                               $"-message \"{message}\" " +
                               $"-appid \"StarReminder.SecurityCenter\" " +
                               $"-appname \"Windows 安全中心\"";
                
                // 如果 defender.png 存在，添加图标参数
                if (System.IO.File.Exists(defenderIconPath))
                {
                    arguments += $" -icon \"{defenderIconPath}\"";
                }
                
                // 启动 ToastNotifier 子程序
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = toastNotifierPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };
                
                System.Diagnostics.Process.Start(startInfo);
                System.Diagnostics.Debug.WriteLine($"[AlertManager] 已调用 ToastNotifier 显示 Defender 样式通知");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"显示Defender样式通知失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示Windows原生Toast通知
        /// </summary>
        private void ShowNativeToastNotification(string processName, string displayName, MediaDeviceUsage? mediaUsage = null)
        {
            try
            {
                // 使用自定义标题和消息
                string title = ReplaceMessageVariables(_config.NotificationTitle, processName, displayName, mediaUsage);
                string message = ReplaceMessageVariables(_config.NotificationMessage, processName, displayName, mediaUsage);
                
                // 复刻Windows Defender威胁中心样式的Toast通知
                new ToastContentBuilder()
                    .AddArgument("action", "processDetected")
                    .AddArgument("processName", processName)
                    .AddText(title)
                    .AddText(message)
                    .Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"显示原生Toast通知失败: {ex.Message}");
                
                // 降级方案：使用MessageBox
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show(
                            $"检测到进程启动\n\n{displayName}",
                            "StarReminder",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information);
                    });
                }
                catch
                {
                    // 静默失败
                }
            }
        }

        /// <summary>
        /// 显示Windows激活水印
        /// </summary>
        private void ShowWatermark()
        {
            if (_watermarkWindow != null)
                return;

            // 获取工作区域（不包括任务栏的区域）
            var workArea = SystemParameters.WorkArea;

            _watermarkWindow = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = System.Windows.Media.Brushes.Transparent,
                Width = workArea.Width,
                Height = workArea.Height,
                Left = workArea.Left,
                Top = workArea.Top,
                Topmost = true, // 置顶显示
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                IsHitTestVisible = false // 点击穿透
            };

            // 创建文字面板
            var textPanel = new System.Windows.Controls.StackPanel
            {
                Background = System.Windows.Media.Brushes.Transparent
            };

            // 第一行文字 - 与Windows激活水印完全一致
            var text1 = new System.Windows.Controls.TextBlock
            {
                Text = _config.WatermarkText1,
                FontSize = 18,
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"), // Windows默认字体
                Foreground = new SolidColorBrush(Color.FromArgb(102, 128, 128, 128)), // 灰色，60%透明度（40%不透明度）
                Margin = new Thickness(0, 0, 0, 2)
            };
            textPanel.Children.Add(text1);

            // 第二行文字 - 与Windows激活水印完全一致
            var text2 = new System.Windows.Controls.TextBlock
            {
                Text = _config.WatermarkText2,
                FontSize = 14,
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"), // Windows默认字体
                Foreground = new SolidColorBrush(Color.FromArgb(102, 128, 128, 128)), // 灰色，60%透明度（40%不透明度）
            };
            textPanel.Children.Add(text2);

            // 根据位置设置对齐和边距
            // 使用工作区域边界，确保水印在任务栏上方
            switch (_config.WatermarkPosition)
            {
                case "BottomRight"://右下角
                    textPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    textPanel.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                    textPanel.Margin = new Thickness(0, 0, 65, 45);//左，上，右，下
                    break;
                case "BottomLeft"://左下角
                    textPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    textPanel.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                    textPanel.Margin = new Thickness(20, 0, 0, 45);
                    break;
                case "TopRight"://右上角
                    textPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    textPanel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    textPanel.Margin = new Thickness(0, 20, 20, 0);
                    break;
                case "TopLeft"://左上角
                    textPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    textPanel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    textPanel.Margin = new Thickness(20, 20, 0, 0);
                    break;
            }

            _watermarkWindow.Content = textPanel;
            _watermarkWindow.Show();
        }

        /// <summary>
        /// 隐藏水印
        /// </summary>
        private void HideWatermark()
        {
            if (_watermarkWindow != null)
            {
                _watermarkWindow.Close();
                _watermarkWindow = null;
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            HideContinuousAlert();
        }
    }
}
