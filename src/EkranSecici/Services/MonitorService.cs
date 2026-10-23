using EkranSecici.Models;

namespace EkranSecici.Services;

public sealed record MonitorInfo(int Index, string DeviceName, Rectangle Bounds, Rectangle WorkingArea, bool Primary)
{
    public string Label =>
        $"Monitör {Index}{(Primary ? " (Ana)" : "")} — {Bounds.Width}x{Bounds.Height} @ {Bounds.X},{Bounds.Y}  [{DeviceName}]";

    public override string ToString() => Label;
}

internal static class MonitorService
{
    /// <summary>Monitörler: 1 = ana monitör, ardından soldan sağa, yukarıdan aşağıya.</summary>
    public static IReadOnlyList<MonitorInfo> GetMonitors()
    {
        return Screen.AllScreens
            .OrderByDescending(s => s.Primary)
            .ThenBy(s => s.Bounds.X)
            .ThenBy(s => s.Bounds.Y)
            .Select((s, i) => new MonitorInfo(i + 1, s.DeviceName, s.Bounds, s.WorkingArea, s.Primary))
            .ToList();
    }

    public static MonitorInfo Primary => GetMonitors()[0];

    /// <summary>Profilin hedef monitörünü bulur; bağlı değilse null.</summary>
    public static MonitorInfo? Resolve(LaunchProfile profile)
    {
        var monitors = GetMonitors();
        if (!string.IsNullOrWhiteSpace(profile.MonitorDeviceName))
        {
            var byName = monitors.FirstOrDefault(m =>
                string.Equals(m.DeviceName, profile.MonitorDeviceName, StringComparison.OrdinalIgnoreCase));
            if (byName is not null) return byName;
        }

        return profile.MonitorIndex >= 1 && profile.MonitorIndex <= monitors.Count
            ? monitors[profile.MonitorIndex - 1]
            : null;
    }

    public static string Describe(LaunchProfile profile)
    {
        var m = Resolve(profile);
        return m is null ? $"Monitör {profile.MonitorIndex} (bağlı değil)" : $"Monitör {m.Index}{(m.Primary ? " (Ana)" : "")}";
    }
}
