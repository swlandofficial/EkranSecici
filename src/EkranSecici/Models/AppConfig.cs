namespace EkranSecici.Models;

public sealed class AppConfig
{
    /// <summary>Ekran Seçici'yi Windows oturum açılışında başlat (HKCU Run).</summary>
    public bool StartWithWindows { get; set; } = true;
    /// <summary>Açılışta profilleri başlatmadan önce bekleme (ağ, masaüstü hazır olsun).</summary>
    public int StartupDelaySeconds { get; set; } = 10;
    /// <summary>Hedef monitör bağlı değilse en fazla bu kadar bekle.</summary>
    public int MonitorWaitSeconds { get; set; } = 60;
    public List<LaunchProfile> Profiles { get; set; } = new();
}
