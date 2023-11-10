namespace Tests.FileUploadServiceTests
{
    public class FileUploadServiceTests
    {
        [Fact]
        public async Task UploadFileAsync_Returns_SuccessResponse()
        {
            // Arrange
            var httpClient = new HttpClient();
            var fileUploadService = new FileUploadService(httpClient);
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory + "FileUploadServiceTests", "sample-8kb.jpg");

            // Act
            var response = await fileUploadService.UploadFileAsync(filePath);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task UploadFileAsync_Returns_ErrorSizeLimit()
        {
            // Arrange
            var httpClient = new HttpClient();
            var fileUploadService = new FileUploadService(httpClient);
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory + "FileUploadServiceTests", "sample-5mb.jpg");

            // Act
            var response = await fileUploadService.UploadFileAsync(filePath);

            var errorMessage = string.Empty;
            if (response.Content != null)
            {
                // Lee el contenido de la respuesta como una cadena.
                errorMessage = await response.Content.ReadAsStringAsync();
            }

            // Assert
            Assert.Equal("The upload failed. Please contact the Help Desk  for support. Error: The file exceeds 2,0 MB.", errorMessage);
        }

        [Fact]
        public async Task UploadFileAsync_Returns_ErrorFileLength()
        {
            // Arrange
            var httpClient = new HttpClient();
            var fileUploadService = new FileUploadService(httpClient);
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory + "FileUploadServiceTests", "sample-0kb.jpg");

            // Act
            var response = await fileUploadService.UploadFileAsync(filePath);

            var errorMessage = string.Empty;
            if (response.Content != null)
            {
                // Lee el contenido de la respuesta como una cadena.
                errorMessage = await response.Content.ReadAsStringAsync();
            }

            // Assert
            Assert.Equal("The upload failed. Please contact the Help Desk  for support. Error: The file is empty.", errorMessage);
        }

        [Fact]
        public async Task UploadFileAsync_Returns_ErrorSignature()
        {
            // Arrange
            var httpClient = new HttpClient();
            var fileUploadService = new FileUploadService(httpClient);
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory + "FileUploadServiceTests", "sample-fake.jpg");

            // Act
            var response = await fileUploadService.UploadFileAsync(filePath);

            var errorMessage = string.Empty;
            if (response.Content != null)
            {
                // Lee el contenido de la respuesta como una cadena.
                errorMessage = await response.Content.ReadAsStringAsync();
            }

            // Assert
            Assert.Equal("The upload failed. Please contact the Help Desk  for support. Error: The file type isn't permitted or the file's signature doesn't match the file's extension.", errorMessage);
        }

        [Fact]
        public async Task UploadFileAsync_Returns_ErrorExtension()
        {
            // Arrange
            var httpClient = new HttpClient();
            var fileUploadService = new FileUploadService(httpClient);
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory + "FileUploadServiceTests", "sample.3xr");

            // Act
            var response = await fileUploadService.UploadFileAsync(filePath);

            var errorMessage = string.Empty;
            if (response.Content != null)
            {
                // Lee el contenido de la respuesta como una cadena.
                errorMessage = await response.Content.ReadAsStringAsync();
            }

            // Assert
            Assert.Equal("The upload failed. Please contact the Help Desk  for support. Error: The file type isn't permitted or the file's signature doesn't match the file's extension.", errorMessage);
        }
    }
}