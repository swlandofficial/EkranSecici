using System.Text;

namespace EkranSecici.Services;

/// <summary>Basit günlük dosyası: %APPDATA%\EkranSecici\logs\log-YYYYMMDD.txt</summary>
internal static class Log
{
    private static readonly object Sync = new();
    private const int KeepDays = 7;

    public static void Info(string message) => Write("BİLGİ", message);
    public static void Warn(string message) => Write("UYARI", message);
    public static void Error(string message, Exception? ex = null) =>
        Write("HATA", ex is null ? message : $"{message}: {ex}");

    private static void Write(string level, string message)
    {
        try
        {
            lock (Sync)
            {
                Directory.CreateDirectory(AppInfo.LogDirectory);
                var file = Path.Combine(AppInfo.LogDirectory, $"log-{DateTime.Now:yyyyMMdd}.txt");
                File.AppendAllText(file, $"{DateTime.Now:HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}", Encoding.UTF8);
            }
        }
        catch
        {
            // Günlük yazılamıyorsa uygulamayı durdurma.
        }
    }

    public static void Cleanup()
    {
        try
        {
            if (!Directory.Exists(AppInfo.LogDirectory)) return;
            foreach (var f in Directory.GetFiles(AppInfo.LogDirectory, "log-*.txt"))
            {
                if (File.GetLastWriteTime(f) < DateTime.Now.AddDays(-KeepDays)) File.Delete(f);
            }
        }
        catch
        {
        }
    }
}
