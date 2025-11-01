using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WinForms = System.Windows.Forms;
using MediaDetectionSystem.ViewModels;
using MediaDetectionSystem.Views;

namespace MediaDetectionSystem
{
public partial class MainWindow : Window
{
        private MainViewModel? _viewModel;
        private WinForms.NotifyIcon? _notifyIcon;
        private bool _allowClose = false;

    public MainWindow()
    {
        InitializeComponent();
            
            // 不自动请求管理员权限，避免UAC弹窗
            // 如果需要管理员权限，用户可以右键"以管理员身份运行"或通过托盘菜单重启
            
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            
            // 订阅日志条目变化事件，自动滚动到最新
            _viewModel.LogEntries.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    // 在UI线程上执行滚动
                    Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        if (LogListBox != null && LogListBox.Items.Count > 0)
                        {
                            LogListBox.ScrollIntoView(LogListBox.Items[LogListBox.Items.Count - 1]);
                        }
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            };
            
            // 初始化托盘图标
            InitializeTrayIcon();
            
            // 根据设置决定是否启动时最小化
            this.Loaded += (s, e) =>
            {
                UpdateAdminStatus();
                
                // 应用启动设置
                ApplyStartupSettings();
                
                // 只有在设置中启用"启动时最小化"时才隐藏到托盘
                if (_viewModel?.ConfigManager?.AppSettings?.StartMinimized == true)
                {
                    HideToTray();
                }
                
                // 启动时自动清理日志
                if (_viewModel?.ConfigManager?.AppSettings?.AutoCleanLogs == true)
                {
                    CleanupOldLogs(_viewModel.ConfigManager.AppSettings.LogRetentionDays);
                }
                
                // 启动时检查更新
                if (_viewModel?.ConfigManager?.AppSettings?.CheckForUpdates == true)
                {
                    CheckForUpdatesAsync();
                }
            };
        }

