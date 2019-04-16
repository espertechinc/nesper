using System;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util.apachecommonstext
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Translates codepoints to their Unicode escaped value suitable for Java source.
    /// </summary>
    /// <unknown>@since 1.0</unknown>
    public class JavaUnicodeEscaper : UnicodeEscaper
    {
        /// <summary>
        /// <para />Constructs a <code>JavaUnicodeEscaper</code> above the specified value (exclusive).
        /// </summary>
        /// <param name="codepoint">above which to escape</param>
        /// <returns>the newly created {@code UnicodeEscaper} instance</returns>
        public static JavaUnicodeEscaper Above(int codepoint)
        {
            return OutsideOf(0, codepoint);
        }

        /// <summary>
        /// <para />Constructs a <code>JavaUnicodeEscaper</code> below the specified value (exclusive).
        /// </summary>
        /// <param name="codepoint">below which to escape</param>
        /// <returns>the newly created {@code UnicodeEscaper} instance</returns>
        public static JavaUnicodeEscaper Below(int codepoint)
        {
            return OutsideOf(codepoint, Int32.MaxValue);
        }

        /// <summary>
        /// <para />Constructs a <code>JavaUnicodeEscaper</code> between the specified values (inclusive).
        /// </summary>
        /// <param name="codepointLow">above which to escape</param>
        /// <param name="codepointHigh">below which to escape</param>
        /// <returns>the newly created {@code UnicodeEscaper} instance</returns>
        public static JavaUnicodeEscaper Between(
            int codepointLow,
            int codepointHigh)
        {
            return new JavaUnicodeEscaper(codepointLow, codepointHigh, true);
        }

        /// <summary>
        /// <para />Constructs a <code>JavaUnicodeEscaper</code> outside of the specified values (exclusive).
        /// </summary>
        /// <param name="codepointLow">below which to escape</param>
        /// <param name="codepointHigh">above which to escape</param>
        /// <returns>the newly created {@code UnicodeEscaper} instance</returns>
        public static JavaUnicodeEscaper OutsideOf(
            int codepointLow,
            int codepointHigh)
        {
            return new JavaUnicodeEscaper(codepointLow, codepointHigh, false);
        }

        /// <summary>
        /// <para />Constructs a <code>JavaUnicodeEscaper</code> for the specified range. This is the underlying method for the
        /// other constructors/builders. The <code>below</code> and <code>above</code> boundaries are inclusive when
        /// <code>between</code> is <code>true</code> and exclusive when it is <code>false</code>.
        /// </summary>
        /// <param name="below">int value representing the lowest codepoint boundary</param>
        /// <param name="above">int value representing the highest codepoint boundary</param>
        /// <param name="between">whether to escape between the boundaries or outside them</param>
        public JavaUnicodeEscaper(
            int below,
            int above,
            bool between)
            : base(below, above, between)
        {
        }

        /// <summary>
        /// Converts the given codepoint to a hex string of the form {@code "\\uXXXX\\uXXXX"}.
        /// </summary>
        /// <param name="codepoint">a Unicode code point</param>
        /// <returns>the hex string for the given codepoint</returns>
        protected string ToUtf16Escape(int codepoint)
        {
            char[] surrogatePair = Character.ToChars(codepoint);
            return "\\u" + Hex(surrogatePair[0]) + "\\u" + Hex(surrogatePair[1]);
        }
    }
} // end of namespace