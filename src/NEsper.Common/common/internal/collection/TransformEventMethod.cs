///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.collection
{
    public interface TransformEventMethod
    {
        /// <summary>
        /// Transform event returning the transformed event.
        /// </summary>
        /// <param name="theEvent">event to transform</param>
        /// <returns>transformed event</returns>
        EventBean Transform(EventBean theEvent);
    }
} // End of namespace