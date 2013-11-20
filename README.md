This project is a set of test assertions built on top of [the amazing ApprovalTests.Net library](https://github.com/approvals/ApprovalTests.Net/). Unfortunately, even when using the `Approvals.VerifyPdfFile()` function [ApprovalTests has problems with pdf files]. At the moment this seems to be for the following reasons, [though more can be forthcoming](http://stackoverflow.com/questions/20039691/reason-why-pdf-files-have-differences).

* Pdf files have a CreationDate embedded in them, Of course pdf files can be created at different times and should still be identical to previously verified files. There is also some leniency as far as how this timestamp is denoted.
* The same applies to the modification date (ModDate).
* Finally, many pdf files have a [trailer ID property that will always be unique](http://stackoverflow.com/questions/20085899/what-is-the-id-field-in-a-pdf-file/20091203?noredirect=1#20091203).

This library normalizes these values before delegating to ApprovalTests to verify the normalized pdf file.

## Notes

* This library uses [PdfSharp](http://pdfsharp.com/PDFsharp/) to normalize as much as possible to known values. [Unfortunately, it cannot do so for the ID](http://forum.pdfsharp.net/viewtopic.php?f=2&t=2656&p=7644#p7644)