///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public class JsonHandlerDelegator : JsonHandler<object, object>
    {
        private JsonDelegateBase currentDelegate;
        private string currentName;

        public JsonDelegateBase Delegate {
            get => currentDelegate;
            set => currentDelegate = value;
        }

        public void StartObjectValue(
            object @object,
            string name)
        {
            currentName = name;
        }

        public object StartObject()
        {
            if (currentName != null) {
                currentDelegate.ValueType = JsonValueType.OBJECT;
                var next = currentDelegate.StartObject(currentName);
                if (next == null) { // assign unknown delegate
                    next = new JsonDelegateUnknown(this, currentDelegate);
                }

                currentDelegate = next;
            }

            return null;
        }

        public object StartArray()
        {
            if (currentName != null) {
                currentDelegate.ValueType = JsonValueType.ARRAY;
                var next = currentDelegate.StartArray(currentName);
                if (next == null) { // assign unknown delegate
                    next = new JsonDelegateUnknown(this, currentDelegate);
                }

                currentDelegate = next;
            }

            return null;
        }

        public void EndString(string @string)
        {
            currentDelegate.EndString(@string);
        }

        public void EndNumber(string @string)
        {
            currentDelegate.EndNumber(@string);
        }

        public void EndNull()
        {
            currentDelegate.EndNull();
        }

        public void EndBoolean(bool value)
        {
            currentDelegate.EndBoolean(value);
        }

        public void EndObjectValue(
            object @object,
            string name)
        {
            currentDelegate.EndObjectValue(name);
        }

        public void EndArrayValue(object array)
        {
            currentDelegate.EndArrayValue(currentName);
        }

        public void EndArray(object array)
        {
            var result = currentDelegate.GetResult();
            currentDelegate = currentDelegate.Parent;
            if (currentDelegate != null) {
                currentDelegate.ObjectValue = result;
            }
        }

        public void EndObject(object @object)
        {
            var result = currentDelegate.GetResult();
            currentDelegate = currentDelegate.Parent;
            if (currentDelegate != null) {
                currentDelegate.ObjectValue = result;
            }
        }
    }
} // end of namespace