///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events.avro;

namespace com.espertech.esper.client.util
{
    /// <summary>Enumeration of event representation.</summary>
    public enum EventUnderlyingType
    {
        /// <summary>Event representation is object-array (Object[]).</summary>
        OBJECTARRAY,

        /// <summary>
        /// Event representation is Map (any IDictionary interface implementation).
        /// </summary>
        MAP,

        /// <summary>Event representation is Avro (GenericRecord).</summary>
        AVRO
    }

    public static class EventUnderlyingTypeExtensions
    {
        private static readonly string OA_TYPE_NAME = typeof(Object[]).FullName;
        private static readonly string MAP_TYPE_NAME = typeof(IDictionary<string, object>).FullName;
        private static readonly string AVRO_TYPE_NAME = AvroConstantsNoDep.GENERIC_RECORD_CLASSNAME;
    
        /// <summary>
        /// Returns the default underlying type.
        /// </summary>
        /// <returns>default underlying type</returns>
        public static EventUnderlyingType GetDefault()
        {
            return EventUnderlyingType.MAP;
        }
    
        /// <summary>
        /// Returns the class name of the default underlying type.
        /// </summary>
        /// <returns>default underlying type class name</returns>
        public static string GetUnderlyingClassName(this EventUnderlyingType enumValue)
        {
            switch (enumValue)
            {
                case EventUnderlyingType.OBJECTARRAY:
                    return OA_TYPE_NAME;
                case EventUnderlyingType.MAP:
                    return MAP_TYPE_NAME;
                case EventUnderlyingType.AVRO:
                    return AVRO_TYPE_NAME;
            }

            throw new ArgumentException("illegal enum value");
        }
    }
} // end of namespace
