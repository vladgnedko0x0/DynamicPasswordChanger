using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPasswordService_Uploader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Check if our service is already installed
            ServiceController[] services = ServiceController.GetServices();
            bool serviceExists = false;
            foreach (ServiceController service in services)
            {
                if (service.ServiceName == "Dynamic Password Changer")
                {
                    serviceExists = true;
                    break;
                }
            }

            // If the service is not installed, create it
            if (!serviceExists)
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                path += "\\Dynamic\\DynamicPasswordService.exe";
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.FileName = "sc";
                processInfo.Arguments = "create \"Dynamic Password Changer\" binPath= \"" + path + "\" start= auto"; // Set service to start automatically
                processInfo.Verb = "runas"; // Run as administrator

                ProcessStartInfo processInfoStart = new ProcessStartInfo();
                processInfoStart.FileName = "sc";
                processInfoStart.Arguments = "start \"Dynamic Password Changer\""; // Start the service
                processInfoStart.Verb = "runas"; // Run as administrator
                try
                {
                    Process.Start(processInfo);
                    Process.Start(processInfoStart);
                    Console.WriteLine("Service created successfully.");
                    Console.ReadKey();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the service: {ex.Message}");
                }
            }
        }
    }
}
