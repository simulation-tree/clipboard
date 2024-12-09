using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Clipboard.Implementations
{
    public unsafe readonly partial struct MacClipboard : IClipboardImplementation
    {
        static string? IClipboardImplementation.GetText()
        {
            throw new NotImplementedException();
        }

        static void IClipboardImplementation.SetText(string text)
        {
            throw new NotImplementedException();
        }
    }

    public unsafe readonly partial struct LinuxClipboard : IClipboardImplementation
    {
        static string? IClipboardImplementation.GetText()
        {
            throw new NotImplementedException();
        }

        static void IClipboardImplementation.SetText(string text)
        {
            throw new NotImplementedException();
        }
    }

    public unsafe readonly partial struct WindowsClipboard : IClipboardImplementation
    {
        private const uint UnicodeTextFormat = 13;
        private const uint Movable = 2;

        static string? IClipboardImplementation.GetText()
        {
            if (!IsClipboardFormatAvailable(UnicodeTextFormat))
            {
                return null;
            }

            if (!OpenClipboard(default))
            {
                return null;
            }

            nint data = GetClipboardData(UnicodeTextFormat);
            if (data == default)
            {
                return null;
            }

            void* lockedData = (void*)GlobalLock(data);
            if (lockedData == null)
            {
                return null;
            }

            int length = GlobalSize(data);
            string text = Encoding.Unicode.GetString(new Span<byte>(lockedData, length)).TrimEnd('\0');
            GlobalUnlock(data);
            CloseClipboard();
            return text;
        }

        static void IClipboardImplementation.SetText(string text)
        {
            if (!IsClipboardFormatAvailable(UnicodeTextFormat))
            {
                //check if its TextFormat too, where each char is 1 byte
                return;
            }

            if (OpenClipboard(default))
            {
                int length = (text.Length + 1) * sizeof(char);
                nint global = GlobalAlloc(Movable, length);
                if (global == default)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                nint target = GlobalLock(global);
                Encoding.Unicode.GetBytes(text, new Span<byte>((void*)target, length));
                GlobalUnlock(target);
                if (SetClipboardData(UnicodeTextFormat, global) == default)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                GlobalFree(global);
                CloseClipboard();
            }
        }

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool IsClipboardFormatAvailable(uint format);

        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial nint GetClipboardData(uint uFormat);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        public static partial nint GlobalAlloc(uint uFlags, nint dwBytes);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        public static partial nint GlobalFree(nint hMem);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        public static partial nint GlobalLock(nint hMem);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GlobalUnlock(nint hMem);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool OpenClipboard(nint hWndNewOwner);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CloseClipboard();

        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial nint SetClipboardData(uint uFormat, nint data);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool EmptyClipboard();

        [LibraryImport("kernel32.dll", SetLastError = true)]
        public static partial int GlobalSize(nint hMem);
    }
}