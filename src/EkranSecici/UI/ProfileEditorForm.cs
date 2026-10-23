using EkranSecici.Models;
using EkranSecici.Services;
using Microsoft.Win32;

namespace EkranSecici.UI;

/// <summary>Profil ekleme/düzenleme penceresi.</summary>
internal sealed class ProfileEditorForm : Form
{
    private sealed record ModeItem(WindowMode Mode)
    {
        public override string ToString() => LaunchProfile.ModeText(Mode);
    }

    private readonly AppLauncher _launcher;
    private readonly LaunchProfile _profile;

    private readonly TextBox _name = new();
    private readonly TextBox _exe = new();
    private readonly TextBox _args = new();
    private readonly TextBox _workDir = new();
    private readonly ComboBox _monitor = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _mode = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _enabled = new() { Text = "Profil aktif (pasif profil başlatılmaz, silinmez)", AutoSize = true };
    private readonly CheckBox _sendF11 = new() { Text = "Tam ekran için F11 tuşu gönder (tarayıcılar, --kiosk kullanmıyorsanız)", AutoSize = true };
    private readonly CheckBox _topMost = new() { Text = "Her zaman üstte", AutoSize = true };
    private readonly CheckBox _autoStart = new() { Text = "Windows açılışında otomatik başlat", AutoSize = true };
    private readonly CheckBox _keepOnMonitor = new() { Text = "Pencereyi seçili monitörde ve modda tut (kaydırılırsa geri getir)", AutoSize = true };
    private readonly CheckBox _restart = new() { Text = "Kapanırsa yeniden başlat", AutoSize = true };
    private readonly TextBox _processName = new();
    private readonly TextBox _title = new();
    private readonly NumericUpDown _wait = new() { Minimum = 5, Maximum = 300, Width = 80 };

    /// <summary>Kaydedilen profil (Tamam'a basıldıysa).</summary>
    public LaunchProfile Result => _profile;

    public ProfileEditorForm(LaunchProfile profile, AppLauncher launcher, bool isNew)
    {
        _profile = profile.Clone();
        _launcher = launcher;

        SuspendLayout();
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = isNew ? "Yeni Profil" : $"Profil Düzenle — {profile.Name}";
        Icon = AppIcons.Load();
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        var grid = new TableLayoutPanel
        {
            ColumnCount = 3,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 460));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var template = new Button { Text = "Firefox hasta çağrı ekranı şablonu", AutoSize = true };
        template.Click += (_, _) => ApplyFirefoxTemplate();
        grid.Controls.Add(template, 1, grid.RowCount++);
        grid.SetColumnSpan(template, 2);

        var browse = new Button { Text = "Gözat…", AutoSize = true };
        browse.Click += (_, _) => BrowseExe();
        var identify = new Button { Text = "Tanımla", AutoSize = true };
        identify.Click += (_, _) => MonitorIdentifyForm.ShowAll();
        var refresh = new Button { Text = "Yenile", AutoSize = true };
        refresh.Click += (_, _) => FillMonitors(SelectedMonitor()?.DeviceName ?? _profile.MonitorDeviceName, SelectedMonitor()?.Index ?? _profile.MonitorIndex);
        var monitorButtons = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
        monitorButtons.Controls.AddRange(new Control[] { identify, refresh });

        AddRow(grid, "Profil adı:", _name);
        AddRow(grid, "", _enabled);
        AddRow(grid, "Program (exe):", _exe, browse);
        AddRow(grid, "Parametreler:", _args);
        AddHint(grid, "Örn. Firefox: -no-remote -profile \"%APPDATA%\\EkranSecici\\FirefoxCagri\" --kiosk \"https://adres\"");
        AddRow(grid, "Çalışma klasörü:", _workDir);
        AddRow(grid, "Monitör:", _monitor, monitorButtons);
        AddRow(grid, "Pencere modu:", _mode);
        AddRow(grid, "", _sendF11);
        AddRow(grid, "", _topMost);
        AddRow(grid, "", _autoStart);
        AddRow(grid, "", _keepOnMonitor);
        AddRow(grid, "", _restart);

        var advanced = new Label { Text = "Gelişmiş (pencere bulma)", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(3, 12, 3, 3) };
        grid.Controls.Add(advanced, 0, grid.RowCount++);
        grid.SetColumnSpan(advanced, 3);
        AddRow(grid, "İşlem adı:", _processName);
        AddHint(grid, "Boş bırakılırsa exe adı kullanılır (ör. firefox). Pencere farklı bir işleme aitse girin.");
        AddRow(grid, "Başlık içerir:", _title);
        AddHint(grid, "İsteğe bağlı. Doluysa yalnızca başlığı bu metni içeren pencere taşınır.");
        AddRow(grid, "Pencere bekleme (sn):", _wait);

