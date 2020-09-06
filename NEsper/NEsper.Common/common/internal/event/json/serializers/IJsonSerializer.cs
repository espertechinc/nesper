///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.@event.json.serde;

namespace com.espertech.esper.common.@internal.@event.json.serializers
{
    public interface IJsonSerializer
    {
        /// <summary>
        /// Serialize the provided value.  The serialization context is provided.
        /// </summary>
        /// <param name="context">serialization context</param>
        /// <param name="value">value to be serialized</param>
        public void Serialize(
            JsonSerializationContext context,
            object value);
    }

    public class ProxyJsonSerializer : IJsonSerializer
    {
        public Action<JsonSerializationContext, object> ProcSerialize { get; set; }

        public ProxyJsonSerializer()
        {
        }

        public ProxyJsonSerializer(Action<JsonSerializationContext, object> procSerialize)
        {
            ProcSerialize = procSerialize;
        }

        public void Serialize(
            JsonSerializationContext context,
            object value)
        {
            ProcSerialize.Invoke(context, value);
        }
    }
}