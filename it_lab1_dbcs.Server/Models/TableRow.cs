using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Models
{
    public class TableRow
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Словник: Назва колонки -> Значення
        /// </summary>
        public Dictionary<string, string?> Values { get; set; } = new Dictionary<string, string?>();

        public TableRow() { }

        public TableRow(Dictionary<string, string?> initialValues)
        {
            Values = new Dictionary<string, string?>(initialValues);
        }

        /// <summary>
        /// Зручний індексатор для швидкого доступу до значень за назвою поля
        /// </summary>
        public string? this[string fieldName]
        {
            get => Values.TryGetValue(fieldName, out var value) ? value : null;
            set => Values[fieldName] = value;
        }
    }
}
