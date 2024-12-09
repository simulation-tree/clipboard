namespace Clipboard;

public interface IClipboardImplementation
{
    static abstract string? GetText();
    static abstract void SetText(string text);
}


public delegate string? GetText();
public delegate void SetText(string text);