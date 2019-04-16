///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class SubordTableLookupStrategyQuadTreeBase
    {
        private readonly SubordTableLookupStrategyFactoryQuadTree factory;
        protected internal readonly EventTableQuadTree index;

        public SubordTableLookupStrategyQuadTreeBase(
            EventTableQuadTree index,
            SubordTableLookupStrategyFactoryQuadTree factory)
        {
            this.index = index;
            this.factory = factory;
        }

        public LookupStrategyDesc StrategyDesc => factory.LookupStrategyDesc;

        protected ICollection<EventBean> LookupInternal(
            EventBean[] events,
            ExprEvaluatorContext context,
            EventTableQuadTree index,
            SubordTableLookupStrategy strategy)
        {
            var x = Eval(factory.X, events, context, "x");
            var y = Eval(factory.Y, events, context, "y");
            var width = Eval(factory.Width, events, context, "width");
            var height = Eval(factory.Height, events, context, "height");

            if (context.InstrumentationProvider.Activated()) {
                context.InstrumentationProvider.QIndexSubordLookup(strategy, index, null);
                var result = this.index.QueryRange(x, y, width, height);
                context.InstrumentationProvider.AIndexSubordLookup(result, null);
                return result;
            }

            return this.index.QueryRange(x, y, width, height);
        }

        public string ToQueryPlan()
        {
            return GetType().GetSimpleName();
        }

        private double Eval(
            ExprEvaluator eval,
            EventBean[] events,
            ExprEvaluatorContext context,
            string name)
        {
            var number = eval.Evaluate(events, true, context);
            if (number == null) {
                throw new EPException("Invalid null value for '" + name + "'");
            }

            return number.AsDouble();
        }
    }
} // end of namespace