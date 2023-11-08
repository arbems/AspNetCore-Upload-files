using Buffering.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Buffering.Controllers
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

        /// <summary>
        /// Action for upload file
        /// </summary>
        /// <param name="fileUploadModel"></param>
        /// <returns></returns>
        [HttpPost]
        // Handle requests up to 50 MB
        //[RequestSizeLimit(50 * 1024 * 1024)]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostUploadAsync([FromForm] FileUploadModel fileUploadModel)
        {
            try
            {
                var formFileContent =
                    await FileHelpers.ProcessFormFile<FileUploadModel>(
                        fileUploadModel.File, _permittedExtensions,
                        _fileSizeLimit);

                // For the file name of the uploaded file stored
                // server-side, use Path.GetRandomFileName to generate a safe
                // random file name.
                var trustedFileNameForFileStorage = Path.GetRandomFileName();
                var filePath = Path.Combine(
                    _targetFilePath, trustedFileNameForFileStorage);

                // **WARNING!**
                // In the following example, the file is saved without
                // scanning the file's contents. In most production
                // scenarios, an anti-virus/anti-malware scanner API
                // is used on the file before making the file available
                // for download or for use by other systems. 
                // For more information, see the topic that accompanies 
                // this sample.

                using var fileStream = System.IO.File.Create(filePath);
                await fileStream.WriteAsync(formFileContent);

                // To work directly with a FormFile, use the following instead:
                //await fileUploadModel.File.CopyToAsync(fileStream);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(new { Message = "Solicitud procesada con éxito", Metadata = fileUploadModel.AdditionalMetadata });
        }
    }

    public class FileUploadModel
    {
        public string? AdditionalMetadata { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }

}
