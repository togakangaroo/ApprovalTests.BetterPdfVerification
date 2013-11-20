using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace ApprovalTests.BetterPdfVerification
{
    /// <summary>
    /// Provides ApprovalTest verifications for pdf files that are scrubbed of things like
    /// creation date (regardless of specific syntax), modification date
    /// and pdf ID
    /// </summary>
    public static class PdfApprovals
    {
        public static void Verify(string file)
        {
            verify(PdfReader.Open(file, PdfDocumentOpenMode.Modify));
        }
        public static void Verify(Stream pdf)
        {
            rewind(pdf);
            verify(PdfReader.Open(pdf, PdfDocumentOpenMode.Modify));
        }

        static void verify(PdfDocument doc)
        {
            var knownDate = new DateTime(2010, 1, 1);
            doc.Info.CreationDate = doc.Info.ModificationDate = knownDate;
            using (var ms = new MemoryStream())
            {
                doc.Save(ms, closeStream: false);
                rewind(ms);
                clearId(ms);
                Approvals.VerifyBinaryFile(ms.ToArray(), "pdf");
            }
        }

        static void rewind(Stream stream) { stream.Seek(0, SeekOrigin.Begin); }

        /// <summary>
        /// Ids look like this:
        /// /ID[<7B8D63B6A42B804386D5145B251346BC><952D5CBCFB77E14A80CFFC76AC1D2F6A>]
        /// </summary>
        static void clearId(MemoryStream ms)
        {
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

        static long find(byte[] toMatch, Stream stream)
        {
            while (stream.Length != stream.Position)
            {
                var initialPosition = stream.Position;
                stream.ReadByte(); //this is almost certainly not correct but seems to work
                if (isMatchingAtCurrentPosition(toMatch, stream))
                    return initialPosition;
                stream.Seek(initialPosition, SeekOrigin.Begin);
                stream.ReadByte();
            }
            return -1;
        }

        static bool isMatchingAtCurrentPosition(byte[] toMatch, Stream stream)
        {
            int idx;
            stream.Seek(-1, SeekOrigin.Current); //rewind the stream a bit so the first bit is considered
            for (idx = 0; idx < toMatch.Length && stream.Position < stream.Length
                && compare(toMatch[idx], stream.ReadByte()); idx++) { }
            return idx == toMatch.Length;
        }

        static bool compare(byte matchingByte, int targetByte)
        {
            return matchingByte == default(byte) || matchingByte == targetByte;
        }
    }
}
