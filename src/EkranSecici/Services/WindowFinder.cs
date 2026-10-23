using System.Diagnostics;
using System.Text;
using static EkranSecici.Native.NativeMethods;

namespace EkranSecici.Services;

internal sealed record WindowInfo(IntPtr Handle, int ProcessId, string ProcessName, string Title);

/// <summary>Görünür, sahipsiz üst seviye pencereleri listeler (Alt+Tab'da görünenlere yakın).</summary>
internal static class WindowFinder
{
    public static List<WindowInfo> GetTopLevelWindows()
    {
        var result = new List<WindowInfo>();
        var names = new Dictionary<uint, string>();

        EnumWindows((hWnd, _) =>
        {
            if (!IsCandidate(hWnd)) return true;

            GetWindowThreadProcessId(hWnd, out var pid);
            if (!names.TryGetValue(pid, out var name))
            {
                try
                {
                    using var p = Process.GetProcessById((int)pid);
                    name = p.ProcessName;
                }
                catch
                {
                    name = "";
                }
                names[pid] = name;
            }

            result.Add(new WindowInfo(hWnd, (int)pid, name, GetTitle(hWnd)));
            return true;
        }, IntPtr.Zero);

        return result;
    }

    private static bool IsCandidate(IntPtr hWnd)
    {
        if (!IsWindowVisible(hWnd)) return false;
        if (GetWindow(hWnd, GW_OWNER) != IntPtr.Zero) return false;
        if ((GetWindowLong(hWnd, GWL_EXSTYLE) & WS_EX_TOOLWINDOW) != 0) return false;
        if (DwmGetWindowAttribute(hWnd, DWMWA_CLOAKED, out var cloaked, sizeof(int)) == 0 && cloaked != 0) return false;
        if (!GetWindowRect(hWnd, out var r) || r.Right - r.Left < 50 || r.Bottom - r.Top < 50) return false;
        return true;
    }

    public static string GetTitle(IntPtr hWnd)
    {
        var len = GetWindowTextLength(hWnd);
        if (len <= 0) return "";
        var sb = new StringBuilder(len + 1);
        GetWindowText(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }
}
