namespace Clipboard;

public static class ClipboardExtensions
{
    public static GetText GetGetter<T>() where T : unmanaged, IClipboardImplementation
    {
        return T.GetText;
    }

    public static SetText GetSetter<T>() where T : unmanaged, IClipboardImplementation
    {
        return T.SetText;
    }
}
