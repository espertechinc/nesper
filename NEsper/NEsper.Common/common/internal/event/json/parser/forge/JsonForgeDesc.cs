///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.@event.json.parser.deserializers.forge;
using com.espertech.esper.common.@internal.@event.json.serializers;

namespace com.espertech.esper.common.@internal.@event.json.parser.forge
{
    public class JsonForgeDesc
    {
        public JsonForgeDesc(
            string fieldName,
            JsonDeserializerForge deserializerForge,
            JsonSerializerForge serializerForge)
        {
            DeserializerForge = deserializerForge;
            SerializerForge = serializerForge;
            if (deserializerForge == null || serializerForge == null) {
                throw new ArgumentException("Unexpected null forge for deserialize or write forge for field '" + fieldName + "'");
            }
        }
        public JsonDeserializerForge DeserializerForge { get; }

        public JsonSerializerForge SerializerForge { get; }
    }
} // end of namespace