///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.index.quadtree
{
    public class SubordTableLookupStrategyFactoryQuadTree : SubordTableLookupStrategyFactory
    {
        private readonly bool _isNWOnTrigger;
        private readonly int _streamCountOuter;

        public SubordTableLookupStrategyFactoryQuadTree(ExprEvaluator x, ExprEvaluator y, ExprEvaluator width,
            ExprEvaluator height, bool isNWOnTrigger, int streamCountOuter, LookupStrategyDesc lookupStrategyDesc)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _isNWOnTrigger = isNWOnTrigger;
            _streamCountOuter = streamCountOuter;
            LookupStrategyDesc = lookupStrategyDesc;
        }

        public ExprEvaluator X { get; }

        public ExprEvaluator Y { get; }

        public ExprEvaluator Width { get; }

        public ExprEvaluator Height { get; }

        public LookupStrategyDesc LookupStrategyDesc { get; }

        public SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw)
        {
            if (_isNWOnTrigger)
                return new SubordTableLookupStrategyQuadTreeNW((EventTableQuadTree) eventTable[0], this);
            return new SubordTableLookupStrategyQuadTreeSubq((EventTableQuadTree) eventTable[0], this, _streamCountOuter);
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace