using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ApprovalTests.Reporters;
using ApprovalTests.Writers;
using ApprovalUtilities.Utilities;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Xunit;
using System.IO;

namespace ApprovalTests.BetterPdfVerification.Tests
{
    [UseReporter(/*typeof(FileLauncherReporter),*/ typeof(WinMergeReporter), typeof(ClipboardReporter))]
    public class PdfVerificationTests
    {
        [Fact]
        public void can_verify_pdfs_created_with_pdfsharp() {
            var pdf = createSamplePdf();
            PdfApprovals.VerifyFile(pdf);
        }

        /// <summary>
        /// Create hello world pdf, from http://www.pdfsharp.com/PDFsharp/index.php?option=com_content&task=view&id=15&Itemid=35
        /// </summary>
        static string createSamplePdf() {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 20, XFontStyle.Bold);

            gfx.DrawString(
                "Hello, World!", font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height),
                XStringFormat.Center);

            var pdf = PathUtilities.GetAdjacentFile("temp.pdf");
            document.Save(pdf);
            return pdf;
        }

    }

    public static class PdfApprovals
    {
        public static void VerifyFile(string file) {
            var doc = PdfReader.Open(file, PdfDocumentOpenMode.Modify);
            var knownDate = new DateTime(2010, 1, 1);
            doc.Info.CreationDate = doc.Info.ModificationDate = knownDate;
            using (var ms = new MemoryStream()) {
                doc.Save(ms, closeStream: false);
                rewind(ms);
                clearId(ms);
                Approvals.VerifyBinaryFile(ms.ToArray(), "pdf");
            }
        }

        static void rewind(Stream stream) { stream.Seek(0, SeekOrigin.Begin);}

        static void clearId(MemoryStream ms) {
            var sr = new StreamReader(ms);
            var enc = sr.CurrentEncoding;
            var toMatch = enc.GetBytes("/ID[<").Concat(wildcardBytes(32)).Concat(enc.GetBytes("><")).Concat(wildcardBytes(32)).Concat(enc.GetBytes(">]")).ToArray();
            var matchLocation = find(toMatch, ms);
            if (matchLocation < 0)
                return;
            ms.Seek(matchLocation, SeekOrigin.Begin);
            var replaceWith = enc.GetBytes("/ID[<").Concat(aBytes(32)).Concat(enc.GetBytes("><")).Concat(aBytes(32)).Concat(enc.GetBytes(">]")).ToArray();
            ms.Write(replaceWith, 0, replaceWith.Length);
        }

        static IEnumerable<byte> wildcardBytes(int count) { return Enumerable.Repeat(default(byte), count); }
        static IEnumerable<byte> aBytes(int count) { return Enumerable.Repeat((byte)'A', count); } 

        static long find(byte[] toMatch, Stream stream) {
            while (stream.Length != stream.Position) {
                var initialPosition = stream.Position;
                stream.ReadByte();
                if (isMatchingAtCurrentPosition(toMatch, stream))
                    return initialPosition;
                stream.Seek(initialPosition, SeekOrigin.Begin);
                stream.ReadByte();
            }
            return -1;
        }

        static bool isMatchingAtCurrentPosition(byte[] toMatch, Stream stream) {
            int idx;
            stream.Seek(-1, SeekOrigin.Current);
            for (idx = 0; idx < toMatch.Length && stream.Position < stream.Length && compare(toMatch[idx], stream.ReadByte()); idx++ ) { }
            return idx == toMatch.Length;
        }

        static bool compare(byte matchingByte, int targetByte) {
            return matchingByte == default(byte) || matchingByte == targetByte;
        }

        /*
         * /ID[<7B8D63B6A42B804386D5145B251346BC><952D5CBCFB77E14A80CFFC76AC1D2F6A>]
         * input eg: /ID[<********************************><********************************>]
         * b1 != b2 -> b2++, restart
         * b1++, b2++
         */
    }
}