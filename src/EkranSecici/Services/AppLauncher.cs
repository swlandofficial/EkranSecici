using System.Diagnostics;
using System.Text.RegularExpressions;
using EkranSecici.Models;
using static EkranSecici.Native.NativeMethods;

namespace EkranSecici.Services;

/// <summary>Programı başlatır, yeni penceresini bulur, hedef monitöre yerleştirir ve takip eder.</summary>
internal sealed partial class AppLauncher
{
    private readonly Dictionary<Guid, IntPtr> _windows = new();
    private readonly HashSet<Guid> _busy = new();
    private readonly Dictionary<Guid, DateTime> _lastStart = new();

    /// <summary>Kapanınca yeniden başlatma arasındaki en kısa süre.</summary>
    private static readonly TimeSpan RestartCooldown = TimeSpan.FromSeconds(15);

    public bool IsRunning(LaunchProfile profile) =>
        _windows.TryGetValue(profile.Id, out var h) && IsWindow(h);

    /// <summary>Bu oturumda en az bir kez başlatılmış (takip edilen) profil mi?</summary>
    public bool IsTracked(LaunchProfile profile) => _windows.ContainsKey(profile.Id);

    public bool IsBusy(LaunchProfile profile) => _busy.Contains(profile.Id);

    /// <summary>Program açık değilse başlatır; açıksa yalnızca yeniden konumlandırır.</summary>
    /// <returns>Hata yoksa null, varsa kullanıcıya gösterilecek mesaj.</returns>
    public async Task<string?> StartOrPlaceAsync(LaunchProfile profile)
    {
        if (!_busy.Add(profile.Id)) return null;
        try
        {
            if (IsRunning(profile))
            {
                Place(profile, _windows[profile.Id]);
                return null;
            }

            if (string.IsNullOrWhiteSpace(profile.ExePath)) return $"'{profile.Name}': Program yolu boş.";

            var processName = profile.EffectiveProcessName;
            var before = WindowFinder.GetTopLevelWindows().Select(w => w.Handle).ToHashSet();

            int pid;
            try
            {
                pid = Start(profile);
            }
            catch (Exception ex)
            {
                Log.Error($"'{profile.Name}' başlatılamadı", ex);
                return $"'{profile.Name}' başlatılamadı:\n{ex.Message}";
            }
            _lastStart[profile.Id] = DateTime.Now;
            Log.Info($"'{profile.Name}' başlatıldı (PID {pid}), '{processName}' penceresi bekleniyor");

            var hwnd = IntPtr.Zero;
            var deadline = DateTime.Now.AddSeconds(Math.Max(5, profile.WindowWaitSeconds));
            while (DateTime.Now < deadline)
            {
                await Task.Delay(300);
                hwnd = FindNewWindow(profile, processName, pid, before);
                if (hwnd != IntPtr.Zero) break;
            }

            // Program zaten açıktı ve yeni pencere açmadıysa: başlık filtresi varsa mevcut pencereyi kullan.
            if (hwnd == IntPtr.Zero && !string.IsNullOrWhiteSpace(profile.WindowTitleContains))
            {
                hwnd = FindExistingWindow(profile, processName);
            }

            if (hwnd == IntPtr.Zero)
            {
                Log.Warn($"'{profile.Name}' penceresi {profile.WindowWaitSeconds} sn içinde bulunamadı");
                return $"'{profile.Name}' başlatıldı ancak penceresi bulunamadı.\n" +
                       "İşlem adı / pencere başlığı alanlarını kontrol edin.";
            }

            _windows[profile.Id] = hwnd;
            Log.Info($"'{profile.Name}' penceresi bulundu: \"{WindowFinder.GetTitle(hwnd)}\"");

            // Pencere ilk çizimini bitirsin; bazı programlar açıldıktan sonra kayıtlı konumuna döner, tekrar uygula.
            await Task.Delay(800);
            Place(profile, hwnd);
            await Task.Delay(2000);
            Place(profile, hwnd);
            if (profile.AlwaysOnTop) WindowPlacer.BringToFront(hwnd, topMost: true);
            return null;
        }
        finally
        {
            _busy.Remove(profile.Id);
        }
    }

