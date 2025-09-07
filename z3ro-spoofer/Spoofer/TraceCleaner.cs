using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace z3ro_spoofer.Spoofer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    struct PathHelper
    {
        public static string User() => EnvHelper.GetUser();
        public static string Sys() => EnvHelper.GetSysDrive();
    }

    class Cleaner
    {
        static void CleanVers(string baseDir)
        {
            if (!Directory.Exists(baseDir)) return;
            foreach (var d in Directory.EnumerateDirectories(baseDir))
            {
                var name = Path.GetFileName(d);
                if (name.StartsWith("version-"))
                {
                    EnvHelper.BulkDelete(d, new List<string>
                {
                    "RobloxPlayerBeta.exe",
                    "RobloxPlayerBeta.dll",
                    "RobloxCrashHandler.exe",
                    "RobloxPlayerLauncher.exe"
                });
                }
            }
        }

        static void RmvReferents(string filePath, string itemClass)
        {
            if (!File.Exists(filePath))
            {
                LoggingService.Log("File does not exists: " + filePath, 2);
                return;
            }

            string content = File.ReadAllText(filePath);

            string pattern = $"<Item class=\\\"{Regex.Escape(itemClass)}\\\" referent=\\\"[^\"]+\\\">";
            string replacement = $"<Item class=\\\"{itemClass}\\\" referent=\\\"{EnvHelper.GenRand()}\\\">";

            content = Regex.Replace(content, pattern, replacement);
            File.WriteAllText(filePath, content);
            LoggingService.Log("Successfully wrote to: " + filePath);
        }


        public static void CleanRbx()
        {
            LoggingService.Log("Cleaning traces: Searching for Roblox versions...");
            var userBase = Path.Combine(PathHelper.User(), "AppData", "Local", "Roblox");

            var robLnk = Path.Combine(PathHelper.Sys(), "Program Files (x86)", "Roblox");
            if (robLnk != null)
            {
                LoggingService.Log("Cleaning traces: Cleaning Roblox (native)...");
                CleanVers(Directory.GetParent(Directory.GetParent(robLnk).FullName).FullName);
            }
                

            CleanVers(Path.Combine(PathHelper.Sys(), "Program Files (x86)", "Roblox", "Versions"));

            var bxLnk = Path.Combine(PathHelper.User(), "AppData", "Local", "Bloxstrap");
            if (bxLnk != null)
            {
                LoggingService.Log("Cleaning traces: Cleaning Bloxstrap...");
                CleanVers(Path.Combine(bxLnk, "Versions"));
            }

            CleanVers(Path.Combine(PathHelper.User(), "AppData", "Local", "Bloxstrap", "Versions"));

            var fsLnk = Path.Combine(PathHelper.User(), "AppData", "Local", "Fishstrap");
            if (fsLnk != null)
            {
                LoggingService.Log("Cleaning traces: Cleaning Fishstrap...");
                CleanVers(Path.Combine(fsLnk, "Versions"));
            }
                

            CleanVers(Path.Combine(PathHelper.User(), "AppData", "Local", "Fishstrap", "Versions"));

            List<string> dirsToDelete = new List<string>
        {
            "Temp/Roblox",
            "Roblox/logs",
            "Roblox/LocalStorage",
            "Roblox/Downloads",
            "Roblox/ClientSettings",
            "Roblox/rbx-storage",
            "Roblox/Versions"
        };

            foreach (var sub in dirsToDelete)
            {
                string dirPath = Path.Combine(PathHelper.User(), "AppData", "Local", sub);
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                    LoggingService.Log("Deleted: " + dirPath);
                }
            }

            List<string> filesToDelete = new List<string>
        {
            "rbx-storage.db",
            "rbx-storage.db-shm",
            "rbx-storage.db-wal",
            "rbx-storage.id",
            "frm.cfg"
        };

            foreach (var file in filesToDelete)
            {
                string filePath = Path.Combine(userBase, file);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    LoggingService.Log("Deleted: " + filePath);
                }
            }
            LoggingService.Log("Cleaning referents...");
            RmvReferents(Path.Combine(userBase, "GlobalBasicSettings_13.xml"), "UserGameSettings");
            RmvReferents(Path.Combine(userBase, "GlobalSettings_13.xml"), "UserGameSettings");
            RmvReferents(Path.Combine(userBase, "AnalysticsSettings.xml"), "GoogleAnalyticsConfiguration");
        }
    }

}
