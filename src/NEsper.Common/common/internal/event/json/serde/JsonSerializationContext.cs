///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.Json;

namespace com.espertech.esper.common.@internal.@event.json.serde
{
    /// <summary>
    ///     This object provides context during serialization.
    /// </summary>
    public class JsonSerializationContext : IDisposable
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="writer"></param>
        public JsonSerializationContext(Utf8JsonWriter writer)
        {
            Writer = writer;
        }

        /// <summary>
        ///     The underling json writer for this context.
        /// </summary>
        public Utf8JsonWriter Writer { get; }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or
        ///     resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        ///     Returns a JsonWriter for a specific type.
        /// </summary>
        public IJsonSerializer SerializerFor(Type type)
        {
            throw new NotImplementedException("C31D92BF-D489-4824-B3CE-E5776D017AC2");
        }
    }
} // end of namespace