using it_lab1_dbcs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Validation
{
    public class EmailValidator : IFieldValidator
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ValidationResult Validate(string? value, FieldScheme scheme)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ValidationResult.Fail($"Поле '{scheme.Name}' не може бути порожнім.");

            if (!EmailRegex.IsMatch(value.Trim()))
                return ValidationResult.Fail($"Поле '{scheme.Name}' містить некоректну e-mail адресу (наприклад: user@domain.com).");

            return ValidationResult.Success();
        }
    }
}
