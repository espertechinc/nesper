///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.prev
{
    public class ExprPreviousEvalStrategyWindow : ExprPreviousEvalStrategy
    {
        private readonly int _streamNumber;
        private readonly ExprEvaluator _evalNode;
        private readonly Type _componentType;
        private readonly RandomAccessByIndexGetter _randomAccessGetter;
        private readonly RelativeAccessByEventNIndexGetter _relativeAccessGetter;

        public ExprPreviousEvalStrategyWindow(
            int streamNumber,
            ExprEvaluator evalNode,
            Type componentType,
            RandomAccessByIndexGetter randomAccessGetter,
            RelativeAccessByEventNIndexGetter relativeAccessGetter)
        {
            this._streamNumber = streamNumber;
            this._evalNode = evalNode;
            this._componentType = componentType;
            this._randomAccessGetter = randomAccessGetter;
            this._relativeAccessGetter = relativeAccessGetter;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            IEnumerator<EventBean> events;
            int size;
            if (_randomAccessGetter != null) {
                RandomAccessByIndex randomAccess = _randomAccessGetter.Accessor;
                events = randomAccess.GetWindowEnumerator();
                size = (int) randomAccess.WindowCount;
            }
            else {
                EventBean evalEvent = eventsPerStream[_streamNumber];
                RelativeAccessByEventNIndex relativeAccess = _relativeAccessGetter.GetAccessor(evalEvent);
                if (relativeAccess == null) {
                    return null;
                }

                size = relativeAccess.WindowToEventCount;
                events = relativeAccess.WindowToEvent;
            }

            if (size <= 0) {
                return null;
            }

            EventBean originalEvent = eventsPerStream[_streamNumber];
            Array result = Array.CreateInstance(_componentType, size);

            for (int i = 0; i < size; i++) {
                events.MoveNext();
                eventsPerStream[_streamNumber] = events.Current;
                result.SetValue(_evalNode.Evaluate(eventsPerStream, true, exprEvaluatorContext), i);
            }

            eventsPerStream[_streamNumber] = originalEvent;
            return result;
        }

        public ICollection<EventBean> EvaluateGetCollEvents(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            ICollection<EventBean> events;
            if (_randomAccessGetter != null) {
                RandomAccessByIndex randomAccess = _randomAccessGetter.Accessor;
                events = randomAccess.WindowCollectionReadOnly;
            }
            else {
                EventBean evalEvent = eventsPerStream[_streamNumber];
                RelativeAccessByEventNIndex relativeAccess = _relativeAccessGetter.GetAccessor(evalEvent);
                if (relativeAccess == null) {
                    return null;
                }

                events = relativeAccess.WindowToEventCollReadOnly;
            }

            return events;
        }

        public ICollection<object> EvaluateGetCollScalar(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            IEnumerator<EventBean> events;
            int size;
            if (_randomAccessGetter != null) {
                RandomAccessByIndex randomAccess = _randomAccessGetter.Accessor;
                events = randomAccess.GetWindowEnumerator();
                size = (int) randomAccess.WindowCount;
            }
            else {
                EventBean evalEvent = eventsPerStream[_streamNumber];
                RelativeAccessByEventNIndex relativeAccess = _relativeAccessGetter.GetAccessor(evalEvent);
                if (relativeAccess == null) {
                    return null;
                }

                size = relativeAccess.WindowToEventCount;
                events = relativeAccess.WindowToEvent;
            }

            if (size <= 0) {
                return Collections.GetEmptyList<object>();
            }

            EventBean originalEvent = eventsPerStream[_streamNumber];
            Deque<object> deque = new ArrayDeque<object>(size);
            for (int i = 0; i < size; i++) {
                events.MoveNext();
                eventsPerStream[_streamNumber] = events.Current;
                object evalResult = _evalNode.Evaluate(eventsPerStream, true, context);
                deque.Add(evalResult);
            }

            eventsPerStream[_streamNumber] = originalEvent;
            return deque;
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            return null;
        }
    }
} // end of namespace