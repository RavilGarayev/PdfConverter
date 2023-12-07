using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace converter_server
{
    public class PdfService
    {

        private readonly LogService _logService;

        public PdfService(LogService logService)
        {
            _logService = logService;
        }

        public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
        {
            const int maxBlockSize = 100000;
            var htmlBlocks = SplitHtmlIntoBlocks(html, maxBlockSize);
            var pdfResults = new List<byte[]>();

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
            };

            var browser = await Puppeteer.LaunchAsync(launchOptions);
            var tasks = new List<Task<byte[]>>();

            foreach (var htmlBlock in htmlBlocks)
            {
                tasks.Add(ProcessHtmlBlockAsync((PuppeteerSharp.Browser)browser, htmlBlock));
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                pdfResults.Add(task.Result);
            }

            var mergedPdf = MergePdfResults(pdfResults);
            await SaveResultToFileSystemAsync(mergedPdf, String.Format("{0}.pdf", Guid.NewGuid()));

            return mergedPdf;
        }

        private async Task SaveResultToFileSystemAsync(byte[] pdfBytes, string fileName)
        {
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            await _logService.LogConversionResultAsync(fileName, pdfBytes);
            await File.WriteAllBytesAsync(filePath, pdfBytes);
        }

        private IEnumerable<string> SplitHtmlIntoBlocks(string html, int maxBlockSize)
        {
            var utf8Bytes = Encoding.UTF8.GetBytes(html);
            for (int i = 0; i < utf8Bytes.Length; i += maxBlockSize)
            {
                var blockSize = Math.Min(maxBlockSize, utf8Bytes.Length - i);
                yield return Encoding.UTF8.GetString(utf8Bytes, i, blockSize);
            }
        }

        private byte[] MergePdfResults(List<byte[]> pdfResults)
        {
            var memoryStream = new MemoryStream();
            foreach (var pdfResult in pdfResults)
            {
                memoryStream.Write(pdfResult, 0, pdfResult.Length);
            }

            return memoryStream.ToArray();
        }

        private async Task<byte[]> ProcessHtmlBlockAsync(Browser browser, string htmlBlock)
        {
            var page = await browser.NewPageAsync();
            await page.SetContentAsync(htmlBlock);
            return await page.PdfDataAsync();
        }
    }
}
