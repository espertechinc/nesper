///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportReferenceCountedMapState : AggregationMultiFunctionState
    {
        public IDictionary<object, int> CountPerReference { get; } = new LinkedHashMap<object, int>();

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // no need to implement, we mutate using enter and leave instead
            throw new UnsupportedOperationException("Use enter instead");
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // no need to implement, we mutate using enter and leave instead
            throw new UnsupportedOperationException("Use leave instead");
        }

        public void Clear()
        {
            CountPerReference.Clear();
        }

        public void Enter(object key)
        {
            if (!CountPerReference.TryGetValue(key, out var count)) {
                CountPerReference.Put(key, 1);
            }
            else {
                CountPerReference.Put(key, count + 1);
            }
        }

        public void Leave(object key)
        {
            if (CountPerReference.TryGetValue(key, out var count)) {
                if (count == 1) {
                    CountPerReference.Remove(key);
                }
                else {
                    CountPerReference.Put(key, count - 1);
                }
            }
        }
    }
} // end of namespace