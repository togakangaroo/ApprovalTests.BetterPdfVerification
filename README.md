This project is a set of test assertions built on top of [the amazing ApprovalTests.Net library](https://github.com/approvals/ApprovalTests.Net/). Unfortunately, even when using the `Approvals.VerifyPdfFile()` function [ApprovalTests has problems with pdf files]. At the moment this seems to be for the following reasons, [though more can be forthcoming](http://stackoverflow.com/questions/20039691/reason-why-pdf-files-have-differences).

* Pdf files have a CreationDate embedded in them, Of course pdf files can be created at different times and should still be identical to previously verified files. There is also some leniency as far as how this timestamp is denoted.
* The same applies to the modification date (ModDate).
* Finally, many pdf files have a [trailer ID property that will always be unique](http://stackoverflow.com/questions/20085899/what-is-the-id-field-in-a-pdf-file/20091203?noredirect=1#20091203).

This library normalizes these values before delegating to ApprovalTests to verify the normalized pdf file, therefore providing a more useful pdf file verifier.

## Usage

	Stream pdf = createSamplePdf();
    PdfApprovals.Verify(pdf);
        
or
	
	string pdfFilePath = getSamplePdfPath();
    PdfApprovals.Verify(pdfFilePath);

The above will attempt to normalize inconsequential but inconsistent values in pdf files but will ultimately compare pdfs as text. Sometimes however there's no way around differences in pdf files (eg timezone differences which come from which computer the code ran on). In that case, the library will allow you to convert the pdf to a tiff image which will drop any metadata and allow you to compare them visually pixel-by-pixel. To use this functionality use

    PdfApprovals.VerifyVisually(pdf);

The methods on PdfApprovals are simply wrappers for calls to `PdfScrubbingVerifier` or `PdfAsImageVerifier`. You can use these directly. This is especially useful if you want to provide a custom approval reporter or namer. To set these, both classes expose settable `GetReporter` and `GetNamer` fields.

## Notes

* This library uses [PdfSharp](http://pdfsharp.com/PDFsharp/) to normalize as much as possible to known values. [Unfortunately, it cannot do so for the ID](http://forum.pdfsharp.net/viewtopic.php?f=2&t=2656&p=7644#p7644) [nor timezone offsets](//https://pdfsharp.codeplex.com/workitem/16846)