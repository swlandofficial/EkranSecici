using System.Reflection;

namespace EkranSecici;

internal static class AppInfo
{
    public const string Name = "Ekran Seçici";
    public const string Id = "EkranSecici";

    public static string Version { get; } =
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    /// <summary>%APPDATA%\EkranSecici — kurulum ve ayarlar burada (yetki gerektirmez).</summary>
    public static string DataDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Id);

    public static string ConfigPath => Path.Combine(DataDirectory, "config.json");

    public static string LogDirectory => Path.Combine(DataDirectory, "logs");

    public static string ExecutablePath => Environment.ProcessPath ?? Application.ExecutablePath;
}
