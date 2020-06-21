///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.@event.json.parser.core.JsonValueType;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public abstract class JsonDelegateBase
    {
        public JsonDelegateBase(
            JsonHandlerDelegator baseHandler,
            JsonDelegateBase parent)
        {
            BaseHandler = baseHandler;
            Parent = parent;
        }

        public JsonHandlerDelegator BaseHandler { get; }

        public JsonDelegateBase Parent { get; }

        public string StringValue { get; set; }

        public JsonValueType ValueType { get; set; }

        public object ObjectValue { get; set; }

        public abstract JsonDelegateBase StartObject(string name);

        public abstract JsonDelegateBase StartArray(string name);

        public abstract bool EndObjectValue(string name);

        public virtual void EndArrayValue(string name)
        {
        }

        public abstract object GetResult();

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

        public void SetObjectValue(object @object)
        {
            ObjectValue = @object;
        }

        public JsonDelegateBase GetParent()
        {
            return Parent;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="event">event</param>
        /// <param name="name">name</param>
        public void AddGeneralJson(
            IDictionary<string, object> @event,
            string name)
        {
            @event.Put(name, ValueToObject());
        }

        protected object ValueToObject()
        {
            if (ValueType == STRING) {
                return StringValue;
            }

            if (ValueType == NUMBER) {
                return JsonNumberFromString(StringValue);
            }

            if (ValueType == NULL) {
                return null;
            }

            return ObjectValue;
        }

        public static object JsonNumberFromString(string text)
        {
            if (int.TryParse(text, out var intValue)) {
                return intValue;
            }

            return double.Parse(text);
        }
    }
} // end of namespace