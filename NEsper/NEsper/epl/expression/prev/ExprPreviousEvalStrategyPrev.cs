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
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view.window;

namespace com.espertech.esper.epl.expression.prev
{
    public class ExprPreviousEvalStrategyPrev : ExprPreviousEvalStrategy
    {
        private readonly int? _constantIndexNumber;
        private readonly ExprEvaluator _evalNode;
        private readonly ExprEvaluator _indexNode;
        private readonly bool _isConstantIndex;
        private readonly bool _isTail;
        private readonly RandomAccessByIndexGetter _randomAccessGetter;
        private readonly RelativeAccessByEventNIndexGetter _relativeAccessGetter;
        private readonly int _streamNumber;

        public ExprPreviousEvalStrategyPrev(
            int streamNumber,
            ExprEvaluator indexNode,
            ExprEvaluator evalNode,
            RandomAccessByIndexGetter randomAccessGetter,
            RelativeAccessByEventNIndexGetter relativeAccessGetter,
            bool constantIndex,
            int? constantIndexNumber,
            bool tail)
        {
            _streamNumber = streamNumber;
            _indexNode = indexNode;
            _evalNode = evalNode;
            _randomAccessGetter = randomAccessGetter;
            _relativeAccessGetter = relativeAccessGetter;
            _isConstantIndex = constantIndex;
            _constantIndexNumber = constantIndexNumber;
            _isTail = tail;
        }

        #region ExprPreviousEvalStrategy Members

        public Object Evaluate(EventBean[] eventsPerStream,
                               ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean substituteEvent = GetSubstitute(eventsPerStream, exprEvaluatorContext);
            if (substituteEvent == null)
            {
                return null;
            }

            // Substitute original event with prior event, evaluate inner expression
            EventBean originalEvent = eventsPerStream[_streamNumber];
            eventsPerStream[_streamNumber] = substituteEvent;
            Object evalResult = _evalNode.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            eventsPerStream[_streamNumber] = originalEvent;

            return evalResult;
        }

        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            return GetSubstitute(eventsPerStream, context);
        }

        public ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            return null;
        }

        public ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            Object result = Evaluate(eventsPerStream, context);
            if (result == null)
            {
                return null;
            }
            return result.AsSingleton();
        }

        #endregion

        private EventBean GetSubstitute(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            // Use constant if supplied
            int? index;
            if (_isConstantIndex)
            {
                index = _constantIndexNumber;
            }
            else
            {
                // evaluate first child, which returns the index
                var indexResult = _indexNode.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                if (indexResult == null)
                {
                    return null;
                }
                index = indexResult.AsInt();
            }

            // access based on index returned
            EventBean substituteEvent;
            if (_randomAccessGetter != null)
            {
                RandomAccessByIndex randomAccess = _randomAccessGetter.Accessor;
                if (!_isTail)
                {
                    substituteEvent = randomAccess.GetNewData(index.Value);
                }
                else
                {
                    substituteEvent = randomAccess.GetNewDataTail(index.Value);
                }
            }
            else
            {
                var evalEvent = eventsPerStream[_streamNumber];
                var relativeAccess = _relativeAccessGetter.GetAccessor(evalEvent);
                if (relativeAccess == null)
                {
                    return null;
                }
                if (!_isTail)
                {
                    substituteEvent = relativeAccess.GetRelativeToEvent(evalEvent, index.Value);
                }
                else
                {
                    substituteEvent = relativeAccess.GetRelativeToEnd(index.Value);
                }
            }
            return substituteEvent;
        }
    }
}