///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public class JsonHandlerDelegator
    {
        private JsonDeserializerBase _currentDeserializer;
        private string currentName;

        public JsonDeserializerBase Deserializer {
            get => _currentDeserializer;
            set => _currentDeserializer = value;
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
                _currentDeserializer.ValueType = JsonValueType.OBJECT;
                var next = _currentDeserializer.StartObject(currentName);
                if (next == null) { // assign unknown delegate
                    next = new JsonDeserializerUnknown(_currentDeserializer);
                }

                _currentDeserializer = next;
            }

            return null;
        }

        public object StartArray()
        {
            if (currentName != null) {
                _currentDeserializer.ValueType = JsonValueType.ARRAY;
                var next = _currentDeserializer.StartArray(currentName);
                if (next == null) { // assign unknown delegate
                    next = new JsonDeserializerUnknown(_currentDeserializer);
                }

                _currentDeserializer = next;
            }

            return null;
        }

        public void EndString(string @string)
        {
            _currentDeserializer.EndString(@string);
        }

        public void EndNumber(string @string)
        {
            _currentDeserializer.EndNumber(@string);
        }

        public void EndNull()
        {
            _currentDeserializer.EndNull();
        }

        public void EndBoolean(bool value)
        {
            _currentDeserializer.EndBoolean(value);
        }

        public void EndObjectValue(
            object @object,
            string name)
        {
            _currentDeserializer.EndObjectValue(name);
        }

        public void EndArrayValue(object array)
        {
            _currentDeserializer.EndArrayValue(currentName);
        }

        public void EndArray(object array)
        {
            var result = _currentDeserializer.GetResult();
            _currentDeserializer = _currentDeserializer.Parent;
            if (_currentDeserializer != null) {
                _currentDeserializer.ObjectValue = result;
            }
        }

        public void EndObject(object @object)
        {
            var result = _currentDeserializer.GetResult();
            _currentDeserializer = _currentDeserializer.Parent;
            if (_currentDeserializer != null) {
                _currentDeserializer.ObjectValue = result;
            }
        }
    }
} // end of namespace