        /// <summary>
        /// 更新管理员状态显示
        /// </summary>
        private void UpdateAdminStatus()
        {
            var isAdmin = IsRunningAsAdministrator();
            
            // 查找UI元素（简洁文字版本）
            var adminStatusText = this.FindName("AdminStatusText") as TextBlock;
            
            if (adminStatusText != null)
            {
                if (isAdmin)
                {
                    // 管理员状态 - 蓝色文字
                    adminStatusText.Text = "管理员";
                    adminStatusText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(59, 130, 246)); // #3B82F6
                    adminStatusText.ToolTip = "当前以管理员身份运行，具有完整系统权限";
                }
                else
                {
                    // 普通用户状态 - 灰色文字
                    adminStatusText.Text = "普通用户";
                    adminStatusText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(156, 163, 175)); // #9CA3AF
                    adminStatusText.ToolTip = "当前以普通用户运行，部分功能可能受限\n可通过托盘菜单切换到管理员模式";
                }
            }
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new WinForms.NotifyIcon
            {
                Text = "StarReminder - 星熠提醒",
                Visible = true  // 始终显示托盘图标
            };

            // 设置图标 - 使用logo.png，高质量缩放
            try
            {
                var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
                
                if (System.IO.File.Exists(iconPath))
                {
                    // 使用高质量算法创建托盘图标
                    _notifyIcon.Icon = CreateHighQualityIcon(iconPath);
                }
                else
                {
                    // 如果logo.png不存在，创建一个简单的星形图标
                    _notifyIcon.Icon = CreateDefaultIcon();
                    System.Diagnostics.Debug.WriteLine($"警告: logo.png未找到于 {iconPath}，使用默认图标");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"托盘图标加载失败: {ex.Message}");
                _notifyIcon.Icon = SystemIcons.Application;
            }

            // 双击托盘图标显示窗口
            _notifyIcon.DoubleClick += (s, e) =>
            {
                ShowFromTray();
            };

            // 右键菜单
            var contextMenu = new WinForms.ContextMenuStrip();
            
            var showMenuItem = new WinForms.ToolStripMenuItem("显示主窗口");
            showMenuItem.Click += (s, e) => ShowFromTray();
            
            var hideMenuItem = new WinForms.ToolStripMenuItem("隐藏到托盘");
            hideMenuItem.Click += (s, e) => HideToTray();
            
            var separatorMenuItem = new WinForms.ToolStripSeparator();
            
            var adminMenuItem = new WinForms.ToolStripMenuItem("以管理员身份重启");
            adminMenuItem.Click += (s, e) => RestartAsAdministrator();
            
            var exitMenuItem = new WinForms.ToolStripMenuItem("退出程序");
            exitMenuItem.Click += (s, e) => ExitApplication();

            contextMenu.Items.AddRange(new WinForms.ToolStripItem[]
            {
                showMenuItem,
                hideMenuItem,
                separatorMenuItem,
                adminMenuItem,
                exitMenuItem
            });

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void ShowFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
            // 托盘图标保持显示
        }

        private void HideToTray()
        {
            Hide();
            // 托盘图标保持显示
            // 不显示气泡提示 - 用户要求移除
        }

        private void ExitApplication()
        {
            _allowClose = true;
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
            }
            System.Windows.Application.Current.Shutdown();
        }

        private void CheckAndRequestAdmin()
        {
            // 默认以管理员身份启动 - 用户要求
            if (!IsRunningAsAdministrator())
            {
                // 静默请求管理员权限，不弹窗询问
                RestartAsAdministrator();
            }
        }

        /// <summary>
        /// 创建高质量托盘图标
        /// </summary>
        private System.Drawing.Icon CreateHighQualityIcon(string imagePath)
        {
            try
            {
                using (var original = new Bitmap(imagePath))
                {
                    // 创建高质量缩放的16x16图标
                    var icon16 = ResizeImageHighQuality(original, 16, 16);
                    var hIcon = icon16.GetHicon();
                    var icon = System.Drawing.Icon.FromHandle(hIcon);
                    return icon;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"创建高质量图标失败: {ex.Message}");
                return CreateDefaultIcon();
            }
        }

        /// <summary>
        /// 高质量图像缩放
        /// </summary>
        private Bitmap ResizeImageHighQuality(Bitmap source, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (var graphics = System.Drawing.Graphics.FromImage(destImage))
            {
                // 高质量缩放设置
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(source, destRect, 0, 0, source.Width, source.Height, System.Drawing.GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private System.Drawing.Icon CreateDefaultIcon()
        {
            // 创建一个16x16的简单星形图标（备用）
            var bitmap = new Bitmap(16, 16);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // 绘制蓝色到青色渐变背景
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new System.Drawing.Rectangle(0, 0, 16, 16),
                    System.Drawing.Color.FromArgb(70, 130, 249), // 蓝色
                    System.Drawing.Color.FromArgb(95, 220, 230), // 青色
                    45f))
                {
                    g.FillEllipse(brush, 1, 1, 14, 14);
                }
                
                // 绘制白色星形轮廓
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.White, 2))
                {
                    g.DrawEllipse(pen, 4, 4, 8, 8);
                }
            }
            
            var hIcon = bitmap.GetHicon();
            return System.Drawing.Icon.FromHandle(hIcon);
        }

        private bool IsRunningAsAdministrator()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private void RestartAsAdministrator()
        {
            try
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "",
                    Verb = "runas" // 请求管理员权限
                };

                System.Diagnostics.Process.Start(processInfo);
                _allowClose = true;
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "请求管理员权限失败: " + ex.Message + "\n\n程序将以普通权限继续运行。",
                    "权限提升失败",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void ProcessCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string processName)
            {
                _viewModel?.OpenConfigWindow(processName);
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            // 最小化到托盘而不是任务栏
            HideToTray();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // 点击关闭按钮时隐藏到托盘，而不是退出
            HideToTray();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_allowClose)
            {
                // 取消关闭操作，改为隐藏到托盘
                e.Cancel = true;
                HideToTray();
            }
            else
            {
                // 真正关闭时清理资源
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                }
            }
            
            base.OnClosing(e);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            
            // 当窗口最小化时，隐藏到托盘
            if (WindowState == WindowState.Minimized)
            {
                HideToTray();
            }
        }

        /// <summary>
        /// 应用启动设置
        /// </summary>
        private void ApplyStartupSettings()
        {
            try
            {
                var settings = _viewModel?.ConfigManager?.AppSettings;
                if (settings == null) return;

                // 应用主题
                ApplyTheme(settings.ThemeMode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"应用启动设置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 应用主题
        /// </summary>
        private void ApplyTheme(string themeMode)
        {
            try
            {
                string themeName;
                
                // 处理自动主题
                if (themeMode == "Auto")
                {
                    // 检测系统主题
                    bool isDarkMode = IsSystemDarkMode();
                    themeName = isDarkMode ? "DarkTheme" : "LightTheme";
                }
                else
                {
                    themeName = themeMode == "Dark" ? "DarkTheme" : "LightTheme";
                }
                
                var themeUri = new Uri($"Themes/{themeName}.xaml", UriKind.Relative);
                
                // 移除现有主题
                var existingThemes = System.Windows.Application.Current.Resources.MergedDictionaries
                    .Where(d => d.Source != null && d.Source.OriginalString.Contains("Theme"))
                    .ToList();
                
                foreach (var theme in existingThemes)
                {
                    System.Windows.Application.Current.Resources.MergedDictionaries.Remove(theme);
                }
                
                // 加载新主题
                var newTheme = new System.Windows.ResourceDictionary { Source = themeUri };
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(newTheme);
                
                System.Diagnostics.Debug.WriteLine($"已切换到主题: {themeName} (模式: {themeMode})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"应用主题失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检测系统是否使用深色主题
        /// </summary>
        private bool IsSystemDarkMode()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("AppsUseLightTheme");
                        if (value != null)
                        {
                            // 返回值为0表示使用深色主题
                            return (int)value == 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检测系统主题失败: {ex.Message}");
            }
            
            // 默认返回浅色主题
            return false;
        }

        /// <summary>
        /// 清理旧日志
        /// </summary>
        private void CleanupOldLogs(int retentionDays)
        {
            try
            {
                var logsDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!System.IO.Directory.Exists(logsDirectory))
                    return;

                var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                var logFiles = System.IO.Directory.GetFiles(logsDirectory, "*.json");

                int deletedCount = 0;
                foreach (var file in logFiles)
                {
                    try
                    {
                        var fileInfo = new System.IO.FileInfo(file);
                        if (fileInfo.LastWriteTime < cutoffDate)
                        {
                            System.IO.File.Delete(file);
                            deletedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"删除日志文件失败: {file}, {ex.Message}");
                    }
                }

                if (deletedCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"已清理 {deletedCount} 个过期日志文件");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"清理日志时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置按钮点击事件
        /// </summary>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 确保ViewModel已初始化
                if (_viewModel == null)
                {
                    System.Windows.MessageBox.Show(
                        "系统初始化未完成，请稍后再试。",
                        "提示",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // 获取当前设置（如果为null则创建新的）
                var currentSettings = _viewModel.ConfigManager?.AppSettings ?? new Models.AppSettings();
                
                var settingsWindow = new Views.SettingsWindow(
                    currentSettings,
                    (settings) =>
                    {
                        // 保存设置回调 - 热更新所有设置
                        try
                        {
                            if (_viewModel?.ConfigManager != null)
                            {
                                // 保存配置到文件
                                _viewModel.ConfigManager.SaveConfiguration();
                                
                                // === 立即生效的设置（热更新） ===
                                
                                // 1. 界面设置
                                ApplyTheme(settings.ThemeMode);          // 主题模式
                                
                                // 2. 监控设置
                                _viewModel.UpdateMonitorInterval(settings.MonitorInterval);  // 监控间隔
                                _viewModel.UpdateLoggingSettings(settings.EnableDetailedLogging);  // 详细日志
                                
                                // 3. 通知设置（已通过引用自动生效）
                                // EnableTrayNotifications, EnableSoundNotifications, ShowStartupDialog
                                // NotificationDuration - AlertManager使用AppSettings引用，自动生效
                                
                                // 4. 高级设置
                                if (settings.AutoCleanLogs)
                                {
                                    CleanupOldLogs(settings.LogRetentionDays);
                                }
                                
                                // === 需要重启才能生效的设置 ===
                                // - StartWithWindows: 开机自启动（通过注册表设置，立即生效）
                                // - StartMinimized: 启动时最小化
                                // - Language: 界面语言
                                // - CheckForUpdates: 启动时检查更新
                                
                                System.Diagnostics.Debug.WriteLine("[设置] 热更新完成，所有设置已实时生效");
                            }
                        }
                        catch (Exception saveEx)
                        {
                            System.Windows.MessageBox.Show(
                                $"保存设置时出错：\n{saveEx.Message}",
                                "保存失败",
                                System.Windows.MessageBoxButton.OK,
                                System.Windows.MessageBoxImage.Error);
                        }
                    }
                );
                
                settingsWindow.Owner = this;
                settingsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"打开设置窗口失败：\n{ex.Message}\n\n详细信息：\n{ex.StackTrace}",
                    "错误",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 异步检查更新
        /// </summary>
        private async void CheckForUpdatesAsync()
        {
            try
            {
                var updateChecker = new Services.UpdateChecker();
                var updateInfo = await updateChecker.CheckForUpdatesAsync();

                if (updateInfo != null)
                {
                    // 在UI线程上显示更新对话框
                    Dispatcher.Invoke(() =>
                    {
                        var result = System.Windows.MessageBox.Show(
                            $"发现新版本！\n\n" +
                            $"当前版本：{updateInfo.CurrentVersion}\n" +
                            $"最新版本：{updateInfo.LatestVersion}\n" +
                            $"发布时间：{updateInfo.PublishedAt:yyyy-MM-dd}\n\n" +
                            $"是否立即前往下载？",
                            "StarReminder - 更新可用",
                            System.Windows.MessageBoxButton.YesNo,
                            System.Windows.MessageBoxImage.Information);

                        if (result == MessageBoxResult.Yes)
                        {
                            updateChecker.OpenDownloadPage(updateInfo.DownloadUrl);
                        }
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[UpdateChecker] 当前已是最新版本");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateChecker] 检查更新时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 清空日志按钮点击事件
        /// </summary>
        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel?.LogEntries != null)
                {
                    var result = System.Windows.MessageBox.Show(
                        "确定要清空所有活动日志吗？\n\n此操作不可恢复。",
                        "清空日志",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        _viewModel.LogEntries.Clear();
                        System.Diagnostics.Debug.WriteLine("[MainWindow] 活动日志已清空");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainWindow] 清空日志失败: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"清空日志失败：\n{ex.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
