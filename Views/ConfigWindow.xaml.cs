using System;
using System.Windows;
using System.Windows.Threading;
using MediaDetectionSystem.Models;

namespace MediaDetectionSystem.Views
{
    /// <summary>
    /// ConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private readonly string _processName;
        private readonly ProcessConfig _config;
        private readonly AlertConfig _alertConfig;
        private readonly Action<string, ProcessConfig> _onSave;
        private readonly Action<AlertConfig>? _onAlertConfigSave;

        public ConfigWindow(string processName, ProcessConfig config, Action<string, ProcessConfig> onSave, 
                           AlertConfig? globalAlertConfig = null, Action<AlertConfig>? onAlertConfigSave = null)
        {
            InitializeComponent();
            
            _processName = processName;
            _config = config;
            
            // 优先使用进程自己的AlertConfig，如果没有则使用全局配置或创建新的
            _alertConfig = config.AlertConfig ?? globalAlertConfig ?? new AlertConfig();
            
            // 如果进程还没有独立的AlertConfig，创建一个
            if (config.AlertConfig == null)
            {
                config.AlertConfig = _alertConfig;
            }
            
            _onSave = onSave;
            _onAlertConfigSave = onAlertConfigSave;

            LoadConfig();
            LoadAlertConfig();
            UpdateTitle();
            SetupEventHandlers();
        }
        
        /// <summary>
        /// 设置事件处理
        /// </summary>
        private void SetupEventHandlers()
        {
            // 新的提醒方式不需要额外的事件处理
        }

        private void UpdateTitle()
        {
            var displayNames = new System.Collections.Generic.Dictionary<string, string>
            {
                ["media_capture.exe"] = "摄像头/麦克风 配置",
                ["screenCapture.exe"] = "屏幕捕获 配置",
                ["rtcRemoteDesktop.exe"] = "远程控制 配置"
            };
            
            // SVG图标路径数据
            var iconPaths = new System.Collections.Generic.Dictionary<string, string>
            {
                // 摄像头图标
                ["media_capture.exe"] = "M17 10.5V7c0-.55-.45-1-1-1H4c-.55 0-1 .45-1 1v10c0 .55.45 1 1 1h12c.55 0 1-.45 1-1v-3.5l4 4v-11l-4 4z",
                // 屏幕捕获图标  
                ["screenCapture.exe"] = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V5h14v14zm-5.04-6.71l-2.75 3.54-1.96-2.36L6.5 17h11l-3.54-4.71z",
                // 远程控制图标（电脑图标）
                ["rtcRemoteDesktop.exe"] = "M4 6h16v2H4zm0 5h16v6H4zm2 2h12v2H6z"
            };

            if (displayNames.ContainsKey(_processName))
            {
                TitleProcessName.Text = displayNames[_processName];
            }
            else
            {
                TitleProcessName.Text = _processName + " 配置";
            }
            
            // 设置对应的SVG图标
            if (iconPaths.ContainsKey(_processName))
            {
                ProcessIconPath.Data = System.Windows.Media.Geometry.Parse(iconPaths[_processName]);
            }
        }

        private void LoadConfig()
        {
            EnabledCheckBox.IsChecked = _config.IsEnabled;
            MaxRunTimeTextBox.Text = (_config.MaxRuntime.TotalMinutes).ToString();

            switch (_config.ActionType)
            {
                case "None":
                    ActionNoneRadio.IsChecked = true;
                    break;
                case "Suspend":
                    ActionSuspendRadio.IsChecked = true;
                    break;
                case "Terminate":
                    ActionTerminateRadio.IsChecked = true;
                    break;
            }
            
            AlertTimeTextBox.Text = _config.AlertBeforeAction.ToString();
            AutoResumeCheckBox.IsChecked = _config.AutoResumeAfter.TotalSeconds > 0;
            SuspendDurationTextBox.Text = _config.AutoResumeAfter.TotalSeconds.ToString();
        }

