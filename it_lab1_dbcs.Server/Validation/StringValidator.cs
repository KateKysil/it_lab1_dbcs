using it_lab1_dbcs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Validation
{
    public class StringValidator : IFieldValidator
    {
        public ValidationResult Validate(string? value, FieldScheme scheme)
        {
            if (value == null)
                return ValidationResult.Fail($"Поле '{scheme.Name}' не може бути null.");

            return ValidationResult.Success();
        }
    }
}
