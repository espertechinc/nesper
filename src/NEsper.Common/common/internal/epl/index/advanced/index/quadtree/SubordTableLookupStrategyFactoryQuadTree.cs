///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class SubordTableLookupStrategyFactoryQuadTree : SubordTableLookupStrategyFactory
    {
        public ExprEvaluator X { get; set; }

        public ExprEvaluator Y { get; set; }

        public ExprEvaluator Width { get; set; }

        public ExprEvaluator Height { get; set; }

        public bool IsNwOnTrigger { get; set; }

        public string[] LookupExpressions { get; set; }

        public int StreamCountOuter { get; set; }

        public SubordTableLookupStrategy MakeStrategy(
            EventTable[] eventTable,
            ExprEvaluatorContext exprEvaluatorContext,
            VirtualDWView vdw)
        {
            if (IsNwOnTrigger) {
                return new SubordTableLookupStrategyQuadTreeNW((EventTableQuadTree) eventTable[0], this);
            }

            return new SubordTableLookupStrategyQuadTreeSubq(
                (EventTableQuadTree) eventTable[0],
                this,
                StreamCountOuter);
        }

        public LookupStrategyDesc LookupStrategyDesc =>
            new LookupStrategyDesc(LookupStrategyType.ADVANCED, LookupExpressions);

        public string ToQueryPlan()
        {
            return GetType().GetSimpleName();
        }
    }
} // end of namespace