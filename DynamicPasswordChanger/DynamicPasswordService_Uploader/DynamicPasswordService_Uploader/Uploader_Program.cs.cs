// DynamicPasswordService_Uploader
// Installs "Dynamic Password Changer" as an auto-start Windows Service.
//
// FIX 1 (Race condition): The original code called Process.Start for both
//   "sc create" and "sc start" without waiting for the first to finish.
//   "sc start" would fire before the service was registered, causing a
//   "service not found" error. Now we call WaitForExit() between them.
//
// FIX 2 (Path): The service executable path is now read from the same
//   directory as this installer, instead of being hardcoded to Desktop.

using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace DynamicPasswordService_Uploader
{
    internal class Program
    {
        private const string ServiceName = "Dynamic Password Changer";

        static void Main(string[] args)
        {
            // Check whether the service is already installed
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController svc in services)
            {
                if (svc.ServiceName == ServiceName)
                {
                    Console.WriteLine($"Service '{ServiceName}' is already installed.");
                    Console.ReadKey();
                    return;
                }
            }

            // FIX: Resolve the service executable path relative to this installer,
            // not hardcoded to the Desktop (which is wrong in a SYSTEM service context).
            string installerDir = AppDomain.CurrentDomain.BaseDirectory;
            string servicePath  = Path.Combine(installerDir, "DynamicPasswordService.exe");

            if (!File.Exists(servicePath))
            {
                Console.WriteLine($"Error: '{servicePath}' not found.\n" +
                                  "Place DynamicPasswordService.exe in the same folder as this installer.");
                Console.ReadKey();
                return;
            }

            // Step 1 — Create the service
            Console.WriteLine("Creating service...");
            var createInfo = new ProcessStartInfo
            {
                FileName  = "sc",
                Arguments = $"create \"{ServiceName}\" " +
                            $"binPath= \"{servicePath}\" start= auto",
                Verb             = "runas",
                UseShellExecute  = true,
                CreateNoWindow   = false
            };

            using (var createProcess = Process.Start(createInfo))
            {
                // FIX: Wait for "sc create" to complete before calling "sc start".
                // The original code launched both asynchronously, so "sc start"
                // would often run before the service was registered.
                createProcess.WaitForExit();

                if (createProcess.ExitCode != 0)
                {
                    Console.WriteLine($"'sc create' failed with exit code {createProcess.ExitCode}.");
                    Console.ReadKey();
                    return;
                }
            }

            // Brief pause to let the SCM register the new service entry
            Thread.Sleep(500);

            // Step 2 — Start the service
            Console.WriteLine("Starting service...");
            var startInfo = new ProcessStartInfo
            {
                FileName        = "sc",
                Arguments       = $"start \"{ServiceName}\"",
                Verb            = "runas",
                UseShellExecute = true,
                CreateNoWindow  = false
            };

            using (var startProcess = Process.Start(startInfo))
            {
                startProcess.WaitForExit();

                if (startProcess.ExitCode != 0)
                {
                    Console.WriteLine($"'sc start' failed with exit code {startProcess.ExitCode}.");
                }
                else
                {
                    Console.WriteLine($"Service '{ServiceName}' installed and started successfully.");
                }
            }

            Console.ReadKey();
        }
    }
}
