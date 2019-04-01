///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.core.context.mgr
{
    /// <summary>
    /// For testing, only used within SPIs; Replaced by applicable EsperHA bindings.
    /// </summary>
    public class ContextStateCacheNoSave : ContextStateCache
    {
        public static ContextStatePathValueBinding DEFAULT_SPI_TEST_BINDING =
            new MyContextStatePathValueBindingSerializable();

        public ContextStatePathValueBinding GetBinding(Object bindingInfo)
        {
            if (bindingInfo is ContextDetailInitiatedTerminated)
            {
                return new ContextStateCacheNoSaveInitTermBinding();
            }
            return DEFAULT_SPI_TEST_BINDING;
        }

        public void UpdateContextPath(String contextName, ContextStatePathKey key, ContextStatePathValue value)
        {
            // no action required
        }

        public void AddContextPath(
            String contextName,
            int level,
            int parentPath,
            int subPath,
            int? optionalContextPartitionId,
            Object additionalInfo,
            ContextStatePathValueBinding binding)
        {
            // no action required
        }

        public void RemoveContextParentPath(String contextName, int level, int parentPath)
        {
            // no action required
        }

        public void RemoveContextPath(String contextName, int level, int parentPath, int subPath)
        {
            // no action required
        }

        public OrderedDictionary<ContextStatePathKey, ContextStatePathValue> GetContextPaths(String contextName)
        {
            return null; // no state required
        }

        public void RemoveContext(String contextName)
        {
            // no action required
        }

        /// <summary>For testing, only used within SPIs; Replaced by applicable EsperHA bindings. </summary>
        public class MyContextStatePathValueBindingSerializable : ContextStatePathValueBinding
        {
            public Object ByteArrayToObject(byte[] bytes, EventAdapterService eventAdapterService)
            {
                return SerializerUtil.ByteArrToObject(bytes);
            }

            public byte[] ToByteArray(Object contextInfo)
            {
                return SerializerUtil.ObjectToByteArr(contextInfo);
            }
        }

        /// <summary>
        /// For testing, only used within SPIs; Replaced by applicable EsperHA bindings. 
        /// Simple binding where any events get changed to type name and byte array.
        /// </summary>
        public class ContextStateCacheNoSaveInitTermBinding : ContextStatePathValueBinding
        {
            public Object ByteArrayToObject(byte[] bytes, EventAdapterService eventAdapterService)
            {
                var state = (ContextControllerInitTermState) SerializerUtil.ByteArrToObject(bytes);
                foreach (var entry in state.PatternData.ToArray())
                {
                    if (entry.Value is EventBeanNameValuePair)
                    {
                        var @event = (EventBeanNameValuePair) entry.Value;
                        var type = eventAdapterService.GetEventTypeByName(@event.EventTypeName);
                        var underlying = SerializerUtil.ByteArrToObject(@event.Bytes);
                        state.PatternData.Put(entry.Key, eventAdapterService.AdapterForType(underlying, type));
                    }
                }
                return state;
            }

            public byte[] ToByteArray(Object contextInfo)
            {
                var state = (ContextControllerInitTermState) contextInfo;
                var serializableProps = new Dictionary<String, Object>();
                if (state.PatternData != null)
                {
                    serializableProps.PutAll(state.PatternData);
                    foreach (var entry in state.PatternData)
                    {
                        if (entry.Value is EventBean)
                        {
                            var @event = (EventBean) entry.Value;
                            serializableProps.Put(
                                entry.Key,
                                new EventBeanNameValuePair(
                                    @event.EventType.Name, SerializerUtil.ObjectToByteArr(@event.Underlying)));
                        }
                    }
                }
                var serialized = new ContextControllerInitTermState(state.StartTime, serializableProps);
                return SerializerUtil.ObjectToByteArr(serialized);
            }
        }

        [Serializable]
        private class EventBeanNameValuePair
        {
            internal EventBeanNameValuePair(String eventTypeName, byte[] bytes)
            {
                EventTypeName = eventTypeName;
                Bytes = bytes;
            }

            public string EventTypeName { get; private set; }

            public byte[] Bytes { get; private set; }
        }
    }
}
