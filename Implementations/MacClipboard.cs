using System.Runtime.InteropServices;

namespace Clipboard.Implementations
{
    public unsafe readonly partial struct MacClipboard : IClipboardImplementation
    {
        private static readonly nint NSString = objc_getClass("NSString");
        private static readonly nint UTF8String = sel_registerName("UTF8String");
        private static readonly nint NSPasteboard = objc_getClass("NSPasteboard");
        private static readonly nint InitWithUTF8String = sel_registerName("initWithUTF8String:");
        private static readonly nint Alloc = sel_registerName("alloc");
        private static readonly nint Release = sel_registerName("release");
        private static readonly nint GeneralPasteboard = sel_registerName("generalPasteboard");
        private static readonly nint SetStringForType = sel_registerName("setString:forType:");
        private static readonly nint StringForType = sel_registerName("stringForType:");
        private static readonly nint ClearContents = sel_registerName("clearContents");

        static string? IClipboardImplementation.GetText()
        {
            nint generalPasteboard = objc_msgSend(NSPasteboard, GeneralPasteboard);
            nint nsStringPboardType = objc_msgSend(objc_msgSend(NSString, Alloc), InitWithUTF8String, "NSStringPboardType");
            nint ptr = objc_msgSend(generalPasteboard, StringForType, nsStringPboardType);
            nint charArray = objc_msgSend(ptr, UTF8String);
            return Marshal.PtrToStringAnsi(charArray);
        }

        static void IClipboardImplementation.SetText(string text)
        {
            nint str = objc_msgSend(objc_msgSend(NSString, Alloc), InitWithUTF8String, text);
            nint dataType = objc_msgSend(objc_msgSend(NSString, Alloc), InitWithUTF8String, NSPasteboardTypeString);
            nint generalPasteboard = objc_msgSend(NSPasteboard, GeneralPasteboard);
            objc_msgSend(generalPasteboard, ClearContents);
            objc_msgSend(generalPasteboard, SetStringForType, str, dataType);
            objc_msgSend(str, Release);
            objc_msgSend(dataType, Release);
        }

        [LibraryImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        public static partial nint objc_getClass([MarshalAs(UnmanagedType.LPStr)] string className);

        [LibraryImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        public static partial nint objc_msgSend(nint receiver, nint selector);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        static extern nint objc_msgSend(nint receiver, nint selector, [MarshalAs(UnmanagedType.LPStr)] string arg1);

        [LibraryImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        public static partial nint objc_msgSend(nint receiver, nint selector, nint arg1);

        [LibraryImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        public static partial nint objc_msgSend(nint receiver, nint selector, nint arg1, nint arg2);

        [LibraryImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        public static partial nint sel_registerName([MarshalAs(UnmanagedType.LPStr)] string selectorName);

        const string NSPasteboardTypeString = "public.utf8-plain-text";
    }
}