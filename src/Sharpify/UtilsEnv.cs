using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Sharpify;

public static partial class Utils {
    /// <summary>
    /// Opens the specified URL in the default web browser based on the operating system.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    /// <remarks>
    /// Currently only Windows, Linux and Mac are supported.
    /// </remarks>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("Linux")]
    [SupportedOSPlatform("MacOS")]
    public static void OpenLink(string url) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            var processInfo = new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true
            };
            using var process = Process.Start(processInfo);
            return;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            using var process = Process.Start("x-www-browser", url);
            return;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            using var process = Process.Start("open", url);
            return;
        }
        throw new PlatformNotSupportedException();
    }
}