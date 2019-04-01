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
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util.apachecommonstext
{
    /// <summary>
    ///     Translates a value using a lookup table.
    /// </summary>
    /// <unknown>@since 1.0</unknown>
    public class LookupTranslator : CharSequenceTranslator
    {
        /// <summary>
        ///     The length of the longest key in the lookupMap.
        /// </summary>
        private readonly int _longest;

        /// <summary>
        ///     The mapping to be used in translation.
        /// </summary>
        private readonly IDictionary<string, string> _lookupMap;

        /// <summary>
        ///     The first character of each key in the lookupMap.
        /// </summary>
        private readonly HashSet<char> _prefixSet;

        /// <summary>
        ///     The length of the shortest key in the lookupMap.
        /// </summary>
        private readonly int _shortest;

        /// <summary>
        ///     Define the lookup table to be used in translation
        ///     <para />
        ///     Note that, as of Lang 3.1 (the origin of this code), the key to the lookup
        ///     table is converted to a java.lang.String. This is because we need the key
        ///     to support hashCode and equals(Object), allowing it to be the key for a
        ///     HashMap. See LANG-882.
        /// </summary>
        /// <param name="lookupMap">
        ///     Map&amp;lt;CharSequence, CharSequence&amp;gt; table of translatormappings
        /// </param>
        public LookupTranslator(IDictionary<string, string> lookupMap)
        {
            if (lookupMap == null) {
                throw new ArgumentNullException("lookupMap cannot be null", nameof(lookupMap));
            }

            _lookupMap = new Dictionary<string, string>();
            _prefixSet = new HashSet<char>();
            var currentShortest = int.MaxValue;
            var currentLongest = 0;

            foreach (var pair in lookupMap) {
                _lookupMap.Put(pair.Key, pair.Value);
                _prefixSet.Add(pair.Key[0]);
                int sz = pair.Key.Length;
                if (sz < currentShortest) {
                    currentShortest = sz;
                }

                if (sz > currentLongest) {
                    currentLongest = sz;
                }
            }

            _shortest = currentShortest;
            _longest = currentLongest;
        }

        /// <summary>
        ///     {@inheritDoc}
        /// </summary>
        public override int Translate(
            string input,
            int index,
            TextWriter @out)
        {
            // check if translation exists for the input at position index
            if (_prefixSet.Contains(input[index])) {
                var max = _longest;
                if (index + _longest > input.Length) {
                    max = input.Length - index;
                }

                // implement greedy algorithm by trying maximum match first
                for (var i = max; i >= _shortest; i--) {
                    string subSeq = input.Between(index, index + i);
                    var result = _lookupMap.Get(subSeq);

                    if (result != null) {
                        @out.Write(result);
                        return i;
                    }
                }
            }

            return 0;
        }
    }
} // end of namespace