///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
    public class JsonApplicationClassSerializationDesc
    {
        public JsonApplicationClassSerializationDesc(
            string serializerClassName,
            string deserializerClassName,
            IList<FieldInfo> fields)
        {
            SerializerClassName = serializerClassName;
            DeserializerClassName = deserializerClassName;
            Fields = fields;
        }

        public string SerializerClassName { get; }

        public string DeserializerClassName { get; }

        public IList<FieldInfo> Fields { get; }
    }
} // end of namespace