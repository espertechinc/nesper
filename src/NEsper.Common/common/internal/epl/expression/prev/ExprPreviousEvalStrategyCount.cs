///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.access;

namespace com.espertech.esper.common.@internal.epl.expression.prev
{
    public class ExprPreviousEvalStrategyCount : ExprPreviousEvalStrategy
    {
        private readonly int _streamNumber;
        private readonly RandomAccessByIndexGetter _randomAccessGetter;
        private readonly RelativeAccessByEventNIndexGetter _relativeAccessGetter;

        public ExprPreviousEvalStrategyCount(
            int streamNumber,
            RandomAccessByIndexGetter randomAccessGetter,
            RelativeAccessByEventNIndexGetter relativeAccessGetter)
        {
            _streamNumber = streamNumber;
            _randomAccessGetter = randomAccessGetter;
            _relativeAccessGetter = relativeAccessGetter;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            long size;
            if (_randomAccessGetter != null) {
                var randomAccess = _randomAccessGetter.Accessor;
                size = randomAccess.WindowCount;
            }
            else {
                var evalEvent = eventsPerStream[_streamNumber];
                var relativeAccess = _relativeAccessGetter.GetAccessor(evalEvent);
                if (relativeAccess == null) {
                    return null;
                }

                size = relativeAccess.WindowToEventCount;
            }

            return size;
        }

        public ICollection<EventBean> EvaluateGetCollEvents(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<object> EvaluateGetCollScalar(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            return null;
        }
    }
} // end of namespace