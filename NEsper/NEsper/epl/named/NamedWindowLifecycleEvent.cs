///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.named
{
    /// <summary>Event indicating named window lifecycle management. </summary>
    public class NamedWindowLifecycleEvent
    {
        private readonly String _name;
        private readonly NamedWindowProcessor _processor;
        private readonly LifecycleEventType _eventType;
        private readonly Object[] _paramList;
    
        /// <summary>Event types. </summary>
        public enum LifecycleEventType {
            /// <summary>Named window created. </summary>
            CREATE,
    
            /// <summary>Named window removed. </summary>
            DESTROY
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">is the name of the named window</param>
        /// <param name="processor">instance for processing the named window contents</param>
        /// <param name="eventType">the type of event</param>
        /// <param name="paramList">event parameters</param>
        protected internal NamedWindowLifecycleEvent(String name, NamedWindowProcessor processor, LifecycleEventType eventType, params Object[] paramList)
        {
            _name = name;
            _processor = processor;
            _eventType = eventType;
            _paramList = paramList;
        }

        /// <summary>Returns the named window name. </summary>
        /// <value>name</value>
        public string Name => _name;

        /// <summary>Return the processor originating the event. </summary>
        /// <value>processor</value>
        public NamedWindowProcessor Processor => _processor;

        /// <summary>Returns the event type. </summary>
        /// <value>type of event</value>
        public LifecycleEventType EventType => _eventType;

        /// <summary>Returns event parameters. </summary>
        /// <value>paramList</value>
        public object[] Params => _paramList;
    }
}
