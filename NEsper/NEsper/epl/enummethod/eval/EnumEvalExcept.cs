///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalExcept : EnumEval
    {
        private readonly int _numStreams;
        private readonly ExprEvaluatorEnumeration _evaluator;
        private readonly bool _scalar;

        public EnumEvalExcept(int numStreams, ExprEvaluatorEnumeration evaluator, bool scalar)
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
                set = _evaluator.EvaluateGetROCollectionScalar(new EvaluateParams(eventsLambda, isNewData, context));
            }
            else
            {
                set = _evaluator
                    .EvaluateGetROCollectionEvents(new EvaluateParams(eventsLambda, isNewData, context))
                    .TransformInto(o => (EventBean) o, e => (Object) e);
            }

            if (set == null || set.IsEmpty() || target.IsEmpty())
            {
                return target;
            }

            if (_scalar)
            {
                var resultX = new List<Object>(target);
                resultX.RemoveAll(set);
                return resultX;
            }

            var targetEvents = target.OfType<EventBean>();
            var sourceEvents = set.OfType<EventBean>();
            var result = new List<EventBean>();

            // we compare event underlying
            foreach (EventBean targetEvent in targetEvents)
            {
                if (targetEvent == null)
                {
                    result.Add(null);
                    continue;
                }

                bool found = false;
                foreach (EventBean sourceEvent in sourceEvents)
                {
                    if (targetEvent == sourceEvent)
                    {
                        found = true;
                        break;
                    }
                    if (sourceEvent == null)
                    {
                        continue;
                    }
                    if (targetEvent.Underlying.Equals(sourceEvent.Underlying))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    result.Add(targetEvent);
                }
            }
            return result;
        }
    }
}
