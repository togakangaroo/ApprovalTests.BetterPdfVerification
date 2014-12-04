using System;
using System.IO;
using PdfSharp.Pdf.IO;

namespace ApprovalTests.BetterPdfVerification
{
    /// <summary>
    /// Provides ApprovalTest verifications for pdf files that are scrubbed of things like
    /// creation date (regardless of specific syntax), modification date
    /// and pdf ID. Note that any files passed into these methods will be scrubbed, so while they
    /// should match their original versions physically they will likely not match them byte for byte.
    /// </summary>
    public static class PdfApprovals
    {
        public static void Verify(FileInfo pdfFile) {
            if (pdfFile == null)
                throw new ArgumentNullException("pdfFile");
            if (!pdfFile.Exists)
                throw new ArgumentException(String.Format("Non-existant file: '{0}'", pdfFile.FullName));
            Verify(pdfFile.OpenRead());
        }

        public static void Verify(string pdfFile) {
            if (pdfFile == null)
                throw new ArgumentNullException("pdfFile");
            new PdfScrubbingVerifier().Verify(PdfReader.Open(pdfFile, PdfDocumentOpenMode.Modify));
        }

        public static void Verify(Stream pdf, bool asImage = false) {
            if (pdf == null)
                throw new ArgumentNullException("pdf");
            if (pdf.Length <= 0)
                throw new ArgumentException("Pdf stream has no contents");
            pdf.Rewind();
            if (asImage)
                new PdfAsImageVerifier().Verify(pdf);
            else
                new PdfScrubbingVerifier().Verify(PdfReader.Open(pdf, PdfDocumentOpenMode.Modify));
        }
    }
}
