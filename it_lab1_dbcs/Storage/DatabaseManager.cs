using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using it_lab1_dbcs.Models;

namespace it_lab1_dbcs.Storage
{
    public class DatabaseManager
    {
        private const string DatabasesFolder = "Databases";
        private readonly JsonSerializerOptions _jsonOptions;

        public DatabaseManager()
        {
            _jsonOptions = new JsonSerializerOptions{ WriteIndented = true };
            if (!Directory.Exists(DatabasesFolder))
            {
                Directory.CreateDirectory(DatabasesFolder);
            }
        }

        /// <summary>
        /// Збереження бази даних у JSON-файл
        /// </summary>
        public void SaveDatabase(Database database, string? customFilePath = null)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            string filePath = customFilePath ?? Path.Combine(DatabasesFolder, $"{database.Name}.json");
            string json = JsonSerializer.Serialize(database, _jsonOptions);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Зчитати базу даних із файлу
        /// </summary>
        public Database LoadDatabase(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл бази даних не знайдено.", filePath);
            }
            string json = File.ReadAllText(filePath);
            var database = JsonSerializer.Deserialize<Database>(json, _jsonOptions);

            if (database == null)
            {
                throw new InvalidOperationException("Не вдалося десеріалізувати файл бази даних.");
            }
            return database;
        }

        /// <summary>
        /// Отримати список файлів баз даних, що належать поточному користувачу
        /// </summary>
        public List<string> GetUserDatabaseFiles(string ownerUsername)
        {
            var result = new List<string>();

            if (!Directory.Exists(DatabasesFolder))
                return result;
            var files = Directory.GetFiles(DatabasesFolder, "*.json");

            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("OwnerUsername", out var ownerProp))
                    {
                        string? owner = ownerProp.GetString();
                        if (string.Equals(owner, ownerUsername, StringComparison.OrdinalIgnoreCase))
                        {
                            result.Add(file);
                        }
                    }
                }
                catch
                {
                    // Пропускаємо пошкоджені або сторонні файли
                }
            }

            return result;
        }

        /// <summary>
        /// Видалити файл бази даних з диску
        /// </summary>
        public bool DeleteDatabaseFile(string databaseName)
        {
            string filePath = Path.Combine(DatabasesFolder, $"{databaseName}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
    }
}