///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.@event.json.deserializers.forge;
using com.espertech.esper.common.@internal.@event.json.serializers.forge;

namespace com.espertech.esper.common.@internal.@event.json.forge
{
    public class JsonForgeDesc
    {
        public JsonForgeDesc(
            JsonDeserializerForge deserializerForge,
            JsonSerializerForge serializerForge)
        {
            DeserializerForge = deserializerForge ?? throw new ArgumentException("Deserializer forge must not be null");
            SerializerForge = serializerForge ?? throw new ArgumentException("Serialzier forget must not be null");
        }

        public JsonDeserializerForge DeserializerForge { get; }

        public JsonSerializerForge SerializerForge { get; }
    }
} // end of namespace