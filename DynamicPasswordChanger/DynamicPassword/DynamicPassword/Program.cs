using System;
using System.DirectoryServices.AccountManagement;

class Program
{
    static void Main(string[] args)
    {
        // Specify the username for which you want to change the password
        string username = "test";

        // Set the new password to the current date in the format "yyyyMMdd" (e.g., "20240527" for May 27, 2024)
        string newPassword = DateTime.Now.ToString("yyyyMMdd");

        try
        {
            // Creating a principal context
            using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
            {
                // Searching for the user
                UserPrincipal user = UserPrincipal.FindByIdentity(context, username);

                if (user != null)
                {
                    // Setting the new password
                    user.SetPassword(newPassword);
                    user.Save();

                    Console.WriteLine($"Password for user {username} successfully changed to the current date.");
                }
                else
                {
                    Console.WriteLine("User not found.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error changing password: " + ex.Message);
        }
    }
}
