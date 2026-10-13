using it_lab1_dbcs.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Validation
{
    public class RealValidator : IFieldValidator
    {
        public ValidationResult Validate(string? value, FieldScheme scheme)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ValidationResult.Fail($"Поле '{scheme.Name}' не може бути порожнім.");
            string normalizedValue = value.Trim().Replace(',', '.');

            if (!double.TryParse(normalizedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                return ValidationResult.Fail($"Поле '{scheme.Name}' має бути дійсним числом.");

            return ValidationResult.Success();
        }
    }
}
