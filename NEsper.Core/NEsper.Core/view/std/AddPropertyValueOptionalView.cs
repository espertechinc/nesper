///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.events;
using com.espertech.esper.view;

namespace com.espertech.esper.view.std
{
    /// <summary>
    /// This view simply adds a property to the events posted to it. This is useful for the group-merge views.
    /// </summary>
    public sealed class AddPropertyValueOptionalView
        : ViewSupport
        , CloneableView
        , StoppableView
    {
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly string[] _propertyNames;
        private readonly object _propertyValues;
        private readonly EventType _eventType;
        private bool _mustAddProperty;

        // Keep a history of posted old events to avoid reconstructing the event
        // and adhere to the contract of posting the same reference to child views.
        // Only for must-add-property.
        private IDictionary<EventBean, EventBean> _newToOldEventMap;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="propertyNames">is the name of the field that is added to any events received by this view.</param>
        /// <param name="mergeValues">is the values of the field that is added to any events received by this view.</param>
        /// <param name="mergedResultEventType">is the event type that the merge view reports to it's child views</param>
        /// <param name="agentInstanceContext">contains required view services</param>
        public AddPropertyValueOptionalView(AgentInstanceViewFactoryChainContext agentInstanceContext, string[] propertyNames, object mergeValues, EventType mergedResultEventType)
        {
            _propertyNames = propertyNames;
            _propertyValues = mergeValues;
            _eventType = mergedResultEventType;
            _agentInstanceContext = agentInstanceContext;
            _newToOldEventMap = new Dictionary<EventBean, EventBean>();
        }

        public View CloneView()
        {
            return new AddPropertyValueOptionalView(_agentInstanceContext, _propertyNames, _propertyValues, _eventType);
        }

        public override Viewable Parent
        {
            set
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".setParent parent=" + value);
                }
                base.Parent = value;

                if (!Equals(value.EventType, _eventType))
                {
                    _mustAddProperty = true;
                    _newToOldEventMap = new Dictionary<EventBean, EventBean>();
                }
                else
                {
                    _mustAddProperty = false;
                }
            }
        }

        /// <summary>
        /// Returns field name for which to set the merge value for.
        /// </summary>
        /// <value>field name to use to set value</value>
        public string[] PropertyNames
        {
            get { return _propertyNames; }
        }

        /// <summary>
        /// Returns the value to set for the field.
        /// </summary>
        /// <value>value to set</value>
        public object PropertyValues
        {
            get { return _propertyValues; }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (!_mustAddProperty)
            {
                UpdateChildren(newData, oldData);
                return;
            }

            EventBean[] newEvents = null;
            EventBean[] oldEvents = null;

            if (newData != null)
            {
                newEvents = new EventBean[newData.Length];

                int index = 0;
                foreach (EventBean newEvent in newData)
                {
                    EventBean theEvent = AddProperty(newEvent, _propertyNames, _propertyValues, _eventType, _agentInstanceContext.StatementContext.EventAdapterService);
                    newEvents[index++] = theEvent;

                    _newToOldEventMap.Put(newEvent, theEvent);
                }
            }

            if (oldData != null)
            {
                oldEvents = new EventBean[oldData.Length];

                int index = 0;
                foreach (EventBean oldEvent in oldData)
                {
                    var outgoing = _newToOldEventMap.Delete(oldEvent);
                    if (outgoing != null)
                    {
                        oldEvents[index++] = outgoing;
                    }
                    else
                    {
                        EventBean theEvent = AddProperty(oldEvent, _propertyNames, _propertyValues, _eventType, _agentInstanceContext.StatementContext.EventAdapterService);
                        oldEvents[index++] = theEvent;
                    }
                }
            }

            UpdateChildren(newEvents, oldEvents);
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            foreach (var nextEvent in Parent)
            {
                if (_mustAddProperty)
                {
                    yield return AddProperty(
                        nextEvent, _propertyNames, _propertyValues, _eventType, _agentInstanceContext.StatementContext.EventAdapterService);
                }
                else
                {
                    yield return nextEvent;
                }
            }
        }

        public void Stop()
        {
            if (!_newToOldEventMap.IsEmpty())
            {
                var oldEvents = new OneEventCollection();
                foreach (var oldEvent in _newToOldEventMap)
                {
                    oldEvents.Add(oldEvent.Value);
                }
                if (!oldEvents.IsEmpty())
                {
                    UpdateChildren(null, oldEvents.ToArray());
                }
                _newToOldEventMap.Clear();
            }
        }

        /// <summary>
        /// Add a property to the event passed in.
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
            EventAdapterService eventAdapterService)
        {
            IDictionary<string, object> values = new Dictionary<string, object>();
            if (propertyValues is MultiKeyUntyped)
            {
                var props = (MultiKeyUntyped)propertyValues;
                var propertyValuesArr = props.Keys;

                for (int i = 0; i < propertyNames.Length; i++)
                {
                    values.Put(propertyNames[i], propertyValuesArr[i]);
                }
            }
            else
            {
                values.Put(propertyNames[0], propertyValues);
            }

            return eventAdapterService.AdapterForTypedWrapper(originalEvent, values, targetEventType);
        }

        public override string ToString()
        {
            return string.Format("{0} propertyNames={1} propertyValue={2}", GetType().Name, _propertyNames.Render(), _propertyValues);
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
