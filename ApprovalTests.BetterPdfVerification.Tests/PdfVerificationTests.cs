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
        public void will_normalize_different_timezones() {
            //Note if you are in Central Time (CTZ) this test isn't very meaningful as thats what the approval is currently tuned to. Sorry.
            PdfApprovals.Verify(PathUtilities.GetAdjacentFile("pdf-created-in-odessa-timezone.pdf"));
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

        [Fact]
        public void can_use_image_approval_mode() {
            PdfApprovals.VerifyVisually(createSamplePdf());
        }
        [Fact]
        public void can_use_image_approval_mode_with_multipage() {
            PdfApprovals.VerifyVisually(createSamplePdf("Page 1", "Page 2"));
        }

        /// <summary>
        /// Create hello world pdf, from http://www.pdfsharp.com/PDFsharp/index.php?option=com_content&task=view&id=15&Itemid=35
        /// </summary>
        static Stream createSamplePdf(string page1Text = "Hello, World!", params string[] otherPageTexts) {
            var document = new PdfDocument();
            addPageWithText(document, page1Text);
            otherPageTexts.ForEach(txt =>addPageWithText(document, txt));
            var ms = new MemoryStream();
            document.Save(ms, closeStream: false);
            return ms;
        }

        static PdfPage addPageWithText(PdfDocument document, string text) {
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 20, XFontStyle.Bold);

            gfx.DrawString(
                text, font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height),
                XStringFormats.Center);
            return page;
        }

    }
}