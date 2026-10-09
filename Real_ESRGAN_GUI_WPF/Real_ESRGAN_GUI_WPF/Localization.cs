using System.Collections.Generic;
using System.Globalization;

namespace Real_ESRGAN_GUI_WPF
{
    /// <summary>
    /// 界面文本。直接放在代码里，方便维护和查找。
    /// </summary>
    public static class Localization
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Texts =
            new Dictionary<string, Dictionary<string, string>>
        {
            {
                "zh-CN", new Dictionary<string, string>
                {
                    { "FileMenu",       "文件" },
                    { "FileOpen",       "打开" },
                    { "FileExit",       "退出" },
                    { "LanguageMenu",   "语言" },
                    { "LanguageSelect", "选择语言" },
                    { "AboutMenu",      "关于" },
                    { "AboutItem",      "Real ESRGAN GUI" },
                    { "Scale",          "放大倍数" },
                    { "Model",          "放大算法" },
                    { "Extension",      "生成格式" },
                    { "HideProcess",    "后台运行" },
                    { "SaveConfig",     "保存配置" },
                    { "AboutTitle",     "关于 Real ESRGAN GUI" },
                    { "GithubPage",     "Github主页" },
                    { "License",        "许可证" },
                    { "Confirm",        "确定" },

                    { "NoticeTitle",    "提示" },
                    { "ErrorTitle",     "错误" },
                    { "ConfigSaved",    "配置已保存。" },
                    { "UnsupportedFile","不支持的文件格式。请提供 JPG、PNG 或 WEBP 文件。" },
                    { "PermissionDenied","应用程序没有在 {0} 内运行的权限，错误为: {1}" },
                    { "ComponentsMissing","程序工作路径下 Real ESRGAN 组件不完整，无法启动处理流程。" },
                    { "ResourceMissing","没有找到资源: {0}" },
                    { "FolderCreateFailed","创建目标文件夹时出错，错误为: {0}" },
                    { "GenericError",   "出现错误: {0}" },
                }
            },
            {
                "zh-TW", new Dictionary<string, string>
                {
                    { "FileMenu",       "文件" },
                    { "FileOpen",       "打開" },
                    { "FileExit",       "退出" },
                    { "LanguageMenu",   "語言" },
                    { "LanguageSelect", "選擇語言" },
                    { "AboutMenu",      "關於" },
                    { "AboutItem",      "Real ESRGAN GUI" },
                    { "Scale",          "放大倍數" },
                    { "Model",          "放大算法" },
                    { "Extension",      "生成格式" },
                    { "HideProcess",    "後臺運行" },
                    { "SaveConfig",     "保存配置" },
                    { "AboutTitle",     "關於 Real ESRGAN GUI" },
                    { "GithubPage",     "Github主頁" },
                    { "License",        "許可證" },
                    { "Confirm",        "確定" },

                    { "NoticeTitle",    "提示" },
                    { "ErrorTitle",     "錯誤" },
                    { "ConfigSaved",    "配置已保存。" },
                    { "UnsupportedFile","不支持的文件格式。請提供 JPG、PNG 或 WEBP 文件。" },
                    { "PermissionDenied","應用程式沒有在 {0} 内運行的權限，錯誤為: {1}" },
                    { "ComponentsMissing","程式工作路徑下 Real ESRGAN 組件不完整，無法啓動處理流程。" },
                    { "ResourceMissing","沒有找到資源: {0}" },
                    { "FolderCreateFailed","創建目標文件夾時出錯，錯誤為: {0}" },
                    { "GenericError",   "出現錯誤: {0}" },
                }
            },
            {
                "en-US", new Dictionary<string, string>
                {
                    { "FileMenu",       "File" },
                    { "FileOpen",       "Open" },
                    { "FileExit",       "Exit" },
                    { "LanguageMenu",   "Language" },
                    { "LanguageSelect", "Select Language" },
                    { "AboutMenu",      "About" },
                    { "AboutItem",      "Real ESRGAN GUI" },
                    { "Scale",          "Scale" },
                    { "Model",          "Model" },
                    { "Extension",      "Extension" },
                    { "HideProcess",    "Hide Process" },
                    { "SaveConfig",     "Save Config" },
                    { "AboutTitle",     "About Real ESRGAN GUI" },
                    { "GithubPage",     "Github Page" },
                    { "License",        "License" },
                    { "Confirm",        "OK" },

                    { "NoticeTitle",    "Notice" },
                    { "ErrorTitle",     "Error" },
                    { "ConfigSaved",    "Configuration Saved." },
                    { "UnsupportedFile","Unsupported file format. Please provide JPG, PNG, or WEBP files." },
                    { "PermissionDenied","Permission denied to run the application in {0}, error message is: {1}" },
                    { "ComponentsMissing","The Real ESRGAN components in the working directory are incomplete." },
                    { "ResourceMissing","Resource not found: {0}" },
                    { "FolderCreateFailed","Failed to create target folder: {0}" },
                    { "GenericError",   "An error occurred: {0}" },
                }
            },
        };

        private static readonly HashSet<string> SupportedLanguages = new HashSet<string>
        {
            "zh-CN", "zh-TW", "en-US"
        };

        /// <summary>根据当前语言取字符串，找不到就退回英文。</summary>
        public static string Get(string key)
        {
            if (Texts.TryGetValue(Parameters.CurrentLanguage, out var dict) &&
                dict.TryGetValue(key, out var value))
            {
                return value;
            }

            if (Texts["en-US"].TryGetValue(key, out var fallback))
            {
                return fallback;
            }

            return key;
        }

        /// <summary>判断系统语言是否被支持，不支持时返回英文。</summary>
        public static string DetectDefaultLanguage()
        {
            var name = CultureInfo.CurrentUICulture.Name;
            return SupportedLanguages.Contains(name) ? name : "en-US";
        }

        public static bool IsSupported(string language)
        {
            return !string.IsNullOrEmpty(language) && SupportedLanguages.Contains(language);
        }
    }
}