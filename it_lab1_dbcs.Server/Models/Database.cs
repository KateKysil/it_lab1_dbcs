using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Models
{
    public class Database
    {
        public string Name { get; set; } = "NewDatabase";

        /// <summary>
        /// Логін користувача, який створив і володіє цією БД
        /// </summary>
        public string OwnerUsername { get; set; } = string.Empty;

        public ObservableCollection<Table> Tables { get; set; } = new ObservableCollection<Table>();

        public Database() { }

        public Database(string name, string ownerUsername)
        {
            Name = name;
            OwnerUsername = ownerUsername;
        }

        public void AddTable(Table table)
        {
            if (Tables.Any(t => t.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Таблиця з назвою '{table.Name}' вже існує у базі даних.");
            }
            Tables.Add(table);
        }

        public bool RemoveTable(string tableName)
        {
            var table = Tables.FirstOrDefault(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (table != null)
            {
                return Tables.Remove(table);
            }
            return false;
        }
        public void RenameTable(string oldName, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Назва таблиці не може бути порожньою.");

            if (oldName.Equals(newName, StringComparison.OrdinalIgnoreCase))
                return;

            if (Tables.Any(t => t.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Таблиця з назвою '{newName}' вже існує.");

            var table = GetTable(oldName);
            if (table != null)
            {
                table.Name = newName;
            }
        }
        public Table? GetTable(string tableName)
        {
            return Tables.FirstOrDefault(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
