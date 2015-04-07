///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// MapIndex lookup strategy that coerces the key values before performing a lookup.
    /// </summary>
    public class SubordIndexedTableLookupStrategySingleCoercing : SubordIndexedTableLookupStrategySingleExpr
    {
        private readonly Type _coercionType;
    
        public SubordIndexedTableLookupStrategySingleCoercing(int streamCountOuter, ExprEvaluator evaluator, PropertyIndexedEventTableSingle index, Type coercionType, LookupStrategyDesc strategyDesc)
            : base(streamCountOuter, evaluator, index, strategyDesc)
        {
            _coercionType = coercionType;
        }
    
        protected override Object GetKey(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            Object key = base.GetKey(eventsPerStream, context);
            return EventBeanUtility.Coerce(key, _coercionType);
        }
    }
}
