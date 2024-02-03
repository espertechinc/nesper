///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.deploymentlifesvc
{
	/// <summary>
	///     Event indicating statement lifecycle management.
	/// </summary>
	public class StatementListenerEvent
    {
	    /// <summary>
	    ///     Event types.
	    /// </summary>
	    public enum ListenerEventType
        {
	        /// <summary>
	        ///     listener added
	        /// </summary>
	        LISTENER_ADD,

	        /// <summary>
	        ///     Listener removed.
	        /// </summary>
	        LISTENER_REMOVE,

	        /// <summary>
	        ///     All listeners removed.
	        /// </summary>
	        LISTENER_REMOVE_ALL
        }

	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    /// <param name="statement">the statement</param>
	    /// <param name="eventType">the type of event</param>
	    /// <param name="listener">the listener</param>
	    public StatementListenerEvent(
            EPStatement statement,
            ListenerEventType eventType,
            UpdateListener listener)
        {
            Statement = statement;
            EventType = eventType;
            Listener = listener;
        }

        public StatementListenerEvent(
            EPStatement statement,
            ListenerEventType eventType) : this(statement, eventType, null)
        {
        }

        /// <summary>
        ///     Returns the statement instance for the event.
        /// </summary>
        /// <value>statement</value>
        public EPStatement Statement { get; }

        /// <summary>
        ///     Returns the event type.
        /// </summary>
        /// <value>type of event</value>
        public ListenerEventType EventType { get; }

        /// <summary>
        ///     Returns the listener
        /// </summary>
        /// <value>listener</value>
        public UpdateListener Listener { get; }
    }
} // end of namespace