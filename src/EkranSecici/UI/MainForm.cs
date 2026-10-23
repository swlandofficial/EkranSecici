using System.Diagnostics;
using EkranSecici.Models;
using EkranSecici.Services;

namespace EkranSecici.UI;

/// <summary>Profil listesi ve genel ayarlar. Kapatılınca tepsiye gizlenir.</summary>
internal sealed class MainForm : Form
{
    private readonly ConfigStore _store;
    private readonly AppLauncher _launcher;

    private readonly ListView _list = new()
    {
        View = View.Details,
        FullRowSelect = true,
        MultiSelect = false,
        HideSelection = false,
        Dock = DockStyle.Fill,
        CheckBoxes = true, // işaret = profil aktif
    };
    private readonly CheckBox _startWithWindows = new() { Text = "Windows açılışında Ekran Seçici'yi otomatik başlat", AutoSize = true };
    private readonly NumericUpDown _startupDelay = new() { Minimum = 0, Maximum = 600, Width = 70 };
    private readonly NumericUpDown _monitorWait = new() { Minimum = 0, Maximum = 600, Width = 70 };
    private readonly System.Windows.Forms.Timer _refreshTimer = new() { Interval = 2000 };
    private readonly Button[] _selectionButtons;
    private bool _loading;
    private bool _refreshingList;
    private bool _doubleClicking;

    /// <summary>Uygulamadan tamamen çıkılırken true yapılır; aksi halde kapatma = gizleme.</summary>
    public bool AllowClose { get; set; }

    public MainForm(ConfigStore store, AppLauncher launcher)
    {
        _store = store;
        _launcher = launcher;

        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = $"{AppInfo.Name} {AppInfo.Version}";
        Icon = AppIcons.Load();
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(900, 520);
        MinimumSize = new Size(760, 420);
        Padding = new Padding(10);

        _list.Columns.Add("Profil", 180);
        _list.Columns.Add("Program", 230);
        _list.Columns.Add("Monitör", 150);
        _list.Columns.Add("Mod", 110);
        _list.Columns.Add("Açılışta", 70);
        _list.Columns.Add("Durum", 90);
        _list.DoubleClick += (_, _) => EditSelected();
        _list.SelectedIndexChanged += (_, _) => UpdateButtons();
        // ListView çift tıklamada kutuyu da değiştirir; çift tıklama yalnızca düzenlemeyi açsın.
        _list.MouseDown += (_, e) => _doubleClicking = e.Clicks > 1;
        _list.MouseUp += (_, _) => _doubleClicking = false;
        _list.ItemCheck += (_, e) =>
        {
            if (_doubleClicking) e.NewValue = e.CurrentValue;
        };
        _list.ItemChecked += (_, e) =>
        {
            if (!_refreshingList && e.Item.Tag is LaunchProfile p) SetEnabled(p, e.Item.Checked);
        };

        var add = MakeButton("Yeni profil", (_, _) => AddProfile());
        var edit = MakeButton("Düzenle", (_, _) => EditSelected());
        var toggle = MakeButton("Aktif / Pasif", (_, _) => { if (Selected is { } p) SetEnabled(p, !p.Enabled); });
        var remove = MakeButton("Sil", (_, _) => RemoveSelected());
        var start = MakeButton("Başlat / Konumla", async (_, _) => await StartSelectedAsync());
        var startAll = MakeButton("Tümünü başlat", async (_, _) => await StartAllAsync());
        var identify = MakeButton("Monitörleri tanımla", (_, _) => MonitorIdentifyForm.ShowAll());
        _selectionButtons = new[] { edit, toggle, remove, start };

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 6, 0, 6) };
        buttons.Controls.AddRange(new Control[] { add, edit, toggle, remove, start, startAll, identify });

        var header = new Label
        {
            Text = "Programların hangi monitörde ve hangi modda açılacağını belirleyin. " +
                   "Soldaki kutu işaretli değilse profil pasiftir (başlatılmaz, silinmez). " +
                   "Pencereyi kapatınca program sağ alttaki simgede çalışmaya devam eder.",
            AutoSize = true,
            MaximumSize = new Size(860, 0),
            Margin = new Padding(0, 0, 0, 8),
        };

        var settings = new GroupBox { Text = "Genel ayarlar", Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(8) };
        var settingsFlow = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Dock = DockStyle.Fill };
        settingsFlow.Controls.Add(_startWithWindows, 0, 0);
        settingsFlow.SetColumnSpan(_startWithWindows, 2);
        settingsFlow.Controls.Add(new Label { Text = "Açılışta programları başlatmadan önce bekle (sn):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        settingsFlow.Controls.Add(_startupDelay, 1, 1);
        settingsFlow.Controls.Add(new Label { Text = "Hedef monitör bağlı değilse bağlanmasını bekle (sn):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
        settingsFlow.Controls.Add(_monitorWait, 1, 2);
        settings.Controls.Add(settingsFlow);

        var openFolder = new LinkLabel { Text = "Ayar ve günlük klasörünü aç", AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
        openFolder.LinkClicked += (_, _) => OpenDataFolder();

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(header, 0, 0);
        root.Controls.Add(_list, 0, 1);
        root.Controls.Add(buttons, 0, 2);
        root.Controls.Add(settings, 0, 3);
        root.Controls.Add(openFolder, 0, 4);
        Controls.Add(root);

        _startWithWindows.CheckedChanged += (_, _) => SaveSettings();
        _startupDelay.ValueChanged += (_, _) => SaveSettings();
        _monitorWait.ValueChanged += (_, _) => SaveSettings();

        _refreshTimer.Tick += (_, _) => RefreshStatus();
        VisibleChanged += (_, _) => _refreshTimer.Enabled = Visible;

        ResumeLayout(false);
        PerformLayout();

        LoadSettings();
        RefreshList();
    }

    public void ShowAndActivate()
    {
        LoadSettings();
        RefreshList();
        if (!Visible) Show();
        if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
        Activate();
        BringToFront();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!AllowClose && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }
        base.OnFormClosing(e);
    }

    private static Button MakeButton(string text, EventHandler onClick)
    {
        var b = new Button { Text = text, AutoSize = true, MinimumSize = new Size(100, 30) };
        b.Click += onClick;
        return b;
    }

    private void UpdateButtons()
    {
        var hasSelection = _list.SelectedItems.Count > 0;
        foreach (var b in _selectionButtons) b.Enabled = hasSelection;
    }

    private void LoadSettings()
    {
        _loading = true;
        var c = _store.Config;
        _startWithWindows.Checked = c.StartWithWindows;
        _startupDelay.Value = Math.Clamp(c.StartupDelaySeconds, 0, 600);
        _monitorWait.Value = Math.Clamp(c.MonitorWaitSeconds, 0, 600);
        _loading = false;
    }

    private void SaveSettings()
    {
        if (_loading) return;
        var c = _store.Config;
        c.StartWithWindows = _startWithWindows.Checked;
        c.StartupDelaySeconds = (int)_startupDelay.Value;
        c.MonitorWaitSeconds = (int)_monitorWait.Value;
        Save();
        AutoStartService.Set(c.StartWithWindows);
    }

    private void Save()
    {
        try
        {
            _store.Save();
        }
        catch (Exception ex)
        {
            Log.Error("Ayarlar kaydedilemedi", ex);
            MessageBox.Show(this, $"Ayarlar kaydedilemedi:\n{ex.Message}", AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private LaunchProfile? Selected =>
        _list.SelectedItems.Count > 0 ? _list.SelectedItems[0].Tag as LaunchProfile : null;

    private void RefreshList()
    {
        var selectedId = Selected?.Id;
        _refreshingList = true;
        _list.BeginUpdate();
        _list.Items.Clear();
        foreach (var p in _store.Config.Profiles)
        {
            var item = new ListViewItem(p.Name)
            {
                Tag = p,
                Checked = p.Enabled,
                ForeColor = p.Enabled ? SystemColors.WindowText : SystemColors.GrayText,
            };
            item.SubItems.Add(Path.GetFileName(p.ExePath.Trim('"')));
            item.SubItems.Add(MonitorService.Describe(p));
            item.SubItems.Add(LaunchProfile.ModeText(p.WindowMode));
            item.SubItems.Add(p.AutoStart ? "Evet" : "Hayır");
            item.SubItems.Add(StatusText(p));
            _list.Items.Add(item);
            if (p.Id == selectedId) item.Selected = true;
        }
        if (_list.SelectedItems.Count == 0 && _list.Items.Count > 0) _list.Items[0].Selected = true;
        _list.EndUpdate();
        _refreshingList = false;
        UpdateButtons();
    }

    private void RefreshStatus()
    {
        foreach (ListViewItem item in _list.Items)
        {
            if (item.Tag is not LaunchProfile p) continue;
            item.SubItems[2].Text = MonitorService.Describe(p);
            item.SubItems[5].Text = StatusText(p);
        }
    }

    private string StatusText(LaunchProfile p) =>
        !p.Enabled ? (_launcher.IsRunning(p) ? "Pasif (açık)" : "Pasif") :
        _launcher.IsBusy(p) ? "Başlatılıyor…" : _launcher.IsRunning(p) ? "Açık" : "—";

    private void AddProfile()
    {
        using var dlg = new ProfileEditorForm(new LaunchProfile(), _launcher, isNew: true);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        _store.Config.Profiles.Add(dlg.Result);
        Save();
        RefreshList();
    }

    private void EditSelected()
    {
        if (Selected is not { } p) return;
        using var dlg = new ProfileEditorForm(p, _launcher, isNew: false);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        var index = _store.Config.Profiles.IndexOf(p);
        if (index < 0) return;
        _store.Config.Profiles[index] = dlg.Result;
        if (!dlg.Result.Enabled) _launcher.Forget(p.Id);
        Save();
        RefreshList();
    }

    private void RemoveSelected()
    {
        if (Selected is not { } p) return;
        if (MessageBox.Show(this, $"'{p.Name}' profili silinsin mi?\n(Açık olan program kapatılmaz.)", AppInfo.Name,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _store.Config.Profiles.Remove(p);
        _launcher.Forget(p.Id);
        Save();
        RefreshList();
    }

    private void SetEnabled(LaunchProfile p, bool enabled)
    {
        if (p.Enabled == enabled) return;
        p.Enabled = enabled;
        // Pasif yapılınca açık programı kapatmıyoruz; yalnızca izlemeyi bırakıyoruz (yeniden başlatma/konumlama yok).
        if (!enabled) _launcher.Forget(p.Id);
        Log.Info($"'{p.Name}' {(enabled ? "aktif" : "pasif")} yapıldı");
        Save();
        RefreshList();
    }

    private async Task StartSelectedAsync()
    {
        if (Selected is not { } p) return;
        RefreshStatusSoon();
        var error = await _launcher.StartOrPlaceAsync(p);
        RefreshStatus();
        if (error is not null) MessageBox.Show(this, error, AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private async Task StartAllAsync()
    {
        var errors = new List<string>();
        foreach (var p in _store.Config.Profiles.Where(p => p.Enabled).ToList())
        {
            RefreshStatusSoon();
            var error = await _launcher.StartOrPlaceAsync(p);
            if (error is not null) errors.Add(error);
        }
        RefreshStatus();
        if (errors.Count > 0) MessageBox.Show(this, string.Join("\n\n", errors), AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void RefreshStatusSoon() => BeginInvoke(RefreshStatus);

    private static void OpenDataFolder()
    {
        try
        {
            Directory.CreateDirectory(AppInfo.DataDirectory);
            Process.Start(new ProcessStartInfo { FileName = AppInfo.DataDirectory, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log.Error("Klasör açılamadı", ex);
        }
    }
}
