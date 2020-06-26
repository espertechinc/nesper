///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.view.groupwin
{
    /// <summary>
    ///     This view simply adds a property to the events posted to it. This is useful for the group-merge views.
    /// </summary>
    public class AddPropertyValueOptionalView : ViewSupport,
        AgentInstanceMgmtCallback
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AddPropertyValueOptionalView));

        private readonly AgentInstanceViewFactoryChainContext agentInstanceContext;
        private readonly GroupByViewFactory groupByViewFactory;
        private readonly object propertyValues;

        // Keep a history of posted old events to avoid reconstructing the event
        // and adhere to the contract of posting the same reference to child views.
        // Only for must-add-property.
        private readonly IDictionary<EventBean, EventBean> newToOldEventMap;

        public AddPropertyValueOptionalView(
            GroupByViewFactory groupByViewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            object mergeValues)
        {
            this.groupByViewFactory = groupByViewFactory;
            propertyValues = mergeValues;
            this.agentInstanceContext = agentInstanceContext;
            newToOldEventMap = new Dictionary<EventBean, EventBean>();
        }

        public override EventType EventType => groupByViewFactory.EventType;

        public void Stop(AgentInstanceStopServices services)
        {
            if (!newToOldEventMap.IsEmpty()) {
                var oldEvents = new OneEventCollection();
                foreach (var oldEvent in newToOldEventMap) {
                    oldEvents.Add(oldEvent.Value);
                }

                if (!oldEvents.IsEmpty()) {
                    Child.Update(null, oldEvents.ToArray());
                }

                newToOldEventMap.Clear();
            }
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            EventBean[] newEvents = null;
            EventBean[] oldEvents = null;

            if (newData != null) {
                newEvents = new EventBean[newData.Length];

                var index = 0;
                foreach (var newEvent in newData) {
                    var theEvent = AddProperty(
                        newEvent,
                        groupByViewFactory.PropertyNames,
                        propertyValues,
                        groupByViewFactory.EventType,
                        agentInstanceContext.EventBeanTypedEventFactory);
                    newEvents[index++] = theEvent;

                    newToOldEventMap.Put(newEvent, theEvent);
                }
            }

            if (oldData != null) {
                oldEvents = new EventBean[oldData.Length];

                var index = 0;
                foreach (var oldEvent in oldData) {
                    var outgoing = newToOldEventMap.Delete(oldEvent);
                    if (outgoing != null) {
                        oldEvents[index++] = outgoing;
                    }
                    else {
                        var theEvent = AddProperty(
                            oldEvent,
                            groupByViewFactory.PropertyNames,
                            propertyValues,
                            groupByViewFactory.EventType,
                            agentInstanceContext.EventBeanTypedEventFactory);
                        oldEvents[index++] = theEvent;
                    }
                }
            }

            agentInstanceContext.InstrumentationProvider.QViewIndicate(groupByViewFactory, newEvents, oldEvents);
            Child.Update(newEvents, oldEvents);
            agentInstanceContext.InstrumentationProvider.AViewIndicate();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            foreach (var nextEvent in Parent) {
                yield return AddProperty(
                    nextEvent,
                    groupByViewFactory.PropertyNames,
                    propertyValues,
                    groupByViewFactory.EventType,
                    agentInstanceContext.EventBeanTypedEventFactory);
            }
        }

        /// <summary>
        ///     Add a property to the event passed in.
        /// </summary>
        /// <param name="originalEvent">event to add property to</param>
        /// <param name="propertyNames">names of properties to add</param>
        /// <param name="propertyValues">value of properties to add</param>
        /// <param name="targetEventType">new event type</param>
        /// <param name="eventAdapterService">service for generating events and handling event types</param>
        /// <returns>event with added property</returns>
        internal static EventBean AddProperty(
            EventBean originalEvent,
            string[] propertyNames,
            object propertyValues,
            EventType targetEventType,
            EventBeanTypedEventFactory eventAdapterService)
        {
            var values = new Dictionary<string, object>();
            if (propertyValues is MultiKey props) {
                for (int i = 0; i < propertyNames.Length; i++) {
                    values.Put(propertyNames[i], props.GetKey(i));
                }
            } else {
                if (propertyValues is MultiKeyArrayWrap multiKeyArrayWrap) {
                    propertyValues = multiKeyArrayWrap.Array;
                }
                values.Put(propertyNames[0], propertyValues);
            }

            return eventAdapterService.AdapterForTypedWrapper(originalEvent, values, targetEventType);
        }

        public override string ToString()
        {
            return GetType().Name + " propertyValue=" + propertyValues;
        }
        
        public void Transfer(AgentInstanceTransferServices services)
        {
        }
    }
} // end of namespace