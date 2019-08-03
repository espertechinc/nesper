///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.context
{
    /// <summary>
    ///     Listener for context-specific partition-related events. See @link{<seealso cref="ContextStateListener" /> for
    ///     context-creation and state.}
    /// </summary>
    public interface ContextPartitionStateListener
    {
        /// <summary>
        ///     Invoked when a context is activated.
        /// </summary>
        /// <param name="@event">event</param>
        void OnContextActivated(ContextStateEventContextActivated @event);

        /// <summary>
        ///     Invoked when a context is de-activated.
        /// </summary>
        /// <param name="@event">event</param>
        void OnContextDeactivated(ContextStateEventContextDeactivated @event);

        /// <summary>
        ///     Invoked when a statement is added to a context.
        /// </summary>
        /// <param name="@event">event</param>
        void OnContextStatementAdded(ContextStateEventContextStatementAdded @event);

        /// <summary>
        ///     Invoked when a statement is removed from a context.
        /// </summary>
        /// <param name="@event">event</param>
        void OnContextStatementRemoved(ContextStateEventContextStatementRemoved @event);

        /// <summary>
        ///     Invoked when a context partition is allocated, provided once per context
        ///     and per partition independent of the number of statements.
        /// </summary>
        /// <param name="@event">event</param>
        void OnContextPartitionAllocated(ContextStateEventContextPartitionAllocated @event);

        /// <summary>
        ///     Invoked when a context partition is destroyed, provided once per context
        ///     and per partition independent of the number of statements.
        /// </summary>
        /// <param name="@event">event</param>
        void OnContextPartitionDeallocated(ContextStateEventContextPartitionDeallocated @event);
    }
} // end of namespace