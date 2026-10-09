using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Real_ESRGAN_GUI_WPF
{
    public partial class MainWindow : Window
    {
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".webp" };

        // 界面初始化期间屏蔽各种 SelectedIndexChanged 事件
        private bool _isUiReady;

        /// <summary>为 true 时表示本次启动只是处理命令行文件，不需要显示窗口。</summary>
        public bool CloseRequested { get; private set; }

        public MainWindow(string[] args)
        {
            InitializeComponent();

            Parameters.HasPermission = ComponentManager.CheckPathWritable(
                Parameters.WorkPath, out var permissionError);
            Parameters.IsCreatedNewFolder = true;

            if (!Parameters.HasPermission)
            {
                ShowPermissionError(Parameters.WorkPath, permissionError);
                CloseRequested = true;
                return;
            }

            ConfigManager.EnsureValid(Parameters.XmlPath);

            Parameters.CurrentLanguage = ConfigManager.GetLanguage(Parameters.XmlPath);
            Parameters.OldScreenWidth = ConfigManager.GetScreenWidth(Parameters.XmlPath);
            Parameters.OldScreenHeight = ConfigManager.GetScreenHeight(Parameters.XmlPath);
            Parameters.Scale = ConfigManager.GetScale(Parameters.XmlPath);
            Parameters.Model = ConfigManager.GetModel(Parameters.XmlPath);
            Parameters.Extension = ConfigManager.GetExtension(Parameters.XmlPath);
            Parameters.ProcessHidden = ConfigManager.GetProcessHidden(Parameters.XmlPath);

            ApplyLanguage();
            BuildModelMenu();
            BuildExtensionMenu();
            BuildScaleMenu();
            HideProcessCheckBox.IsChecked = Parameters.ProcessHidden;

            _isUiReady = true;

            if (!ComponentManager.AllPresent())
            {
                ComponentManager.ExtractAll();
                if (!Parameters.IsCreatedNewFolder)
                {
                    CloseRequested = true;
                    return;
                }
            }

            if (args != null && args.Length > 0)
            {
                RunBatch(args);
                CloseRequested = true;
            }
        }

        // ---------- 界面语言 ----------

        private void ApplyLanguage()
        {
            Title = "Real ESRGAN";

            FileMenu.Header = Localization.Get("FileMenu");
            FileOpenMenu.Header = Localization.Get("FileOpen");
            FileExitMenu.Header = Localization.Get("FileExit");
            LanguageMenu.Header = Localization.Get("LanguageMenu");
            LanguageSelectMenu.Header = Localization.Get("LanguageSelect");
            AboutMenu.Header = Localization.Get("AboutMenu");
            AboutMenuItem.Header = Localization.Get("AboutItem");

            ScaleLabel.Text = Localization.Get("Scale");
            ModelLabel.Text = Localization.Get("Model");
            ExtensionLabel.Text = Localization.Get("Extension");
            HideProcessCheckBox.Content = Localization.Get("HideProcess");
            SaveConfigButton.Content = Localization.Get("SaveConfig");
        }

        // ---------- 下拉菜单构建 ----------

        private void BuildModelMenu()
        {
            ModelComboBox.Items.Clear();
            ModelComboBox.Items.Add("realesrgan-x4plus");
            ModelComboBox.Items.Add("realesrgan-x4plus-anime");
            ModelComboBox.Items.Add("realesr-animevideov3");

            switch (Parameters.Model)
            {
                case "realesrgan-x4plus": ModelComboBox.SelectedIndex = 0; break;
                case "realesrgan-x4plus-anime": ModelComboBox.SelectedIndex = 1; break;
                case "realesr-animevideov3": ModelComboBox.SelectedIndex = 2; break;
                default: ModelComboBox.SelectedIndex = 0; break;
            }
        }

        private void BuildExtensionMenu()
        {
            ExtensionComboBox.Items.Clear();
            ExtensionComboBox.Items.Add("jpg");
            ExtensionComboBox.Items.Add("png");
            ExtensionComboBox.Items.Add("webp");

            switch (Parameters.Extension)
            {
                case "jpg": ExtensionComboBox.SelectedIndex = 0; break;
                case "png": ExtensionComboBox.SelectedIndex = 1; break;
                case "webp": ExtensionComboBox.SelectedIndex = 2; break;
                default: ExtensionComboBox.SelectedIndex = 1; break;
            }
        }

        /// <summary>
        /// 根据当前模型重建缩放选项。animevideov3 支持 2/3/4 倍，其它模型只有 4 倍。
        /// </summary>
        private void BuildScaleMenu()
        {
            bool wasReady = _isUiReady;
            _isUiReady = false;

            ScaleComboBox.Items.Clear();

            if (Parameters.Model == "realesr-animevideov3")
            {
                ScaleComboBox.Items.Add("2");
                ScaleComboBox.Items.Add("3");
                ScaleComboBox.Items.Add("4");

                switch (Parameters.Scale)
                {
                    case "2": ScaleComboBox.SelectedIndex = 0; break;
                    case "3": ScaleComboBox.SelectedIndex = 1; break;
                    default: ScaleComboBox.SelectedIndex = 2; break;
                }
            }
            else
            {
                ScaleComboBox.Items.Add("4");
                ScaleComboBox.SelectedIndex = 0;

                if (Parameters.Scale != "4")
                    Parameters.Scale = "4";
            }

            _isUiReady = wasReady;
        }

        // ---------- 主流程 ----------

        private void RunBatch(string[] files)
        {
            if (files == null || files.Length == 0) return;
            if (files.Any(string.IsNullOrEmpty)) return;

            Parameters.IsMultipleFiles = files.Length > 1;

            foreach (var file in files)
            {
                Parameters.FilePath = file;
                Parameters.FileName = Path.GetFileNameWithoutExtension(file);
                Parameters.DirectoryPath = Path.GetDirectoryName(file);

                if (!ComponentManager.AllPresent())
                {
                    ComponentManager.ExtractAll();
                    if (!Parameters.IsCreatedNewFolder)
                    {
                        ShowMessage(Localization.Get("ComponentsMissing"),
                                    Localization.Get("ErrorTitle"),
                                    MessageBoxImage.Error);
                        break;
                    }
                }

                if (!ValidateExtension(file))
                    continue;

                RunRealesrgan();
            }
        }

        private bool ValidateExtension(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                ShowMessage(Localization.Get("UnsupportedFile"),
                            Localization.Get("ErrorTitle"),
                            MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void RunRealesrgan()
        {
            string timestamp = DateTime.Now.ToString("HH-mm-ss");

            // 上一次处理留下的时间戳会被剥离，避免反复处理时后缀叠加
            string baseName = Regex.Replace(Parameters.FileName, @"_\d{2}-\d{2}-\d{2}$", "");
            string outputName = $"{baseName}_x{Parameters.Scale}_{timestamp}.{Parameters.Extension}";
            string outputPath = Path.Combine(Parameters.DirectoryPath, outputName);

            string arguments =
                $"-i \"{Parameters.FilePath}\" -o \"{outputPath}\" -n {Parameters.Model} -s {Parameters.Scale}";

            var startInfo = new ProcessStartInfo
            {
                FileName = Parameters.RealesrganPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = HideProcessCheckBox.IsChecked == true
            };

            Process.Start(startInfo);
        }

        // ---------- 事件处理 ----------

        private void ScaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isUiReady) return;
            Parameters.Scale = ScaleComboBox.SelectedItem?.ToString() ?? "4";
        }

        private void ModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isUiReady) return;
            Parameters.Model = ModelComboBox.SelectedItem?.ToString() ?? "realesrgan-x4plus";
            BuildScaleMenu();
        }

        private void ExtensionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isUiReady) return;
            Parameters.Extension = ExtensionComboBox.SelectedItem?.ToString() ?? "png";
        }

        private void HideProcessCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            Parameters.ProcessHidden = HideProcessCheckBox.IsChecked == true;
        }

        private void SaveConfigButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigManager.Update(Parameters.XmlPath, "Language", Parameters.CurrentLanguage);
            ConfigManager.Update(Parameters.XmlPath, "Scale", Parameters.Scale);
            ConfigManager.Update(Parameters.XmlPath, "Model", Parameters.Model);
            ConfigManager.Update(Parameters.XmlPath, "Extension", Parameters.Extension);
            ConfigManager.Update(Parameters.XmlPath, "ProcessHidden",
                Parameters.ProcessHidden.ToString().ToLowerInvariant());
            SaveLocation();

            ShowMessage(Localization.Get("ConfigSaved"),
                        Localization.Get("NoticeTitle"),
                        MessageBoxImage.Information);
        }

        private void FileOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JPG (*.jpg)|*.jpg|JPEG (*.jpeg)|*.jpeg|PNG (*.png)|*.png|WEBP (*.webp)|*.webp"
            };

            if (dialog.ShowDialog(this) != true) return;

            Parameters.FilePath = dialog.FileName;
            Parameters.FileName = Path.GetFileNameWithoutExtension(dialog.FileName);
            Parameters.DirectoryPath = Path.GetDirectoryName(dialog.FileName);

            if (!ComponentManager.AllPresent())
            {
                ComponentManager.ExtractAll();
                if (!Parameters.IsCreatedNewFolder)
                {
                    ShowMessage(Localization.Get("ComponentsMissing"),
                                Localization.Get("ErrorTitle"),
                                MessageBoxImage.Error);
                    return;
                }
            }

            if (!ValidateExtension(dialog.FileName)) return;
            RunRealesrgan();
        }

        private void FileExitMenu_Click(object sender, RoutedEventArgs e)
        {
            SaveLocation();
            Close();
        }

        private void LangZhCN_Click(object sender, RoutedEventArgs e) => SwitchLanguage("zh-CN");
        private void LangZhTW_Click(object sender, RoutedEventArgs e) => SwitchLanguage("zh-TW");
        private void LangEnUS_Click(object sender, RoutedEventArgs e) => SwitchLanguage("en-US");

        private void SwitchLanguage(string language)
        {
            Parameters.CurrentLanguage = language;
            ApplyLanguage();
            ConfigManager.Update(Parameters.XmlPath, "Language", language);
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow { Owner = this };
            about.ShowDialog();
        }

        // ---------- 拖放 ----------

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;

            bool allValid = files.All(f =>
            {
                var ext = Path.GetExtension(f).ToLowerInvariant();
                return AllowedExtensions.Contains(ext);
            });

            if (allValid)
                e.Effects = DragDropEffects.Copy;

            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;

            RunBatch(files);
            e.Handled = true;
        }

        // ---------- 窗口位置 ----------

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double screenW = SystemParameters.PrimaryScreenWidth;
            double screenH = SystemParameters.PrimaryScreenHeight;

            double fallbackX = (screenW - Width) / 2;
            double fallbackY = (screenH - Height) / 2;

            int savedX = ConfigManager.GetLocationX(Parameters.XmlPath, (int)fallbackX);
            int savedY = ConfigManager.GetLocationY(Parameters.XmlPath, (int)fallbackY);

            // 屏幕尺寸变了就用中心位置，避免窗口跑到看不见的地方
            bool screenChanged =
                Parameters.OldScreenWidth != (int)screenW ||
                Parameters.OldScreenHeight != (int)screenH;

            if (screenChanged || !IsOnScreen(savedX, savedY))
            {
                Left = fallbackX;
                Top = fallbackY;
            }
            else
            {
                Left = savedX;
                Top = savedY;
            }
        }

        private bool IsOnScreen(int x, int y)
        {
            double virtualLeft = SystemParameters.VirtualScreenLeft;
            double virtualTop = SystemParameters.VirtualScreenTop;
            double virtualRight = virtualLeft + SystemParameters.VirtualScreenWidth;
            double virtualBottom = virtualTop + SystemParameters.VirtualScreenHeight;

            return x >= virtualLeft - 10 && x <= virtualRight - 50 &&
                   y >= virtualTop - 10 && y <= virtualBottom - 50;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveLocation();
        }

        private void SaveLocation()
        {
            ConfigManager.Update(Parameters.XmlPath, "LocationX", ((int)Left).ToString());
            ConfigManager.Update(Parameters.XmlPath, "LocationY", ((int)Top).ToString());
            ConfigManager.Update(Parameters.XmlPath, "ScreenWidth",
                ((int)SystemParameters.PrimaryScreenWidth).ToString());
            ConfigManager.Update(Parameters.XmlPath, "ScreenHeight",
                ((int)SystemParameters.PrimaryScreenHeight).ToString());
        }

        // ---------- 错误提示 ----------

        private void ShowPermissionError(string path, Exception ex)
        {
            var msg = string.Format(Localization.Get("PermissionDenied"), path, ex?.Message ?? "");
            ShowMessage(msg, Localization.Get("ErrorTitle"), MessageBoxImage.Error);
        }

        private void ShowMessage(string message, string title, MessageBoxImage icon)
        {
            MessageBox.Show(this, message, title, MessageBoxButton.OK, icon);
        }
    }
}