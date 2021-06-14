///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.@event.bean.core;

namespace com.espertech.esper.common.@internal.@event.bean.introspect
{
    /// <summary>
    ///     Interface for an introspector that generates a list of event property descriptors
    ///     given a clazz. The clazz could be a class or any other legacy type.
    /// </summary>
    public interface PropertyListBuilder
    {
        /// <summary>
        ///     Introspect the clazz and deterime exposed event properties.
        /// </summary>
        /// <param name="clazz">to introspect</param>
        /// <returns>list of event property descriptors</returns>
        IList<PropertyStem> AssessProperties(Type clazz);
    }
} // end of namespace