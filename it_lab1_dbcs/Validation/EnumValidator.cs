using it_lab1_dbcs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Validation
{
    public class EnumValidator : IFieldValidator
    {
        public ValidationResult Validate(string? value, FieldScheme scheme)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ValidationResult.Fail($"Поле '{scheme.Name}' має містити вибране значення з переліку.");

            bool isAllowed = scheme.AllowedEnumValues.Any(v => v.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));

            if (!isAllowed)
            {
                string allowedList = string.Join(", ", scheme.AllowedEnumValues);
                return ValidationResult.Fail($"Значення '{value}' не допустиме для поля '{scheme.Name}'. Дозволені: [{allowedList}].");
            }

            return ValidationResult.Success();
        }
    }
}
