///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    /// <summary>
    ///     Writer for a set of event properties to a bean event.
    /// </summary>
    public class BeanEventBeanWriter : EventBeanWriter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly BeanEventPropertyWriter[] _writers;

        /// <summary>
        ///     Writes to use.
        /// </summary>
        /// <param name="writers">writers</param>
        public BeanEventBeanWriter(BeanEventPropertyWriter[] writers)
        {
            _writers = writers;
        }

        public void Write(
            object[] values,
            EventBean theEvent)
        {
            for (var i = 0; i < values.Length; i++) {
                _writers[i].Write(values[i], theEvent);
            }
        }
    }
} // end of namespace