///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalUnion : EnumEval
    {
        private readonly int _numStreams;
        private readonly ExprEvaluatorEnumeration _evaluator;
        private readonly bool _scalar;

        public EnumEvalUnion(int numStreams, ExprEvaluatorEnumeration evaluator, bool scalar)
        {
            _numStreams = numStreams;
            _evaluator = evaluator;
            _scalar = scalar;
        }

        public int StreamNumSize
        {
            get { return _numStreams; }
        }

        public Object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            if (target == null)
            {
                return null;
            }

            ICollection<object> set;
            if (_scalar)
            {
                set = _evaluator.EvaluateGetROCollectionScalar(eventsLambda, isNewData, context);
            }
            else
            {
                set = _evaluator.EvaluateGetROCollectionEvents(eventsLambda, isNewData, context).TransformInto(
                    o => (EventBean)o,
                    e => (Object)e);
            }

            if (set == null || set.IsEmpty())
            {
                return target;
            }

            var result = new List<Object>(target);
            result.AddAll(set);

            return result;
        }
    }
}
