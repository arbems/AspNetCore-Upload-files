using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace Streaming.Utilities
{
    public static class MultipartRequestHelper
    {
        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue? contentDisposition)
        {
            // Content-Disposition: form-data; name="key";
            return contentDisposition != null
            && contentDisposition.DispositionType.Equals("form-data")
            && contentDisposition.IsFormDisposition();
        }

        public static bool HasFileContentDisposition(ContentDispositionHeaderValue? contentDisposition)
        {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && contentDisposition.IsFileDisposition();
        }

        public static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader =
                MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            // UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }

    }
}
