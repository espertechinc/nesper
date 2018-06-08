///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.join.@base;
using com.espertech.esper.events;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.view
{
    public class OutputStrategyUtil
    {
        public static void Output(bool forceUpdate, UniformPair<EventBean[]> result, UpdateDispatchView finalView)
        {
            EventBean[] newEvents = result != null ? result.First : null;
            EventBean[] oldEvents = result != null ? result.Second : null;
            if(newEvents != null || oldEvents != null)
            {
                finalView.NewResult(result);
            }
            else if(forceUpdate)
            {
                finalView.NewResult(result);
            }
        }

        /// <summary>
        /// Indicate statement result.
        /// </summary>
        /// <param name="statementContext">The statement context.</param>
        /// <param name="newOldEvents">result</param>
        public static void IndicateEarlyReturn(StatementContext statementContext, UniformPair<EventBean[]> newOldEvents) {
            if (newOldEvents == null) {
                return;
            }
            if ((statementContext.MetricReportingService != null) &&
                (statementContext.MetricReportingService.StatementOutputHooks != null) &&
                (!statementContext.MetricReportingService.StatementOutputHooks.IsEmpty())) {
                foreach (StatementResultListener listener in statementContext.MetricReportingService.StatementOutputHooks) {
                    listener.Update(newOldEvents.First, newOldEvents.Second, statementContext.StatementName, null, null);
                }
            }
        }
    
        public static IEnumerator<EventBean> GetEnumerator(JoinExecutionStrategy joinExecutionStrategy, ResultSetProcessor resultSetProcessor, Viewable parentView, bool distinct)
        {
            IEnumerator<EventBean> enumerator;
            EventType eventType;
            if (joinExecutionStrategy != null)
            {
                var joinSet = joinExecutionStrategy.StaticJoin();
                enumerator = resultSetProcessor.GetEnumerator(joinSet);
                eventType = resultSetProcessor.ResultEventType;
            }
            else if (resultSetProcessor != null)
        	{
                enumerator = resultSetProcessor.GetEnumerator(parentView);
                eventType = resultSetProcessor.ResultEventType;
        	}
        	else
        	{
        		enumerator = parentView.GetEnumerator();
                eventType = parentView.EventType;
        	}
    
            if (!distinct)
            {
                return enumerator;
            }

            return DistinctEnumeration(enumerator, eventType);
        }

        private static IEnumerator<EventBean> DistinctEnumeration(IEnumerator<EventBean> source, EventType eventType)
        {
            EventBeanReader eventBeanReader = null;
            if (eventType is EventTypeSPI)
            {
                eventBeanReader = ((EventTypeSPI) eventType).Reader;
            }
            if (eventBeanReader == null)
            {
                eventBeanReader = new EventBeanReaderDefaultImpl(eventType);
            }

            if (source != null)
            {
                var events = new LinkedList<EventBean>();
                while(source.MoveNext())
                {
                    events.AddLast(source.Current);
                }

                if (events.Count <= 1)
                {
                    return events.GetEnumerator();
                }

                IList<EventBean> result = EventBeanUtility.GetDistinctByProp(events, eventBeanReader);
                return result.GetEnumerator();
            }

            return EnumerationHelper.Empty<EventBean>();
        }
    }
}
