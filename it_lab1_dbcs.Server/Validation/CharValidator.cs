using it_lab1_dbcs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Validation
{
    public class CharValidator : IFieldValidator
    {
        public ValidationResult Validate(string? value, FieldScheme scheme)
        {
            if (string.IsNullOrEmpty(value))
                return ValidationResult.Fail($"Поле '{scheme.Name}' не може бути порожнім.");

            if (value.Length != 1)
                return ValidationResult.Fail($"Поле '{scheme.Name}' має містити рівно один символ.");

            return ValidationResult.Success();
        }
    }
}
