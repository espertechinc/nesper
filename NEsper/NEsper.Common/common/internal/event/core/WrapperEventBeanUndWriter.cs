///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Writer for values to a wrapper event.
    /// </summary>
    public class WrapperEventBeanUndWriter : EventBeanWriter
    {
        private readonly EventBeanWriter undWriter;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="undWriter">writer to the underlying object</param>
        public WrapperEventBeanUndWriter(EventBeanWriter undWriter)
        {
            this.undWriter = undWriter;
        }

        public void Write(
            object[] values,
            EventBean theEvent)
        {
            var wrappedEvent = (DecoratingEventBean) theEvent;
            var eventWrapped = wrappedEvent.UnderlyingEvent;
            undWriter.Write(values, eventWrapped);
        }
    }
} // end of namespace