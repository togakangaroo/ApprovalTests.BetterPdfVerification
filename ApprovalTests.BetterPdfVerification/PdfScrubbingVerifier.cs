using System;
using System.IO;
using PdfSharp.Pdf;

namespace ApprovalTests.BetterPdfVerification
{
    /// <summary>
    /// Applies ApprovalTest verifications for pdf files that are scrubbed of things like
    /// creation date (regardless of specific syntax), modification date
    /// and pdf ID. Note that any files passed into these methods will be scrubbed, so while they
    /// should match their original versions visually they will likely not match them byte for byte.
    /// </summary>
    public class PdfScrubbingVerifier
    {
        public void Verify(PdfDocument doc)
        {
            doc.Info.CreationDate = doc.Info.ModificationDate = aKnownDate;
            using (var ms = new MemoryStream())
            {
                doc.Save(ms, closeStream: false);
                apply(ms, clearId);
                apply(ms, fixTimezone);
                Approvals.VerifyBinaryFile(ms.ToArray(), "pdf");
            }
        }

        static void apply(MemoryStream ms, Action<MemoryStream> action) {
            ms.Rewind();
            action(ms);
            ms.Rewind();
        }


        /// <summary>
        /// Ids look like this (32chars*2):
        /// /ID[<7B8D63B6A42B804386D5145B251346BC><952D5CBCFB77E14A80CFFC76AC1D2F6A>]
        /// </summary>
        static void clearId(MemoryStream ms) {
            new BytewiseSearchReplace(ms).Replace(
                "/ID[<********************************><********************************>]",
                "/ID[<AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA><AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA>]");
        }

        /// <summary>
        /// PdfSharp has no way of fixing the timezone to a specific offset so we're stuck doing it manually
        /// https://pdfsharp.codeplex.com/workitem/16846
        /// Fixes created and modified timestamps to CST because thats where I am
        /// Created and modified stamps look like this (the specific date is set earlier)
        /// /CreationDate(D:20100101000000-05'00')
        /// /ModDate(D:20100101000000-03'00')
        /// </summary>
        static void fixTimezone(MemoryStream ms) {
            //So if we go through the times anyways, why use PdfSharp at all?
            //The reason is there are different ways to represent these timestamps and PdfSharp normalizes these
            new BytewiseSearchReplace(ms).Replace(
                "/CreationDate(D:{0:yyyyMMddHHmmss}-**'**')".Fmt(aKnownDate),
                "/CreationDate(D:{0:yyyyMMddHHmmss}-06'00')".Fmt(aKnownDate)); //Why 6? Because that's where I am, it doesn't matter so long as its the same every time
            ms.Rewind();
            new BytewiseSearchReplace(ms).Replace(
                "/ModDate(D:{0:yyyyMMddHHmmss}-**'**')".Fmt(aKnownDate),
                "/ModDate(D:{0:yyyyMMddHHmmss}-06'00')".Fmt(aKnownDate)); //Why 6? Because that's where I am, it doesn't matter so long as its the same every time
        }

        /// <summary>
        /// Just any known date that is constant
        /// </summary>
        static readonly DateTime aKnownDate = new DateTime(2010, 1, 1);
    }
}