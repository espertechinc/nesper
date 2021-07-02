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
    ///     Writer for a set of wrapper event object values.
    /// </summary>
    public class WrapperEventBeanPropertyWriter : EventBeanWriter
    {
        private readonly EventPropertyWriter[] _writerArr;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="writerArr">writers are writing properties.</param>
        public WrapperEventBeanPropertyWriter(EventPropertyWriter[] writerArr)
        {
            this._writerArr = writerArr;
        }

        public void Write(
            object[] values,
            EventBean theEvent)
        {
            for (var i = 0; i < values.Length; i++) {
                _writerArr[i].Write(values[i], theEvent);
            }
        }
    }
} // end of namespace