///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.unindexed;
using com.espertech.esper.common.@internal.epl.virtualdw;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Factory for lookup on an unindexed table returning the full table as matching events.
    /// </summary>
    public class SubordFullTableScanLookupStrategyFactory : SubordTableLookupStrategyFactory
    {
        public static readonly SubordFullTableScanLookupStrategyFactory INSTANCE =
            new SubordFullTableScanLookupStrategyFactory();

        private SubordFullTableScanLookupStrategyFactory()
        {
        }

        public SubordTableLookupStrategy MakeStrategy(
            EventTable[] eventTable,
            ExprEvaluatorContext exprEvaluatorContext,
            VirtualDWView vdw)
        {
            return new SubordFullTableScanLookupStrategy((UnindexedEventTable)eventTable[0]);
        }

        public LookupStrategyDesc LookupStrategyDesc => LookupStrategyDesc.SCAN;

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace