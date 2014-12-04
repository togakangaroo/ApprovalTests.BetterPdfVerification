using System.IO;
using System.Threading.Tasks;

namespace ApprovalTests.BetterPdfVerification
{
    public static class StreamExtensions
    {
        public static Stream Rewind(this Stream stream) {
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static MemoryStream ToNewMemoryStream(this Stream stream) {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms;
        }

    }
}