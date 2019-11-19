///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    /// Pattern matching utility.
    /// </summary>
    public class StringPatternSetUtil
    {
        /// <summary>
        /// Executes a seriers of include/exclude patterns against a match string,
        /// returning the last pattern match result as bool in/out.
        /// </summary>
        /// <param name="defaultValue">the default value if there are no patterns or no matches change the value</param>
        /// <param name="patterns">to match against, true in the pair for include, false for exclude</param>
        /// <param name="literal">to match</param>
        /// <returns>true for included, false for excluded</returns>
        public static bool Evaluate(
            bool defaultValue,
            IEnumerable<Pair<StringPatternSet, bool>> patterns,
            string literal)
        {
            bool result = defaultValue;

            foreach (var item in patterns) {
                if (result) {
                    if (!item.Second) {
                        bool testResult = item.First.Match(literal);
                        if (testResult) {
                            result = false;
                        }
                    }
                }
                else {
                    if (item.Second) {
                        bool testResult = item.First.Match(literal);
                        if (testResult) {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }
    }
}