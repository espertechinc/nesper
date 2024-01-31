///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>
    /// A virtual data window exposes externally-managed data transparently as a named window 
    /// without the need to retain any data in memory. <para/>An instance is associated to each 
    /// named window that is backed by a virtual data window.
    /// </summary>
    public interface VirtualDataWindow : IEnumerable<EventBean>,
        IDisposable
    {
        /// <summary>
        /// Returns the lookup strategy for use by an EPL statement to obtain data.
        /// <para/>
        /// This method is invoked one or more times at the time an EPL statement is created that 
        /// performs a subquery, join, on-action or fire-and-forget query against the virtual data window.
        /// <para/>
        /// The lookup strategy returned is used when the EPL statement for which it was created performs 
        /// a read-operation against the managed data. Multiple lookup strategies for the same EPL statement 
        /// are possible for join statements.
        /// <para/>
        /// The context object passed in is derived from an analysis of the where-clause and lists the 
        /// unique property names of the event type that are index fields, i.e. fields against which the 
        /// lookup occurs.
        /// <para/>
        /// The order of hash and btree properties provided by the context matches the order that lookup 
        /// values are provided to the lookup strategy.
        /// </summary>
        /// <param name="desc">hash and binary tree (sorted access for ranges) index fields</param>
        /// <returns>
        /// lookup strategy, or null to veto the statement
        /// </returns>
        VirtualDataWindowLookup GetLookup(VirtualDataWindowLookupContext desc);

        /// <summary>
        /// Handle a management event.
        /// <para/>
        /// Management events indicate:
        /// <li>Create/Start of an index on a virtual data window.</li>
        /// <li>Stop/Dispose of an index.</li>
        /// <li>Dispose of the virtual data window.</li>
        /// <li>Add/Remove of a consumer to the virtual data window.</li>
        /// </summary>
        /// <param name="theEvent">to handle</param>
        void HandleEvent(VirtualDataWindowEvent theEvent);

        /// <summary>
        /// This method is invoked when events are inserted-into or removed-from the virtual data window. 
        /// <para/>
        /// When a statement uses insert-into to insert events into the virtual data window the newData 
        /// parameter carries the inserted event. 
        /// <para/>
        /// When a statement uses on-delete to delete events from the virtual data window the oldData 
        /// parameter carries the deleted event. 
        /// <para/>
        /// When a statement uses on-merge to merge events with the virtual data window the events passed 
        /// depends on the action: For then-delete the oldData carries the removed event, for then-Update 
        /// the newData carries the after-Update event and the oldData carries the before-Update event, 
        /// for then-insert the newData carries the inserted event. 
        /// <para/>
        /// When a statement uses on-Update to Update events in the virtual data window the newData carries 
        /// the after-Update event and the oldData parameter carries the before-Update event. 
        /// <para/>
        /// Implement as follows to post all inserted or removed events to consuming statements:
        ///  context.OutputStream.Update(newData, oldData); 
        /// <para/>
        /// For data originating from the virtual data window use the SendEvent() method with "insert-into" 
        /// statement to insert events.
        /// </summary>
        /// <param name="newData">the insert stream</param>
        /// <param name="oldData">the remove stream</param>
        void Update(
            EventBean[] newData,
            EventBean[] oldData);
    }
}