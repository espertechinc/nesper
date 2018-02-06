///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.index.quadtree
{
    public class SubordTableLookupStrategyQuadTreeBase
    {
        private readonly SubordTableLookupStrategyFactoryQuadTree _factory;
        private readonly EventTableQuadTree _index;

        public SubordTableLookupStrategyQuadTreeBase(EventTableQuadTree index,
            SubordTableLookupStrategyFactoryQuadTree factory)
        {
            _index = index;
            _factory = factory;
        }

        public LookupStrategyDesc StrategyDesc => _factory.LookupStrategyDesc;

        protected ICollection<EventBean> LookupInternal(EventBean[] events, ExprEvaluatorContext context)
        {
            var x = Eval(_factory.X, events, context, "x");
            var y = Eval(_factory.Y, events, context, "y");
            var width = Eval(_factory.Width, events, context, "width");
            var height = Eval(_factory.Height, events, context, "height");
            return _index.QueryRange(x, y, width, height);
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }

        private double Eval(ExprEvaluator eval, EventBean[] events, ExprEvaluatorContext context, string name)
        {
            var number = eval.Evaluate(new EvaluateParams(events, true, context));
            if (number == null) throw new EPException("Invalid null value for '" + name + "'");
            return number.AsDouble();
        }
    }
} // end of namespace