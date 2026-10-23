using EkranSecici.Services;
using EkranSecici.UI;

namespace EkranSecici;

internal static class Program
{
    private const string MutexName = "EkranSecici_SingleInstance_v1";
    private const string ShowEventName = "EkranSecici_Show_v1";
    private const string ExitEventName = "EkranSecici_Exit_v1";

    [STAThread]
    private static void Main(string[] args)
    {
        var autostart = args.Any(a => a.Equals(AutoStartService.AutoStartArgument, StringComparison.OrdinalIgnoreCase));
        var exit = args.Any(a => a.Equals("--exit", StringComparison.OrdinalIgnoreCase));

        using var mutex = new Mutex(true, MutexName, out var isFirstInstance);
        if (!isFirstInstance)
        {
            // Zaten çalışıyor: ilk örneğe göster/çık sinyali gönder.
            if (!autostart) Signal(exit ? ExitEventName : ShowEventName);
            return;
        }
        if (exit) return;

        using var showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);
        using var exitEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ExitEventName);

        ApplicationConfiguration.Initialize();
        Application.ThreadException += (_, e) => Log.Error("Beklenmeyen hata (UI)", e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log.Error("Beklenmeyen hata", e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Error("Gözlenmeyen görev hatası", e.Exception);
            e.SetObserved();
        };

        using var context = new TrayApplicationContext(autostart);
        var showWait = ThreadPool.RegisterWaitForSingleObject(showEvent, (_, _) => context.RequestShow(), null, Timeout.Infinite, false);
        var exitWait = ThreadPool.RegisterWaitForSingleObject(exitEvent, (_, _) => context.RequestExit(), null, Timeout.Infinite, true);

        Application.Run(context);

        showWait.Unregister(null);
        exitWait.Unregister(null);
        GC.KeepAlive(mutex);
    }

    private static void Signal(string name)
    {
        try
        {
            if (EventWaitHandle.TryOpenExisting(name, out var handle))
            {
                using (handle) handle.Set();
            }
        }
        catch (Exception ex)
        {
            Log.Error("Çalışan örneğe sinyal gönderilemedi", ex);
        }
    }
}
