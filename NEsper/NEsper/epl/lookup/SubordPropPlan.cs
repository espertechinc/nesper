///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.lookup
{
    public class SubordPropPlan
    {
        public SubordPropPlan()
        {
            HashProps = new LinkedHashMap<String, SubordPropHashKey>();
            RangeProps = new LinkedHashMap<String, SubordPropRangeKey>();
            InKeywordSingleIndex = null;
            InKeywordMultiIndex = null;
        }

        public SubordPropPlan(
            IDictionary<String, SubordPropHashKey> hashProps,
            IDictionary<String, SubordPropRangeKey> rangeProps,
            SubordPropInKeywordSingleIndex inKeywordSingleIndex,
            SubordPropInKeywordMultiIndex inKeywordMultiIndex)
        {
            HashProps = hashProps;
            RangeProps = rangeProps;
            InKeywordSingleIndex = inKeywordSingleIndex;
            InKeywordMultiIndex = inKeywordMultiIndex;
        }

        public IDictionary<string, SubordPropRangeKey> RangeProps { get; private set; }

        public IDictionary<string, SubordPropHashKey> HashProps { get; private set; }

        public SubordPropInKeywordSingleIndex InKeywordSingleIndex { get; private set; }

        public SubordPropInKeywordMultiIndex InKeywordMultiIndex { get; private set; }
    }
}
