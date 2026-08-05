// FIX: Password change logic moved directly into OnStart.
// The original code launched DynamicPassword.exe with UseShellExecute = true,
// which fails in Windows Session 0 (the non-interactive service session).
// Running as SYSTEM already has the privileges needed to call SetPassword —
// no subprocess or runas is required.

using System;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Diagnostics;
// Upewnij się, że projekt ma dodane odwołanie do zestawu System.DirectoryServices.AccountManagement.dll.
// W Visual Studio: kliknij prawym przyciskiem myszy na "References" → "Add Reference..." → "Assemblies" → "Framework" → zaznacz "System.DirectoryServices.AccountManagement" → OK.

// Poprawka nie wymaga zmian w kodzie źródłowym, ale wymaga dodania odwołania do odpowiedniego zestawu (DLL) w projekcie.
using System.ServiceProcess;


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
            try
            {
                // FIX: Read username from App.config instead of hardcoding "test".
                // Add <add key="TargetUsername" value="YourWindowsUsername"/> to App.config.
                string username = ConfigurationManager.AppSettings["TargetUsername"]
                                  ?? Environment.UserName;

                // Password formula: current date as yyyyMMdd (e.g. "20240908")
                // The user always knows the password = today's date.
                string newPassword = DateTime.Now.ToString("yyyyMMdd");

                using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(context, username);

                    if (user != null)
                    {
                        user.SetPassword(newPassword);
                        user.Save();

                        EventLog.WriteEntry(
                            "DynamicPasswordChanger",
                            $"Password for '{username}' changed successfully.",
                            EventLogEntryType.Information);
                    }
                    else
                    {
                        EventLog.WriteEntry(
                            "DynamicPasswordChanger",
                            $"User '{username}' not found. Password was not changed.",
                            EventLogEntryType.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(
                    "DynamicPasswordChanger",
                    $"Failed to change password: {ex.Message}",
                    EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            // Nothing to clean up — no subprocess is launched anymore.
        }
    }
}
