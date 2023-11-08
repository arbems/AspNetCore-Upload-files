namespace Tests.FileUploadServiceTests
{
    internal interface IFileUploadService
    {
        Task<HttpResponseMessage> UploadFileAsync(string filePath);
    }
}
