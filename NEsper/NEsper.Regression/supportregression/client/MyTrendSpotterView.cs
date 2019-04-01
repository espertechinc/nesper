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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view;

namespace com.espertech.esper.supportregression.client
{
    public class MyTrendSpotterView : ViewSupport
    {
        private const String PROPERTY_NAME = "trendcount";
    
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly EventType _eventType;
        private readonly ExprNode _expression;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];

        private long? _trendcount;
        private double? _lastDataPoint;
    
        // The remove stream must post the same object event reference
        private EventBean _lastInsertStreamEvent;
    
        /// <summary>Constructor requires the name of the field to use in the parent view to Compute a trend. </summary>
        /// <param name="expression">is the name of the field within the parent view to use to get numeric data points for this view</param>
        /// <param name="agentInstanceContext">contains required view services</param>
        public MyTrendSpotterView(AgentInstanceViewFactoryChainContext agentInstanceContext, ExprNode expression)
        {
            _agentInstanceContext = agentInstanceContext;
            _expression = expression;
            _eventType = CreateEventType(agentInstanceContext.StatementContext);
        }
    
        public View CloneView(AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            return new MyTrendSpotterView(agentInstanceContext, _expression);
        }
    
        /// <summary>Returns expression to report statistics on. </summary>
        /// <returns>expression providing values</returns>
        public ExprNode GetExpression()
        {
            return _expression;
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            // The remove stream most post the same exact object references of events that were posted as the insert stream
            EventBean[] removeStreamToPost;
            if (_lastInsertStreamEvent != null)
            {
                removeStreamToPost = new[] {_lastInsertStreamEvent};
            }
            else
            {
                removeStreamToPost = new[] {PopulateMap(null)};
            }
    
            // add data points
            if (newData != null)
            {
                foreach (EventBean aNewData in newData)
                {
                    _eventsPerStream[0] = aNewData;
                    double dataPoint = _expression.ExprEvaluator.Evaluate(new EvaluateParams(_eventsPerStream, true, null)).AsDouble();
    
                    if (_lastDataPoint == null)
                    {
                        _trendcount = 1L;
                    }
                    else if (_lastDataPoint < dataPoint)
                    {
                        _trendcount++;
                    }
                    else if (_lastDataPoint > dataPoint)
                    {
                        _trendcount = 0L;
                    }
                    _lastDataPoint = dataPoint;
                }
            }
    
            if (HasViews)
            {
                EventBean newDataPost = PopulateMap(_trendcount);
                _lastInsertStreamEvent = newDataPost;
                UpdateChildren(new[] {newDataPost}, removeStreamToPost);
            }
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            EventBean theEvent = PopulateMap(_trendcount);
            yield return theEvent;
        }
    
        public override String ToString()
        {
            return GetType().FullName + " expression=" + _expression;
        }
    
        private EventBean PopulateMap(long? trendcount)
        {
            IDictionary<String, Object> result = new Dictionary<String, Object>();
            result.Put(PROPERTY_NAME, trendcount);
            return _agentInstanceContext.StatementContext.EventAdapterService.AdapterForTypedMap(result, _eventType);
        }
    
        /// <summary>Creates the event type for this view. </summary>
        /// <returns>event type of view</returns>
        public static EventType CreateEventType(StatementContext statementContext)
        {
            IDictionary<String, Object> eventTypeMap = new Dictionary<String, Object>();
            eventTypeMap.Put(PROPERTY_NAME, typeof(long));
            return statementContext.EventAdapterService.CreateAnonymousMapType("test", eventTypeMap, true);
        }
    }
}
