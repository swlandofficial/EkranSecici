using Microsoft.Win32;

namespace EkranSecici.Services;

/// <summary>HKCU Run kaydı: yönetici yetkisi gerektirmez.</summary>
internal static class AutoStartService
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    public const string AutoStartArgument = "--autostart";

    private static string Command => $"\"{AppInfo.ExecutablePath}\" {AutoStartArgument}";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey);
        return key?.GetValue(AppInfo.Id) is string;
    }

    public static void Set(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKey, writable: true);
            if (enabled)
            {
                // Exe taşındıysa yolu güncelle.
                if (key.GetValue(AppInfo.Id) as string != Command) key.SetValue(AppInfo.Id, Command);
            }
            else if (key.GetValue(AppInfo.Id) is not null)
            {
                key.DeleteValue(AppInfo.Id, throwOnMissingValue: false);
            }
        }
        catch (Exception ex)
        {
            Log.Error("Otomatik başlatma kaydı güncellenemedi", ex);
        }
    }
}
