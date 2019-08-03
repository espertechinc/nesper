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
    ///     Listener for event in respect to context management. See @link{<seealso cref="ContextPartitionStateListener" /> for
    ///     partition state.}
    /// </summary>
    public interface ContextStateListener
    {
        /// <summary>
        ///     Invoked when a new context is created.
        /// </summary>
        /// <param name="@event">event</param>
        void OnContextCreated(ContextStateEventContextCreated @event);

        /// <summary>
        ///     Invoked when a context is destroyed.
        /// </summary>
        /// <param name="@event">event</param>
        void OnContextDestroyed(ContextStateEventContextDestroyed @event);
    }
} // end of namespace