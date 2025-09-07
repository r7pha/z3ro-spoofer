using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace z3ro_spoofer
{
    public static class EnvHelper
    {
        public static string GetUser()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        public static string GetSysDrive()
        {
            return Path.GetPathRoot(Environment.SystemDirectory);
        }
        public static void BulkDelete(string dirPath, List<string> filePatterns)
        {
            if (!Directory.Exists(dirPath)) return;
            foreach (var entry in Directory.GetFiles(dirPath))
            {
                var filename = Path.GetFileName(entry);
                foreach (var pattern in filePatterns)
                {
                    if (filename == pattern)
                    {
                        File.Delete(entry);
                        LoggingService.Log("Deleted: " + entry);
                    }
                }
            }
        }

        public static string GenRand()
        {
            return Guid.NewGuid().ToString();
        }
        public static string ToUtf8(string s)
        {
            if (s == null) return null;
            var bytes = Encoding.UTF8.GetBytes(s);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string StringToWString(string s)
        {
            return s;
        }

        private static readonly ThreadLocal<Random> tlRandom = new ThreadLocal<Random>(() => new Random());

        public static string GenRand(int length = 12)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var r = tlRandom.Value;
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++) sb.Append(chars[r.Next(chars.Length)]);
            return sb.ToString();
        }

        public static string GenUsers()
        {
            string[] users = { "Operator", "Admin", "Administrator", "OP" };
            return users[tlRandom.Value.Next(users.Length)];
        }

        public static string GenMac()
        {
            var r = tlRandom.Value;
            byte[] mac = new byte[6];
            r.NextBytes(mac);
            mac[0] = (byte)((mac[0] & 0xFE) | 0x02);
            return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}", mac[0], mac[1], mac[2], mac[3], mac[4], mac[5]);
        }

        public static string GenGuid()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }

        public static string GenSerial()
        {
            var r = tlRandom.Value;
            var sb = new StringBuilder(12);
            for (int i = 0; i < 12; i++) sb.Append((char)('0' + r.Next(10)));
            return sb.ToString();
        }

        public static string GenBaseBoardManufacturer()
        {
            string[] m = { "ASUSTeK COMPUTER INC.", "MSI", "Gigabyte Technology Co., Ltd.", "Dell Inc.", "Hewlett-Packard" };
            return m[tlRandom.Value.Next(m.Length)];
        }

        public static string GenSystemManufacturer()
        {
            string[] m = { "Dell Inc.", "Lenovo", "Hewlett-Packard", "ASUSTeK COMPUTER INC.", "Acer Inc.", "MSI", "Samsung Electronics" };
            return m[tlRandom.Value.Next(m.Length)];
        }

        public static string GenBiosVersion()
        {
            var r = tlRandom.Value;
            return $"{r.Next(1, 10)}.{r.Next(0, 10)}.{r.Next(0, 10)}";
        }

        public static string GenBiosReleaseDate()
        {
            var r = tlRandom.Value;
            var now = DateTime.Now;
            int daysBack = r.Next(0, 365 * 5);
            var d = now.AddDays(-daysBack);
            return d.ToString("yyyy-MM-dd");
        }

        public static string GenEdid()
        {
            var r = tlRandom.Value;
            int a = r.Next(0, 0xFFFF);
            int b = r.Next(0, 0xFFFF);
            int uid = r.Next(10000, 100000);
            return $"5&{a:X}{b:X}&0&UID{uid}";
        }

        public static string RndWindName(int length = 24)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var r = tlRandom.Value;
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++) sb.Append(chars[r.Next(chars.Length)]);
            return sb.ToString();
        }

        public static void BulkDelete(string dirPath, IEnumerable<string> filePatterns)
        {
            if (!Directory.Exists(dirPath)) return;
            var files = Directory.EnumerateFiles(dirPath);
            var patterns = new HashSet<string>(filePatterns);
            foreach (var f in files)
            {
                var name = Path.GetFileName(f);
                if (patterns.Contains(name))
                {
                    try { File.Delete(f);LoggingService.Log($"Deleted: {f}"); } catch { }
                }
            }
        }

        public static string ResolveTarget(string shortcutPath)
        {
            if (!File.Exists(shortcutPath)) throw new Exception("Shortcut file does not exist: " + shortcutPath);
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) throw new Exception("WScript.Shell not available");
            dynamic shell = Activator.CreateInstance(shellType);
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            string target = (string)shortcut.TargetPath;
            Marshal.ReleaseComObject(shortcut);
            Marshal.ReleaseComObject(shell);
            return target;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, int BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        private const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const uint TOKEN_QUERY = 0x0008;
        private const uint SE_PRIVILEGE_ENABLED = 0x2;

        public static bool TsAdjustAccess()
        {
            if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out var token)) return false;
            foreach (var priv in new[] { "SeDebugPrivilege", "SeBackupPrivilege", "SeRestorePrivilege" })
            {
                if (!LookupPrivilegeValue(null, priv, out var luid)) { CloseHandle(token); return false; }
                var tp = new TOKEN_PRIVILEGES { PrivilegeCount = 1, Privileges = new LUID_AND_ATTRIBUTES { Luid = luid, Attributes = SE_PRIVILEGE_ENABLED } };
                if (!AdjustTokenPrivileges(token, false, ref tp, Marshal.SizeOf<TOKEN_PRIVILEGES>(), IntPtr.Zero, IntPtr.Zero)) { CloseHandle(token); return false; }
                if (Marshal.GetLastWin32Error() != 0) { CloseHandle(token); return false; }
            }
            CloseHandle(token);
            return true;
        }

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        public static void SetWindow()
        {
            var rndName = RndWindName();
            Console.Title = rndName;
        }

        public static void ForceCloseHandles(Process proc)
        {
            try
            {
                if (proc == null || proc.HasExited) return;
                proc.CloseMainWindow();
                if (!proc.WaitForExit(2000)) proc.Kill();
            }
            catch { }
        }

        public static void TerminateRoblox()
        {
            SetWindow();
            string[] names = { "RobloxPlayerBeta", "RobloxCrashHandler", "Bloxstrap", "RobloxStudioBetaLauncher", "RobloxStudioBeta" };
            var procs = Process.GetProcesses();
            var toWait = new List<Process>();
            foreach (var p in procs)
            {
                try
                {
                    if (names.Any(n => string.Equals(n, p.ProcessName, StringComparison.OrdinalIgnoreCase)))
                    {
                        ForceCloseHandles(p);
                        toWait.Add(p);
                    }
                }
                catch { }
            }
            foreach (var p in toWait)
            {
                try { p.WaitForExit(); } catch { }
                try { p.Dispose(); } catch { }
            }
        }


        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    }

}
