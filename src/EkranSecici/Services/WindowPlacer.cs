using EkranSecici.Models;
using static EkranSecici.Native.NativeMethods;

namespace EkranSecici.Services;

/// <summary>
/// Pencereyi hedef monitöre taşır ve pencere modunu uygular.
/// EnsurePlaced idempotenttir: bekçi her birkaç saniyede bir çağırır, doğru yerdeyse hiçbir şey yapmaz.
/// </summary>
internal static class WindowPlacer
{
    public static void EnsurePlaced(IntPtr hwnd, MonitorInfo monitor, LaunchProfile profile)
    {
        if (!IsWindow(hwnd)) return;

        var onTarget = IsOnMonitor(hwnd, monitor);
        switch (profile.WindowMode)
        {
            case WindowMode.Normal:
                if (onTarget && !IsIconic(hwnd) && !IsZoomed(hwnd)) break;
                MoveNormal(hwnd, monitor);
                break;

            case WindowMode.Maximized:
                if (onTarget && IsZoomed(hwnd)) break;
                MoveNormal(hwnd, monitor);
                ShowWindow(hwnd, SW_MAXIMIZE);
                break;

            case WindowMode.Fullscreen:
                if (CoversExactly(hwnd, monitor.Bounds)) break;
                if (profile.SendF11) FullscreenWithF11(hwnd, monitor);
                else BorderlessFullscreen(hwnd, monitor);
                break;
        }

        if (profile.AlwaysOnTop && (GetWindowLong(hwnd, GWL_EXSTYLE) & WS_EX_TOPMOST) == 0)
        {
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }
    }

    /// <summary>Hedef monitör yokken: personelin ekranını kapatmamak için ana monitörde normal pencere.</summary>
    public static void PlaceFallback(IntPtr hwnd)
    {
        if (!IsWindow(hwnd)) return;
        MoveNormal(hwnd, MonitorService.Primary);
    }

    public static bool IsOnMonitor(IntPtr hwnd, MonitorInfo monitor)
    {
        var center = new POINT(monitor.Bounds.Left + monitor.Bounds.Width / 2, monitor.Bounds.Top + monitor.Bounds.Height / 2);
        return MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST) == MonitorFromPoint(center, MONITOR_DEFAULTTONEAREST);
    }

    private static bool CoversExactly(IntPtr hwnd, Rectangle bounds) =>
        !IsIconic(hwnd) && GetWindowRect(hwnd, out var r) && r.ToRectangle() == bounds;

    private static bool CoversAnyMonitor(IntPtr hwnd) =>
        MonitorService.GetMonitors().Any(m => CoversExactly(hwnd, m.Bounds));

    private static void MoveNormal(IntPtr hwnd, MonitorInfo monitor)
    {
        if (IsIconic(hwnd) || IsZoomed(hwnd)) ShowWindow(hwnd, SW_RESTORE);

        var work = monitor.WorkingArea;
        GetWindowRect(hwnd, out var r);
        int w = r.Right - r.Left, h = r.Bottom - r.Top;
        if (w <= 0 || h <= 0 || w > work.Width || h > work.Height)
        {
            w = work.Width * 8 / 10;
            h = work.Height * 8 / 10;
        }

        int x = work.Left + (work.Width - w) / 2;
        int y = work.Top + (work.Height - h) / 2;

        // İkinci çağrı: farklı DPI'lı monitöre geçişte pencere kendini yeniden ölçekler, boyutu düzelt.
        for (var i = 0; i < 2; i++)
        {
            SetWindowPos(hwnd, IntPtr.Zero, x, y, w, h, SWP_NOZORDER | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
    }

    private static void BorderlessFullscreen(IntPtr hwnd, MonitorInfo monitor)
    {
        if (IsIconic(hwnd) || IsZoomed(hwnd)) ShowWindow(hwnd, SW_RESTORE);

        var style = GetWindowLong(hwnd, GWL_STYLE);
        style &= ~(WS_CAPTION | WS_THICKFRAME | WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        SetWindowLong(hwnd, GWL_STYLE, style);

        var ex = GetWindowLong(hwnd, GWL_EXSTYLE);
        ex &= ~(WS_EX_DLGMODALFRAME | WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE);
        SetWindowLong(hwnd, GWL_EXSTYLE, ex);

        var b = monitor.Bounds;
        for (var i = 0; i < 2; i++)
        {
            SetWindowPos(hwnd, IntPtr.Zero, b.Left, b.Top, b.Width, b.Height,
                SWP_FRAMECHANGED | SWP_NOZORDER | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
    }

    private static void FullscreenWithF11(IntPtr hwnd, MonitorInfo monitor)
    {
        var previous = GetForegroundWindow();

        // Başka bir monitörde tam ekransa önce tam ekrandan çık, yoksa F11 ters etki yapar.
        if (CoversAnyMonitor(hwnd))
        {
            ForceForeground(hwnd);
            PressKey(VK_F11);
            Thread.Sleep(400);
        }

        MoveNormal(hwnd, monitor);
        Thread.Sleep(200);
        ForceForeground(hwnd);
        Thread.Sleep(150);
        PressKey(VK_F11);
        Thread.Sleep(300);

        // Odağı personelin çalıştığı pencereye iade et.
        if (previous != IntPtr.Zero && previous != hwnd && IsWindow(previous)) ForceForeground(previous);
    }

    public static void ForceForeground(IntPtr hwnd)
    {
        var fg = GetForegroundWindow();
        var fgThread = GetWindowThreadProcessId(fg, out _);
        var current = GetCurrentThreadId();
        var attached = fgThread != 0 && fgThread != current && AttachThreadInput(current, fgThread, true);
        try
        {
            // Alt tuşu hilesi: Windows'un odak çalma kısıtlamasını aşar.
            keybd_event(VK_MENU, 0, 0, UIntPtr.Zero);
            keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            if (IsIconic(hwnd)) ShowWindow(hwnd, SW_RESTORE);
            SetForegroundWindow(hwnd);
            BringWindowToTop(hwnd);
        }
        finally
        {
            if (attached) AttachThreadInput(current, fgThread, false);
        }
    }

    private static void PressKey(byte vk)
    {
        keybd_event(vk, 0, 0, UIntPtr.Zero);
        keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }
}
