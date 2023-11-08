using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace SampleApp.Utilities
{
    public static class FileHelpers
    {
        // If you require a check on specific characters in the IsValidFileExtensionAndSignature
        // method, supply the characters in the _allowedChars field.
        private static readonly byte[] _allowedChars = { };
        // For more file signatures, see the File Signatures Database (https://www.filesignatures.net/)
        // and the official specifications for the file types you wish to add.
        private static readonly Dictionary<string, List<byte[]>> _fileSignature = new Dictionary<string, List<byte[]>>
        {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
            { ".zip", new List<byte[]>
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
        };

        // **WARNING!**
        // In the following file processing methods, the file's content isn't scanned.
        // In most production scenarios, an anti-virus/anti-malware scanner API is
        // used on the file before making the file available to users or other
        // systems. For more information, see the topic that accompanies this sample
        // app.

        public static async Task<FileStream> ProcessStreamedFile(
            MultipartSection section, ContentDispositionHeaderValue contentDisposition,
            string[] permittedExtensions, long sizeLimit)
        {
            try
            {
                // Get the temporary folder, and combine a random file name with it
                var fileName = Path.GetRandomFileName();
                var saveToPath = Path.Combine(Path.GetTempPath(), fileName);

                using var targetStream = File.Create(saveToPath);
                await section.Body.CopyToAsync(targetStream);

                // Check the file length.
                if (targetStream.Length == 0)
                {
                    throw new Exception("The file is empty.");
                }
                else if (targetStream.Length > sizeLimit)
                {
                    var megabyteSizeLimit = sizeLimit / 1048576;
                    throw new Exception($"The file exceeds {megabyteSizeLimit:N1} MB.");
                }
                else if (!IsValidFileExtensionAndSignature(
                    contentDisposition.FileName.Value!, targetStream, permittedExtensions))
                {
                    throw new Exception(
                        "The file type isn't permitted or the file's " +
                        "signature doesn't match the file's extension.");
                }
                else
                {
                    return targetStream;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "The upload failed. Please contact the Help Desk " +
                    $" for support. Error: {ex.Message}"); // Error: {ex.HResult});
                // Log the exception
            }
        }

        private static bool IsValidFileExtensionAndSignature(string fileName, Stream data, string[] permittedExtensions)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
                return false;

            data.Position = 0;

            using (var reader = new BinaryReader(data))
            {
                // File signature check
                // --------------------
                // With the file signatures provided in the _fileSignature
                // dictionary, the following code tests the input content's
                // file signature.
                var signatures = _fileSignature[ext];
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                return signatures.Any(signature =>
                    headerBytes.Take(signature.Length).SequenceEqual(signature));
            }
        }
    }
}
