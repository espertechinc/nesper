///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
    public class StringValue
    {
        public const string UNNAMED = "(unnamed)";

        /// <summary>
        ///     Parse the string literal consisting of text between double-quotes or single-quotes.
        /// </summary>
        /// <param name="value">is the text wthin double or single quotes</param>
        /// <returns>parsed value</returns>
        public static string ParseString(string value)
        {
            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'"))) {
                if (value.Length > 1) {
                    if (value.IndexOf('\\') != -1) {
                        return Unescape(value.Substring(1, value.Length - 2));
                    }

                    return value.Substring(1, value.Length - 2);
                }
            }

            throw new ArgumentException("String value of '" + value + "' cannot be parsed");
        }

        /// <summary>
        ///     Find the index of an unescaped dot (.) character, or return -1 if none found.
        /// </summary>
        /// <param name="identifier">text to find an un-escaped dot character</param>
        /// <returns>index of first unescaped dot</returns>
        public static int UnescapedIndexOfDot(string identifier)
        {
            var indexof = identifier.IndexOf(".");
            if (indexof == -1) {
                return -1;
            }

            for (var i = 0; i < identifier.Length; i++) {
                var c = identifier[i];
                if (c != '.') {
                    continue;
                }

                if (i > 0) {
                    if (identifier[i - 1] == '\\') {
                        continue;
                    }
                }

                return i;
            }

            return -1;
        }

        /// <summary>
        ///     Escape all unescape dot characters in the text (identifier only) passed in.
        /// </summary>
        /// <param name="identifierToEscape">text to escape</param>
        /// <returns>text where dots are escaped</returns>
        protected internal static string EscapeDot(string identifierToEscape)
        {
            var indexof = identifierToEscape.IndexOf(".");
            if (indexof == -1) {
                return identifierToEscape;
            }

            var builder = new StringBuilder();
            for (var i = 0; i < identifierToEscape.Length; i++) {
                var c = identifierToEscape[i];
                if (c != '.') {
                    builder.Append(c);
                    continue;
                }

                if (i > 0) {
                    if (identifierToEscape[i - 1] == '\\') {
                        builder.Append('.');
                        continue;
                    }
                }

                builder.Append('\\');
                builder.Append('.');
            }

            return builder.ToString();
        }

        /// <summary>
        ///     Un-Escape all escaped dot characters in the text (identifier only) passed in.
        /// </summary>
        /// <param name="identifierToUnescape">text to un-escape</param>
        /// <returns>string</returns>
        public static string UnescapeDot(string identifierToUnescape)
        {
            var indexof = identifierToUnescape.IndexOf(".");
            if (indexof == -1) {
                return identifierToUnescape;
            }

            indexof = identifierToUnescape.IndexOf("\\");
            if (indexof == -1) {
                return identifierToUnescape;
            }

            var builder = new StringBuilder();
            var index = -1;
            var max = identifierToUnescape.Length - 1;
            do {
                index++;
                var c = identifierToUnescape[index];
                if (c != '\\') {
                    builder.Append(c);
                    continue;
                }

                if (index < identifierToUnescape.Length - 1) {
                    if (identifierToUnescape[index + 1] == '.') {
                        builder.Append('.');
                        index++;
                    }
                }
            } while (index < max);

            return builder.ToString();
        }

        public static string UnescapeBacktick(string text)
        {
            var indexof = text.IndexOf("`");
            if (indexof == -1) {
                return text;
            }

            var builder = new StringBuilder();
            var index = -1;
            var max = text.Length - 1;
            var skip = false;
            do {
                index++;
                var c = text[index];
                if (c == '`') {
                    skip = !skip;
                }
                else {
                    builder.Append(c);
                }
            } while (index < max);

            return builder.ToString();
        }

        /// <summary>
        ///     Renders a constant as an EPL.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="constant">to render</param>
        public static void RenderConstantAsEPL(
            TextWriter writer,
            object constant)
        {
            constant.RenderAny(writer);

#if false
            if (constant == null) {
                writer.Write("null");
                return;
            }

            if (constant is string ||
                constant is char) {
                writer.Write('\"');
                writer.Write(constant.ToString());
                writer.Write('\"');
            }
            else if (constant is long) {
                writer.Write(constant + "L");
            }
            else if (constant is double) {
                writer.Write(constant + "d");
            }
            else if (constant is float) {
                writer.Write(constant + "f");
            }
            else if (constant is decimal) {
                writer.Write(constant + "m");
            }
            else if (constant is bool asBool) {
                writer.Write(asBool ? "true" : "false");
            }
            else {
                writer.Write(constant.ToString());
            }
#endif
        }

        /// <summary>
        ///     Remove tick '`' character from a string start and end.
        /// </summary>
        /// <param name="tickedString">delimited string</param>
        /// <returns>delimited string with ticks removed, if starting and ending with tick</returns>
        public static string RemoveTicks(string tickedString)
        {
            var indexFirst = tickedString.IndexOf('`');
            var indexLast = tickedString.LastIndexOf('`');
            if (indexFirst != indexLast && indexFirst != -1 && indexLast != -1) {
                return tickedString.Substring(indexFirst + 1, indexLast - indexFirst - 1);
            }

            return tickedString;
        }

        public static string StringDelimitedTo60Char(string text)
        {
            if (text == null) {
                return "<null>";
            }

            if (text.Length <= 40) {
                return text;
            }

            var buf = new StringBuilder();
            buf.Append(text.Substring(0, 30));
            buf.Append("...(");
            buf.Append(Convert.ToString(text.Length - 40));
            buf.Append(" more)...");
            buf.Append(text.Substring(text.Length - 15));
            return buf.ToString();
        }

        public static string UnnamedWhenNullOrEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? UNNAMED : value;
        }

        private static string Unescape(string s)
        {
            int i = 0, len = s.Length;
            char c;
            var sb = new StringBuilder(len);
            while (i < len) {
                c = s[i++];
                if (c == '\\') {
                    if (i < len) {
                        c = s[i++];
                        if (c == 'u') {
                            c = (char)Convert.ToInt32(s.Substring(i, 4), 16);
                            i += 4;
                        } // add other cases here as desired...
                    }
                } // fall through: \ escapes itself, quotes any character but u

                sb.Append(c);
            }

            return sb.ToString();
        }
    }
} // end of namespace