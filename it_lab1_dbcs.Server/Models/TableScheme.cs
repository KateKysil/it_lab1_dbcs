using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Models
{
    public class TableSchema
    {
        public List<FieldScheme> Fields { get; set; } = new List<FieldScheme>();

        public void AddField(FieldScheme field)
        {
            if (Fields.Any(f => f.Name.Equals(field.Name, System.StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Поле з назвою '{field.Name}' вже існує в цій таблиці.");
            }
            Fields.Add(field);
        }

        public FieldScheme? GetField(string fieldName)
        {
            return Fields.FirstOrDefault(f => f.Name.Equals(fieldName, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
