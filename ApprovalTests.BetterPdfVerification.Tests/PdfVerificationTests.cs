using System;
using ApprovalTests.Reporters;
using ApprovalUtilities.Utilities;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Xunit;
using System.IO;

namespace ApprovalTests.BetterPdfVerification.Tests
{
    [UseReporter(typeof(FileLauncherReporter), typeof(WinMergeReporter), typeof(ClipboardReporter))]
    public class PdfVerificationTests
    {
        [Fact]
        public void can_verify_pdfs_created_with_pdfsharp() {
            var pdf = createSamplePdf();
            PdfApprovals.Verify(pdf);
        }

        [Fact]
        public void react_to_empty_stream_properly() {
            Assert.Throws<ArgumentException>(() =>
                PdfApprovals.Verify(new MemoryStream(0))
                );
        }

        [Fact]
        public void react_to_nonexistant_file_properly() {
            Assert.Throws<ArgumentException>(() =>
                PdfApprovals.Verify(new FileInfo(PathUtilities.GetAdjacentFile("this-file-does-not-exist.pdf")))
                );
        }

        [Fact]
        public void react_to_null_argument_properly() {
            Assert.Throws<ArgumentNullException>(() =>
                PdfApprovals.Verify(null as Stream)
                );
            Assert.Throws<ArgumentNullException>(() =>
                PdfApprovals.Verify(null as string)
                );
        }

        /// <summary>
        /// Create hello world pdf, from http://www.pdfsharp.com/PDFsharp/index.php?option=com_content&task=view&id=15&Itemid=35
        /// </summary>
        static Stream createSamplePdf() {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 20, XFontStyle.Bold);

            gfx.DrawString(
                "Hello, World!", font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height),
                XStringFormat.Center);

            var ms = new MemoryStream();
            document.Save(ms, closeStream: false);
            return ms;
        }
    }
}