///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text.Json;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    /// <summary>
    /// A base class for general-purpose opaque deserialization.  All type specific behavior
    /// should be encapsulated within the deserializer implementation.  What is exposed through
    /// the API is an object deserializer.
    /// </summary>
    public abstract class JsonDeserializerBase
    {
        public JsonDeserializerBase()
        {
        }

        public JsonElement JsonValue { get; set; }

        /// <summary>
        /// Returns the value that was deserialized.
        /// </summary>
        /// <returns></returns>
        public abstract object GetResult();

        /// <summary>
        /// Called to deserialize a JsonElement.
        /// </summary>
        /// <param name="element"></param>
        public abstract object Deserialize(JsonElement element);

#if false
        public string StringValue { get; set; }

        public JsonValueType ValueType { get; set; }

        public object ObjectValue { get; set; }
        
        public abstract JsonDelegateBase StartObject(string name);

        public abstract JsonDelegateBase StartArray(string name);

        public abstract bool EndObjectValue(string name);

        public virtual void EndArrayValue(string name)
        {
        }

        public void EndString(string @string)
        {
            StringValue = @string;
            ValueType = STRING;
        }

        public void EndNumber(string @string)
        {
            StringValue = @string;
            ValueType = NUMBER;
        }

        public void EndNull()
        {
            ValueType = NULL;
            StringValue = null;
            ObjectValue = null;
        }

        public void EndBoolean(bool value)
        {
            ValueType = BOOLEAN;
            if (value) {
                ObjectValue = true;
                StringValue = "true";
            }
            else {
                ObjectValue = false;
                StringValue = "false";
            }
        }
#endif

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="event">event</param>
        /// <param name="name">name</param>
        public void AddGeneralJson(
            IDictionary<string, object> @event,
            string name)
        {
            @event.Put(name, JsonValue.ElementToValue());
        }
    }
} // end of namespace