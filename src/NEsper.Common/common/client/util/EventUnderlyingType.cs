///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.@event.avro;

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    ///     Enumeration of event representation.
    /// </summary>
    public enum EventUnderlyingType
    {
        /// <summary>
        ///     Event representation is object-array (Object[]).
        /// </summary>
        OBJECTARRAY,

        /// <summary>
        ///     Event representation is Map (any IDictionary interface implementation).
        /// </summary>
        MAP,

        /// <summary>
        ///     Event representation is Avro (GenericRecord).
        /// </summary>
        AVRO,

        /// <summary>
        /// Event representation is JSON with underlying generation.
        /// </summary>
        JSON
    }

    public static class EventUnderlyingTypeExtensions
    {
        /// <summary>
        ///     Returns the class name of the default underlying type.
        /// </summary>
        /// <returns>default underlying type class name</returns>
        public static string GetUnderlyingClassName(this EventUnderlyingType underlyingType)
        {
            return underlyingType switch {
                EventUnderlyingType.OBJECTARRAY => typeof(object[]).FullName,
                EventUnderlyingType.MAP => typeof(IDictionary<string, object>).FullName,
                EventUnderlyingType.AVRO => AvroConstantsNoDep.GENERIC_RECORD_CLASSNAME,
                EventUnderlyingType.JSON => typeof(object).FullName,
                _ => throw new ArgumentException("invalid value", nameof(underlyingType))
            };
        }

        /// <summary>
        ///     Returns the class name of the default underlying type.
        /// </summary>
        /// <returns>default underlying type</returns>
        public static Type GetUnderlyingClass(this EventUnderlyingType underlyingType)
        {
            switch (underlyingType) {
                case EventUnderlyingType.OBJECTARRAY:
                    return typeof(object[]);

                case EventUnderlyingType.MAP:
                    return typeof(IDictionary<string, object>);

                case EventUnderlyingType.AVRO:
                    return null;

                case EventUnderlyingType.JSON:
                    return typeof(object);
            }

            throw new ArgumentException("invalid value", nameof(underlyingType));
        }

        /// <summary>
        ///     Returns the default underlying type.
        /// </summary>
        /// <returns>default underlying type</returns>
        public static EventUnderlyingType GetDefault()
        {
            return EventUnderlyingType.MAP;
        }
    }
} // end of namespace