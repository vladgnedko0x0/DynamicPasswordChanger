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
            // Проверяем, был ли уже установлен наш сервис
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

            // Если сервис не был установлен, то создаем его
            if (!serviceExists)
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                 path += "\\Dynamic\\DynamicPasswordService.exe";
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.FileName = "sc";
                processInfo.Arguments = "create \"Dynamic Password Changer\" binPath= \"" + path + "\" start= auto"; // Запускать службу автоматически
                processInfo.Verb = "runas"; // Запустить от имени администратора

                ProcessStartInfo processInfoStart = new ProcessStartInfo();
                processInfoStart.FileName = "sc";
                processInfoStart.Arguments = "start \"Dynamic Password Changer\""; // Запускать службу автоматически
                processInfoStart.Verb = "runas"; // Запустить от имени администратора
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
