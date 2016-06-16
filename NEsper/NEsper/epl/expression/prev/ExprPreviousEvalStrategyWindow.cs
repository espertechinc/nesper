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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view.window;

namespace com.espertech.esper.epl.expression.prev
{
    public class ExprPreviousEvalStrategyWindow : ExprPreviousEvalStrategy
    {
        private readonly int _streamNumber;
        private readonly ExprEvaluator _evalNode;
        private readonly Type _componentType;
        private readonly RandomAccessByIndexGetter _randomAccessGetter;
        private readonly RelativeAccessByEventNIndexGetter _relativeAccessGetter;

        public ExprPreviousEvalStrategyWindow(int streamNumber, ExprEvaluator evalNode, Type componentType, RandomAccessByIndexGetter randomAccessGetter, RelativeAccessByEventNIndexGetter relativeAccessGetter)
        {
            _streamNumber = streamNumber;
            _evalNode = evalNode;
            _componentType = componentType;
            _randomAccessGetter = randomAccessGetter;
            _relativeAccessGetter = relativeAccessGetter;
        }

        public Object Evaluate(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            IEnumerator<EventBean> events;
            int size;
            if (_randomAccessGetter != null)
            {
                var randomAccess = _randomAccessGetter.Accessor;
                events = randomAccess.GetWindowEnumerator();
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
                events = relativeAccess.GetWindowToEvent();
            }

            if (size <= 0)
            {
                return null;
            }

            var originalEvent = eventsPerStream[_streamNumber];
            var result = Array.CreateInstance(_componentType, size);

            for (int i = 0; i < size; i++)
            {
                events.MoveNext();
                eventsPerStream[_streamNumber] = events.Current;
                var evalResult = _evalNode.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                result.SetValue(evalResult, i);
            }

            eventsPerStream[_streamNumber] = originalEvent;
            return result;
        }

        public ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            ICollection<EventBean> events;
            if (_randomAccessGetter != null)
            {
                RandomAccessByIndex randomAccess = _randomAccessGetter.Accessor;
                events = randomAccess.WindowCollectionReadOnly;
            }
            else
            {
                EventBean evalEvent = eventsPerStream[_streamNumber];
                RelativeAccessByEventNIndex relativeAccess = _relativeAccessGetter.GetAccessor(evalEvent);
                if (relativeAccess == null)
                {
                    return null;
                } 
                
                events = relativeAccess.GetWindowToEventCollReadOnly();
            }
            return events;
        }

        public ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            IEnumerator<EventBean> events;
            int size;
            if (_randomAccessGetter != null)
            {
                var randomAccess = _randomAccessGetter.Accessor;
                events = randomAccess.GetWindowEnumerator();
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
                events = relativeAccess.GetWindowToEvent();
            }

            if (size <= 0)
            {
                return new object[0];
            }

            var originalEvent = eventsPerStream[_streamNumber];
            var deque = new LinkedList<object>();
            for (int i = 0; i < size; i++)
            {
                events.MoveNext();
                eventsPerStream[_streamNumber] = events.Current;
                var evalResult = _evalNode.Evaluate(new EvaluateParams(eventsPerStream, true, context));
                deque.AddLast(evalResult);
            }
            eventsPerStream[_streamNumber] = originalEvent;
            return deque;
        }

        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            return null;
        }
    }
}
