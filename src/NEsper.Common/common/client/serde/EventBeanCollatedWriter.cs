///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.client.serde
{
    /// <summary>
    ///     Collated writer for events, intended for passing along as a parameter and not
    ///     intended to be implemented by an application
    /// </summary>
    public interface EventBeanCollatedWriter
    {
        /// <summary>
        ///     Write event collated.
        /// </summary>
        /// <param name="event">to write</param>
        /// <param name="dataOutput">destination</param>
        /// <param name="pageFullKey">page key</param>
        /// <throws>IOException for io exceptions</throws>
        void WriteCollatedEvent(
            EventBean @event,
            DataOutput dataOutput,
            byte[] pageFullKey);

        /// <summary>
        ///     Write event id collated.
        /// </summary>
        /// <param name="id">to write</param>
        /// <param name="output">destination</param>
        /// <param name="pageFullKey">page key</param>
        /// <throws>IOException for io exceptions</throws>
        void WriteCollatedOID(
            long id,
            DataOutput output,
            byte[] pageFullKey);
    }
} // end of namespace