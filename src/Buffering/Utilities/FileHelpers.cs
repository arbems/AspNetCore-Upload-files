namespace Buffering.Utilities
{
    public static class FileHelpers
    {
        // For more file signatures, see the File Signatures Database
        // and the official specifications for the file types you wish to add.
        private static readonly Dictionary<string, List<byte[]>> _fileSignature = new()
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

        public static async Task<byte[]> ProcessFormFile<T>(
            IFormFile formFile, string[] permittedExtensions,
            long sizeLimit)
        {
            try
            {
                // Check the file length.
                if (formFile.Length == 0)
                {
                    throw new Exception($"The file is empty.");
                }
                // Check the size of an uploaded file.
                if (formFile.Length > sizeLimit)
                {
                    var megabyteSizeLimit = sizeLimit / 1048576;
                    throw new Exception($"The file exceeds {megabyteSizeLimit:N1} MB.");
                }

                using var memoryStream = new MemoryStream();
                await formFile.CopyToAsync(memoryStream);

                // Check the content length in case the file's only
                // content was a BOM and the content is actually
                // empty after removing the BOM.
                if (memoryStream.Length == 0)
                {
                    throw new Exception($"The file is empty.");
                }

                // Check file extension and signature 
                if (!IsValidFileExtensionAndSignature(
                    formFile.FileName, memoryStream, permittedExtensions))
                {
                    throw new Exception(
                        "The file type isn't permitted or the file's " +
                        "signature doesn't match the file's extension.");
                }
                else
                {
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "The upload failed. Please contact the Help Desk " +
                    $" for support. Error: {ex.Message}"); // Error: {ex.HResult}
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

            using var reader = new BinaryReader(data);
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
