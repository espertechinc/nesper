///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Text.RegularExpressions;

using Force.Crc32;

namespace com.espertech.esper.compat
{
    public static class StringExtensions
    {
        public static string Between(this string input, int startIndex, int endIndex)
        {
            return input.Substring(startIndex, endIndex - startIndex);
        }

        public static string Capitalize(this string input)
        {
            if (input == null)
                return null;
            if (input.Length == 1)
                return char.ToUpperInvariant(input[0]) + "";

            return char.ToUpperInvariant(input[0]) + input.Substring(1);
        }

        public static string[] SplitCsv(this string input)
        {
            return input.Split(',');
        }

        public static bool Matches(this string input, string regex)
        {
            if (regex.Length > 0)
            {
                if (regex[0] != '^')
                    regex = '^' + regex;
                if (regex[regex.Length - 1] != '$') 
                    regex = regex + '$';
            }

            return Regex.IsMatch(input, regex);
        }

        public static string[] RegexSplit(this string input, string pattern)
        {
            return Regex.Split(input, pattern);
        }

        public static string RegexReplaceAll(this string input, string pattern, string replacement)
        {
            return Regex.Replace(
                input,
                pattern,
                match => replacement);
        }

        public static long GetCrc32(this string input, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            return Crc32Algorithm.Compute(encoding.GetBytes(input));
        }

        /// <summary>
        /// Gets the UTF8 byte encoding for the input string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static byte[] GetUTF8Bytes(this string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        /// <summary>
        /// Gets the unicode byte encoding for the input string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static byte[] GetUnicodeBytes(this string input)
        {
            return Encoding.Unicode.GetBytes(input);
        }
    }
}
