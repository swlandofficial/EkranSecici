namespace EkranSecici.UI;

internal static class AppIcons
{
    public static Icon Load(Size? size = null)
    {
        using var stream = typeof(AppIcons).Assembly.GetManifestResourceStream("EkranSecici.app.ico");
        if (stream is null) return (Icon)SystemIcons.Application.Clone();
        return size is { } s ? new Icon(stream, s) : new Icon(stream);
    }
}
