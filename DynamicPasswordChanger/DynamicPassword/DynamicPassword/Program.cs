using System;
using System.DirectoryServices.AccountManagement;

class Program
{
    static void Main(string[] args)
    {
        // Укажите имя пользователя, для которого нужно изменить пароль
        string username = "test";

        // Установите новый пароль на текущую дату в формате "yyyyMMdd" (например, "20240527" для 27 мая 2024 года)
        string newPassword = DateTime.Now.ToString("yyyyMMdd");

        try
        {
            // Создание контекста принципала
            using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
            {
                // Поиск пользователя
                UserPrincipal user = UserPrincipal.FindByIdentity(context, username);

                if (user != null)
                {
                    // Установка нового пароля
                    user.SetPassword(newPassword);
                    user.Save();

                    Console.WriteLine($"Пароль для пользователя {username} успешно изменен на текущую дату.");
                }
                else
                {
                    Console.WriteLine("Пользователь не найден.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при изменении пароля: " + ex.Message);
        }
    }
}
