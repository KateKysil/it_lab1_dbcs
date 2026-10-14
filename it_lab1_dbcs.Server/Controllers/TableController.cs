using Microsoft.AspNetCore.Mvc;
using it_lab1_dbcs.Models;
using it_lab1_dbcs.Validation;

namespace it_lab1_dbcs.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly ValidationService _validationService;

        public TableController()
        {
            _validationService = new ValidationService();
        }

        [HttpPost("validate-row")]
        public IActionResult ValidateRow([FromBody] ValidateRowDto dto)
        {
            try
            {
                if (dto == null || dto.Schema == null || dto.Row == null)
                {
                    return BadRequest(new { message = "Передано порожні або некоректні дані." });
                }

                var result = _validationService.ValidateRow(dto.Schema, dto.Row);

                if (!result.IsValid)
                {
                    return BadRequest(new { message = result.ErrorMessage });
                }

                return Ok(new { isValid = true, message = "Дані валідні." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Помилка валідації на сервері: {ex.Message}" });
            }
        }
    }

    public class ValidateRowDto
    {
        public TableSchema Schema { get; set; } = new TableSchema();
        public TableRow Row { get; set; } = new TableRow();
    }
}