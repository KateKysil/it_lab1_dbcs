using it_lab1_dbcs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it_lab1_dbcs.Validation
{
    public interface IFieldValidator
    {
        ValidationResult Validate(string? value, FieldScheme scheme);
    }
}
