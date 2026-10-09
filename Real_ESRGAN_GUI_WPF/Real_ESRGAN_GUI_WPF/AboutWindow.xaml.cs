using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Real_ESRGAN_GUI_WPF
{
    public partial class AboutWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        // 注意：这里不再清 WS_THICKFRAME，否则边缘就拉不动了

        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        // 系统命令消息，双击标题栏、Aero Snap 等都从这里过
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_MAXIMIZE = 0xF030;

        private static readonly bool Is64Bit = IntPtr.Size == 8;

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            return Is64Bit
                ? GetWindowLongPtr64(hWnd, nIndex)
                : GetWindowLong32(hWnd, nIndex);
        }

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return Is64Bit
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : SetWindowLong32(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        public AboutWindow()
        {
            InitializeComponent();
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            Title = Localization.Get("AboutTitle");
            TitleLabel.Text = "Real ESRGAN GUI";
            GithubText.Text = Localization.Get("GithubPage");
            LicenseText.Text = Localization.Get("License");
            ConfirmButton.Content = Localization.Get("Confirm");
            CopyrightLabel.Text = BuildCopyrightText();
        }

        private static string BuildCopyrightText()
        {
            const int startYear = 2024;
            int currentYear = DateTime.Now.Year;

            return currentYear == startYear
                ? $"Copyright © {startYear} LatteYogurt. All rights reserved."
                : $"Copyright © {startYear}-{currentYear} LatteYogurt. All rights reserved.";
        }

        /// <summary>
        /// 句柄有了但窗口还没显示时动样式，避免用户看到按钮闪一下。
        /// </summary>
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            if (handle == IntPtr.Zero) return;

            // 只清掉两个按钮位，WS_THICKFRAME 保留给缩放用
            long style = GetWindowLongPtr(handle, GWL_STYLE).ToInt64();
            style &= ~WS_MINIMIZEBOX;
            style &= ~WS_MAXIMIZEBOX;
            SetWindowLongPtr(handle, GWL_STYLE, new IntPtr(style));

            // 边框需要重画一次，不然按钮虽然没了但视觉上还残留
            SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
                SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE |
                SWP_NOZORDER | SWP_NOACTIVATE);

            // 挂消息钩子，拦住双击标题栏和 Aero Snap 触发的最大化
            HwndSource.FromHwnd(handle)?.AddHook(WndProc);
        }

        /// <summary>
        /// 拦截系统命令。只掐最小化和最大化，其它原样放行。
        /// </summary>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SYSCOMMAND)
            {
                // 低四位是系统内部使用的，比较前先屏蔽掉
                int command = wParam.ToInt32() & 0xFFF0;

                if (command == SC_MINIMIZE || command == SC_MAXIMIZE)
                {
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 屏幕配置变了就回到主屏中央
            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://github.com/Latte-Yogurt");
        }

        private void License_Click(object sender, RoutedEventArgs e)
        {
            OpenUrl("https://www.gnu.org/licenses/gpl-3.0.html");
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                // 没有默认浏览器就直接跳过，不用弹框打扰
            }
        }
    }
}