///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

namespace com.espertech.esper.common.@internal.epl.expression.dot.walk
{
    public class DotEscaper
    {
        /// <summary>
        ///     Escape all unescape dot characters in the text (identifier only) passed in.
        /// </summary>
        /// <param name="identifierToEscape">text to escape</param>
        /// <returns>text where dots are escaped</returns>
        public static string EscapeDot(string identifierToEscape)
        {
            var indexof = identifierToEscape.IndexOf('.');
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
            var indexof = identifierToUnescape.IndexOf('.');
            if (indexof == -1) {
                return identifierToUnescape;
            }

            indexof = identifierToUnescape.IndexOf('\\');
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
    }
} // end of namespace