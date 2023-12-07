using System.Threading.Tasks;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace converter_server
{
    public class LogService
    {
        private readonly string logFilePath;

        public LogService(string logFilePath)
        {
            this.logFilePath = logFilePath;
        }

        public async Task LogConversionResultAsync(string fileName, byte[] pdfBytes)
        {
            var logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}: {fileName}";
            await File.AppendAllTextAsync(logFilePath, logEntry + Environment.NewLine);
        }

        public async Task<IEnumerable<string>> GetConversionLogAsync()
        {
            if (File.Exists(logFilePath))
            {
                return await File.ReadAllLinesAsync(logFilePath);
            }

            return Enumerable.Empty<string>();
        }
    }
}
