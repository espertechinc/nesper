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

using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.@internal.util.apachecommonstext
{
    /// <summary>
    ///     Executes a sequence of translators one after the other. Execution ends whenever
    ///     the first translator consumes codepoints from the input.
    /// </summary>
    /// <unknown>@since 1.0</unknown>
    public class AggregateTranslator : CharSequenceTranslator
    {
        /// <summary>
        ///     Translator list.
        /// </summary>
        private readonly IList<CharSequenceTranslator> translators = new List<CharSequenceTranslator>();

        /// <summary>
        ///     Specify the translators to be used at creation time.
        /// </summary>
        /// <param name="translators">CharSequenceTranslator array to aggregate</param>
        public AggregateTranslator(params CharSequenceTranslator[] translators)
        {
            if (translators != null) {
                foreach (var translator in translators) {
                    if (translator != null) {
                        this.translators.Add(translator);
                    }
                }
            }
        }

        /// <summary>
        ///     The first translator to consume codepoints from the input is the 'winner'.
        ///     Execution stops with the number of consumed codepoints being returned.
        ///     {@inheritDoc}
        /// </summary>
        public override int Translate(
            string input,
            int index,
            TextWriter @out)
        {
            foreach (var translator in translators) {
                var consumed = translator.Translate(input, index, @out);
                if (consumed != 0) {
                    return consumed;
                }
            }

            return 0;
        }
    }
} // end of namespace