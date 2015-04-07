///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events
{
    /// <summary>Writer for values to a wrapper event. </summary>
    public class WrapperEventBeanUndWriter : EventBeanWriter
    {
        private readonly EventBeanWriter _undWriter;
    
        /// <summary>Ctor. </summary>
        /// <param name="undWriter">writer to the underlying object</param>
        public WrapperEventBeanUndWriter(EventBeanWriter undWriter)
        {
           _undWriter = undWriter;
        }
    
        public void Write(Object[] values, EventBean theEvent)
        {
            DecoratingEventBean wrappedEvent = (DecoratingEventBean) theEvent;
            EventBean eventWrapped = wrappedEvent.UnderlyingEvent;
            _undWriter.Write(values, eventWrapped);
        }
    }
}
