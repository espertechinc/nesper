///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Interface for event types that provide decorating event properties as a name-value map.
    /// </summary>
    public interface DecoratingEventBean
    {
        /// <summary>Returns decorating properties.</summary>
        /// <returns>property name and values</returns>
        IDictionary<string, object> DecoratingProperties { get; }

        /// <summary>
        ///     Returns the underlying event to the decorated event.
        /// </summary>
        EventBean UnderlyingEvent { get; }
    }
} // End of namespace