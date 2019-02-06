/******************************************************************************************************************************

Copyright (c) 2018-2019 InterlockLedger Network
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

* Neither the name of the copyright holder nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

******************************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InterlockLedger
{
    public static class StringExtensions
    {
        public static ulong AsUlong(this string s) => ulong.TryParse(s?.Trim(), out var value) ? value : 0ul;

        public static string JoinedBy<T>(this IEnumerable<T> list, string joiner) => list == null ? string.Empty : string.Join(joiner, list);

        public static string Parenthesize(this string s) => s.ParenthesizeIf(!s.Parenthesized());

        public static bool Parenthesized(this string s) {
            if (string.IsNullOrWhiteSpace(s))
                return false;
            s = s.Trim();
            var count = 0;
            if (!s.StartsWith("(", StringComparison.Ordinal)) return false;
            for (var i = 0; i < s.Length; i++) {
                var c = s[i];
                if (c == '(') {
                    count++;
                    continue;
                }
                if (c == ')') {
                    count--;
                    if (count == 0 && (i + 1) < s.Length) return false;
                }
            }
            return count == 0;
        }

        public static string ParenthesizeIf(this string s, bool condition = false) => (condition) ? $"({s})" : s;

        public static string PrefixedBy(this string value, string prefix) => string.IsNullOrWhiteSpace(value) ? null : prefix + value;

        public static string Reversed(this string s) => string.IsNullOrWhiteSpace(s) ? string.Empty : new string(s.ToCharArray().Reverse().ToArray());

        public static string Safe(this string s) => s.WithDefault(string.Empty);

        public static string SimplifyAsFileName(this string name) {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"Name is empty or null");
            return _nameFilter.Replace(name.Trim(), ".").ToLower();
        }

        public static string TrimToNull(this string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        public static byte[] UTF8Bytes(this string s) => Encoding.UTF8.GetBytes(s.WithDefault(string.Empty));

        public static string ValidateNonEmpty(this string parameter, string parameterName)
            => string.IsNullOrWhiteSpace(parameter) ? throw new ArgumentNullException(parameterName) : parameter.Trim();

        public static string WithCommas<T>(this IEnumerable<T> list, bool noSpaces = false) => JoinedBy(list, noSpaces ? "," : ", ");

        public static string WithDefault(this string s, string @default) => string.IsNullOrWhiteSpace(s) ? @default : s.Trim();

        public static string WithDefault(this string s, Func<string> resolver) {
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));
            return string.IsNullOrWhiteSpace(s) ? resolver() : s.Trim();
        }

        public static string WithoutWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s) ? string.Empty : Regex.Replace(s, @"[\r\n\s]+", " ").Trim();

        public static string WithSuffix(this string s, string suffix, char separator = '.') {
            if (s == null)
                return null;
            if (string.IsNullOrWhiteSpace(suffix))
                return s.Trim();
            suffix = suffix.Trim();
            s = s.Trim().TrimEnd(separator);
            return s.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) ? s : s + separator + suffix.TrimStart(separator);
        }

        private static readonly Regex _nameFilter = new Regex(@"[\.\s\r\n<>\:""/\\|\?\*]+");
    }
}