        var ok = new Button { Text = "Kaydet", DialogResult = DialogResult.OK, AutoSize = true, MinimumSize = new Size(90, 0) };
        var cancel = new Button { Text = "İptal", DialogResult = DialogResult.Cancel, AutoSize = true, MinimumSize = new Size(90, 0) };
        var test = new Button { Text = "▶ Test et", AutoSize = true, MinimumSize = new Size(90, 0) };
        test.Click += async (_, _) => await TestAsync(test);
        ok.Click += (_, e) =>
        {
            if (!TryValidate(out var error))
            {
                MessageBox.Show(this, error, AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            ReadInto(_profile);
        };
        AcceptButton = ok;
        CancelButton = cancel;

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 12, 0, 0) };
        buttons.Controls.AddRange(new Control[] { cancel, ok, test });
        grid.Controls.Add(buttons, 0, grid.RowCount++);
        grid.SetColumnSpan(buttons, 3);

        Controls.Add(grid);

        foreach (var m in Enum.GetValues<WindowMode>()) _mode.Items.Add(new ModeItem(m));
        _mode.SelectedIndexChanged += (_, _) => UpdateEnabled();
        LoadFrom(_profile);

        ResumeLayout(false);
        PerformLayout();
    }

    private static void AddRow(TableLayoutPanel grid, string label, Control control, Control? extra = null)
    {
        var row = grid.RowCount++;
        grid.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 8, 3) }, 0, row);
        if (control is TextBox or ComboBox) control.Dock = DockStyle.Fill;
        grid.Controls.Add(control, 1, row);
        if (extra is not null) grid.Controls.Add(extra, 2, row);
    }

    private static void AddHint(TableLayoutPanel grid, string text)
    {
        var hint = new Label { Text = text, AutoSize = true, ForeColor = SystemColors.GrayText, MaximumSize = new Size(460, 0), Margin = new Padding(3, 0, 3, 6) };
        grid.Controls.Add(hint, 1, grid.RowCount++);
    }

    private void LoadFrom(LaunchProfile p)
    {
        _name.Text = p.Name;
        _enabled.Checked = p.Enabled;
        _exe.Text = p.ExePath;
        _args.Text = p.Arguments;
        _workDir.Text = p.WorkingDirectory;
        FillMonitors(p.MonitorDeviceName, p.MonitorIndex);
        _mode.SelectedIndex = (int)p.WindowMode;
        _sendF11.Checked = p.SendF11;
        _topMost.Checked = p.AlwaysOnTop;
        _autoStart.Checked = p.AutoStart;
        _keepOnMonitor.Checked = p.KeepOnMonitor;
        _restart.Checked = p.RestartIfClosed;
        _processName.Text = p.ProcessName;
        _title.Text = p.WindowTitleContains;
        _wait.Value = Math.Clamp(p.WindowWaitSeconds, (int)_wait.Minimum, (int)_wait.Maximum);
        UpdateEnabled();
    }

    private void ReadInto(LaunchProfile p)
    {
        p.Name = _name.Text.Trim();
        p.Enabled = _enabled.Checked;
        p.ExePath = _exe.Text.Trim();
        p.Arguments = _args.Text.Trim();
        p.WorkingDirectory = _workDir.Text.Trim();
        if (SelectedMonitor() is { } m)
        {
            p.MonitorDeviceName = m.DeviceName;
            p.MonitorIndex = m.Index;
        }
        p.WindowMode = ((ModeItem)_mode.SelectedItem!).Mode;
        p.SendF11 = _sendF11.Checked;
        p.AlwaysOnTop = _topMost.Checked;
        p.AutoStart = _autoStart.Checked;
        p.KeepOnMonitor = _keepOnMonitor.Checked;
        p.RestartIfClosed = _restart.Checked;
        p.ProcessName = _processName.Text.Trim();
        p.WindowTitleContains = _title.Text.Trim();
        p.WindowWaitSeconds = (int)_wait.Value;
    }

    private bool TryValidate(out string error)
    {
        error = "";
        if (string.IsNullOrWhiteSpace(_name.Text)) error = "Profil adı girin.";
        else if (string.IsNullOrWhiteSpace(_exe.Text)) error = "Program yolunu girin.";
        else if (SelectedMonitor() is null) error = "Bir monitör seçin.";
        return error.Length == 0;
    }

    private MonitorInfo? SelectedMonitor() => _monitor.SelectedItem as MonitorInfo;

    private void FillMonitors(string deviceName, int index)
    {
        _monitor.Items.Clear();
        var monitors = MonitorService.GetMonitors();
        foreach (var m in monitors) _monitor.Items.Add(m);

        var match = monitors.FirstOrDefault(m => string.Equals(m.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase))
                    ?? monitors.FirstOrDefault(m => m.Index == index)
                    ?? monitors.FirstOrDefault(m => !m.Primary)
                    ?? monitors[0];
        _monitor.SelectedItem = match;
    }

    private void UpdateEnabled()
    {
        _sendF11.Enabled = _mode.SelectedItem is ModeItem { Mode: WindowMode.Fullscreen };
    }

    private void BrowseExe()
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Program seçin",
            Filter = "Programlar (*.exe)|*.exe|Tüm dosyalar (*.*)|*.*",
        };
        try
        {
            var current = Environment.ExpandEnvironmentVariables(_exe.Text.Trim().Trim('"'));
            if (File.Exists(current)) dlg.InitialDirectory = Path.GetDirectoryName(current);
        }
        catch
        {
        }

        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        _exe.Text = dlg.FileName;
        if (string.IsNullOrWhiteSpace(_name.Text)) _name.Text = Path.GetFileNameWithoutExtension(dlg.FileName);
    }

    private void ApplyFirefoxTemplate()
    {
        var firefox = FindFirefox();
        if (firefox is null)
        {
            MessageBox.Show(this, "Firefox bulunamadı. Program yolunu 'Gözat…' ile seçin.", AppInfo.Name,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        var url = "https://";
        using (var ask = new UrlPromptForm())
        {
            if (ask.ShowDialog(this) == DialogResult.OK) url = ask.Url;
        }

        if (string.IsNullOrWhiteSpace(_name.Text)) _name.Text = "Hasta Çağrı Ekranı";
        if (firefox is not null) _exe.Text = firefox;
        _args.Text = $"-no-remote -profile \"%APPDATA%\\EkranSecici\\FirefoxCagri\" --kiosk \"{url}\"";
        _processName.Text = "firefox";
        _mode.SelectedIndex = (int)WindowMode.Fullscreen;
        _sendF11.Checked = false;
        _autoStart.Checked = true;
        _keepOnMonitor.Checked = true;
        _restart.Checked = true;
        if (SelectedMonitor() is { Primary: true })
        {
            var secondary = _monitor.Items.Cast<MonitorInfo>().FirstOrDefault(m => !m.Primary);
            if (secondary is not null) _monitor.SelectedItem = secondary;
        }
    }

    private static string? FindFirefox()
    {
        foreach (var hive in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            try
            {
                using var key = hive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe");
                if (key?.GetValue(null) is string path && File.Exists(path.Trim('"'))) return path.Trim('"');
            }
            catch
            {
            }
        }

        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Mozilla Firefox", "firefox.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Mozilla Firefox", "firefox.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mozilla Firefox", "firefox.exe"),
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    private async Task TestAsync(Button button)
    {
        if (!TryValidate(out var error))
        {
            MessageBox.Show(this, error, AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var temp = _profile.Clone();
        ReadInto(temp);
        temp.Enabled = true; // Test, pasif profilde de çalışsın
        button.Enabled = false;
        UseWaitCursor = true;
        try
        {
            var result = await _launcher.StartOrPlaceAsync(temp);
            if (result is not null) MessageBox.Show(this, result, AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            button.Enabled = true;
            UseWaitCursor = false;
        }
    }

    /// <summary>Çağrı ekranı adresini soran küçük pencere.</summary>
    private sealed class UrlPromptForm : Form
    {
        private readonly TextBox _url = new() { Text = "https://", Width = 420 };
        public string Url => _url.Text.Trim();

        public UrlPromptForm()
        {
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "Hasta çağrı ekranı adresi";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(12);

            var ok = new Button { Text = "Tamam", DialogResult = DialogResult.OK, AutoSize = true };
            var cancel = new Button { Text = "İptal", DialogResult = DialogResult.Cancel, AutoSize = true };
            AcceptButton = ok;
            CancelButton = cancel;

            var panel = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, Dock = DockStyle.Fill };
            panel.Controls.Add(new Label { Text = "Firefox'ta açılacak çağrı ekranı adresi (URL):", AutoSize = true });
            panel.Controls.Add(_url);
            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Width = 420 };
            buttons.Controls.AddRange(new Control[] { cancel, ok });
            panel.Controls.Add(buttons);
            Controls.Add(panel);
        }
    }
}
