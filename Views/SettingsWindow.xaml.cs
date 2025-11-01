using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using MediaDetectionSystem.Models;
using MediaDetectionSystem.Services;
using Microsoft.Win32;

namespace MediaDetectionSystem.Views
{
    public partial class SettingsWindow : Window
    {
        private AppSettings _settings;
        private readonly Action<AppSettings> _onSave;
        private const string APP_NAME = "StarReminder";
        private const string REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private readonly UpdateChecker _updateChecker;
        private UpdateInfo? _latestUpdateInfo;

        public SettingsWindow(AppSettings settings, Action<AppSettings> onSave)
        {
            InitializeComponent();
            _settings = settings ?? new AppSettings();
            _onSave = onSave;
            _updateChecker = new UpdateChecker();
            
            LoadSettings();
            
            // 设置版本号（自动从程序集读取）
            TxtVersionDisplay.Text = $"版本 {VersionInfo.Version}";
            
            // 如果开启了自动检查更新，在关于页面打开时检查
            if (_settings.CheckForUpdates)
            {
                // 当切换到关于页面时自动检查
                NavAbout.Checked += (s, e) => _ = CheckForUpdatesAsync();
            }
        }

        /// <summary>
        /// 加载设置到UI
        /// </summary>
        private void LoadSettings()
        {
            // 启动设置
            ChkStartWithWindows.IsChecked = _settings.StartWithWindows;
            ChkStartMinimized.IsChecked = _settings.StartMinimized;

            // 界面设置
            switch (_settings.ThemeMode)
            {
                case "Light": RadioThemeLight.IsChecked = true; break;
                case "Dark": RadioThemeDark.IsChecked = true; break;
                case "Auto": RadioThemeAuto.IsChecked = true; break;
            }

            // 通知设置
            ChkEnableTrayNotifications.IsChecked = _settings.EnableTrayNotifications;
            ChkShowStartupDialog.IsChecked = _settings.ShowStartupDialog;

            // 监控设置
            SliderMonitorInterval.Value = _settings.MonitorInterval;
            ChkEnableAutoAction.IsChecked = _settings.EnableAutoAction;
            ChkEnableDetailedLogging.IsChecked = _settings.EnableDetailedLogging;

            // 高级设置
            SliderLogRetentionDays.Value = _settings.LogRetentionDays;
            ChkAutoCleanLogs.IsChecked = _settings.AutoCleanLogs;
            ChkCheckForUpdates.IsChecked = _settings.CheckForUpdates;
            ChkEnableAnalytics.IsChecked = _settings.EnableAnalytics;

            // 安全设置
            ChkEnablePasswordProtection.IsChecked = _settings.EnablePasswordProtection;
        }

        /// <summary>
        /// 从UI保存设置
        /// </summary>
        private void SaveSettingsFromUI()
        {
            // 启动设置
            _settings.StartWithWindows = ChkStartWithWindows.IsChecked ?? false;
            _settings.StartMinimized = ChkStartMinimized.IsChecked ?? false;

            // 界面设置
            if (RadioThemeLight.IsChecked == true) _settings.ThemeMode = "Light";
            else if (RadioThemeDark.IsChecked == true) _settings.ThemeMode = "Dark";
            else if (RadioThemeAuto.IsChecked == true) _settings.ThemeMode = "Auto";

            // 通知设置
            _settings.EnableTrayNotifications = ChkEnableTrayNotifications.IsChecked ?? true;
            _settings.ShowStartupDialog = ChkShowStartupDialog.IsChecked ?? true;

            // 监控设置
            _settings.MonitorInterval = (int)SliderMonitorInterval.Value;
            _settings.EnableAutoAction = ChkEnableAutoAction.IsChecked ?? true;
            _settings.EnableDetailedLogging = ChkEnableDetailedLogging.IsChecked ?? true;

            // 高级设置
            _settings.LogRetentionDays = (int)SliderLogRetentionDays.Value;
            _settings.AutoCleanLogs = ChkAutoCleanLogs.IsChecked ?? true;
            _settings.CheckForUpdates = ChkCheckForUpdates.IsChecked ?? true;
            _settings.EnableAnalytics = ChkEnableAnalytics.IsChecked ?? false;

            // 安全设置
            _settings.EnablePasswordProtection = ChkEnablePasswordProtection.IsChecked ?? false;
        }

