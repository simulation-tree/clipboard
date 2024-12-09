using Clipboard.Implementations;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Clipboard;

public struct Library : IDisposable
{
    private GCHandle getter;
    private GCHandle setter;

    public readonly string? Text
    {
        get
        {
            ThrowIfDisposed();

            GetText get = (GetText)(getter.Target ?? throw new());
            return get();
        }
        set
        {
            ThrowIfDisposed();

            if (value is null) return;

            SetText set = (SetText)(setter.Target ?? throw new());
            set(value);
        }
    }

    public Library()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            getter = GCHandle.Alloc(ClipboardExtensions.GetGetter<WindowsClipboard>());
            setter = GCHandle.Alloc(ClipboardExtensions.GetSetter<WindowsClipboard>());
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            getter = GCHandle.Alloc(ClipboardExtensions.GetGetter<MacClipboard>());
            setter = GCHandle.Alloc(ClipboardExtensions.GetSetter<MacClipboard>());
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            getter = GCHandle.Alloc(ClipboardExtensions.GetGetter<LinuxClipboard>());
            setter = GCHandle.Alloc(ClipboardExtensions.GetSetter<LinuxClipboard>());
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }

    public void Dispose()
    {
        ThrowIfDisposed();

        getter.Free();
        setter.Free();
        getter = default;
        setter = default;
    }

    [Conditional("DEBUG")]
    private readonly void ThrowIfDisposed()
    {
        if (!getter.IsAllocated || getter == default)
        {
            throw new ObjectDisposedException(nameof(Library));
        }
    }
}