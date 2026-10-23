using EkranSecici.Services;

namespace EkranSecici.UI;

/// <summary>Her monitörün ortasında büyük numara gösterir (hangi monitör hangisi?).</summary>
internal sealed class MonitorIdentifyForm : Form
{
    private MonitorIdentifyForm(MonitorInfo monitor)
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        BackColor = monitor.Primary ? Color.FromArgb(55, 65, 81) : Color.FromArgb(13, 148, 136);
        ForeColor = Color.White;

        var w = Math.Min(520, monitor.Bounds.Width * 2 / 3);
        var h = Math.Min(340, monitor.Bounds.Height * 2 / 3);
        Bounds = new Rectangle(
            monitor.Bounds.Left + (monitor.Bounds.Width - w) / 2,
            monitor.Bounds.Top + (monitor.Bounds.Height - h) / 2,
            w, h);

        var number = new Label
        {
            Text = monitor.Index.ToString(),
            Font = new Font("Segoe UI", 110, FontStyle.Bold, GraphicsUnit.Point),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
        };
        var info = new Label
        {
            Text = $"{(monitor.Primary ? "Ana monitör  •  " : "")}{monitor.Bounds.Width}x{monitor.Bounds.Height}  •  {monitor.DeviceName}",
            Font = new Font("Segoe UI", 12, FontStyle.Regular, GraphicsUnit.Point),
            Dock = DockStyle.Bottom,
            Height = 48,
            TextAlign = ContentAlignment.MiddleCenter,
        };
        Controls.Add(number);
        Controls.Add(info);

        number.Click += (_, _) => Close();
        info.Click += (_, _) => Close();
        Click += (_, _) => Close();
    }

    public static void ShowAll(int seconds = 4)
    {
        var forms = MonitorService.GetMonitors().Select(m => new MonitorIdentifyForm(m)).ToList();
        foreach (var f in forms) f.Show();

        var timer = new System.Windows.Forms.Timer { Interval = seconds * 1000 };
        timer.Tick += (_, _) =>
        {
            timer.Dispose();
            foreach (var f in forms.Where(f => !f.IsDisposed)) f.Close();
        };
        timer.Start();
    }
}
