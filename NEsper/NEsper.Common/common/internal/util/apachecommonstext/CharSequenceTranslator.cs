///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using com.espertech.esper.common.client;

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

namespace com.espertech.esper.common.@internal.util.apachecommonstext
{
    /// <summary>
    ///     An API for translating text.
    ///     Its core use is to escape and unescape text. Because escaping and unescaping
    ///     is completely contextual, the API does not present two separate signatures.
    /// </summary>
    /// <unknown>@since 1.0</unknown>
    public abstract class CharSequenceTranslator
    {
        /// <summary>
        ///     Array containing the hexadecimal alphabet.
        /// </summary>
        public static readonly char[] HEX_DIGITS = {
            '0', '1', '2', '3',
            '4', '5', '6', '7',
            '8', '9', 'A', 'B',
            'C', 'D', 'E', 'F'
        };

        /// <summary>
        ///     Translate a set of codepoints, represented by an int index into a CharSequence,
        ///     into another set of codepoints. The number of codepoints consumed must be returned,
        ///     and the only IOExceptions thrown must be from interacting with the Writer so that
        ///     the top level API may reliably ignore StringWriter IOExceptions.
        /// </summary>
        /// <param name="input">CharSequence that is being translated</param>
        /// <param name="index">int representing the current point of translation</param>
        /// <param name="out">Writer to translate the text to</param>
        /// <returns>int count of codepoints consumed</returns>
        /// <throws>IOException if and only if the Writer produces an IOException</throws>
        public abstract int Translate(
            string input,
            int index,
            TextWriter @out);

        /// <summary>
        ///     Helper for non-Writer usage.
        /// </summary>
        /// <param name="input">CharSequence to be translated</param>
        /// <returns>String output of translation</returns>
        public virtual string Translate(string input)
        {
            if (input == null) {
                return null;
            }

            try {
                var writer = new StringWriter();
                Translate(input, writer);
                return writer.ToString();
            }
            catch (IOException ioe) {
                // this should never ever happen while writing to a StringWriter
                throw new EPRuntimeException(ioe);
            }
        }

        /// <summary>
        ///     Translate an input onto a Writer. This is intentionally final as its algorithm is
        ///     tightly coupled with the abstract method of this class.
        /// </summary>
        /// <param name="input">CharSequence that is being translated</param>
        /// <param name="out">Writer to translate the text to</param>
        /// <throws>IOException if and only if the Writer produces an IOException</throws>
        public virtual void Translate(
            string input,
            TextWriter @out)
        {
            if (@out == null) {
                throw new ArgumentException("The Writer must not be null");
            }

            if (input == null) {
                return;
            }

            var pos = 0;
            var len = input.Length;
            while (pos < len) {
                var consumed = Translate(input, pos, @out);
                if (consumed == 0) {
                    // inlined implementation of Character.toChars(Character.codePointAt(input, pos))
                    // avoids allocating temp char arrays and duplicate checks
                    var c1 = input[pos];
                    @out.Write(c1);
                    pos++;
                    if (Character.IsHighSurrogate(c1) && pos < len) {
                        char c2 = input.CharAt(pos);
                        if (Character.IsLowSurrogate(c2)) {
                            @out.Write(c2);
                            pos++;
                        }
                    }

                    continue;
                }

                // contract with translators is that they have to understand codepoints
                // and they just took care of a surrogate pair
                for (var pt = 0; pt < consumed; pt++) {
                    pos += Character.CharCount(Character.CodePointAt(input, pos));
                }
            }
        }

        /// <summary>
        ///     Helper method to create a merger of this translator with another set of
        ///     translators. Useful in customizing the standard functionality.
        /// </summary>
        /// <param name="translators">CharSequenceTranslator array of translators to merge with this one</param>
        /// <returns>CharSequenceTranslator merging this translator with the others</returns>
        public CharSequenceTranslator With(params CharSequenceTranslator[] translators)
        {
            var newArray = new CharSequenceTranslator[translators.Length + 1];
            newArray[0] = this;
            Array.Copy(translators, 0, newArray, 1, translators.Length);
            return new AggregateTranslator(newArray);
        }

        /// <summary>
        ///     <para />
        ///     Returns an upper case hexadecimal <code>String</code> for the given
        /// </summary>
        /// <param name="codepoint">The codepoint to convert.</param>
        /// <returns>An upper case hexadecimal &lt;code&gt;String&lt;/code&gt;</returns>
        public static string Hex(int codepoint)
        {
            return codepoint.ToString("X2").ToUpperInvariant();
        }
    }
} // end of namespace