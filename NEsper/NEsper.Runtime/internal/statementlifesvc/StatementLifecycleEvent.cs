///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.statementlifesvc
{
    /// <summary>
    ///     Event indicating statement lifecycle management.
    /// </summary>
    public class StatementLifecycleEvent : EventArgs
    {
        #region LifecycleEventType enum

        /// <summary>Event types. </summary>
        public enum LifecycleEventType
        {
            /// <summary>Statement created. </summary>
            CREATE,

            /// <summary>Statement state change. </summary>
            STATECHANGE,

            /// <summary>listener added </summary>
            LISTENER_ADD,

            /// <summary>Listener removed. </summary>
            LISTENER_REMOVE,

            /// <summary>All listeners removed. </summary>
            LISTENER_REMOVE_ALL,

            /// <summary>Statement destroyed / disposed.</summary>
            DISPOSED
        }

        #endregion LifecycleEventType enum

        /// <summary>Ctor. </summary>
        /// <param name="statement">the statement</param>
        /// <param name="eventType">the type if event</param>
        /// <param name="parameters">event parameters</param>
        public StatementLifecycleEvent(
            EPStatement statement,
            LifecycleEventType eventType,
            params object[] parameters)
        {
            Statement = statement;
            EventType = eventType;
            Parameters = parameters;
        }

        /// <summary>Returns the statement instance for the event. </summary>
        /// <value>statement</value>
        public EPStatement Statement { get; }

        /// <summary>Returns the event type. </summary>
        /// <value>type of event</value>
        public LifecycleEventType EventType { get; }

        /// <summary>Returns event parameters. </summary>
        /// <value>params</value>
        public object[] Parameters { get; }
    }
}