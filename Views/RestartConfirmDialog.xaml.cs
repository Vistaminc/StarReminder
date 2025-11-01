using System.Windows;
using System.Windows.Input;

namespace MediaDetectionSystem.Views
{
    /// <summary>
    /// 重启确认对话框
    /// </summary>
    public partial class RestartConfirmDialog : Window
    {
        public RestartConfirmDialog()
        {
            InitializeComponent();
            
            // 支持键盘快捷键
            this.KeyDown += OnKeyDown;
            
            // 默认焦点在"否"按钮上（更安全）
            this.Loaded += (s, e) => BtnNo.Focus();
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Y || e.Key == Key.Enter)
            {
                DialogResult = true;
                Close();
            }
            else if (e.Key == Key.N || e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

