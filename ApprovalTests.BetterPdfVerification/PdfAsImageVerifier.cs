using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using ApprovalTests.Core;
using ApprovalTests.Writers;
using BitMiracle.LibTiff.Classic;
using GhostscriptSharp;
using GhostscriptSharp.Settings;

namespace ApprovalTests.BetterPdfVerification
{
	public class PdfAsImageVerifier
	{
		public void Verify(Stream pdf) {
			using (var pdfFile = new DisposableFile(pdf))
				Verify(pdfFile.File);
		}

		public void Verify(FileInfo pdf) {
			if (!pdf.Exists)
				throw new InvalidOperationException("File to verify does not exist '{0}'".Fmt(pdf.FullName));

			using (var destination = new DisposableFile()) {
				GhostscriptWrapper.GenerateOutput(
					inputPath: pdf.FullName,
					outputPath: destination.File.FullName,
					settings: new GhostscriptSettings {
						Device = GhostscriptDevices.tiff24nc,
						Resolution = new Size(72, 72),
						Size = new GhostscriptPageSize {Native = GhostscriptPageSizes.letter}
					});
				//GhostScript embeds a creation timestamp on each page of the tiff. Obviously if its different every time the files won't match up byte for byte. So normalize
				using (var tiff = Tiff.Open(destination.File.FullName, "a")) {
					Enumerable.Range(0, tiff.NumberOfDirectories()).ForEach(i => {
						tiff.SetDirectory((short)i);
						tiff.SetField(TiffTag.DATETIME, Encoding.UTF8.GetBytes("2010:01:01 12:00:00")); //any constant date will do the trick here.
						tiff.CheckpointDirectory();
					});
				}
				Approvals.Verify(new ApprovalBinaryWriter(destination.File.OpenRead().ToNewMemoryStream().ToArray(), "tiff"), GetNamer(), GetReporter());
			}
		}

		/// <summary>
		/// Provide your own function for using a custom reporter
		/// </summary>
		public Func<IApprovalFailureReporter> GetReporter = () => Approvals.GetReporter();
		/// <summary>
		/// Provide your own function for using a custom namer
		/// </summary>
	    public Func<IApprovalNamer> GetNamer = () => Approvals.GetDefaultNamer();
	}
}