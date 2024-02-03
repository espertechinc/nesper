///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public abstract class MethodConversionStrategyCollection : MethodConversionStrategyBase
    {
        protected abstract EventBean GetEventBean(
            object value,
            ExprEvaluatorContext exprEvaluatorContext);

        public override IList<EventBean> Convert(
            object invocationResult,
            MethodTargetStrategy origin,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var collection = invocationResult.AsObjectCollection();
            var length = collection.Count;
            if (length == 0) {
                return EmptyList<EventBean>.Instance;
            }

            if (length == 1) {
                object value = collection.First();
                if (CheckNonNullArrayValue(value, origin)) {
                    var @event = GetEventBean(value, exprEvaluatorContext);
                    return Collections.SingletonList(@event);
                }

                return EmptyList<EventBean>.Instance;
            }

            var rowResult = new List<EventBean>(length);
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext()) {
                var value = enumerator.Current;
                if (CheckNonNullArrayValue(value, origin)) {
                    var @event = GetEventBean(value, exprEvaluatorContext);
                    rowResult.Add(@event);
                }
            }

            return rowResult;
        }
    }
} // end of namespace