using System;
using System.Linq;
using it_lab1_dbcs.Models;

namespace it_lab1_dbcs.Validation
{
    public class SetValidator : IFieldValidator
    {
        public ValidationResult Validate(string? value, FieldScheme scheme)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return ValidationResult.Success();
            }
            var selectedItems = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(s => s.Trim());

            foreach (var item in selectedItems)
            {
                if (!scheme.AllowedEnumValues.Contains(item, StringComparer.OrdinalIgnoreCase))
                {
                    return ValidationResult.Fail(
                        $"Значення '{item}' відсутнє серед дозволених елементів множини [{string.Join(", ", scheme.AllowedEnumValues)}]."
                    );
                }
            }

            return ValidationResult.Success();
        }
    }
}