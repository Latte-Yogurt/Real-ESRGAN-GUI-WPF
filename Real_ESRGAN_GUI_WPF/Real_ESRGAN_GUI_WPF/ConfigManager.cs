using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Real_ESRGAN_GUI_WPF
{
    /// <summary>
    /// 负责读写 Real_ESRGAN_GUI_WPF.xml。所有取值方法在遇到异常时都会退回默认值。
    /// </summary>
    public static class ConfigManager
    {
        // 位置用 -1 表示"还没记录过"，比用 0 更明确
        private const int NoLocation = -1;

        public static void EnsureValid(string path)
        {
            var fi = new FileInfo(path);

            if (fi.Exists)
            {
                // 防止用户或第三方工具把配置文件设成只读/隐藏导致后续写不进去
                if ((fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    fi.Attributes = FileAttributes.Normal;
                if ((fi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    fi.Attributes = FileAttributes.Normal;

                try
                {
                    XDocument.Load(path);
                    return;
                }
                catch
                {
                    // 内容坏了就当没写过，下面重建
                }
            }

            CreateDefault(path);
        }

        public static void CreateDefault(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.WriteAllText(path, string.Empty);

                var doc = new XDocument(
                    new XElement("Configuration",
                        new XElement("Language", Localization.DetectDefaultLanguage()),
                        new XElement("LocationX", NoLocation),
                        new XElement("LocationY", NoLocation),
                        new XElement("ScreenWidth", 0),
                        new XElement("ScreenHeight", 0),
                        new XElement("Scale", "4"),
                        new XElement("Model", "realesrgan-x4plus"),
                        new XElement("Extension", "png"),
                        new XElement("ProcessHidden", "false")));

                doc.Save(path);
            }
            catch (Exception ex)
            {
                ShowMessage(string.Format(Localization.Get("GenericError"), ex.Message),
                            Localization.Get("ErrorTitle"),
                            System.Windows.MessageBoxImage.Error);
            }
        }

        public static void Update(string path, string key, string value)
        {
            try
            {
                var doc = XDocument.Load(path);
                var node = doc.Descendants(key).FirstOrDefault();

                if (node != null)
                    node.Value = value;
                else
                    doc.Root?.Add(new XElement(key, value));

                doc.Save(path);
            }
            catch
            {
                // 写不进去就重建一份，尽量避免配置文件彻底崩掉
                CreateDefault(path);
            }
        }

        // ---------- 各字段的读取 ----------

        public static string GetLanguage(string path)
        {
            var value = ReadNode(path, "Language");
            if (!string.IsNullOrEmpty(value) && Localization.IsSupported(value))
                return value;
            return Localization.DetectDefaultLanguage();
        }

        public static int GetScreenWidth(string path)
        {
            return ParseInt(ReadNode(path, "ScreenWidth"), 0);
        }

        public static int GetScreenHeight(string path)
        {
            return ParseInt(ReadNode(path, "ScreenHeight"), 0);
        }

        public static int GetLocationX(string path, int fallback)
        {
            return ParseLocation(ReadNode(path, "LocationX"), fallback);
        }

        public static int GetLocationY(string path, int fallback)
        {
            return ParseLocation(ReadNode(path, "LocationY"), fallback);
        }

        public static string GetScale(string path)
        {
            var value = ReadNode(path, "Scale");
            if (value == "2" || value == "3" || value == "4")
                return value;
            return "4";
        }

        public static string GetModel(string path)
        {
            var value = ReadNode(path, "Model");
            if (value == "realesrgan-x4plus" ||
                value == "realesrgan-x4plus-anime" ||
                value == "realesr-animevideov3")
            {
                return value;
            }
            return "realesrgan-x4plus";
        }

        public static string GetExtension(string path)
        {
            var value = ReadNode(path, "Extension");
            if (value == "jpg" || value == "png" || value == "webp")
                return value;
            return "png";
        }

        public static bool GetProcessHidden(string path)
        {
            var value = ReadNode(path, "ProcessHidden");
            if (bool.TryParse(value, out var result))
                return result;
            return false;
        }

        // ---------- 内部工具 ----------

        private static string ReadNode(string path, string key)
        {
            try
            {
                var doc = XDocument.Load(path);
                return doc.Descendants(key).FirstOrDefault()?.Value ?? string.Empty;
            }
            catch (XmlException)
            {
                CreateDefault(path);
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static int ParseInt(string text, int fallback)
        {
            if (int.TryParse(text, out var value) && value > 0)
                return value;
            return fallback;
        }

        private static int ParseLocation(string text, int fallback)
        {
            if (int.TryParse(text, out var value) && value != NoLocation)
                return value;
            return fallback;
        }

        private static void ShowMessage(string message, string title,
                                        System.Windows.MessageBoxImage icon)
        {
            System.Windows.MessageBox.Show(message, title,
                System.Windows.MessageBoxButton.OK, icon);
        }
    }
}