        /// <summary>
        /// 导航按钮点击事件
        /// </summary>
        private void NavButton_Checked(object sender, RoutedEventArgs e)
        {
            // 确保控件已初始化
            if (PanelStartup == null || PanelAppearance == null || 
                PanelNotification == null || PanelMonitor == null || 
                PanelAdvanced == null || PanelSecurity == null || PanelAbout == null)
            {
                return;
            }

            // 隐藏所有面板
            PanelStartup.Visibility = Visibility.Collapsed;
            PanelAppearance.Visibility = Visibility.Collapsed;
            PanelNotification.Visibility = Visibility.Collapsed;
            PanelMonitor.Visibility = Visibility.Collapsed;
            PanelAdvanced.Visibility = Visibility.Collapsed;
            PanelSecurity.Visibility = Visibility.Collapsed;
            PanelAbout.Visibility = Visibility.Collapsed;

            // 显示对应面板
            if (sender == NavStartup)
                PanelStartup.Visibility = Visibility.Visible;
            else if (sender == NavAppearance)
                PanelAppearance.Visibility = Visibility.Visible;
            else if (sender == NavNotification)
                PanelNotification.Visibility = Visibility.Visible;
            else if (sender == NavMonitor)
                PanelMonitor.Visibility = Visibility.Visible;
            else if (sender == NavAdvanced)
                PanelAdvanced.Visibility = Visibility.Visible;
            else if (sender == NavSecurity)
                PanelSecurity.Visibility = Visibility.Visible;
            else if (sender == NavAbout)
                PanelAbout.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 保存按钮点击事件
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var oldSettings = new AppSettings
                {
                    StartWithWindows = _settings.StartWithWindows,
                    StartMinimized = _settings.StartMinimized
                };

                SaveSettingsFromUI();

                // 处理开机自启动
                SetStartWithWindows(_settings.StartWithWindows);

                // 调用回调保存设置
                _onSave?.Invoke(_settings);

                // 检查是否有需要重启的设置被修改
                bool needRestart = oldSettings.StartMinimized != _settings.StartMinimized;

                // 显示保存成功通知
                ShowSaveSuccessNotification(needRestart);
                
                // 如果需要重启，提示用户
                if (needRestart)
                {
                    // 延迟1.5秒后显示重启提示（给用户时间看到保存成功通知）
                    var restartTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(1.5)
                    };
                    restartTimer.Tick += (s, args) =>
                    {
                        restartTimer.Stop();
                        
                        var dialog = new RestartConfirmDialog
                        {
                            Owner = this
                        };
                        
                        var result = dialog.ShowDialog();
                        
                        if (result == true)
                        {
                            // 重启应用
                            RestartApplication();
                        }
                    };
                    restartTimer.Start();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"保存设置时出错：\n{ex.Message}",
                    "保存失败",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// 重置设置按钮点击事件
        /// </summary>
        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show(
                "确定要重置所有设置到默认值吗？\n\n此操作不可撤销！",
                "⚠️ 确认重置",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _settings = new AppSettings();
                LoadSettings();
                
                System.Windows.MessageBox.Show(
                    "所有设置已重置为默认值。\n\n点击【保存设置】按钮以应用更改。",
                    "重置成功",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 显示保存成功通知（右上角，1秒后自动消失）
        /// </summary>
        private void ShowSaveSuccessNotification(bool needRestart = false)
        {
            try
            {
                // 获取通知文本控件
                var notificationText = SaveSuccessNotification.Child as StackPanel;
                TextBlock? textBlock = null;
                
                if (notificationText != null && notificationText.Children.Count > 1)
                {
                    textBlock = notificationText.Children[1] as TextBlock;
                    if (textBlock != null)
                    {
                        textBlock.Text = needRestart ? "设置已保存（需重启）" : "设置已保存";
                    }
                }

                // 显示通知
                SaveSuccessNotification.Visibility = Visibility.Visible;

                // 淡入动画
                var fadeInAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                SaveSuccessNotification.BeginAnimation(OpacityProperty, fadeInAnimation);

                // 1秒后淡出
                var hideTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                hideTimer.Tick += (s, args) =>
                {
                    hideTimer.Stop();

                    // 淡出动画
                    var fadeOutAnimation = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(300),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                    };

                    fadeOutAnimation.Completed += (sender, e) =>
                    {
                        SaveSuccessNotification.Visibility = Visibility.Collapsed;
                        
                        // 恢复默认文本
                        var tb = textBlock;
                        if (tb != null)
                        {
                            tb.Text = "设置已保存";
                        }
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
        /// 重启应用
        /// </summary>
        private void RestartApplication()
        {
            try
            {
                var exePath = Environment.ProcessPath ?? 
                             System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                
                if (!string.IsNullOrEmpty(exePath))
                {
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true,
                        WorkingDirectory = Environment.CurrentDirectory
                    };
                    
                    System.Diagnostics.Process.Start(startInfo);
                    System.Windows.Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"重启应用失败: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"重启应用失败：{ex.Message}\n\n请手动重启应用以使设置生效。",
                    "重启失败",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 设置开机自启动
        /// </summary>
        private void SetStartWithWindows(bool enable)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true))
                {
                    if (key != null)
                    {
                        if (enable)
                        {
                            // 获取当前执行文件路径
                            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                            if (!string.IsNullOrEmpty(exePath))
                            {
                                key.SetValue(APP_NAME, $"\"{exePath}\"");
                            }
                        }
                        else
                        {
                            // 移除注册表项
                            key.DeleteValue(APP_NAME, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置开机自启动失败: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"设置开机自启动失败：\n{ex.Message}\n\n可能需要管理员权限。",
                    "警告",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// GitHub按钮点击事件
        /// </summary>
        private void BtnGitHub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 打开GitHub仓库链接
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/vistaminc/StarReminder",
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"打开GitHub链接失败: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"无法打开浏览器：\n{ex.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 检查更新按钮点击事件
        /// </summary>
        private async void BtnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            await CheckForUpdatesAsync();
        }

        /// <summary>
        /// 检查更新（简洁版）
        /// </summary>
        private async System.Threading.Tasks.Task CheckForUpdatesAsync()
        {
            try
            {
                // 显示检查中状态
                UpdateStatusPanel.Visibility = Visibility.Visible;
                NewVersionPanel.Visibility = Visibility.Collapsed;
                BtnDownloadUpdate.Visibility = Visibility.Collapsed;
                
                TxtUpdateStatus.Text = "检查中...";
                TxtUpdateStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#6B7280"));
                TxtUpdateSubStatus.Visibility = Visibility.Collapsed;

                // 检查更新
                _latestUpdateInfo = await _updateChecker.CheckForUpdatesAsync();

                if (_latestUpdateInfo != null)
                {
                    // 发现新版本
                    TxtUpdateStatus.Text = "发现新版本";
                    TxtUpdateStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#10B981"));
                    
                    // 使用 VersionInfo 自动读取当前版本
                    TxtCurrentVersion.Text = VersionInfo.Version;
                    TxtLatestVersion.Text = _latestUpdateInfo.LatestVersion;
                    NewVersionPanel.Visibility = Visibility.Visible;
                    BtnDownloadUpdate.Visibility = Visibility.Visible;
                }
                else
                {
                    // 已是最新版本
                    TxtUpdateStatus.Text = "已是最新版本";
                    TxtUpdateStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#10B981"));
                    NewVersionPanel.Visibility = Visibility.Collapsed;
                    BtnDownloadUpdate.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                // 检查失败
                TxtUpdateStatus.Text = "检查失败";
                TxtUpdateStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF4444"));
                TxtUpdateSubStatus.Text = ex.Message;
                TxtUpdateSubStatus.Visibility = Visibility.Visible;
                NewVersionPanel.Visibility = Visibility.Collapsed;
                BtnDownloadUpdate.Visibility = Visibility.Collapsed;
                
                System.Diagnostics.Debug.WriteLine($"检查更新失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 下载更新按钮点击事件
        /// </summary>
        private void BtnDownloadUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_latestUpdateInfo != null && !string.IsNullOrEmpty(_latestUpdateInfo.DownloadUrl))
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = _latestUpdateInfo.DownloadUrl,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"打开下载页面失败: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"无法打开浏览器：\n{ex.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

