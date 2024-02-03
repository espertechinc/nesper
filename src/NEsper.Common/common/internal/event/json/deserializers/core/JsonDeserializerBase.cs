///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.Json;

namespace com.espertech.esper.common.@internal.@event.json.deserializers.core
{
    /// <summary>
    /// A base class for general-purpose opaque deserialization.  All type specific behavior
    /// should be encapsulated within the deserializer implementation.  What is exposed through
    /// the API is an object deserializer.
    /// </summary>
    public abstract class JsonDeserializerBase : IJsonDeserializer
    {
        public JsonDeserializerBase()
        {
        }

        /// <summary>
        /// Allocates a vanilla json composite object.
        /// </summary>
        public Func<object> Allocator => throw new NotImplementedException();

        public JsonElement JsonValue { get; set; }

        /// <summary>
        /// Called to deserialize a JsonElement.
        /// </summary>
        /// <param name="element"></param>
        public abstract object Deserialize(JsonElement element);
    }
} // end of namespace