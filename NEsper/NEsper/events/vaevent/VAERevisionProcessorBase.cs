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
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.named;
using com.espertech.esper.view;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// Base revision processor.
    /// </summary>
    public abstract class VAERevisionProcessorBase : ValueAddEventProcessor
    {
        /// <summary>Revision type specification. </summary>
        protected readonly RevisionSpec RevisionSpec;

        /// <summary>Name of type. </summary>
        protected readonly String RevisionEventTypeName;

        /// <summary>Revision event type. </summary>
        protected RevisionEventType RevisionEventType;

        /// <summary>For interogating nested properties. </summary>
        protected EventAdapterService EventAdapterService;

        /// <summary>Map of participating type to descriptor. </summary>
        protected IDictionary<EventType, RevisionTypeDesc> TypeDescriptors;

        /// <summary>Ctor. </summary>
        /// <param name="revisionSpec">specification</param>
        /// <param name="revisionEventTypeName">name of event type</param>
        /// <param name="eventAdapterService">for nested property handling</param>
        protected VAERevisionProcessorBase(
            RevisionSpec revisionSpec, 
            String revisionEventTypeName, 
            EventAdapterService eventAdapterService)
        {
            RevisionSpec = revisionSpec;
            RevisionEventTypeName = revisionEventTypeName;
            EventAdapterService = eventAdapterService;
            TypeDescriptors = new Dictionary<EventType, RevisionTypeDesc>();
        }

        public virtual EventType ValueAddEventType
        {
            get { return RevisionEventType; }
        }

        public virtual void ValidateEventType(EventType eventType)
        {
            if (eventType == RevisionSpec.BaseEventType)
            {
                return;
            }
            if (TypeDescriptors.ContainsKey(eventType))
            {
                return;
            }

            if (eventType == null)
            {
                throw new ExprValidationException(GetMessage());
            }

            // Check all the supertypes to see if one of the matches the full or delta types
            IEnumerable<EventType> deepSupers = eventType.DeepSuperTypes;
            if (deepSupers != null) {
                foreach (EventType type in deepSupers) {
                    if (type == RevisionSpec.BaseEventType) {
                        return;
                    }
                    if (TypeDescriptors.ContainsKey(type)) {
                        return;
                    }
                }
            }

            throw new ExprValidationException(GetMessage());
        }

        private String GetMessage()
        {
            return "Selected event type is not a valid base or delta event type of revision event type '"
                    + RevisionEventTypeName + "'";
        }

        /// <summary>For use in executing an insert-into, wraps the given event applying the revision event type, but not yet computing a new revision. </summary>
        /// <param name="theEvent">to wrap</param>
        /// <returns>revision event bean</returns>
        public abstract EventBean GetValueAddEventBean(
            EventBean theEvent);

        /// <summary>Upon new events arriving into a named window (new data), and upon events being deleted via on-delete (old data), Update child views of the root view and apply to index repository as required (fast deletion). </summary>
        /// <param name="newData">new events</param>
        /// <param name="oldData">remove stream</param>
        /// <param name="namedWindowRootView">the root view</param>
        /// <param name="indexRepository">delete and select indexes</param>
        public abstract void OnUpdate(
            EventBean[] newData, 
            EventBean[] oldData, 
            NamedWindowRootViewInstance namedWindowRootView,
            EventTableIndexRepository indexRepository);

        /// <summary>Handle iteration over revision event contents. </summary>
        /// <param name="createWindowStmtHandle">statement handle for safe iteration</param>
        /// <param name="parent">the provider of data</param>
        /// <returns>collection to iterate</returns>
        public abstract ICollection<EventBean> GetSnapshot(
            EPStatementAgentInstanceHandle createWindowStmtHandle,
            Viewable parent);

        /// <summary>
        /// Called each time a data window posts a remove stream event, to indicate that a data window remove
        /// an event as it expired according to a specified expiration policy.
        /// </summary>
        /// <param name="oldData">to remove</param>
        /// <param name="indexRepository">the indexes to Update</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        public abstract void RemoveOldData(
            EventBean[] oldData, 
            EventTableIndexRepository indexRepository,
            AgentInstanceContext agentInstanceContext);
    }
}
