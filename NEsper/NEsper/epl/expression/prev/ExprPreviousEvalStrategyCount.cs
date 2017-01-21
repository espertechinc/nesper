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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view.window;

namespace com.espertech.esper.epl.expression.prev
{
    public class ExprPreviousEvalStrategyCount : ExprPreviousEvalStrategy
    {
        private readonly RandomAccessByIndexGetter _randomAccessGetter;
        private readonly RelativeAccessByEventNIndexGetter _relativeAccessGetter;
        private readonly int _streamNumber;

        public ExprPreviousEvalStrategyCount(
            int streamNumber,
            RandomAccessByIndexGetter randomAccessGetter,
            RelativeAccessByEventNIndexGetter relativeAccessGetter)
        {
            _streamNumber = streamNumber;
            _randomAccessGetter = randomAccessGetter;
            _relativeAccessGetter = relativeAccessGetter;
        }

        #region ExprPreviousEvalStrategy Members

        public Object Evaluate(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            long size;
            if (_randomAccessGetter != null)
            {
                var randomAccess = _randomAccessGetter.Accessor;
                size = randomAccess.WindowCount;
            }
            else
            {
                var evalEvent = eventsPerStream[_streamNumber];
                var relativeAccess = _relativeAccessGetter.GetAccessor(evalEvent);
                if (relativeAccess == null)
                {
                    return null;
                } 
                size = relativeAccess.GetWindowToEventCount();
            }

            return size;
        }

        public ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            return null;
        }

        #endregion
    }
}