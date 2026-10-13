using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using it_lab1_dbcs.Models;
using it_lab1_dbcs.Security;

namespace it_lab1_dbcs.Storage
{
    public class UserManager
    {
        private const string UsersFileName = "users.json";
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Поточний авторизований користувач у системі
        /// </summary>
        public static User? CurrentUser { get; private set; }

        public UserManager()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        /// <summary>
        /// Реєстрація нового користувача
        /// </summary>
        public (bool success, string message) Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Логін та пароль не можуть бути порожніми.");
            }

            var users = LoadAllUsers();

            if (users.Any(u => u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Користувач з таким логіном вже існує.");
            }

            var (hash, salt) = PasswordHasher.HashPassword(password);
            var newUser = new User(username.Trim(), hash, salt);

            users.Add(newUser);
            SaveAllUsers(users);

            return (true, "Реєстрація успішна! Тепер ви можете увійти.");
        }

        /// <summary>
        /// Авторизація користувача в системі
        /// </summary>
        public (bool success, string message) Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Заповніть усі поля.");
            }

            var users = LoadAllUsers();
            var user = users.FirstOrDefault(u => u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return (false, "Користувача з таким логіном не знайдено.");
            }

            bool isPasswordValid = PasswordHasher.VerifyPassword(password, user.PasswordHash, user.Salt);
            if (!isPasswordValid)
            {
                return (false, "Невірний пароль.");
            }

            CurrentUser = user;
            return (true, $"Ласкаво просимо, {user.Username}!");
        }

        /// <summary>
        /// Вихід з облікового запису
        /// </summary>
        public static void Logout()
        {
            CurrentUser = null;
        }

        private List<User> LoadAllUsers()
        {
            if (!File.Exists(UsersFileName))
            {
                return new List<User>();
            }

            try
            {
                string json = File.ReadAllText(UsersFileName);
                return JsonSerializer.Deserialize<List<User>>(json, _jsonOptions) ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        private void SaveAllUsers(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, _jsonOptions);
            File.WriteAllText(UsersFileName, json);
        }
    }
}