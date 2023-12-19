using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Sharpify;

public static partial class Utils {
    /// <summary>
    /// Provides utility methods for <see cref="Environment"/>
    /// </summary>
    public static class Env {
        /// <summary>
        /// Checks if the application is running on Windows.
        /// </summary>
        public static bool IsRunningOnWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Checks if the application is running with administrator privileges.
        /// </summary>
        /// <remarks>
        /// On platforms other than Windows, it returns <see langword="false"/> automatically.
        /// </remarks>
        public static bool IsRunningAsAdmin() {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return false;
            }

            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Returns the base directory of the application.
        /// </summary>
        /// <remarks>
        /// <para>This is tested and works on Windows</para>
        /// <para>This is not tested on Linux and Mac but should work</para>
        /// <para>Do not use in .NET Maui, it has a special api for this.</para>
        /// </remarks>
        public static string GetBaseDirectory() => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Combines the base directory path with the specified filename.
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <returns>The combined path.</returns>
        public static string PathInBaseDirectory(string filename) => Path.Combine(GetBaseDirectory(), filename);

        /// <summary>
        /// Checks whether Internet connection is available
        /// </summary>
        public static bool IsInternetAvailable => System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();

        /// <summary>
        /// Opens the specified URL in the default web browser based on the operating system.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        /// <remarks>
        /// Currently only Windows, Linux and Mac are supported.
        /// </remarks>
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
}