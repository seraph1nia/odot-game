using System.Runtime.InteropServices;

namespace Game;

internal static class OriginalProcessArguments
{
    internal static string[] Read(string[] fallback)
    {
        string[] args = fallback;
        if (OperatingSystem.IsWindows()) args = WindowsArguments(Marshal.PtrToStringUni(GetCommandLineW())!);
        else if (OperatingSystem.IsLinux())
        {
            try { args = System.IO.File.ReadAllText("/proc/self/cmdline").Split('\0'); }
            catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException) { }
        }
        return args.TakeWhile(a => a != "--").ToArray();
    }

    internal static string[] WindowsArguments(string commandLine)
    {
        nint argv = CommandLineToArgvW(commandLine, out int count);
        if (argv == 0) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        try
        {
            var args = new string[count];
            for (int i = 0; i < count; i++) args[i] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv, i * IntPtr.Size))!;
            return args;
        }
        finally { _ = LocalFree(argv); }
    }

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern nint GetCommandLineW();
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern nint CommandLineToArgvW(string commandLine, out int count);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern nint LocalFree(nint memory);
}
