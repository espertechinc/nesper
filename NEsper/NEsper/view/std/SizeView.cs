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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view.stat;

namespace com.espertech.esper.view.std
{
    /// <summary>
    /// This view is a very simple view presenting the number of elements in a stream 
    /// or view. The view computes a single long-typed count of the number of events 
    /// passed through it similar to the base statistics COUNT column.
    /// </summary>
    public class SizeView : ViewSupport, CloneableView
    {
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly EventType _eventType;
        private readonly StatViewAdditionalProps _additionalProps;

        private long _size = 0;
        private EventBean _lastSizeEvent;
        private Object[] _lastValuesEventNew;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="agentInstanceContext">is services</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="additionalProps">The additional props.</param>
        public SizeView(AgentInstanceContext agentInstanceContext, EventType eventType, StatViewAdditionalProps additionalProps)
        {
            _agentInstanceContext = agentInstanceContext;
            _eventType = eventType;
            _additionalProps = additionalProps;
        }

        public View CloneView()
        {
            return new SizeView(_agentInstanceContext, _eventType, _additionalProps);
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, SizeViewFactory.NAME, newData, oldData); }
            var priorSize = _size;

            // If we have child views, keep a reference to the old values, so we can Update them as old data event.
            EventBean oldDataMap = null;
            if (_lastSizeEvent == null)
            {
                if (HasViews)
                {
                    IDictionary<String, Object> postOldData = new Dictionary<String, Object>();
                    postOldData.Put(ViewFieldEnum.SIZE_VIEW__SIZE.GetName(), priorSize);
                    AddProperties(postOldData);
                    oldDataMap = _agentInstanceContext.StatementContext.EventAdapterService.AdapterForTypedMap(postOldData, _eventType);
                }
            }

            // add data points to the window
            if (newData != null)
            {
                _size += newData.Length;

                if ((_additionalProps != null) && (newData.Length != 0))
                {
                    if (_lastValuesEventNew == null)
                    {
                        _lastValuesEventNew = new Object[_additionalProps.AdditionalExpr.Length];
                    }
                    for (var val = 0; val < _additionalProps.AdditionalExpr.Length; val++)
                    {
                        _lastValuesEventNew[val] = _additionalProps.AdditionalExpr[val].Evaluate(
                            new EvaluateParams(new EventBean[] { newData[newData.Length - 1] }, true, _agentInstanceContext));
                    }
                }
            }

            if (oldData != null)
            {
                _size -= oldData.Length;
            }

            // If there are child views, fireStatementStopped Update method
            if ((HasViews) && (priorSize != _size))
            {
                IDictionary<String, Object> postNewData = new Dictionary<String, Object>();
                postNewData.Put(ViewFieldEnum.SIZE_VIEW__SIZE.GetName(), _size);
                AddProperties(postNewData);
                var newEvent = _agentInstanceContext.StatementContext.EventAdapterService.AdapterForTypedMap(postNewData, _eventType);

                EventBean[] oldEvents;
                if (_lastSizeEvent != null)
                {
                    oldEvents = new EventBean[] { _lastSizeEvent };
                }
                else
                {
                    oldEvents = new EventBean[] { oldDataMap };
                }
                var newEvents = new EventBean[] { newEvent };

                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, SizeViewFactory.NAME, newEvents, oldEvents); }
                UpdateChildren(newEvents, oldEvents);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate(); }

                _lastSizeEvent = newEvent;
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream(); }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            var current = new Dictionary<String, Object>();
            current.Put(ViewFieldEnum.SIZE_VIEW__SIZE.GetName(), _size);
            AddProperties(current);
            yield return _agentInstanceContext.StatementContext.EventAdapterService.AdapterForTypedMap(current, _eventType);
        }

        public override String ToString()
        {
            return GetType().FullName;
        }

        /// <summary>
        /// Creates the event type for this view
        /// </summary>
        /// <param name="statementContext">is the event adapter service</param>
        /// <param name="additionalProps">The additional props.</param>
        /// <param name="streamNum">The stream num.</param>
        /// <returns>event type for view</returns>
        public static EventType CreateEventType(StatementContext statementContext, StatViewAdditionalProps additionalProps, int streamNum)
        {
            var schemaMap = new Dictionary<string, object>();
            schemaMap.Put(ViewFieldEnum.SIZE_VIEW__SIZE.GetName(), typeof(long?));
            StatViewAdditionalProps.AddCheckDupProperties(schemaMap, additionalProps, ViewFieldEnum.SIZE_VIEW__SIZE);
            var outputEventTypeName = statementContext.StatementId + "_sizeview_" + streamNum;
            return statementContext.EventAdapterService.CreateAnonymousMapType(outputEventTypeName, schemaMap, false);
        }

        private void AddProperties(IDictionary<String, Object> newDataMap)
        {
            if (_additionalProps == null)
            {
                return;
            }
            _additionalProps.AddProperties(newDataMap, _lastValuesEventNew);
        }
    }
}
