///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.variant
{
    /// <summary>
    ///     A variant event is a type that can represent many event types.
    /// </summary>
    public interface VariantEvent
    {
        /// <summary>
        ///     Returns the underlying event.
        /// </summary>
        /// <returns>underlying event</returns>
        EventBean UnderlyingEventBean { get; }
    }
} // end of namespace