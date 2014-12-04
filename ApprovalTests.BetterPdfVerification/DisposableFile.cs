using System;
using System.IO;

namespace ApprovalTests.BetterPdfVerification
{
	public class DisposableFile : IDisposable
	{
		public DisposableFile() {
			File = new FileInfo(Path.Combine(GetTempPath(), Guid.NewGuid().ToString()));            
		}
		public DisposableFile(Stream stream) : this() {
			using (var fs = File.Create())
				stream.Rewind().CopyTo(fs);        
		}

		public static Func<string> GetTempPath = () => Path.GetTempPath(); 
		public readonly FileInfo File;
		public void Dispose() {
			try { File.Delete(); }
			catch (Exception) {}
		}
	}
}