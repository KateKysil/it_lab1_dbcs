using Xunit;
using System.Collections.Generic;
using it_lab1_dbcs.Models;
using it_lab1_dbcs.Validation;

namespace it_lab1_dbcs.Test
{
    public class SetValidationTests
    {
        [Fact]
        public void SetValidator_ValidatesAllowedAndInvalidItems()
        {
            var validator = new SetValidator();
            var scheme = new FieldScheme
            {
                Name = "Skills",
                Type = FieldType.Set,
                AllowedEnumValues = new List<string> { "C#", "SQL", "Git", "Docker" }
            };
            var validResult = validator.Validate("C#, Git", scheme);
            var invalidResult = validator.Validate("C#, Python", scheme);

            Assert.True(validResult.IsValid);
            Assert.False(invalidResult.IsValid);
            Assert.Contains("Python", invalidResult.ErrorMessage);
        }
    }
}