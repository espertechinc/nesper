///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportAggMFMultiRTArrayCollScalarState : AggregationMultiFunctionState
    {
        private readonly SupportAggMFMultiRTArrayCollScalarStateFactory factory;
        private readonly IList<object> values = new List<object>();

        public SupportAggMFMultiRTArrayCollScalarState(SupportAggMFMultiRTArrayCollScalarStateFactory factory)
        {
            this.factory = factory;
        }

        public ICollection<object> ValueAsCollection => values;

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var value = factory.Evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);
            values.Add(value);
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // ever semantics
        }

        public void Clear()
        {
            values.Clear();
        }

        public int Size()
        {
            return values.Count;
        }

        public object GetValueAsArray()
        {
            var array = Array.CreateInstance(factory.EvaluationType, values.Count);
            var it = values.GetEnumerator();
            var count = 0;
            for (; it.MoveNext();) {
                var value = it.Current;
                array.SetValue(value, count++);
            }

            return array;
        }
    }
} // end of namespace