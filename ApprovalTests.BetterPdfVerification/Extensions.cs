using System;
using System.Collections.Generic;

namespace ApprovalTests.BetterPdfVerification
{
    public static class Extensions
    {
        public static string Fmt(this string pattern, params object[] args) {
            return String.Format(pattern, args);
        }
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> act) {
            foreach (var x in collection)
                act(x);
            return collection;
        }
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T, int> act) {
            var i = 0;
            foreach (var x in collection)
                act(x, i++);
            return collection;
        }
    }
}