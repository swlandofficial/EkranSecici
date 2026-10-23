using System.Text.Json;
using System.Text.Json.Serialization;
using EkranSecici.Models;

namespace EkranSecici.Services;

/// <summary>config.json okuma/yazma.</summary>
internal sealed class ConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public AppConfig Config { get; private set; } = new();

    /// <summary>Ayar dosyası ilk kez oluşturuluyorsa true.</summary>
    public bool IsFirstRun { get; private set; }

    public string FilePath => AppInfo.ConfigPath;

    public event EventHandler? Changed;

    public void Load()
    {
        if (!File.Exists(FilePath))
        {
            IsFirstRun = true;
            Config = new AppConfig();
            return;
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            Config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
            Config.Profiles ??= new List<LaunchProfile>();
        }
        catch (Exception ex)
        {
            Log.Error("Ayar dosyası okunamadı, yedeklenip sıfırlanıyor", ex);
            try
            {
                File.Copy(FilePath, Path.Combine(AppInfo.DataDirectory, $"config.bozuk-{DateTime.Now:yyyyMMddHHmmss}.json"), true);
            }
            catch
            {
            }
            Config = new AppConfig();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(AppInfo.DataDirectory);
        var tmp = FilePath + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(Config, JsonOptions));
        File.Move(tmp, FilePath, overwrite: true);
        IsFirstRun = false;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
