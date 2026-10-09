using System;
using System.Windows;

namespace Real_ESRGAN_GUI_WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var main = new MainWindow(e.Args);

                // 被文件关联或"发送到"唤起时，参数处理完就没必要再显示界面了
                if (main.CloseRequested)
                {
                    Shutdown();
                    return;
                }

                MainWindow = main;
                main.Show();
            }
            catch
            {
                Environment.Exit(0);
            }
        }
    }
}