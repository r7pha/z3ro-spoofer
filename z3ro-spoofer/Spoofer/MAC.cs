using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Management;
namespace z3ro_spoofer.Spoofer
{
    class MAC
    {
        private static string Trim(string s)
        {
            return s.Trim();
        }

        private static string GetCurrentSsid()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = "wlan show interfaces",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.Unicode
            };

            var p = Process.Start(psi);
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            const string marker = "SSID                   : ";
            var idx = output.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx == -1) return string.Empty;

            var ssid = output.Substring(idx + marker.Length);
            ssid = ssid.Split('\n')[0];
            return Trim(ssid);
        }

        private static void BounceAdapter(string adapterName)
        {
            RunCmd($"netsh interface set interface name=\"{adapterName}\" admin=disable");
            Thread.Sleep(1000);
            RunCmd($"netsh interface set interface name=\"{adapterName}\" admin=enable");
            Thread.Sleep(2000);

            if (adapterName.Contains("Wi-Fi") || adapterName.Contains("Wireless"))
            {
                var ssid = GetCurrentSsid();
                if (!string.IsNullOrEmpty(ssid))
                {
                    RunCmd("netsh wlan disconnect");
                    RunCmd($"netsh wlan connect name=\"{ssid}\"");
                }
            }
        }

        private static void RunCmd(string cmd)
        {
            var psi = new ProcessStartInfo("cmd.exe", "/c " + cmd)
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(psi)?.WaitForExit();
        }

        private static List<string> GetAdapters()
        {
            var list = new List<string>();
            var mc = new ManagementClass("Win32_NetworkAdapter");
            foreach (ManagementObject mo in mc.GetInstances())
            {
                if (mo["Name"] != null)
                    list.Add(mo["Name"].ToString());
            }
            return list;
        }

        private static string ResolveAdapterGuid(string adapterName)
        {
            var searcher = new ManagementObjectSearcher(
                "SELECT * FROM Win32_NetworkAdapter WHERE Name='" + adapterName + "'");
            foreach (ManagementObject mo in searcher.Get())
            {
                if (mo["GUID"] != null)
                    return mo["GUID"].ToString();
            }
            return null;
        }

        private static string GetAdapterRegPath(string adapterGuid)
        {
            const string basePath = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}";
            var hKey = Registry.LocalMachine.OpenSubKey(basePath);
            if (hKey == null) return null;

            foreach (var sub in hKey.GetSubKeyNames())
            {
                var subKey = hKey.OpenSubKey(sub);
                if (subKey == null) continue;
                var id = subKey.GetValue("NetCfgInstanceId") as string;
                if (id != null && id.Equals(adapterGuid, StringComparison.OrdinalIgnoreCase))
                    return basePath + "\\" + sub;
            }
            return null;
        }

        public static void SpoofMac()
        {
            var adapters = GetAdapters();
            if (adapters.Count == 0) return;

            Parallel.ForEach(adapters, adapter =>
            {
                var guid = ResolveAdapterGuid(adapter);
                if (guid == null) return;

                var regPath = GetAdapterRegPath(guid);
                if (regPath == null) return;

                var newMac = EnvHelper.GenMac();
                var key = Registry.LocalMachine.OpenSubKey(regPath, true);
                if (key == null) return;

                key.SetValue("NetworkAddress", newMac, RegistryValueKind.String);

                LoggingService.Log($"Spoofed: {adapter}, new MAC: {newMac}");
                Task.Run(() => BounceAdapter(adapter));
            });
        }

        public static void Run()
        {
            EnvHelper.SectHeader("MAC Spoofing", 196);
            SpoofMac();
        }
    }

}
