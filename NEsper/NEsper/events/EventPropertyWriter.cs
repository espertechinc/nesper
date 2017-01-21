///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Writer for a single property value to an event.
    /// </summary>
    public interface EventPropertyWriter
    {
        /// <summary>
        /// Value to write to a property.
        /// </summary>
        /// <param name="value">value to write</param>
        /// <param name="target">property to write to</param>
        void Write(Object value, EventBean target);
    }

    public class ProxyEventPropertyWriter : EventPropertyWriter
    {
        public Action<Object, EventBean> ProcWrite { get; set; }

        public ProxyEventPropertyWriter()
        {
        }

        public ProxyEventPropertyWriter(Action<object, EventBean> writeFunction)
        {
            ProcWrite = writeFunction;
        }

        /// <summary>
        /// Value to write to a property.
        /// </summary>
        /// <param name="value">value to write</param>
        /// <param name="target">property to write to</param>
        public void Write(Object value, EventBean target)
        {
            ProcWrite(value, target);
        }
    }
}
