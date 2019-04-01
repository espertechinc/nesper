///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.named;
using com.espertech.esper.view;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// Interface for a processor of base and delta events in a revision event type.
    /// </summary>
    public interface ValueAddEventProcessor
    {
        /// <summary>
        /// Returns the event type that this revision processor generates.
        /// </summary>
        /// <value>
        /// event type
        /// </value>
        EventType ValueAddEventType { get; }

        /// <summary>
        /// For use in checking insert-into statements, validates that the given type is eligible for revision event.
        /// </summary>
        /// <param name="eventType">the type of the event participating in revision event type (or not)</param>
        /// <throws>ExprValidationException if the validation fails</throws>
        void ValidateEventType(EventType eventType);

        /// <summary>
        /// For use in executing an insert-into, wraps the given event applying the revision event type,
        /// but not yet computing a new revision.
        /// </summary>
        /// <param name="theEvent">to wrap</param>
        /// <returns>
        /// revision event bean
        /// </returns>
        EventBean GetValueAddEventBean(EventBean theEvent);

        /// <summary>
        /// Upon new events arriving into a named window (new data), and upon events being deleted via on-delete (old data),
        /// Update child views of the root view and apply to index repository as required (fast deletion).
        /// </summary>
        /// <param name="newData">new events</param>
        /// <param name="oldData">remove stream</param>
        /// <param name="namedWindowRootView">the root view</param>
        /// <param name="indexRepository">delete and select indexes</param>
        void OnUpdate(EventBean[] newData, EventBean[] oldData, NamedWindowRootViewInstance namedWindowRootView, EventTableIndexRepository indexRepository);

        /// <summary>
        /// Handle iteration over revision event contents.
        /// </summary>
        /// <param name="createWindowStmtHandle">statement handle for safe iteration</param>
        /// <param name="parent">the provider of data</param>
        /// <returns>
        /// collection to iterate
        /// </returns>
        ICollection<EventBean> GetSnapshot(EPStatementAgentInstanceHandle createWindowStmtHandle, Viewable parent);

        /// <summary>
        /// Called each time a data window posts a remove stream event, to indicate that a data window remove
        /// an event as it expired according to a specified expiration policy.
        /// </summary>
        /// <param name="oldData">to remove</param>
        /// <param name="indexRepository">the indexes to Update</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        void RemoveOldData(EventBean[] oldData, EventTableIndexRepository indexRepository, AgentInstanceContext agentInstanceContext);
    }
}