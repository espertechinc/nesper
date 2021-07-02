///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.extend.view
{
    public class MyTrendSpotterView : ViewSupport
    {
        private const string PROPERTY_NAME = "trendcount";
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];

        private readonly MyTrendSpotterViewFactory _factory;
        private double? _lastDataPoint;

        // The remove stream must post the same object event reference
        private EventBean _lastInsertStreamEvent;

        private long? _trendcount;

        /// <summary>
        ///     Constructor requires the name of the field to use in the parent view to compute a trend.
        /// </summary>
        /// <param name="factory">is the factory</param>
        /// <param name="agentInstanceContext">contains required view services</param>
        public MyTrendSpotterView(
            MyTrendSpotterViewFactory factory,
            AgentInstanceContext agentInstanceContext)
        {
            this._factory = factory;
            this._agentInstanceContext = agentInstanceContext;
        }

        public override EventType EventType => _factory.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            // The remove stream most post the same exact object references of events that were posted as the insert stream
            EventBean[] removeStreamToPost;
            if (_lastInsertStreamEvent != null) {
                removeStreamToPost = new[] {_lastInsertStreamEvent};
            }
            else {
                removeStreamToPost = new[] {PopulateMap(null)};
            }

            // add data points
            if (newData != null) {
                foreach (var aNewData in newData) {
                    _eventsPerStream[0] = aNewData;
                    var dataPoint = _factory.Parameter.Evaluate(_eventsPerStream, true, null).AsDouble();

                    if (_lastDataPoint == null) {
                        _trendcount = 1L;
                    }
                    else if (_lastDataPoint < dataPoint) {
                        _trendcount++;
                    }
                    else if (_lastDataPoint > dataPoint) {
                        _trendcount = 0L;
                    }

                    _lastDataPoint = dataPoint;
                }
            }

            if (child != null) {
                var newDataPost = PopulateMap(_trendcount);
                _lastInsertStreamEvent = newDataPost;
                child.Update(new[] {newDataPost}, removeStreamToPost);
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            var theEvent = PopulateMap(_trendcount);
            return new SingleEventEnumerator(theEvent);
        }

        private EventBean PopulateMap(long? trendcount)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            result.Put(PROPERTY_NAME, trendcount);
            return _agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(result, _factory.EventType);
        }
    }
} // end of namespace