using Xunit;
using System.Collections.Generic;
using it_lab1_dbcs.Models;
using it_lab1_dbcs.Validation;

namespace it_lab1_dbcs.Test
{
    public class ValidationServiceTests
    {
        [Fact]
        public void ValidateRow_MultipleFields_ReturnsFailOnInvalidEmail()
        {
            var service = new ValidationService();
            var schema = new TableSchema
            {
                Fields = new List<FieldScheme>
                {
                    new FieldScheme { Name = "Age", Type = FieldType.Integer },
                    new FieldScheme { Name = "Email", Type = FieldType.Email },
                    new FieldScheme { Name = "Roles", Type = FieldType.Set, AllowedEnumValues = new List<string> { "Admin", "User" } }
                }
            };

            var invalidRow = new TableRow();
            invalidRow["Age"] = "25";
            invalidRow["Email"] = "invalid-email-address"; 
            invalidRow["Roles"] = "Admin";
            var result = service.ValidateRow(schema, invalidRow);
            Assert.False(result.IsValid);
            Assert.NotEmpty(result.ErrorMessage);
        }
    }
}