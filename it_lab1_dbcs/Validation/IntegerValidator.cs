using it_lab1_dbcs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Validation
{
    public class IntegerValidator : IFieldValidator
    {
        public ValidationResult Validate(string? value, FieldScheme scheme)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ValidationResult.Fail($"Поле '{scheme.Name}' не може бути порожнім.");

            if (!long.TryParse(value.Trim(), out _))
                return ValidationResult.Fail($"Поле '{scheme.Name}' має бути цілим числом (наприклад: 42, -10).");

            return ValidationResult.Success();
        }
    }
}