        private void LoadAlertConfig()
        {
            // 加载启动时提醒配置
            ChkEnableStartupNotification.IsChecked = _alertConfig.EnableStartupNotification;
            PanelNotificationSettings.IsEnabled = _alertConfig.EnableStartupNotification;
            
            // 加载通知样式
            switch (_alertConfig.NotificationType)
            {
                case "Defender":
                    RadioNotificationDefender.IsChecked = true;
                    break;
                case "Native":
                    RadioNotificationNative.IsChecked = true;
                    break;
            }
            
            // 加载自定义通知消息
            TxtNotificationTitle.Text = _alertConfig.NotificationTitle;
            TxtNotificationMessage.Text = _alertConfig.NotificationMessage.Replace("\\n", "\n");
            
            // 加载持续提醒配置
            ChkEnableContinuous.IsChecked = _alertConfig.EnableContinuousAlert;
            PanelContinuousSettings.IsEnabled = _alertConfig.EnableContinuousAlert;
            
            // 水印文字
            TxtWatermarkText1.Text = _alertConfig.WatermarkText1;
            TxtWatermarkText2.Text = _alertConfig.WatermarkText2;
            
            // 水印位置
            switch (_alertConfig.WatermarkPosition)
            {
                case "TopLeft":
                    RadioPosTopLeft.IsChecked = true;
                    break;
                case "TopRight":
                    RadioPosTopRight.IsChecked = true;
                    break;
                case "BottomLeft":
                    RadioPosBottomLeft.IsChecked = true;
                    break;
                case "BottomRight":
                    RadioPosBottomRight.IsChecked = true;
                    break;
            }
            
            // 持续提醒直到
            switch (_alertConfig.ContinuousAlertUntil)
            {
                case "ProcessEnd":
                    RadioUntilEnd.IsChecked = true;
                    break;
                case "ProcessSuspend":
                    RadioUntilSuspend.IsChecked = true;
                    break;
            }
        }
        
        private void ChkEnableStartupNotification_Changed(object sender, RoutedEventArgs e)
        {
            PanelNotificationSettings.IsEnabled = ChkEnableStartupNotification.IsChecked ?? false;
        }
        
