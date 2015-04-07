///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Interface for an introspector that generates a list of event property
    /// descriptors given a clazz.
    /// <para/>
    /// Introspect the clazz and deterime exposed event properties.
    /// </summary>
    /// <param name="clazz">to introspect</param>
    /// <returns>list of event property descriptors</returns>
    public delegate IList<InternalEventPropDescriptor> PropertyListBuilder(Type clazz);
}
