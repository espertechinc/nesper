///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.mostleastfreq {
    public class EnumMostLeastFrequentHelper {
        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="items">items</param>
        /// <param name="mostFrequent">flag</param>
        /// <returns>value</returns>
        public static object GetEnumMostLeastFrequentResult(
            IDictionary<object, int> items,
            bool mostFrequent)
        {
            if (mostFrequent) {
                object maxKey = null;
                var max = int.MinValue;
                foreach (var entry in items) {
                    if (entry.Value > max) {
                        maxKey = entry.Key;
                        max = entry.Value;
                    }
                }

                return maxKey;
            }

            var min = int.MaxValue;
            object minKey = null;
            foreach (var entry in items) {
                if (entry.Value < min) {
                    minKey = entry.Key;
                    min = entry.Value;
                }
            }

            return minKey;
        }
    }
} // end of namespace