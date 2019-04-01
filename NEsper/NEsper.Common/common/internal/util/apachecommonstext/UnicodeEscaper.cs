/*
 * CREDIT: Apache-Common-Text Version 1.2
 * Apache V2 licensed per https://commons.apache.org/proper/commons-text
 */

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;

namespace com.espertech.esper.common.@internal.util.apachecommonstext
{
	/// <summary>
	/// Translates codepoints to their Unicode escaped value.
	/// </summary>
	/// <unknown>@since 1.0</unknown>
	public class UnicodeEscaper : CodePointTranslator {

	    /// <summary>
	    /// int value representing the lowest codepoint boundary.
	    /// </summary>
	    private readonly int below;
	    /// <summary>
	    /// int value representing the highest codepoint boundary.
	    /// </summary>
	    private readonly int above;
	    /// <summary>
	    /// whether to escape between the boundaries or outside them.
	    /// </summary>
	    private readonly bool between;

	    /// <summary>
	    /// <para />Constructs a <code>UnicodeEscaper</code> for all characters.
	    /// </summary>
	    public UnicodeEscaper()
	        : this(0, Int32.MaxValue, true)
	    {
	    }

	    /// <summary>
	    /// <para />Constructs a <code>UnicodeEscaper</code> for the specified range. This is
	    /// the underlying method for the other constructors/builders. The <code>below</code>and <code>above</code> boundaries are inclusive when <code>between</code> is
	    /// </summary>
	    /// <param name="below">int value representing the lowest codepoint boundary</param>
	    /// <param name="above">int value representing the highest codepoint boundary</param>
	    /// <param name="between">whether to escape between the boundaries or outside them</param>
	    protected UnicodeEscaper(int below, int above, bool between) {
	        this.below = below;
	        this.above = above;
	        this.between = between;
	    }

	    /// <summary>
	    /// <p>Constructs a <code>UnicodeEscaper</code> below the specified value (exclusive). </p></summary>
	    /// <param name="codepoint">below which to escape</param>
	    /// <returns>the newly created {@code UnicodeEscaper} instance</returns>
	    public static UnicodeEscaper Below(int codepoint)
	    {
	        return OutsideOf(codepoint, Int32.MaxValue);
	    }

	    /// <summary>
	    /// <p>Constructs a <code>UnicodeEscaper</code> above the specified value (exclusive). </p></summary>
	    /// <param name="codepoint">above which to escape</param>
	    /// <returns>the newly created {@code UnicodeEscaper} instance</returns>
	    public static UnicodeEscaper Above(int codepoint) {
	        return OutsideOf(0, codepoint);
	    }

	    /// <summary>
	    /// <p>Constructs a <code>UnicodeEscaper</code> outside of the specified values (exclusive). </p></summary>
	    /// <param name="codepointLow">below which to escape</param>
	    /// <param name="codepointHigh">above which to escape</param>
	    /// <returns>the newly created {@code UnicodeEscaper} instance</returns>
	    public static UnicodeEscaper OutsideOf(int codepointLow, int codepointHigh) {
	        return new UnicodeEscaper(codepointLow, codepointHigh, false);
	    }

	    /// <summary>
	    /// <p>Constructs a <code>UnicodeEscaper</code> between the specified values (inclusive). </p></summary>
	    /// <param name="codepointLow">above which to escape</param>
	    /// <param name="codepointHigh">below which to escape</param>
	    /// <returns>the newly created {@code UnicodeEscaper} instance</returns>
	    public static UnicodeEscaper Between(int codepointLow, int codepointHigh) {
	        return new UnicodeEscaper(codepointLow, codepointHigh, true);
	    }

	    /// <summary>
	    /// {@inheritDoc}
	    /// </summary>
	    public override bool Translate(int codepoint, TextWriter @out) {
	        if (between) {
	            if (codepoint < below || codepoint > above) {
	                return false;
	            }
	        } else {
	            if (codepoint >= below && codepoint <= above) {
	                return false;
	            }
	        }

	        if (codepoint > 0xffff) {
	            @out.Write(ToUtf16Escape(codepoint));
	        } else {
	            @out.Write("\\u");
	            @out.Write(HEX_DIGITS[(codepoint >> 12) & 15]);
	            @out.Write(HEX_DIGITS[(codepoint >> 8) & 15]);
	            @out.Write(HEX_DIGITS[(codepoint >> 4) & 15]);
	            @out.Write(HEX_DIGITS[codepoint & 15]);
	        }
	        return true;
	    }

	    /// <summary>
	    /// Converts the given codepoint to a hex string of the form {@code "\\uXXXX"}.
	    /// </summary>
	    /// <param name="codepoint">a Unicode code point</param>
	    /// <returns>the hex string for the given codepoint</returns>
	    protected string ToUtf16Escape(int codepoint) {
	        return "\\u" + Hex(codepoint);
	    }
	}
} // end of namespace