///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.view.access
{
    /// <summary>
    /// Provides random-access into window contents by event and index as a combination.
    /// </summary>
    public interface RelativeAccessByEventNIndexGetter
    {
        /// <summary>
        /// Returns the access into window contents given an event.
        /// </summary>
        /// <param name="theEvent">to which the method returns relative access from</param>
        /// <returns>buffer</returns>
        RelativeAccessByEventNIndex GetAccessor(EventBean theEvent);
    }
} // end of namespace