///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

    public interface PropertyListBuilder
    {
        /// <summary>
        /// Introspect the clazz and deterime exposed event properties.
        /// </summary>
        /// <param name="clazz">The clazz to introspect</param>
        /// <returns></returns>
        IList<InternalEventPropDescriptor> AssessProperties(Type clazz);
    }

    public class ProxyPropertyListBuilder : PropertyListBuilder
    {
        public Func<Type, IList<InternalEventPropDescriptor>> ProcAssessProperties;

        public ProxyPropertyListBuilder()
        {
        }

        public ProxyPropertyListBuilder(Func<Type, IList<InternalEventPropDescriptor>> procAssessProperties)
        {
            ProcAssessProperties = procAssessProperties;
        }

        public IList<InternalEventPropDescriptor> AssessProperties(Type clazz)
        {
            return ProcAssessProperties.Invoke(clazz);
        }
    }
}
