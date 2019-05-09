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
    /// <summary>Implementations copy the event object for controlled modification (shallow copy). </summary>
    public interface EventBeanCopyMethod
    {
        /// <summary>Copy the event bean returning a shallow copy. </summary>
        /// <param name="theEvent">to copy</param>
        /// <returns>shallow copy</returns>
        EventBean Copy(EventBean theEvent);
    }
}