using System;
using System.IO;
using System.Reflection;

namespace Real_ESRGAN_GUI_WPF
{
    /// <summary>
    /// 集中存放路径常量和一次运行期间的全局状态。
    /// </summary>
    public static class Parameters
    {
        public static readonly string WorkPath =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static readonly string ExtractPath =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static readonly string XmlPath =
            Path.Combine(WorkPath, "Real_ESRGAN_GUI_WPF.xml");

        public static readonly string RealesrganFolderPath =
            Path.Combine(ExtractPath, "Real_ESRGAN_GUI");

        public static readonly string RealesrganPath =
            Path.Combine(RealesrganFolderPath, "realesrgan.exe");

        public static readonly string Vcomp140Path =
            Path.Combine(RealesrganFolderPath, "vcomp140.dll");

        public static readonly string Vcomp140dPath =
            Path.Combine(RealesrganFolderPath, "vcomp140d.dll");

        public static readonly string ModelsPath =
            Path.Combine(RealesrganFolderPath, "models");

        public static readonly string RealesrAnimeV3X2Bin =
            Path.Combine(ModelsPath, "realesr-animevideov3-x2.bin");
        public static readonly string RealesrAnimeV3X2Param =
            Path.Combine(ModelsPath, "realesr-animevideov3-x2.param");
        public static readonly string RealesrAnimeV3X3Bin =
            Path.Combine(ModelsPath, "realesr-animevideov3-x3.bin");
        public static readonly string RealesrAnimeV3X3Param =
            Path.Combine(ModelsPath, "realesr-animevideov3-x3.param");
        public static readonly string RealesrAnimeV3X4Bin =
            Path.Combine(ModelsPath, "realesr-animevideov3-x4.bin");
        public static readonly string RealesrAnimeV3X4Param =
            Path.Combine(ModelsPath, "realesr-animevideov3-x4.param");
        public static readonly string RealesrganX4PlusBin =
            Path.Combine(ModelsPath, "realesrgan-x4plus.bin");
        public static readonly string RealesrganX4PlusParam =
            Path.Combine(ModelsPath, "realesrgan-x4plus.param");
        public static readonly string RealesrganX4PlusAnimeBin =
            Path.Combine(ModelsPath, "realesrgan-x4plus-anime.bin");
        public static readonly string RealesrganX4PlusAnimeParam =
            Path.Combine(ModelsPath, "realesrgan-x4plus-anime.param");

        // 当前会话状态
        public static string CurrentLanguage { get; set; } = "en-US";
        public static string Scale { get; set; } = "4";
        public static string Model { get; set; } = "realesrgan-x4plus";
        public static string Extension { get; set; } = "png";
        public static bool ProcessHidden { get; set; }
        public static bool HasPermission { get; set; }
        public static bool IsMultipleFiles { get; set; }
        public static bool IsCreatedNewFolder { get; set; }

        // 上次运行时的屏幕信息，用来判断是否应该恢复窗口位置
        public static int OldScreenWidth { get; set; }
        public static int OldScreenHeight { get; set; }

        // 单次处理上下文
        public static string FilePath { get; set; }
        public static string DirectoryPath { get; set; }
        public static string FileName { get; set; }
    }
}