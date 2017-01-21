///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.@join.exec.composite;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// MapIndex lookup strategy for subqueries.
    /// </summary>
    public class SubordCompositeTableLookupStrategyFactory : SubordTableLookupStrategyFactory
    {
        private readonly CompositeIndexQuery _innerIndexQuery;
        private readonly ICollection<SubordPropRangeKey> _rangeDescs;
        private readonly LookupStrategyDesc _strategyDesc;

        public SubordCompositeTableLookupStrategyFactory(
            bool isNWOnTrigger,
            int numStreams,
            ICollection<SubordPropHashKey> keyExpr,
            Type[] coercionKeyTypes,
            ICollection<SubordPropRangeKey> rangeProps,
            Type[] coercionRangeTypes)
        {
            _rangeDescs = rangeProps;
            var expressionTexts = new List<String>();
            _innerIndexQuery = CompositeIndexQueryFactory.MakeSubordinate(
                isNWOnTrigger, numStreams, keyExpr, coercionKeyTypes, rangeProps, coercionRangeTypes, expressionTexts);
            _strategyDesc = new LookupStrategyDesc(LookupStrategyType.COMPOSITE, expressionTexts.ToArray());
        }

        public SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw)
        {
            return new SubordCompositeTableLookupStrategy(
                _innerIndexQuery, (PropertyCompositeEventTable) eventTable[0], _strategyDesc);
        }

        public String ToQueryPlan()
        {
            return GetType().FullName + " ranges=" + SubordPropRangeKey.ToQueryPlan(_rangeDescs);
        }
    }
}
