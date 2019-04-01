///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.lookupplan
{
    public class SubordPropPlan
    {
        public SubordPropPlan()
        {
            HashProps = Collections.GetEmptyMap<string, SubordPropHashKeyForge>();
            RangeProps = Collections.GetEmptyMap<string, SubordPropRangeKeyForge>();
            InKeywordSingleIndex = null;
            InKeywordMultiIndex = null;
            CustomIndexOps = null;
        }

        public SubordPropPlan(
            IDictionary<string, SubordPropHashKeyForge> hashProps,
            IDictionary<string, SubordPropRangeKeyForge> rangeProps,
            SubordPropInKeywordSingleIndex inKeywordSingleIndex, SubordPropInKeywordMultiIndex inKeywordMultiIndex,
            IDictionary<QueryGraphValueEntryCustomKeyForge, QueryGraphValueEntryCustomOperationForge> customIndexOps)
        {
            HashProps = hashProps;
            RangeProps = rangeProps;
            InKeywordSingleIndex = inKeywordSingleIndex;
            InKeywordMultiIndex = inKeywordMultiIndex;
            CustomIndexOps = customIndexOps;
        }

        public IDictionary<string, SubordPropRangeKeyForge> RangeProps { get; }

        public IDictionary<string, SubordPropHashKeyForge> HashProps { get; }

        public SubordPropInKeywordSingleIndex InKeywordSingleIndex { get; }

        public SubordPropInKeywordMultiIndex InKeywordMultiIndex { get; }

        public IDictionary<QueryGraphValueEntryCustomKeyForge, QueryGraphValueEntryCustomOperationForge> CustomIndexOps { get; }
    }
} // end of namespace