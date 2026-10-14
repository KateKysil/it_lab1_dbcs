using Microsoft.AspNetCore.Mvc;
using it_lab1_dbcs.Models;
using it_lab1_dbcs.Storage;

namespace it_lab1_dbcs.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseManager _dbManager;

        public DatabaseController(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        [HttpGet("user/{username}")]
        public IActionResult GetUserDatabases(string username)
        {
            var files = _dbManager.GetUserDatabaseFiles(username);
            var dbNames = files.Select(Path.GetFileNameWithoutExtension).ToList();
            return Ok(dbNames);
        }

        [HttpGet("{dbName}")]
        public IActionResult GetDatabase(string dbName)
        {
            string filePath = Path.Combine("Databases", $"{dbName}.json");
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = "Базу даних не знайдено на сервері." });
            }

            try
            {
                var db = _dbManager.LoadDatabase(filePath);
                return Ok(db);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("save")]
        public IActionResult SaveDatabase([FromBody] Database database)
        {
            if (database == null)
                return BadRequest(new { message = "Передано порожній об'єкт бази даних." });

            try
            {
                _dbManager.SaveDatabase(database);
                return Ok(new { message = "Базу даних успішно збережено на сервері." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}