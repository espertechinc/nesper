///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;

namespace com.espertech.esper.common.@internal.@event.variant
{
    /// <summary>
    ///     Specification for a variant event stream.
    /// </summary>
    public class VariantSpec
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventTypes">types of events for variant stream, or empty list</param>
        /// <param name="typeVariance">enum specifying type variance</param>
        public VariantSpec(
            EventType[] eventTypes,
            TypeVariance typeVariance)
        {
            EventTypes = eventTypes;
            TypeVariance = typeVariance;
        }

        /// <summary>
        ///     Returns types allowed for variant streams.
        /// </summary>
        /// <returns>types</returns>
        public EventType[] EventTypes { get; }

        /// <summary>
        ///     Returns the type variance enum.
        /// </summary>
        /// <value>type variance</value>
        public TypeVariance TypeVariance { get; }
    }
} // end of namespace