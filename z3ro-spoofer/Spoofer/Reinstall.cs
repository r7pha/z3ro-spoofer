using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace z3ro_spoofer.Spoofer
{
    public enum Target { None = 0, Bloxstrap, Fishstrap, RobloxInstaller }

    public class Pick { public Target which; public string path = ""; public bool valid; }

    public static class RobloxInstaller
    {
        private static Pick gCache = new Pick();
        private static readonly object gCacheMtx = new();

        private static string GetUser() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private static string GetSysDrive() => Path.GetPathRoot(Environment.SystemDirectory);

        private static List<string> GetNtfsDrives()
        {
            return DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Fixed && d.IsReady && d.DriveFormat.Equals("NTFS", StringComparison.OrdinalIgnoreCase))
                .Select(d => d.RootDirectory.FullName.TrimEnd('\\'))
                .ToList();
        }

        private static Pick? FallbackCommon(ref string diag)
        {
            string user = GetUser();
            string sys = GetSysDrive();

            var candidates = new List<(Target which, string path)>
            {
                (Target.Bloxstrap, Path.Combine(user, "AppData\\Local\\Bloxstrap\\Bloxstrap.exe")),
                (Target.Fishstrap, Path.Combine(user, "AppData\\Local\\Fishstrap\\Fishstrap.exe")),
                (Target.RobloxInstaller, Path.Combine(sys, "Program Files (x86)\\Roblox\\Versions\\RobloxPlayerInstaller.exe"))
            };

            foreach (var c in candidates)
            {
                if (File.Exists(c.path)) return new Pick { which = c.which, path = c.path, valid = true };
                diag += $"fallback not found: {c.path}\n";
            }
            return null;
        }

        private static Pick? FallbackShortcuts(ref string diag)
        {
            string user = GetUser();
            string sys = GetSysDrive();

            var shortcuts = new List<(Target which, string path)>
            {
                (Target.Bloxstrap, Path.Combine(sys, "ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\Bloxstrap.lnk")),
                (Target.Fishstrap, Path.Combine(sys, "ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\Fishstrap.lnk")),
                (Target.Bloxstrap, Path.Combine(user, "AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Bloxstrap.lnk")),
                (Target.Fishstrap, Path.Combine(user, "AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Fishstrap.lnk"))
            };

            foreach (var s in shortcuts)
            {
                if (!File.Exists(s.path)) { diag += $"shortcut missing: {s.path}\n"; continue; }
                string target = ResolveShortcut(s.path);
                if (!string.IsNullOrEmpty(target) && File.Exists(target)) return new Pick { which = s.which, path = target, valid = true };
                diag += $"shortcut target invalid: {s.path}\n";
            }
            return null;
        }

        private static string? ResolveShortcut(string lnkPath)
        {
            try
            {
                Type shell = Type.GetTypeFromProgID("WScript.Shell");
                dynamic wsh = Activator.CreateInstance(shell);
                dynamic shortcut = wsh.CreateShortcut(lnkPath);
                string? t = shortcut.TargetPath;
                Marshal.ReleaseComObject(shortcut);
                Marshal.ReleaseComObject(wsh);
                return t;
            }
            catch { return null; }
        }

        private static Pick GetPreferredCached(bool forceRescan, ref string diag)
        {
            lock (gCacheMtx)
            {
                if (gCache.valid && !forceRescan) { diag += $"using cached path: {gCache.path}\n"; return gCache; }

                var pick = FallbackShortcuts(ref diag) ?? FallbackCommon(ref diag);
                if (pick != null)
                {
                    gCache = pick;
                    diag += $"selected pick: {gCache.path}\n";
                    return gCache;
                }

                gCache = new Pick();
                diag += "no candidates found.\n";
                return gCache;
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            public string lpVerb;
            public string lpFile;
            public string lpParameters;
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIconOrMonitor;
            public IntPtr hProcess;
        }

        private const uint SEE_MASK_NOCLOSEPROCESS = 0x00000040;
        private const int SW_HIDE = 0;

        private static void LaunchSelected(string path, bool passPlayerArg, Action<int> onPid)
        {
            var sei = new SHELLEXECUTEINFO
            {
                cbSize = Marshal.SizeOf<SHELLEXECUTEINFO>(),
                fMask = SEE_MASK_NOCLOSEPROCESS,
                lpFile = path,
                lpParameters = passPlayerArg ? "-player" : null,
                nShow = SW_HIDE
            };

            if (ShellExecuteEx(ref sei) && sei.hProcess != IntPtr.Zero)
            {
                int pid = Process.GetProcessById((int)sei.hProcess).Id;
                onPid(pid);
                Thread.Sleep(3000);
            }
            else
            {
                LoggingService.Log($"ShellExecuteEx failed for {path}\n", 3);
            }
        }

        public static void Install()
        {
            string diag = "";
            var pick = GetPreferredCached(false, ref diag);

            if (!string.IsNullOrEmpty(diag)) Console.Error.WriteLine(diag);

            if (!pick.valid || pick.which == Target.None || string.IsNullOrEmpty(pick.path))
            {
                LoggingService.Log("Unable to find any bootstrapper.", 3);
                return;
            }

            bool needsPlayerArg = pick.which == Target.Bloxstrap || pick.which == Target.Fishstrap;
            LaunchSelected(pick.path, needsPlayerArg, pid => { /* no-op */ });
        }
    }
}
