///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.join.plan;

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
            CustomIndexOps = null;
        }

        public SubordPropPlan(
            IDictionary<String, SubordPropHashKey> hashProps,
            IDictionary<String, SubordPropRangeKey> rangeProps,
            SubordPropInKeywordSingleIndex inKeywordSingleIndex,
            SubordPropInKeywordMultiIndex inKeywordMultiIndex,
            IDictionary<QueryGraphValueEntryCustomKey, QueryGraphValueEntryCustomOperation> customIndexOps)
        {
            HashProps = hashProps;
            RangeProps = rangeProps;
            InKeywordSingleIndex = inKeywordSingleIndex;
            InKeywordMultiIndex = inKeywordMultiIndex;
            CustomIndexOps = customIndexOps;
        }

        public IDictionary<string, SubordPropRangeKey> RangeProps { get; private set; }

        public IDictionary<string, SubordPropHashKey> HashProps { get; private set; }

        public SubordPropInKeywordSingleIndex InKeywordSingleIndex { get; private set; }

        public SubordPropInKeywordMultiIndex InKeywordMultiIndex { get; private set; }

        public IDictionary<QueryGraphValueEntryCustomKey, QueryGraphValueEntryCustomOperation> CustomIndexOps { get; private set; }
    }
}
