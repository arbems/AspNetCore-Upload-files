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

        [HttpPost]        
        [RequestSizeLimit(256 * 1024 * 1024)]// Handle requests up to 256 MB
        public async Task<IActionResult> OnPostUploadAsync(IFormFile file)
        {
            try
            {
                var formFileContent =
                    await FileHelpers.ProcessFormFile(
                        file, _permittedExtensions,
                        _fileSizeLimit);

                // Generate a safe random file name.
                var trustedFileNameForFileStorage = Path.GetRandomFileName();
                var filePath = Path.Combine(
                    Path.GetTempPath(), trustedFileNameForFileStorage);

                // Save file to directory.
                using var fileStream = System.IO.File.Create(filePath);
                await fileStream.WriteAsync(formFileContent);

                // To work directly with a FormFile, use the following instead:
                //await file.CopyToAsync(fileStream);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(new { Message = "Request processed successfully." });
        }

        [HttpPost]
        [RequestSizeLimit(256 * 1024 * 1024)]// Handle requests up to 256 MB
        public async Task<IActionResult> OnPostUploadAsync(List<IFormFile> files)
        {
            try
            {
                foreach (var file in files)
                {
                    var formFileContent =
                    await FileHelpers.ProcessFormFile(
                        file, _permittedExtensions,
                        _fileSizeLimit);

                    // Generate a safe random file name.
                    var trustedFileNameForFileStorage = Path.GetRandomFileName();
                    var filePath = Path.Combine(
                        Path.GetTempPath(), trustedFileNameForFileStorage);

                    // Save file to directory.
                    using var fileStream = System.IO.File.Create(filePath);
                    await fileStream.WriteAsync(formFileContent);

                    // To work directly with a FormFile, use the following instead:
                    //await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(new { Message = "Solicitud procesada con éxito" });
        }

    }

}