        private void ChkEnableContinuous_Changed(object sender, RoutedEventArgs e)
        {
            PanelContinuousSettings.IsEnabled = ChkEnableContinuous.IsChecked ?? false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 保存进程配置
                _config.IsEnabled = EnabledCheckBox.IsChecked ?? true;

                if (int.TryParse(MaxRunTimeTextBox.Text, out int maxRunTimeMinutes))
                {
                    _config.MaxRuntime = TimeSpan.FromMinutes(maxRunTimeMinutes);
                }

                if (ActionNoneRadio.IsChecked == true)
                    _config.ActionType = "None";
                else if (ActionSuspendRadio.IsChecked == true)
                    _config.ActionType = "Suspend";
                else if (ActionTerminateRadio.IsChecked == true)
                    _config.ActionType = "Terminate";
                
                if (int.TryParse(AlertTimeTextBox.Text, out int alertSeconds))
                {
                    _config.AlertBeforeAction = alertSeconds;
                }
                
                if (int.TryParse(SuspendDurationTextBox.Text, out int suspendSeconds))
                {
                    _config.AutoResumeAfter = (AutoResumeCheckBox.IsChecked == true) ? TimeSpan.FromSeconds(suspendSeconds) : TimeSpan.Zero;
                }

                // 保存提醒配置
                _alertConfig.EnableStartupNotification = ChkEnableStartupNotification.IsChecked ?? true;
                
                // 保存通知样式
                if (RadioNotificationDefender.IsChecked == true)
                    _alertConfig.NotificationType = "Defender";
                else if (RadioNotificationNative.IsChecked == true)
                    _alertConfig.NotificationType = "Native";
                
                // 保存自定义通知消息
                _alertConfig.NotificationTitle = TxtNotificationTitle.Text;
                _alertConfig.NotificationMessage = TxtNotificationMessage.Text.Replace("\r\n", "\\n").Replace("\n", "\\n");
                
                _alertConfig.EnableContinuousAlert = ChkEnableContinuous.IsChecked ?? true;
                
                // 保存水印文字
                _alertConfig.WatermarkText1 = TxtWatermarkText1.Text;
                _alertConfig.WatermarkText2 = TxtWatermarkText2.Text;
                
                // 保存水印位置
                if (RadioPosTopLeft.IsChecked == true)
                    _alertConfig.WatermarkPosition = "TopLeft";
                else if (RadioPosTopRight.IsChecked == true)
                    _alertConfig.WatermarkPosition = "TopRight";
                else if (RadioPosBottomLeft.IsChecked == true)
                    _alertConfig.WatermarkPosition = "BottomLeft";
                else if (RadioPosBottomRight.IsChecked == true)
                    _alertConfig.WatermarkPosition = "BottomRight";
                
                if (RadioUntilEnd.IsChecked == true)
                    _alertConfig.ContinuousAlertUntil = "ProcessEnd";
                else if (RadioUntilSuspend.IsChecked == true)
                    _alertConfig.ContinuousAlertUntil = "ProcessSuspend";

                // 将AlertConfig保存到进程配置中
                _config.AlertConfig = _alertConfig;
                
                _onSave?.Invoke(_processName, _config);
                
                // 不再需要单独的AlertConfig保存回调，因为已经保存在ProcessConfig中
                // _onAlertConfigSave?.Invoke(_alertConfig);

                // 显示保存成功通知
                ShowSaveSuccessNotification();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"保存配置失败: {ex.Message}", "保存失败", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// 显示保存成功通知（右上角，1秒后自动消失）
        /// </summary>
        private void ShowSaveSuccessNotification()
        {
            try
            {
                // 显示通知
                SaveSuccessNotification.Visibility = Visibility.Visible;

                // 淡入动画
                var fadeInAnimation = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new System.Windows.Media.Animation.QuadraticEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
                };

                SaveSuccessNotification.BeginAnimation(OpacityProperty, fadeInAnimation);

                // 1秒后淡出
                var hideTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                hideTimer.Tick += (s, args) =>
                {
                    hideTimer.Stop();

                    // 淡出动画
                    var fadeOutAnimation = new System.Windows.Media.Animation.DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(300),
                        EasingFunction = new System.Windows.Media.Animation.QuadraticEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseIn }
                    };

                    fadeOutAnimation.Completed += (sender, e) =>
                    {
                        SaveSuccessNotification.Visibility = Visibility.Collapsed;
                    };

                    SaveSuccessNotification.BeginAnimation(OpacityProperty, fadeOutAnimation);
                };
                hideTimer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"显示保存通知失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 预览提醒效果
        /// </summary>
        private void PreviewAlert_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                // 临时创建AlertConfig用于预览
                var previewConfig = new AlertConfig
                {
                    EnableStartupNotification = ChkEnableStartupNotification.IsChecked ?? true,
                    NotificationType = RadioNotificationDefender.IsChecked == true ? "Defender" : "Native",
                    NotificationTitle = TxtNotificationTitle.Text,
                    NotificationMessage = TxtNotificationMessage.Text.Replace("\r\n", "\\n").Replace("\n", "\\n"),
                    EnableContinuousAlert = true,
                    WatermarkText1 = TxtWatermarkText1.Text,
                    WatermarkText2 = TxtWatermarkText2.Text,
                    WatermarkPosition = RadioPosTopLeft.IsChecked == true ? "TopLeft" :
                                      RadioPosTopRight.IsChecked == true ? "TopRight" :
                                      RadioPosBottomLeft.IsChecked == true ? "BottomLeft" : "BottomRight"
                };
                
                var previewManager = new Services.AlertManager(previewConfig);
                
                // 显示通知
                if (ChkEnableStartupNotification.IsChecked == true)
                {
                    previewManager.ShowProcessStartAlert(_config.ProcessName, _config.DisplayName);
                }
                
                // 显示水印（3秒后自动关闭）
                if (ChkEnableContinuous.IsChecked == true)
                {
                    previewManager.ShowContinuousAlert();
                    
                    var timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(5)
                    };
                    timer.Tick += (s, args) =>
                    {
                        timer.Stop();
                        previewManager.HideContinuousAlert();
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"预览失败：{ex.Message}",
                    "错误",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            try
            {
                DragMove();
            }
            catch { }
        }
    }
}
