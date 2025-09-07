using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using z3ro_spoofer;

namespace z3ro_spoofer.Spoofer
{
    public class RegSpoofer
    {
        public static void Run()
        {
            if (!Guid())
                LoggingService.Log("Failed to spoof GUID.", 3);

            if (!Users())
                LoggingService.Log("Failed to spoof User Info.", 3);

            if (!Edid())
                LoggingService.Log("Failed to spoof EDID.", 3);
        }

        public static bool Guid()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography", writable: true))
                {
                    if (key == null)
                        return false;

                    string newGuid = EnvHelper.GenGuid();
                    key.SetValue("MachineGuid", newGuid, RegistryValueKind.String);
                    LoggingService.Log("Spoofed GUID, new GUID: " + newGuid);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool Users()
        {
            try
            {
                string spoofedUser = EnvHelper.GenUsers();

                var userInfo = new List<(string path, string value)>
                {
                    (@"SOFTWARE\Microsoft\Windows\CurrentVersion", "RegisteredOwner"),
                    (@"SOFTWARE\Microsoft\Windows\CurrentVersion", "LastLoggedOnUser")
                };

                foreach (var (path, value) in userInfo)
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(path, writable: true))
                    {
                        if (key == null)
                            return false;

                        key.SetValue(value, spoofedUser, RegistryValueKind.String);
                        LoggingService.Log($"Spoofed: {value}, new value: {spoofedUser}");
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Edid()
        {
            try
            {
                using (var displayKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\DISPLAY", writable: true))
                {
                    if (displayKey == null)
                    {
                        LoggingService.Log("Failed to spoof DISPLAY reg info.", 3);
                        return false;
                    }

                    bool spoofed = false;

                    foreach (var subKeyName in displayKey.GetSubKeyNames())
                    {
                        using (var deviceKey = displayKey.OpenSubKey(subKeyName, writable: true))
                        {
                            if (deviceKey == null)
                                continue;

                            foreach (var deviceSubKeyName in deviceKey.GetSubKeyNames())
                            {
                                string deviceSubKeyPath = subKeyName + "\\" + deviceSubKeyName;

                                var edidPaths = new List<string>
                                {
                                    deviceSubKeyPath + "\\Device Parameters",
                                    deviceSubKeyPath + "\\Control\\Device Parameters",
                                    deviceSubKeyPath + "\\Monitor\\Device Parameters"
                                };

                                foreach (var edidPath in edidPaths)
                                {
                                    using (var edidKey = displayKey.OpenSubKey(edidPath, writable: true))
                                    {
                                        if (edidKey == null)
                                            continue;

                                        byte[] spoofedEdid = new byte[128];
                                        new Random().NextBytes(spoofedEdid);

                                        try
                                        {
                                            edidKey.SetValue("EDID", spoofedEdid, RegistryValueKind.Binary);
                                            string newId = EnvHelper.GenEdid();
                                            LoggingService.Log($"Spoofed EDID: {deviceSubKeyName}, new ID: {newId}");
                                            spoofed = true;
                                        }
                                        catch
                                        {
                                            LoggingService.Log("Failed to set EDID value @ " + edidPath, 3);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!spoofed)
                        LoggingService.Log("No EDIDs were spoofed sir.", 2);

                    return spoofed;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

