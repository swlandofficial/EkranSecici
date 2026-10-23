using System.Text.Json.Serialization;

namespace EkranSecici.Models;

public enum WindowMode
{
    /// <summary>Normal pencere, hedef monitörde ortalanır.</summary>
    Normal,
    /// <summary>Ekranı kapla (maximize).</summary>
    Maximized,
    /// <summary>Tam ekran (kenarlıksız veya F11).</summary>
    Fullscreen,
}

/// <summary>Bir programın hangi monitörde ve nasıl açılacağını tanımlar.</summary>
public sealed class LaunchProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    /// <summary>Pasif profil hiçbir şekilde başlatılmaz/izlenmez; silmeden devre dışı bırakmak için.</summary>
    public bool Enabled { get; set; } = true;
    public string ExePath { get; set; } = "";
    public string Arguments { get; set; } = "";
    public string WorkingDirectory { get; set; } = "";

    /// <summary>Windows aygıt adı, ör. \\.\DISPLAY2. Önce buna göre eşleştirilir.</summary>
    public string MonitorDeviceName { get; set; } = "";
    /// <summary>1 = ana monitör, sonrakiler soldan sağa. Aygıt adı değişirse yedek eşleştirme.</summary>
    public int MonitorIndex { get; set; } = 2;

    public WindowMode WindowMode { get; set; } = WindowMode.Fullscreen;
    /// <summary>Tam ekran için kenarlıksız yöntem yerine F11 tuşu gönder (tarayıcılar).</summary>
    public bool SendF11 { get; set; }
    public bool AlwaysOnTop { get; set; }

    /// <summary>Windows açılışında otomatik başlat.</summary>
    public bool AutoStart { get; set; } = true;
    /// <summary>Pencere başka monitöre taşınırsa geri getir.</summary>
    public bool KeepOnMonitor { get; set; } = true;
    /// <summary>Pencere kapanırsa programı yeniden başlat.</summary>
    public bool RestartIfClosed { get; set; }

    /// <summary>
    /// Program Ekran Seçici dışında (elle, kısayoldan) açılsa da penceresini yakalayıp hedef monitöre taşı.
    /// Belirtilmemişse (null, ör. eski ayar dosyaları): Firefox şablonu gibi ayrı profil (-no-remote) kullananlar hariç açık
    /// sayılır, böylece personelin kendi tarayıcı pencereleri taşınmaz.
    /// </summary>
    public bool? FollowExternalWindows { get; set; }

    [JsonIgnore]
    public bool FollowsExternalWindows =>
        FollowExternalWindows ?? !Arguments.Contains("-no-remote", StringComparison.OrdinalIgnoreCase);

    /// <summary>Pencere sahibinin işlem adı (boşsa exe adından türetilir), ör. firefox.</summary>
    public string ProcessName { get; set; } = "";
    /// <summary>İsteğe bağlı: pencere başlığı bu metni içermeli.</summary>
    public string WindowTitleContains { get; set; } = "";
    public int WindowWaitSeconds { get; set; } = 30;

    public LaunchProfile Clone() => (LaunchProfile)MemberwiseClone();

    public string EffectiveProcessName =>
        !string.IsNullOrWhiteSpace(ProcessName)
            ? ProcessName.Trim().Replace(".exe", "", StringComparison.OrdinalIgnoreCase)
            : Path.GetFileNameWithoutExtension(Environment.ExpandEnvironmentVariables(ExePath.Trim().Trim('"')));

    public static string ModeText(WindowMode mode) => mode switch
    {
        WindowMode.Normal => "Normal pencere",
        WindowMode.Maximized => "Ekranı kapla",
        WindowMode.Fullscreen => "Tam ekran",
        _ => mode.ToString(),
    };
}
