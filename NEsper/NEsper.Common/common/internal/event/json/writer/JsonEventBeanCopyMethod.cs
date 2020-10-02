///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text.Json;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.json.serde;

namespace com.espertech.esper.common.@internal.@event.json.writer
{
	/// <summary>
	///     Copy method for Json-underlying events.
	/// </summary>
	public class JsonEventBeanCopyMethod : EventBeanCopyMethod
    {
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly JsonEventType _eventType;

        public JsonEventBeanCopyMethod(
            JsonEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            _eventType = eventType;
            _eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public EventBean CopyUsingSerialization(EventBean theEvent)
        {
            // Serialize
            byte[] streamBytes;
            using (var stream = new MemoryStream()) {
                using (var writer = new Utf8JsonWriter(stream)) {
                    var context = new JsonSerializationContext(writer);
                    _eventType.Serializer.Serialize(context, theEvent.Underlying);
                    writer.Flush();
                }

                stream.Flush();
                streamBytes = stream.ToArray();
            }
            
            // Deserialize
            
            var jsonDocumentOptions = new JsonDocumentOptions();
            var jsonDocument = JsonDocument.Parse(streamBytes, jsonDocumentOptions);
            var underlying = _eventType.Deserializer.Deserialize(jsonDocument.RootElement);
            
            return _eventBeanTypedEventFactory.AdapterForTypedJson(underlying, _eventType);
        }
        
        public EventBean Copy(EventBean theEvent)
        {
            var source = theEvent.Underlying;
            if (source is IJsonComposite sourceComposite) {
                var targetComposite = _eventType.AllocateComposite();
                foreach (var propertyName in sourceComposite.PropertyNames) {
                    var sourceValue = sourceComposite[propertyName];
                    targetComposite[propertyName] = sourceValue;
                }

                return _eventBeanTypedEventFactory.AdapterForTypedJson(targetComposite, _eventType);
            }

            return CopyUsingSerialization(theEvent);
        }
    }
} // end of namespace