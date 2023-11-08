using System.Net.Http.Headers;

namespace Tests.FileUploadServiceTests
{
    internal class FileUploadService : IFileUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly string _serviceUrl;

        public FileUploadService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            //Buffering
            //_serviceUrl = "https://localhost:7082/UploadFiles";

            //Streaming
            _serviceUrl = "https://localhost:7246/UploadFiles/UploadLargeFile";
        }

        public async Task<HttpResponseMessage> UploadFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Don´t exist file.", filePath);
            }

            using var fileStream = new FileStream(filePath, FileMode.Open);

            var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"file\"",
                FileName = $"\"{Path.GetFileName(filePath)}\""
            };
            content.Add(fileContent);

            // Agregar metadatos como una parte del formulario
            var metadataContent = new StringContent("This is my metadata text.");
            metadataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"AdditionalMetadata\""
            };
            content.Add(metadataContent);

            return await _httpClient.PostAsync(_serviceUrl, content);
        }

    }

}
