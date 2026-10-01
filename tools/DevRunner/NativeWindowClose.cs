using System.Runtime.InteropServices;

namespace DevRunner;

// Xlib's Linux x86_64 layout, used only on the supervisor-owned display.
// The observed main window and PID property identify the child; titles are never matched.
internal static class NativeWindowClose
{
    public static void Focus(Child child, Options options, ulong window)
    {
        PrivateDisplay.ValidateWorker(options);
        if (child.HasExited || window == 0) throw new InvalidOperationException("Owned child has no live observed window.");
        nint display = XOpenDisplay(0);
        if (display == 0) throw new InvalidOperationException("Cannot open the owned X11 display.");
        try
        {
            ulong[] values = Property(display, window, XInternAtom(display, "_NET_WM_PID", 0), 6);
            if (values.Length != 1 || values[0] != (ulong)child.ProcessId)
                throw new InvalidOperationException("Observed main window does not belong to the owned child PID.");
            _ = XSetInputFocus(display, window, 1, 0);
            _ = XFlush(display);
        }
        finally { _ = XCloseDisplay(display); }
    }
    public static IDisposable Defocus(Child child, Options options, ulong window)
    {
        // Validate ownership before creating the temporary focus recipient.
        Focus(child, options, window);
        nint display = XOpenDisplay(0);
        if (display == 0) throw new InvalidOperationException("Cannot open the owned X11 display.");
        ulong recipient = XCreateSimpleWindow(display, XDefaultRootWindow(display), 0, 0, 1, 1, 0, 0, 0);
        if (recipient == 0) { _ = XCloseDisplay(display); throw new InvalidOperationException("Cannot create owned focus window."); }
        var attributes = new WindowAttributes { OverrideRedirect = 1 };
        _ = XChangeWindowAttributes(display, recipient, 1UL << 9, ref attributes);
        _ = XMapWindow(display, recipient);
        _ = XSetInputFocus(display, recipient, 1, 0);
        _ = XFlush(display);
        return new FocusRecipient(display, recipient);
    }
    private sealed class FocusRecipient(nint display, ulong window) : IDisposable
    {
        public void Dispose() { _ = XDestroyWindow(display, window); _ = XCloseDisplay(display); }
    }
    [StructLayout(LayoutKind.Explicit, Size = 112)]
    private struct WindowAttributes
    {
        [FieldOffset(88)] public int OverrideRedirect;
    }
    public static void Request(Child child, Options options, ulong window)
    {
        PrivateDisplay.ValidateWorker(options);
        if (child.HasExited) throw new InvalidOperationException("Cannot close an exited owned child.");
        if (window == 0) throw new InvalidOperationException("Owned child has no observed native main window.");
        nint display = XOpenDisplay(0);
        if (display == 0) throw new InvalidOperationException("Cannot open the owned X11 display.");
        try
        {
            ulong[] values = Property(display, window, XInternAtom(display, "_NET_WM_PID", 0), 6);
            if (values.Length != 1 || values[0] != (ulong)child.ProcessId)
                throw new InvalidOperationException("Observed main window does not belong to the owned child PID.");
            var message = new ClientMessage
            {
                Type = 33,
                SendEvent = 1,
                Display = display,
                Window = window,
                MessageType = XInternAtom(display, "WM_PROTOCOLS", 0),
                Format = 32,
                Data0 = XInternAtom(display, "WM_DELETE_WINDOW", 0)
            };
            if (XSendEvent(display, window, 0, 0, ref message) == 0)
                throw new InvalidOperationException("Owned window rejected native close request.");
            _ = XFlush(display);
            Console.WriteLine($"NATIVE CLOSE {child.Name}: owned pid={child.ProcessId}, main window={window}");
        }
        finally { _ = XCloseDisplay(display); }
    }

    private static ulong[] Property(nint display, ulong window, ulong property, ulong type)
    {
        int result = XGetWindowProperty(display, window, property, 0, 4096, 0, type,
            out ulong actualType, out int format, out ulong count, out _, out nint data);
        try
        {
            if (result != 0 || actualType != type || format != 32 || count > 4096 || data == 0) return [];
            // Xlib stores format-32 property values in native longs on this platform.
            var values = new ulong[(int)count];
            for (int i = 0; i < values.Length; i++) values[i] = (ulong)Marshal.ReadInt64(data, i * 8);
            return values;
        }
        finally { if (data != 0) _ = XFree(data); }
    }

    [StructLayout(LayoutKind.Explicit, Size = 192)]
    private struct ClientMessage
    {
        [FieldOffset(0)] public int Type;
        [FieldOffset(16)] public int SendEvent;
        [FieldOffset(24)] public nint Display;
        [FieldOffset(32)] public ulong Window;
        [FieldOffset(40)] public ulong MessageType;
        [FieldOffset(48)] public int Format;
        [FieldOffset(56)] public ulong Data0;
    }
    [DllImport("libX11.so.6")]
    private static extern nint XOpenDisplay(nint name);
    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(nint display);
    [DllImport("libX11.so.6", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    private static extern ulong XInternAtom(nint display, string name, int onlyIfExists);
    [DllImport("libX11.so.6")]
    private static extern int XGetWindowProperty(nint display, ulong window, ulong property, nint offset, nint length,
        int delete, ulong requestedType, out ulong actualType, out int format, out ulong count, out ulong remaining, out nint data);
    [DllImport("libX11.so.6")]
    private static extern int XFree(nint data);
    [DllImport("libX11.so.6")]
    private static extern int XSendEvent(nint display, ulong window, int propagate, nint mask, ref ClientMessage message);
    [DllImport("libX11.so.6")]
    private static extern int XFlush(nint display);
    [DllImport("libX11.so.6")]
    private static extern ulong XDefaultRootWindow(nint display);
    [DllImport("libX11.so.6")]
    private static extern int XSetInputFocus(nint display, ulong window, int revert, ulong time);
    [DllImport("libX11.so.6")]
    private static extern ulong XCreateSimpleWindow(nint display, ulong parent, int x, int y, uint width, uint height, uint borderWidth, ulong border, ulong background);
    [DllImport("libX11.so.6")]
    private static extern int XChangeWindowAttributes(nint display, ulong window, ulong mask, ref WindowAttributes attributes);
    [DllImport("libX11.so.6")]
    private static extern int XMapWindow(nint display, ulong window);
    [DllImport("libX11.so.6")]
    private static extern int XDestroyWindow(nint display, ulong window);
}
