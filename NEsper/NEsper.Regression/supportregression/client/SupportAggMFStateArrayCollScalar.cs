///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.supportregression.client
{
    public class SupportAggMFStateArrayCollScalar : AggregationState
    {
        private readonly SupportAggMFStateArrayCollScalarFactory _factory;
        private readonly List<object> _values = new List<object>();
    
        public SupportAggMFStateArrayCollScalar(SupportAggMFStateArrayCollScalarFactory factory)
        {
            _factory = factory;
        }
    
        public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var value = _factory.Evaluator.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            _values.Add(value);
        }
    
        public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            // ever semantics
        }
    
        public void Clear()
        {
            _values.Clear();
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public object ValueAsArray
        {
            get
            {
                var array = Array.CreateInstance(_factory.Evaluator.ReturnType, _values.Count);
                var it = _values.GetEnumerator();
                int count = 0;
                for (; it.MoveNext();)
                {
                    var value = it.Current;
                    array.SetValue(value, count++);
                }
                return array;
            }
        }

        public object ValueAsCollection
        {
            get { return _values; }
        }
    }
}
