using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;
using System.Text;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly ApplicationContext _dbContext;

        public ValuesController(ApplicationContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        [Route("import/excel")]
        public async Task<List<Client>> ImportEx(IFormFile file)
        {
            var list = new List<Client>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet workSheet = package.Workbook.Worksheets[0];
                    var rowcount = workSheet.Dimension.Rows;
                    for (int row = 2; row <= rowcount; row++)
                    {
                        list.Add(new Client
                        {
                            Id = Guid.Parse(workSheet.Cells[row, 1].Value.ToString()!.Trim()),
                            Name = workSheet.Cells[row, 2].Value.ToString().Trim(),
                            Surname = workSheet.Cells[row, 3].Value.ToString().Trim(),
                            BirthdayYear = workSheet.Cells[row, 4].Value.ToString().Trim(),
                            Email = workSheet.Cells[row, 5].Value.ToString().Trim(),
                            Number = workSheet.Cells[row, 6].Value.ToString().Trim(),
                            Adress = workSheet.Cells[row, 7].Value.ToString().Trim(),
                        });
                    }
                    foreach(var item in list)
                    { 
                        _dbContext.Clients.Add(item);
                    }
                    await _dbContext.SaveChangesAsync();
                }
                return list;
            }
        }

        [HttpGet]
        [Route("export/excel")]
        public async Task<IActionResult> ExportEx()
        {
            var clients = await _dbContext.Clients.ToListAsync(HttpContext.RequestAborted);
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Clients");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Id";
                worksheet.Cell(currentRow, 2).Value = "Name";
                worksheet.Cell(currentRow, 3).Value = "Surname";
                worksheet.Cell(currentRow, 4).Value = "BirthdayYear";
                worksheet.Cell(currentRow, 5).Value = "Email";
                worksheet.Cell(currentRow, 6).Value = "Number";
                worksheet.Cell(currentRow, 7).Value = "Adress";

                foreach (var client in clients)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = client.Id.ToString();
                    worksheet.Cell(currentRow, 2).Value = client.Name;
                    worksheet.Cell(currentRow, 3).Value = client.Surname;
                    worksheet.Cell(currentRow, 4).Value = client.BirthdayYear;
                    worksheet.Cell(currentRow, 5).Value = client.Email;
                    worksheet.Cell(currentRow, 6).Value = client.Number;
                    worksheet.Cell(currentRow, 7).Value = client.Adress;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "users.xlsx");
                }
            }
        }

        [HttpGet]
        [Route("export/json")]
        public async Task<IActionResult> ExportToJson()
        {
            var clients = await _dbContext.Clients.ToListAsync(HttpContext.RequestAborted);

            var json = System.Text.Json.JsonSerializer.Serialize(clients);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            return File(stream, "application/octet-stream", "clients.json");
        }

        [HttpPost]
        [Route("import/json")]
        public async Task<IActionResult> ImportJson(IFormFile file)
        {
            JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web)
            {
                WriteIndented = true
            };

            var clients = await JsonSerializer.DeserializeAsync<List<Client>>(file.OpenReadStream(), jsonOptions);

            if (clients is null)
            {
                throw new NullReferenceException("Результат десериализации null");
            }

            foreach (var client in clients)
            {
                if (client.Id == Guid.Empty)
                {
                    throw new ArgumentException("Id является Guid.Empty");
                }

                _dbContext.Clients.Add(client);
            }

            await _dbContext.SaveChangesAsync();

            return Ok("Clients added to the database.");
        }
    }
}
