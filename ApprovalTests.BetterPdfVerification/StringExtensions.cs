using System;

namespace ApprovalTests.BetterPdfVerification
{
    public static class StringExtensions
    {
        public static string Fmt(this string pattern, params object[] args) {
            return String.Format(pattern, args);
        }
    }
}