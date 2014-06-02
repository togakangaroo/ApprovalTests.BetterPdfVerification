using System;
using System.Diagnostics;
using System.IO;

namespace ApprovalTests.BetterPdfVerification
{
    /// <summary>
    /// Generate a file in the directory denoted by GetPath. This file will be deleted
    /// on dispose. 
    /// </summary>
    public class DisposableFile : IDisposable
    {
        public readonly FileInfo File;
        public DisposableFile() {
            File = new FileInfo(Path.Combine(GetPath(), Guid.NewGuid().ToString()));            
        }
        /// <summary>
        /// Write contents of the stream to the file
        /// </summary>
        public DisposableFile(Stream stream) : this() {
            using (var fs = File.Create())
                stream.Rewind().CopyTo(fs);        
        }

        /// <summary>
        /// Directory for the file to be generated in. Default is the system default.
        /// </summary>
        public static Func<string> GetPath = () => Path.GetTempPath();
        public static event Action<string> FileDeletionError = (fileName) => Debug.WriteLine("ApprovalTests.BetterPdfVerification.DisposableFile: An error occurred deleting '{0}'".Fmt(fileName));
        public void Dispose() {
            try {
                File.Delete();
            }
            catch (Exception) {
                FileDeletionError(File.FullName);
            }
        }
    }
}