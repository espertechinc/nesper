///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    public class LookupStrategyDesc
    {
        public static readonly LookupStrategyDesc SCAN = new LookupStrategyDesc(LookupStrategyType.FULLTABLESCAN);

        public LookupStrategyDesc(LookupStrategyType lookupStrategy)
        {
            LookupStrategy = lookupStrategy;
            ExpressionsTexts = CollectionUtil.STRINGARRAY_EMPTY;
        }

        public LookupStrategyDesc(
            LookupStrategyType lookupStrategy,
            string[] expressionsTexts)
        {
            LookupStrategy = lookupStrategy;
            ExpressionsTexts = expressionsTexts;
        }

        public LookupStrategyType LookupStrategy { get; }

        public string[] ExpressionsTexts { get; }

        public override string ToString()
        {
            return "LookupStrategyDesc{" +
                   "lookupStrategy=" +
                   LookupStrategy +
                   ", expressionsTexts=" +
                   ExpressionsTexts +
                   '}';
        }
    }
} // end of namespace