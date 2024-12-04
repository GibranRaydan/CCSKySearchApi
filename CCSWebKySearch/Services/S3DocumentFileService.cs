using Amazon.S3;
using Amazon.S3.Model;
using CCSWebKySearch.Utils;
using CCSWebKySearch.Exceptions;

namespace CCSWebKySearch.Services
{
    public class S3DocumentService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3DocumentService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:BucketName"];
        }

        public async Task<byte[]> GetMergedFileFromS3(string book, string page, string fileType)
        {

            string folderPrefix = $"book/BOOK{book}/{page.PadLeft(10, '0')}/";

            // Get list of TIFF files in the specified folder
            var listRequest = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = folderPrefix
            };

            List<S3Object> tiffFiles = new List<S3Object>();
            ListObjectsV2Response listResponse;
            do
            {
                listResponse = await _s3Client.ListObjectsV2Async(listRequest);
                tiffFiles.AddRange(listResponse.S3Objects);

                listRequest.ContinuationToken = listResponse.NextContinuationToken;
            } while (listResponse.IsTruncated);

            if (tiffFiles.Count == 0)
            {
                throw new FileNotFoundException("No TIFF files found in the specified folder.");
            }

            string folderPath = await CreateLocalTiffFilesFromS3(book, page, folderPrefix, tiffFiles);

            CCSImageHelper.MakePDFFile(folderPath);

            // Determine the file path based on the requested file type
            string completeFilePath = fileType switch
            {
                "pdf" => Path.Combine(folderPath, "working", "merged.pdf"),
                "tif" => Path.Combine(folderPath, "working", "merged.tiff"),
                _ => throw new InvalidInputException("Invalid file type requested.")
            };

            // Check if the file exists
            if (!System.IO.File.Exists(completeFilePath))
            {
                throw new InvalidInputException("The specified file does not exist.");
            }

            // Read and return the file content
            return await System.IO.File.ReadAllBytesAsync(completeFilePath);

        }

        private async Task<string> CreateLocalTiffFilesFromS3(string book, string page, string folderPrefix, List<S3Object> tiffFiles)
        {
            Directory.CreateDirectory(folderPrefix);

            string folderPath = $"./tiff/BOOK{book}/{page.PadLeft(10, '0')}/";


            foreach (var file in tiffFiles)
            {
                var filePath = Path.Combine(folderPath, file.Key.Substring(folderPrefix.Length));
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                var getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = file.Key
                };

                using (var response = await _s3Client.GetObjectAsync(getRequest))
                using (var responseStream = response.ResponseStream)
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await responseStream.CopyToAsync(fileStream);
                }
            }

            return folderPath;
        }
    }
}
