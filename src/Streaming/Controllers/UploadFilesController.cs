using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SampleApp.Utilities;
using Streaming.Utilities;

namespace Streaming.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadFilesController : ControllerBase
    {
        private readonly ILogger<UploadFilesController> _logger;
        private readonly IConfiguration _config;
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions = { ".jpg" };
        private readonly string _targetFilePath;

        public UploadFilesController(ILogger<UploadFilesController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            _fileSizeLimit = _config.GetValue<long>("FileSizeLimit");

            // To save physical files to a path provided by configuration:
            _targetFilePath = _config.GetValue<string>("StoredFilesPath")!;
        }

        [HttpPost]
        [RequestSizeLimit(256 * 1024 * 1024)]
        [Route(nameof(UploadLargeFile))]
        public async Task<IActionResult> UploadLargeFile()
        {
            var request = HttpContext.Request;

            FileUploadModel fileUploadModel = new();

            try
            {
                // validation of Content-Type
                // 1. first, it must be a form-data request
                // 2. a boundary should be found in the Content-Type
                if (!request.HasFormContentType ||
                    !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) ||
                    string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
                {
                    return new UnsupportedMediaTypeResult();
                }

                var boundary = HeaderUtilities.RemoveQuotes(mediaTypeHeader.Boundary.Value).Value;
                var reader = new MultipartReader(boundary!, request.Body);
                var section = await reader.ReadNextSectionAsync();

                // This sample try to get the first file from request and save it
                // Make changes according to your needs in actual use
                while (section != null)
                {
                    var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                        out var contentDisposition);

                    if (hasContentDispositionHeader
                        && MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition!.Name);
                        var encoding = MultipartRequestHelper.GetEncoding(section);

                        using var streamReader = new StreamReader(section.Body, encoding, true, 1024, true);
                        var value = await streamReader.ReadToEndAsync();
                        if (key == "AdditionalMetadata")
                        {
                            fileUploadModel.AdditionalMetadata = value;
                        }
                    }
                    else if (hasContentDispositionHeader
                        && MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        fileUploadModel.FileStream =
                            await FileHelpers.ProcessStreamedFile(section, contentDisposition!, _permittedExtensions, _fileSizeLimit);
                    }

                    section = await reader.ReadNextSectionAsync();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(new { Message = "Request processed successfully.", Metadata = fileUploadModel.AdditionalMetadata });

        }

        public class FileUploadModel
        {
            public string? AdditionalMetadata { get; set; }
            public FileStream? FileStream { get; set; }
        }

    }
}