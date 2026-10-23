using EkranSecici.Models;
using EkranSecici.Services;
using Microsoft.Win32;

namespace EkranSecici.UI;

/// <summary>
/// Uygulamanın ömrü boyunca yaşayan tepsi (sağ alt köşe) bağlamı:
/// tepsi simgesi ve menüsü, açılışta otomatik başlatma, bekçi zamanlayıcısı.
/// </summary>
internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly ConfigStore _store = new();
    private readonly AppLauncher _launcher = new();
    private readonly NotifyIcon _tray;
    private readonly MainForm _form;
    private readonly ToolStripMenuItem _profilesMenu = new("Profili başlat / konumla");
    private readonly System.Windows.Forms.Timer _watchdog = new() { Interval = 5000 };
    private readonly System.Windows.Forms.Timer _displayChangeDebounce = new() { Interval = 3000 };
    private bool _startupRunning;

    public TrayApplicationContext(bool autostart)
    {
        Log.Cleanup();
        _store.Load();
        if (_store.IsFirstRun)
        {
            // İlk çalıştırma: kurulu sürümde kurulumdaki "otomatik başlat" seçimine uy,
            // taşınabilir (kurulumsuz) kullanımda varsayılan açık.
            var installed = AppInfo.ExecutablePath.StartsWith(AppInfo.DataDirectory, StringComparison.OrdinalIgnoreCase);
            _store.Config.StartWithWindows = !installed || AutoStartService.IsEnabled();
            TrySave();
        }
        AutoStartService.Set(_store.Config.StartWithWindows);

        _form = new MainForm(_store, _launcher);
        _ = _form.Handle; // BeginInvoke için tutamaç oluştur (pencere gösterilmez)

        _tray = new NotifyIcon
        {
            Icon = AppIcons.Load(SystemInformation.SmallIconSize),
            Text = AppInfo.Name,
            Visible = true,
            ContextMenuStrip = BuildMenu(),
        };
        _tray.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left) ShowMainForm();
        };

        _watchdog.Tick += (_, _) => WatchdogTick();
        _watchdog.Start();

        _displayChangeDebounce.Tick += (_, _) =>
        {
            _displayChangeDebounce.Stop();
            Log.Info("Ekran yapılandırması değişti, pencereler yeniden yerleştiriliyor");
            foreach (var p in _store.Config.Profiles) _launcher.EnsurePlaced(p);
        };
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;

        Log.Info($"{AppInfo.Name} {AppInfo.Version} başladı (autostart={autostart}, monitör sayısı={Screen.AllScreens.Length})");

        if (autostart) _ = RunStartupAsync();
        else ShowMainForm();
    }

    /// <summary>Başka bir örnek "göster" isteği gönderdiğinde çağrılır (herhangi bir iş parçacığından).</summary>
    public void RequestShow() => _form.BeginInvoke(ShowMainForm);

    /// <summary>Kurulum/kaldırma "--exit" gönderdiğinde çağrılır.</summary>
    public void RequestExit() => _form.BeginInvoke(ExitApplication);

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        var open = new ToolStripMenuItem("Ayarları aç", null, (_, _) => ShowMainForm()) { Font = new Font(menu.Font, FontStyle.Bold) };
        menu.Items.Add(open);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_profilesMenu);
        menu.Items.Add("Tümünü başlat / konumla", null, async (_, _) => await StartProfilesAsync(_store.Config.Profiles, showErrors: true));
        menu.Items.Add("Monitörleri tanımla", null, (_, _) => MonitorIdentifyForm.ShowAll());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Çıkış", null, (_, _) => ExitApplication());

        menu.Opening += (_, _) =>
        {
            _profilesMenu.DropDownItems.Clear();
            foreach (var p in _store.Config.Profiles)
            {
                var item = new ToolStripMenuItem($"{p.Name}  ({MonitorService.Describe(p)}, {LaunchProfile.ModeText(p.WindowMode)})")
                {
                    Checked = _launcher.IsRunning(p),
                };
                item.Click += async (_, _) => await StartProfilesAsync(new[] { p }, showErrors: true);
                _profilesMenu.DropDownItems.Add(item);
            }
            if (_profilesMenu.DropDownItems.Count == 0)
                _profilesMenu.DropDownItems.Add(new ToolStripMenuItem("(profil yok)") { Enabled = false });
        };
        return menu;
    }

    private void ShowMainForm() => _form.ShowAndActivate();

    private async Task RunStartupAsync()
    {
        _startupRunning = true;
        try
        {
            var config = _store.Config;
            var profiles = config.Profiles.Where(p => p.AutoStart).ToList();
            if (profiles.Count == 0) return;

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(0, config.StartupDelaySeconds)));

            foreach (var p in profiles)
            {
                // Dış ekran (TV vb.) PC'den sonra açılıyor olabilir: bir süre bekle.
                var deadline = DateTime.Now.AddSeconds(Math.Max(0, config.MonitorWaitSeconds));
                if (MonitorService.Resolve(p) is null) Log.Info($"'{p.Name}' için monitör bekleniyor");
                while (MonitorService.Resolve(p) is null && DateTime.Now < deadline) await Task.Delay(2000);
                if (MonitorService.Resolve(p) is null)
                    Log.Warn($"'{p.Name}': hedef monitör bağlı değil, ana monitörde normal pencere olarak açılıyor");
            }

            await StartProfilesAsync(profiles, showErrors: false);
        }
        catch (Exception ex)
        {
            Log.Error("Açılış başlatması başarısız", ex);
        }
        finally
        {
            _startupRunning = false;
        }
    }

    private async Task StartProfilesAsync(IEnumerable<LaunchProfile> profiles, bool showErrors)
    {
        var errors = new List<string>();
        foreach (var p in profiles.ToList())
        {
            var error = await _launcher.StartOrPlaceAsync(p);
            if (error is not null) errors.Add(error);
        }

        if (errors.Count == 0) return;
        if (showErrors)
        {
            MessageBox.Show(string.Join("\n\n", errors), AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        else
        {
            _tray.ShowBalloonTip(8000, AppInfo.Name, string.Join("\n", errors), ToolTipIcon.Warning);
        }
    }

    private void WatchdogTick()
    {
        if (_startupRunning) return;
        foreach (var p in _store.Config.Profiles.ToList())
        {
            if (p.RestartIfClosed && _launcher.ShouldRestart(p))
            {
                Log.Info($"'{p.Name}' penceresi kapanmış, yeniden başlatılıyor");
                _ = _launcher.StartOrPlaceAsync(p);
            }
            else if (p.KeepOnMonitor)
            {
                _launcher.EnsurePlaced(p);
            }
        }
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        // Monitör açılıp kapanırken olay art arda gelir; durulmasını bekle.
        _form.BeginInvoke(() =>
        {
            _displayChangeDebounce.Stop();
            _displayChangeDebounce.Start();
        });
    }

    private void TrySave()
    {
        try
        {
            _store.Save();
        }
        catch (Exception ex)
        {
            Log.Error("Ayarlar kaydedilemedi", ex);
        }
    }

    private void ExitApplication()
    {
        Log.Info("Uygulama kapatılıyor");
        _watchdog.Stop();
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        _tray.Visible = false;
        _form.AllowClose = true;
        _form.Close();
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tray.Dispose();
            _watchdog.Dispose();
            _displayChangeDebounce.Dispose();
            _form.Dispose();
        }
        base.Dispose(disposing);
    }
}
