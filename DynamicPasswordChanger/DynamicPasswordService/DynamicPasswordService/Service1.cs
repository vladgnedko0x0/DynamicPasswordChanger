using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DynamicPasswordService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Укажите путь к вашему исполняемому файлу
            string filePath=Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
             filePath += "\\Dynamic\\DynamicPassword.exe";

            // Создайте новый процесс
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = filePath;
            processInfo.UseShellExecute = true;
            processInfo.Verb = "runas"; // Запустить от имени администратора

            try
            {
                // Запустите процесс
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                Console.WriteLine($"An error occurred while starting the process: {ex.Message}");
            }
        }

        protected override void OnStop()
        {
            string processName = "DynamicPassword";

            // Попытайтесь найти процесс по его имени
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                // Завершите все найденные процессы
                foreach (var process in processes)
                {
                    process.Kill();
                }
            }
        }
    }
}
