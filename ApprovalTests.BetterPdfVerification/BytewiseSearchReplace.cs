using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ApprovalTests.BetterPdfVerification
{
    /// <summary>
    /// Search through the bytes of a stream and replace those matching a pattern (allows wildcards with * matching any single character)
    /// </summary>
    public class BytewiseSearchReplace
    {
        readonly Stream ms;
        public BytewiseSearchReplace(Stream ms) {
            this.ms = ms;
        }

        public void Replace(string pattern, string replaceWith) {
            var sr = new StreamReader(ms);
            var enc = sr.CurrentEncoding;
            var toMatch = bytesWithWildCards(enc, pattern);
            var matchLocation = find(toMatch, ms);
            if (matchLocation < 0)
                return;
            ms.Seek(matchLocation, SeekOrigin.Begin);
            var replacementBytes = enc.GetBytes(replaceWith);
            ms.Write(replacementBytes, 0, replacementBytes.Length);
        }

        static long find(byte[] toMatch, Stream stream) {
            while (stream.Length != stream.Position) {
                var initialPosition = stream.Position;
                stream.ReadByte(); //this is almost certainly not correct but seems to work
                if (isMatchingAtCurrentPosition(toMatch, stream))
                    return initialPosition;
                stream.Seek(initialPosition, SeekOrigin.Begin);
                stream.ReadByte();
            }
            return -1;
        }

        static bool isMatchingAtCurrentPosition(byte[] toMatch, Stream stream) {
            int idx;
            stream.Seek(-1, SeekOrigin.Current); //rewind the stream a bit so the first bit is considered
            for (idx = 0; idx < toMatch.Length && stream.Position < stream.Length
                          && compare(toMatch[idx], stream.ReadByte()); idx++) { }
            return idx == toMatch.Length;
        }

        static bool compare(byte matchingByte, int targetByte) {
            return matchingByte == default(byte) || matchingByte == targetByte;
        }

        /// <summary>
        /// Convert a string in the form "foo*bar**baz" and convert into an array of bytes with the given encoding
        /// all '*'s will be wildcard characters
        /// </summary>
        static byte[] bytesWithWildCards(Encoding enc, string pattern) {
            return Regex.Split(pattern, @"(\*)").Where(x => x != String.Empty)
                .SelectMany(s => s == "*" ? new[] { default(byte) } : enc.GetBytes(s))
                .ToArray();
        }
    }
}