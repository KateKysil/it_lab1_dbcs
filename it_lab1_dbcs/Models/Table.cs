using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Models
{
    public class Table
    {
        public string Name { get; set; } = string.Empty;
        public TableSchema Schema { get; set; } = new TableSchema();
        public ObservableCollection<TableRow> Rows { get; set; } = new ObservableCollection<TableRow>();

        public Table() { }

        public Table(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Сортування записів таблиці за вказаним полем
        /// </summary>
        public void SortByField(string fieldName, bool ascending = true)
        {
            var fieldScheme = Schema.GetField(fieldName);
            if (fieldScheme == null)
                throw new ArgumentException($"Поле '{fieldName}' не знайдено в схемі таблиці.");

            List<TableRow> sortedList;

            // Сортування з урахуванням типу даних
            switch (fieldScheme.Type)
            {
                case FieldType.Integer:
                    sortedList = ascending
                        ? Rows.OrderBy(r => long.TryParse(r[fieldName], out var v) ? v : long.MinValue).ToList()
                        : Rows.OrderByDescending(r => long.TryParse(r[fieldName], out var v) ? v : long.MinValue).ToList();
                    break;

                case FieldType.Real:
                    sortedList = ascending
                        ? Rows.OrderBy(r => double.TryParse(r[fieldName], out var v) ? v : double.MinValue).ToList()
                        : Rows.OrderByDescending(r => double.TryParse(r[fieldName], out var v) ? v : double.MinValue).ToList();
                    break;

                case FieldType.Char:
                case FieldType.String:
                case FieldType.Enum:
                case FieldType.Email:
                default:
                    sortedList = ascending
                        ? Rows.OrderBy(r => r[fieldName] ?? string.Empty, StringComparer.OrdinalIgnoreCase).ToList()
                        : Rows.OrderByDescending(r => r[fieldName] ?? string.Empty, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
            }

            Rows.Clear();
            foreach (var row in sortedList)
            {
                Rows.Add(row);
            }
        }
    }
}
