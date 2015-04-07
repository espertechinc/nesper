///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// MapIndex lookup strategy that coerces the key values before performing a lookup.
    /// </summary>
    public class SubordIndexedTableLookupStrategyCoercingNW : SubordIndexedTableLookupStrategyExprNW
    {
        private readonly Type[] _coercionTypes;
    
        public SubordIndexedTableLookupStrategyCoercingNW(ExprEvaluator[] evaluators, PropertyIndexedEventTable index, Type[] coercionTypes, LookupStrategyDesc strategyDesc)
            : base(evaluators, index, strategyDesc)
        {
            _coercionTypes = coercionTypes;
        }
    
        protected override Object[] GetKeys(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            var keys = base.GetKeys(eventsPerStream, context);
            for (var i = 0; i < keys.Length; i++)
            {
                var value = keys[i];
    
                var coercionType = _coercionTypes[i];
                if ((value != null) && (value.GetType() != coercionType))
                {
                    if (value.IsNumber())
                    {
                        value = CoercerFactory.CoerceBoxed(value, _coercionTypes[i]);
                    }
                    keys[i] = value;
                }
            }
            return keys;
        }
    }
}
