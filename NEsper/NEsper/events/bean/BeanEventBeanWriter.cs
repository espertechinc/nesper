///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Writer for a set of event properties to a bean event.
    /// </summary>
    public class BeanEventBeanWriter : EventBeanWriter
    {
        private readonly BeanEventPropertyWriter[] _writers;

        /// <summary>
        /// Writes to use.
        /// </summary>
        /// <param name="writers">writers</param>
        public BeanEventBeanWriter(BeanEventPropertyWriter[] writers)
        {
            _writers = writers;
        }

        public void Write(Object[] values, EventBean theEvent)
        {
            for (int i = 0; i < values.Length; i++)
            {
                _writers[i].Write(values[i], theEvent);
            }
        }
    }
}
