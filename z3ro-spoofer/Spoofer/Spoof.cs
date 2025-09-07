using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace z3ro_spoofer.Spoofer
{
    public class Spoof
    {
        public static void Autospoof()
        {
            // Trace Section
            LoggingService.Divider("Trace Cleaner");
            CleanTraces();

            // Registry Section
            LoggingService.Divider("Registry Spoofer");
            RegSpoofer.Run();

            // Registry Section
            LoggingService.Divider("MAC Spoofer");
            MAC.SpoofMac();

            // Reinstall Section
            LoggingService.Divider("Roblox Reinstall");
            RobloxInstaller.Install();

            // Finish Section
            LoggingService.Divider("Finish");
            LoggingService.Log("Everything was spoofed successfully.");
            
        }
        public static void CleanTraces()
        {
            Cleaner.CleanRbx();
        }
    }
}