    /// <summary>Takip edilen pencere doğru yerde değilse yerleştirir (bekçi ve ekran değişikliği için).</summary>
    public void EnsurePlaced(LaunchProfile profile)
    {
        if (_busy.Contains(profile.Id)) return;
        if (_windows.TryGetValue(profile.Id, out var hwnd) && IsWindow(hwnd)) Place(profile, hwnd);
    }

    /// <summary>"Her zaman üstte" profillerinde pencereyi en üst katmanda tutar (konumuna dokunmaz).</summary>
    public void EnsureTopMost(LaunchProfile profile)
    {
        if (_busy.Contains(profile.Id)) return;
        if (_windows.TryGetValue(profile.Id, out var hwnd) && IsWindow(hwnd)) WindowPlacer.SetTopMost(hwnd);
    }

    /// <summary>Pencere kapandıysa ve bekleme süresi dolduysa yeniden başlatılmalı mı?</summary>
    public bool ShouldRestart(LaunchProfile profile) =>
        IsTracked(profile) && !IsRunning(profile) && !IsBusy(profile) &&
        (!_lastStart.TryGetValue(profile.Id, out var t) || DateTime.Now - t > RestartCooldown);

    public void Forget(Guid profileId)
    {
        _windows.Remove(profileId);
        _lastStart.Remove(profileId);
    }

    private static void Place(LaunchProfile profile, IntPtr hwnd)
    {
        try
        {
            var monitor = MonitorService.Resolve(profile);
            if (monitor is null) WindowPlacer.PlaceFallback(hwnd);
            else WindowPlacer.EnsurePlaced(hwnd, monitor, profile);
        }
        catch (Exception ex)
        {
            Log.Error($"'{profile.Name}' yerleştirilemedi", ex);
        }
    }

    private static int Start(LaunchProfile profile)
    {
        var exe = Environment.ExpandEnvironmentVariables(profile.ExePath.Trim().Trim('"'));
        var args = Environment.ExpandEnvironmentVariables(profile.Arguments ?? "");
        var workDir = string.IsNullOrWhiteSpace(profile.WorkingDirectory)
            ? (Path.IsPathRooted(exe) ? Path.GetDirectoryName(exe) ?? "" : "")
            : Environment.ExpandEnvironmentVariables(profile.WorkingDirectory);

        EnsureFirefoxProfileDirectory(args);

        var psi = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = args,
            WorkingDirectory = workDir,
            UseShellExecute = true,
        };
        using var process = Process.Start(psi);
        return process?.Id ?? -1;
    }

    /// <summary>Firefox -profile "klasör" parametresindeki klasör yoksa oluşturur.</summary>
    private static void EnsureFirefoxProfileDirectory(string args)
    {
        var m = ProfileArgRegex().Match(args);
        if (!m.Success) return;
        try
        {
            Directory.CreateDirectory(m.Groups[1].Value);
        }
        catch (Exception ex)
        {
            Log.Warn($"Firefox profil klasörü oluşturulamadı: {ex.Message}");
        }
    }

    [GeneratedRegex("-profile\\s+\"([^\"]+)\"", RegexOptions.IgnoreCase)]
    private static partial Regex ProfileArgRegex();

    private static IntPtr FindNewWindow(LaunchProfile profile, string processName, int pid, HashSet<IntPtr> before)
    {
        return WindowFinder.GetTopLevelWindows()
            .Where(w => !before.Contains(w.Handle))
            .Where(w => w.ProcessId == pid || string.Equals(w.ProcessName, processName, StringComparison.OrdinalIgnoreCase))
            .Where(w => TitleMatches(profile, w.Title))
            .Select(w => w.Handle)
            .FirstOrDefault();
    }

    private static IntPtr FindExistingWindow(LaunchProfile profile, string processName)
    {
        return WindowFinder.GetTopLevelWindows()
            .Where(w => string.Equals(w.ProcessName, processName, StringComparison.OrdinalIgnoreCase))
            .Where(w => TitleMatches(profile, w.Title))
            .Select(w => w.Handle)
            .FirstOrDefault();
    }

    private static bool TitleMatches(LaunchProfile profile, string title) =>
        string.IsNullOrWhiteSpace(profile.WindowTitleContains) ||
        title.Contains(profile.WindowTitleContains.Trim(), StringComparison.OrdinalIgnoreCase);
}
