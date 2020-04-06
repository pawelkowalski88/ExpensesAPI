using Expenses.FileImporter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

namespace ExpensesAPI.Controllers
{
    [Authorize(Policy = "ApiUser")]
    public class FileImportController : ControllerBase
    {
        private readonly IFileImporter importer;
        private readonly IWebHostEnvironment host;

        public FileImportController(IFileImporter importer, IWebHostEnvironment host)
        {
            this.importer = importer;
            this.host = host;
        }

        [HttpPost("api/postcsvfile")]
        public IActionResult GetImportedColumns(IFormFile file)
        {
            var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            //var uploadsFolderPath = Path.Combine(host.WebRootPath, "Uploads");
            if (!Directory.Exists(uploadsFolderPath))
                Directory.CreateDirectory(uploadsFolderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var result = importer.ToStringList(filePath);
            return Ok(result);
        }
    }
}
