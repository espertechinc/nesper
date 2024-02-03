///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text.Json;

using com.espertech.esper.common.@internal.@event.json.serde;

namespace com.espertech.esper.common.client.json.util
{
    /// <summary>
    ///     All JSON underlying event objects implement this interface.
    ///     <para>
    ///         In general, byte code does not use the Map methods and instead uses the implementation
    ///         class fields directly.
    ///     </para>
    ///     <para>
    ///         This is a read-only implementation of the IDictionary interface.
    ///     </para>
    ///     <para>
    ///         All predefined properties as well as all dynamic properties become available through the IDictionary interface.
    ///     </para>
    /// </summary>
    public interface JsonEventObject : IDictionary<string, object>
    {
        /// <summary>
        ///     Write JSON to the provided writer and using the provided configuration.
        /// </summary>
        /// <param name="context">the serialization context</param>
        /// <throws>IOException when an IO exception occurs</throws>
        void WriteTo(JsonSerializationContext context);

        /// <summary>
        ///     Returns the JSON string given a writer configuration
        /// </summary>
        /// <param name="jsonWriterOptions">JSON writer options</param>
        /// <returns>JSON</returns>
        string ToString(JsonWriterOptions jsonWriterOptions);
    }
} // end of namespace