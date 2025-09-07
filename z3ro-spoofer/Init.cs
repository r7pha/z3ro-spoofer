using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using z3ro_spoofer.Spoofer;

namespace z3ro_spoofer
{
    internal class Init
    {
        static void Main(string[] args)
        {
            Console.Title = "Z3ro";
            LoggingService.Divider("Info");
            LoggingService.Log("Current spoofer version: " + Settings.Version);

            Spoof.Autospoof();
        }
    }
}
