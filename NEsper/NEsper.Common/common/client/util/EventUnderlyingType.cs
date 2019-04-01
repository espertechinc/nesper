///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.@event.avro;

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    ///     Enumeration of event representation.
    /// </summary>
    public class EventUnderlyingType
    {
        /// <summary>
        ///     Event representation is object-array (Object[]).
        /// </summary>
        public static readonly EventUnderlyingType OBJECTARRAY = new EventUnderlyingType();

        /// <summary>
        ///     Event representation is Map (any java.util.Map interface implementation).
        /// </summary>
        public static readonly EventUnderlyingType MAP = new EventUnderlyingType();

        /// <summary>
        ///     Event representation is Avro (GenericData.Record).
        /// </summary>
        public static readonly EventUnderlyingType AVRO = new EventUnderlyingType();


        private static readonly string OA_TYPE_NAME = typeof(object[]).FullName;
        private static readonly string MAP_TYPE_NAME = typeof(IDictionary<string, object>).FullName;
        private static readonly string AVRO_TYPE_NAME = AvroConstantsNoDep.GENERIC_RECORD_CLASSNAME;

        static EventUnderlyingType()
        {
            OBJECTARRAY.UnderlyingClassName = OA_TYPE_NAME;
            MAP.UnderlyingClassName = MAP_TYPE_NAME;
            AVRO.UnderlyingClassName = AVRO_TYPE_NAME;
        }

        /// <summary>
        ///     Returns the class name of the default underlying type.
        /// </summary>
        /// <returns>default underlying type class name</returns>
        public string UnderlyingClassName { get; private set; }

        /// <summary>
        ///     Returns the default underlying type.
        /// </summary>
        /// <returns>default underlying type</returns>
        public static EventUnderlyingType GetDefault()
        {
            return MAP;
        }
    }
} // end of namespace