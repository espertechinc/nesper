///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly EventBean[] eventsPerStream = new EventBean[1];

        private readonly MyTrendSpotterViewFactory factory;
        private double? lastDataPoint;

        // The remove stream must post the same object event reference
        private EventBean lastInsertStreamEvent;

        private long? trendcount;

        /// <summary>
        ///     Constructor requires the name of the field to use in the parent view to compute a trend.
        /// </summary>
        /// <param name="factory">is the factory</param>
        /// <param name="agentInstanceContext">contains required view services</param>
        public MyTrendSpotterView(
            MyTrendSpotterViewFactory factory,
            AgentInstanceContext agentInstanceContext)
        {
            this.factory = factory;
            this.agentInstanceContext = agentInstanceContext;
        }

        public override EventType EventType => factory.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            // The remove stream most post the same exact object references of events that were posted as the insert stream
            EventBean[] removeStreamToPost;
            if (lastInsertStreamEvent != null) {
                removeStreamToPost = new[] {lastInsertStreamEvent};
            }
            else {
                removeStreamToPost = new[] {PopulateMap(null)};
            }

            // add data points
            if (newData != null) {
                foreach (var aNewData in newData) {
                    eventsPerStream[0] = aNewData;
                    var dataPoint = factory.Parameter.Evaluate(eventsPerStream, true, null).AsDouble();

                    if (lastDataPoint == null) {
                        trendcount = 1L;
                    }
                    else if (lastDataPoint < dataPoint) {
                        trendcount++;
                    }
                    else if (lastDataPoint > dataPoint) {
                        trendcount = 0L;
                    }

                    lastDataPoint = dataPoint;
                }
            }

            if (Child != null) {
                var newDataPost = PopulateMap(trendcount);
                lastInsertStreamEvent = newDataPost;
                Child.Update(new[] {newDataPost}, removeStreamToPost);
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            var theEvent = PopulateMap(trendcount);
            return new SingleEventEnumerator(theEvent);
        }

        private EventBean PopulateMap(long? trendcount)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            result.Put(PROPERTY_NAME, trendcount);
            return agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(result, factory.EventType);
        }
    }
} // end of namespace