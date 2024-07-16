using CCSWebKySearch.Exceptions;
using CCSWebKySearch.Models;
using System.IO;
using System.Threading.Tasks;

namespace CCSWebKySearch.Services
{
    public class DocumentFileService : IDocumentFileService
    {
        private readonly string _documentsPath;

        public DocumentFileService(IConfiguration configuration)
        {
            _documentsPath = configuration["DocumentsPath"];
        }
        public async Task<byte[]> GetDocumentFileAsync(string book, string page, string fileType)
        {
            // Construct the file path
            string paddedPage = page.PadLeft(10, '0');

            string basePath = $"{_documentsPath}\\BOOK{book}\\{paddedPage}";

            // Check if the directory exists
            if (!Directory.Exists(basePath))
            {
                throw new InvalidInputException("The specified book or page does not exist.");
            }

            // Generate the file
            CCSImageHelper.MakePDFFile(basePath);

            // Determine the file path based on the requested file type
            string filePath = fileType switch
            {
                "pdf" => Path.Combine(basePath, "working", "merged.pdf"),
                "tif" => Path.Combine(basePath, "working", "merged.tiff"),
                _ => throw new InvalidInputException("Invalid file type requested.")
            };

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                throw new InvalidInputException("The specified file does not exist.");
            }

            // Read and return the file content
            return await System.IO.File.ReadAllBytesAsync(filePath);
        }
    }
}
