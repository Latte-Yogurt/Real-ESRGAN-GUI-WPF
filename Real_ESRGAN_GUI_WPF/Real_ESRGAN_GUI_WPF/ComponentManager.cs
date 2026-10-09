using System;
using System.IO;
using System.Reflection;

namespace Real_ESRGAN_GUI_WPF
{
    /// <summary>
    /// 负责把内嵌的 Real-ESRGAN 运行组件解压到本地目录，并在启动时检查是否齐全。
    /// </summary>
    public static class ComponentManager
    {
        public static bool CheckPathWritable(string path, out Exception error)
        {
            error = null;
            var probe = Path.Combine(path, "~test_" + Guid.NewGuid().ToString("N") + ".tmp");

            try
            {
                using (var fs = new FileStream(probe, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    fs.WriteByte(0);
                }
                using (var fs = new FileStream(probe, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.ReadByte();
                }
                return true;
            }
            catch (UnauthorizedAccessException ex) { error = ex; return false; }
            catch (Exception ex) { error = ex; return false; }
            finally
            {
                try { if (File.Exists(probe)) File.Delete(probe); }
                catch { /* 清不掉也无所谓，反正只是临时文件 */ }
            }
        }

        public static bool AllPresent()
        {
            return File.Exists(Parameters.RealesrganPath)
                && File.Exists(Parameters.Vcomp140Path)
                && File.Exists(Parameters.Vcomp140dPath)
                && Directory.Exists(Parameters.ModelsPath)
                && File.Exists(Parameters.RealesrAnimeV3X2Bin)
                && File.Exists(Parameters.RealesrAnimeV3X2Param)
                && File.Exists(Parameters.RealesrAnimeV3X3Bin)
                && File.Exists(Parameters.RealesrAnimeV3X3Param)
                && File.Exists(Parameters.RealesrAnimeV3X4Bin)
                && File.Exists(Parameters.RealesrAnimeV3X4Param)
                && File.Exists(Parameters.RealesrganX4PlusBin)
                && File.Exists(Parameters.RealesrganX4PlusParam)
                && File.Exists(Parameters.RealesrganX4PlusAnimeBin)
                && File.Exists(Parameters.RealesrganX4PlusAnimeParam);
        }

        public static void ExtractAll()
        {
            if (!Directory.Exists(Parameters.RealesrganFolderPath))
            {
                Parameters.IsCreatedNewFolder = CreateFolder(Parameters.RealesrganFolderPath);
            }

            if (!Directory.Exists(Parameters.RealesrganFolderPath))
                return;

            ExtractIfMissing(Parameters.RealesrganPath,
                "Real_ESRGAN_GUI_WPF.Resources.realesrgan.exe");
            ExtractIfMissing(Parameters.Vcomp140Path,
                "Real_ESRGAN_GUI_WPF.Resources.vcomp140.dll");
            ExtractIfMissing(Parameters.Vcomp140dPath,
                "Real_ESRGAN_GUI_WPF.Resources.vcomp140d.dll");

            if (!Directory.Exists(Parameters.ModelsPath))
            {
                Parameters.IsCreatedNewFolder = CreateFolder(Parameters.ModelsPath);
            }

            if (!Directory.Exists(Parameters.ModelsPath))
                return;

            FixFolderAttributes(Parameters.ModelsPath);

            ExtractIfMissing(Parameters.RealesrAnimeV3X2Bin,
                "Real_ESRGAN_GUI_WPF.Resources.models.realesr-animevideov3-x2.bin");
            ExtractIfMissing(Parameters.RealesrAnimeV3X2Param,
                "Real_ESRGAN_GUI_WPF.Resources.models.realesr-animevideov3-x2.param");
            ExtractIfMissing(Parameters.RealesrAnimeV3X3Bin,
                "Real_ESRGAN_GUI_WPF.Resources.models.realesr-animevideov3-x3.bin");
            ExtractIfMissing(Parameters.RealesrAnimeV3X3Param,
                "Real_ESRGAN_GUI_WPF.Resources.models.realesr-animevideov3-x3.param");
            ExtractIfMissing(Parameters.RealesrAnimeV3X4Bin,
                "Real_ESRGAN_GUI_WPF.Resources.models.realesr-animevideov3-x4.bin");
            ExtractIfMissing(Parameters.RealesrAnimeV3X4Param,
                "Real_ESRGAN_GUI_WPF.Resources.models.realesr-animevideov3-x4.param");
            ExtractIfMissing(Parameters.RealesrganX4PlusBin,
                "Real_ESRGAN_GUI_WPF.Resources.models.realesrgan-x4plus.bin");
            ExtractIfMissing(Parameters.RealesrganX4PlusParam,
                "Real_ESRGAN_GUI_WPF.Resources.models.realesrgan-x4plus.param");
            ExtractIfMissing(Parameters.RealesrganX4PlusAnimeBin,
                "Real_ESRGAN_GUI_WPF.Resources.models.realesrgan-x4plus-anime.bin");
            ExtractIfMissing(Parameters.RealesrganX4PlusAnimeParam,
                "Real_ESRGAN_GUI_WPF.Resources.models.realesrgan-x4plus-anime.param");
        }

        private static void ExtractIfMissing(string targetPath, string resourceName)
        {
            if (!File.Exists(targetPath))
                ExtractResource(resourceName, targetPath);
        }

        private static bool CreateFolder(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception ex)
            {
                ShowMessage(string.Format(Localization.Get("FolderCreateFailed"), ex.Message),
                            Localization.Get("ErrorTitle"),
                            System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        private static void FixFolderAttributes(string path)
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists) return;

            if ((di.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                di.Attributes = FileAttributes.Normal;
            if ((di.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                di.Attributes = FileAttributes.Normal;
        }

        private static void ExtractResource(string resourceName, string outputPath)
        {
            using (var stream = Assembly.GetExecutingAssembly()
                                        .GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    ShowMessage(string.Format(Localization.Get("ResourceMissing"), resourceName),
                                Localization.Get("ErrorTitle"),
                                System.Windows.MessageBoxImage.Error);
                    return;
                }

                using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fs);
                }
            }
        }

        private static void ShowMessage(string message, string title,
                                        System.Windows.MessageBoxImage icon)
        {
            System.Windows.MessageBox.Show(message, title,
                System.Windows.MessageBoxButton.OK, icon);
        }
    }
}