///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.property;
using com.espertech.esper.events;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.named
{
    /// <summary>
    /// Represents a consumer of a named window that selects from a named window via a from-clause.
    /// <para/> 
    /// The view simply dispatches directly to child views, and keeps the last new event for iteration.
    /// </summary>
    public class NamedWindowConsumerView
        : ViewSupport
        , StopCallback
    {
        private readonly ExprEvaluator[] _filterList;
        private readonly EventType _eventType;
        private readonly NamedWindowConsumerCallback _consumerCallback;
        private readonly ExprEvaluatorContext _exprEvaluatorContext;
        private readonly PropertyEvaluator _optPropertyEvaluator;
        private readonly FlushedEventBuffer _optPropertyContainedBuffer;
        private readonly bool _audit;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="filterList">is a list of filter expressions</param>
        /// <param name="optPropertyEvaluator">The opt property evaluator.</param>
        /// <param name="eventType">the event type of the named window</param>
        /// <param name="consumerCallback">The consumer callback.</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <param name="audit">if set to <c>true</c> [audit].</param>
        public NamedWindowConsumerView(ExprEvaluator[] filterList,
                                       PropertyEvaluator optPropertyEvaluator,
                                       EventType eventType,
                                       NamedWindowConsumerCallback consumerCallback,
                                       ExprEvaluatorContext exprEvaluatorContext,
                                       bool audit)
        {
            _filterList = filterList;
            _optPropertyEvaluator = optPropertyEvaluator;
            _optPropertyContainedBuffer = optPropertyEvaluator != null ? new FlushedEventBuffer() : null;
            _eventType = eventType;
            _consumerCallback = consumerCallback;
            _exprEvaluatorContext = exprEvaluatorContext;
            _audit = audit;
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (_audit)
            {
                if (AuditPath.IsAuditEnabled)
                {
                    AuditPath.AuditLog(_exprEvaluatorContext.EngineURI, _exprEvaluatorContext.StatementName, AuditEnum.STREAM, _eventType.Name + " insert {" + EventBeanUtility.Summarize(newData) + "} remove {" + EventBeanUtility.Summarize(oldData) + "}");
                }
            }

            // if we have a filter for the named window,
            if (_filterList.Length != 0)
            {
                var eventPerStream = new EventBean[1];
                newData = PassFilter(newData, true, _exprEvaluatorContext, eventPerStream);
                oldData = PassFilter(oldData, false, _exprEvaluatorContext, eventPerStream);
            }

            if (_optPropertyEvaluator != null)
            {
                newData = GetUnpacked(newData);
                oldData = GetUnpacked(oldData);
            }

            if ((newData != null) || (oldData != null))
            {
                UpdateChildren(newData, oldData);
            }
        }

        private EventBean[] GetUnpacked(EventBean[] data)
        {
            if (data == null)
            {
                return null;
            }
            if (data.Length == 0)
            {
                return data;
            }

            for (int i = 0; i < data.Length; i++)
            {
                EventBean[] unpacked = _optPropertyEvaluator.GetProperty(data[i], _exprEvaluatorContext);
                _optPropertyContainedBuffer.Add(unpacked);
            }
            return _optPropertyContainedBuffer.GetAndFlush();
        }

        private EventBean[] PassFilter(EventBean[] eventData, bool isNewData, ExprEvaluatorContext exprEvaluatorContext, EventBean[] eventPerStream)
        {
            if ((eventData == null) || (eventData.Length == 0))
            {
                return null;
            }

            OneEventCollection filtered = null;
            foreach (EventBean theEvent in eventData)
            {
                eventPerStream[0] = theEvent;
                bool pass = true;
                foreach (ExprEvaluator filter in _filterList)
                {
                    var result = (bool?)filter.Evaluate(new EvaluateParams(eventPerStream, isNewData, exprEvaluatorContext));
                    if (result == null || !result.Value)
                    {
                        pass = false;
                        break;
                    }
                }

                if (pass)
                {
                    if (filtered == null)
                    {
                        filtered = new OneEventCollection();
                    }
                    filtered.Add(theEvent);
                }
            }

            if (filtered == null)
            {
                return null;
            }
            return filtered.ToArray();
        }

        public override EventType EventType
        {
            get
            {
                if (_optPropertyEvaluator != null)
                {
                    return _optPropertyEvaluator.FragmentEventType;
                }
                return _eventType;
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return FilteredEventEnumerator.Enumerate(
                _filterList,
                _consumerCallback,
                _exprEvaluatorContext);
        }

        public void Stop()
        {
            _consumerCallback.Stopped(this);
        }
    }
}
