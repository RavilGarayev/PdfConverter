using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace converter_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly PdfService _pdfService;
        private readonly LogService _logService;

        public PdfController(PdfService pdfService, LogService logService)
        {
            _pdfService = pdfService;
            _logService = logService;   
        }

        [HttpPost("convert")]
        public async Task<IActionResult> ConvertHtmlToPdf(IFormFile file) 
        {
            try
            {
                if (file == null)
                {
                    return BadRequest();
                }

                var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".html");
                await using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                using (var reader = new StreamReader(tempFilePath)) //.OpenReadStream()))
                {
                    var html = await reader.ReadToEndAsync();

                    if (string.IsNullOrEmpty(html))
                    {
                        return BadRequest("HTML content is required.");
                    }

                    var pdfBytes = await _pdfService.ConvertHtmlToPdfAsync(html);

                    return File(pdfBytes, "application/pdf", "output.pdf");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("log")]
        public async Task<IActionResult> GetConversionLog()
        {
            var logEntries = await _logService.GetConversionLogAsync();
            return Ok(logEntries);
        }
    }
}



