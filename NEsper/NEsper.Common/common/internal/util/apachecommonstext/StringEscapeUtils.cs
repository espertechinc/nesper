///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util.apachecommonstext
{
    public class StringEscapeUtils
    {
        public static CharSequenceTranslator ESCAPE_JAVA { get; set; }

        static StringEscapeUtils()
        {
            var escapeJavaMap = new Dictionary<string, string>();
            escapeJavaMap.Put("\"", "\\\"");
            escapeJavaMap.Put("\\", "\\\\");
            ESCAPE_JAVA = new AggregateTranslator(
                new LookupTranslator(escapeJavaMap),
                new LookupTranslator(EntityArrays.JAVA_CTRL_CHARS_ESCAPE),
                JavaUnicodeEscaper.OutsideOf(32, 0x7f)
            );
        }

        public static Builder NewBuilder(CharSequenceTranslator translator)
        {
            return new Builder(translator);
        }

        // Java and JavaScript
        //--------------------------------------------------------------------------

        /// <summary>
        ///   <para>Escapes the characters in a {@code String} using Java String rules.</para>
        ///   <para>Deals correctly with quotes and control-chars (tab, backslash, cr, ff, etc.) </para>
        ///   <para>
        ///   So a tab becomes the characters {@code '\\'} and {@code 't'}.
        /// </para>
        ///   <para>
        ///   The only difference between Java strings and JavaScript strings is that in JavaScript,
        ///   a single quote and forward-slash (/) are escaped.
        /// </para>
        ///   <para>Example:</para>
        ///   <pre> input string: He didn't say, "Stop!"
        /// output string: He didn't say, \"Stop!\"
        /// </pre>
        /// </summary>
        /// <param name="input">String to escape values in, may be null</param>
        /// <returns>String with escaped values, {@code null} if null string input</returns>
        public static string EscapeJava(string input)
        {
            return ESCAPE_JAVA.Translate(input);
        }

        /// <summary>
        ///     <p>Convenience wrapper for <seealso cref="StringBuilder" /> providing escape methods.</p><p>Example:</p>new
        ///     Builder(ESCAPE_HTML4)
        ///     .append("&lt;p&gt;")
        ///     .escape("This is paragraph 1 and special chars like &amp; get escaped.")
        ///     .append("&lt;/p&gt;&lt;p&gt;")
        ///     .escape("This is paragraph 2 &amp; more...")
        ///     .append("&lt;/p&gt;")
        ///     .toString()
        /// </summary>
        public class Builder
        {
            /// <summary>
            ///     StringBuilder to be used in the Builder class.
            /// </summary>
            private readonly StringBuilder sb;

            /// <summary>
            ///     CharSequenceTranslator to be used in the Builder class.
            /// </summary>
            private readonly CharSequenceTranslator translator;

            /// <summary>
            ///     Builder constructor.
            /// </summary>
            /// <param name="translator">a CharSequenceTranslator.</param>
            internal Builder(CharSequenceTranslator translator)
            {
                sb = new StringBuilder();
                this.translator = translator;
            }

            /// <summary>
            ///     Literal append, no escaping being done.
            /// </summary>
            /// <param name="input">the String to append</param>
            /// <returns>{@code this}, to enable chaining</returns>
            public Builder Append(string input)
            {
                sb.Append(input);
                return this;
            }

            /// <summary>
            ///     <p>Return the escaped string.</p>
            /// </summary>
            /// <returns>the escaped string</returns>
            public override string ToString()
            {
                return sb.ToString();
            }
        }
    }
} // end of namespace