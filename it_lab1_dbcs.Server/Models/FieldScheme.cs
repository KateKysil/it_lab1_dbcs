using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Models
{
    public class FieldScheme
    {
        public string Name { get; set; } = string.Empty;
        public FieldType Type { get; set; }

        /// <summary>
        /// Список допустимих значень, якщо Type == FieldType.Enum
        /// </summary>
        public List<string> AllowedEnumValues { get; set; } = new List<string>();

        public FieldScheme() { }

        public FieldScheme(string name, FieldType type, List<string>? allowedEnumValues = null)
        {
            Name = name;
            Type = type;
            if (allowedEnumValues != null)
            {
                AllowedEnumValues = allowedEnumValues;
            }
        }
    }
}
