using it_lab1_dbcs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Validation
{
    public class ValidationService
    {
        private readonly Dictionary<FieldType, IFieldValidator> _validators;

        public ValidationService()
        {
            _validators = new Dictionary<FieldType, IFieldValidator>
            {
                { FieldType.Integer, new IntegerValidator() },
                { FieldType.Real, new RealValidator() },
                { FieldType.Char, new CharValidator() },
                { FieldType.String, new StringValidator() },
                { FieldType.Enum, new EnumValidator() },
                { FieldType.Email, new EmailValidator() },
                { FieldType.Set, new SetValidator() }
            };
        }

        /// <summary>
        /// Валідує значення одного конкретного поля
        /// </summary>
        public ValidationResult ValidateField(FieldScheme scheme, string? value)
        {
            if (_validators.TryGetValue(scheme.Type, out var validator))
            {
                return validator.Validate(value, scheme);
            }

            return ValidationResult.Fail($"Невідомий тип поля '{scheme.Type}'.");
        }

        /// <summary>
        /// Валідує повністю весь рядок перед збереженням у таблицю
        /// </summary>
        public ValidationResult ValidateRow(TableSchema schema, TableRow row)
        {
            foreach (var field in schema.Fields)
            {
                string? value = row[field.Name];
                var result = ValidateField(field, value);

                if (!result.IsValid)
                {
                    return result; 
                }
            }

            return ValidationResult.Success();
        }
    }
}
