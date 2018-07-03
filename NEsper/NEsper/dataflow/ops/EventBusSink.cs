///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.events;

namespace com.espertech.esper.dataflow.ops
{
    [DataFlowOperator]
    public class EventBusSink : DataFlowOpLifecycle
    {
        private EventAdapterService _eventAdapterService;
        private EPRuntimeEventSender _runtimeEventSender;

#pragma warning disable CS0649
        [DataFlowOpParameter] private EPDataFlowEventCollector collector;
#pragma warning restore CS0649

        private EventBusCollector _eventBusCollector;
        private EventBeanAdapterFactory[] _adapterFactories;

        private readonly IThreadLocal<EPDataFlowEventCollectorContext> _collectorDataTL;

        public EventBusSink(IThreadLocalManager threadLocalManager)
        {
            _collectorDataTL = threadLocalManager.Create<EPDataFlowEventCollectorContext>(() => null);
        }

#pragma warning disable RCS1168
        public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context)
#pragma warning restore RCS1168
        {
            if (!context.OutputPorts.IsEmpty())
            {
                throw new ArgumentException("EventBusSink operator does not provide an output stream");
            }

            var eventTypes = new EventType[context.InputPorts.Count];
            for (int i = 0; i < eventTypes.Length; i++)
            {
                eventTypes[i] = context.InputPorts.Get(i).TypeDesc.EventType;
            }
            _runtimeEventSender = context.RuntimeEventSender;
            _eventAdapterService = context.StatementContext.EventAdapterService;

            if (collector != null)
            {
                _eventBusCollector = new EventBusCollectorImpl(
                    _eventAdapterService,
                    _runtimeEventSender);
            }
            else
            {
                _adapterFactories = new EventBeanAdapterFactory[eventTypes.Length];
                for (int i = 0; i < eventTypes.Length; i++)
                {
                    _adapterFactories[i] = context.ServicesContext.EventAdapterService.GetAdapterFactoryForType(eventTypes[i]);
                }
            }
            return null;
        }

        public void OnInput(int port, Object data)
        {
            if (_eventBusCollector != null)
            {
                EPDataFlowEventCollectorContext holder = _collectorDataTL.GetOrCreate();
                if (holder == null)
                {
                    holder = new EPDataFlowEventCollectorContext(_eventBusCollector, data);
                    _collectorDataTL.Value = holder;
                }
                else
                {
                    holder.Event = data;
                }
                collector.Collect(holder);
            }
            else
            {
                if (data is EventBean)
                {
                    _runtimeEventSender.ProcessWrappedEvent((EventBean)data);
                }
                else
                {
                    EventBean theEvent = _adapterFactories[port].MakeAdapter(data);
                    _runtimeEventSender.ProcessWrappedEvent(theEvent);
                }
            }
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
            // no action
        }

#pragma warning disable RCS1168
        public void Close(DataFlowOpCloseContext openContext)
#pragma warning restore RCS1168
        {
            // no action
        }

        private class EventBusCollectorImpl : EventBusCollector
        {
            private readonly EventAdapterService _eventAdapterService;
            private readonly EPRuntimeEventSender _runtimeEventSender;

            public EventBusCollectorImpl(EventAdapterService eventAdapterService, EPRuntimeEventSender runtimeEventSender)
            {
                _eventAdapterService = eventAdapterService;
                _runtimeEventSender = runtimeEventSender;
            }

            public void SendEvent(Object @object)
            {
                EventBean theEvent = _eventAdapterService.AdapterForObject(@object);
                _runtimeEventSender.ProcessWrappedEvent(theEvent);
            }

            public void SendEvent(IDictionary<string, object> map, String eventTypeName)
            {
                EventBean theEvent = _eventAdapterService.AdapterForMap(map, eventTypeName);
                _runtimeEventSender.ProcessWrappedEvent(theEvent);
            }

            public void SendEvent(Object[] objectArray, String eventTypeName)
            {
                EventBean theEvent = _eventAdapterService.AdapterForObjectArray(objectArray, eventTypeName);
                _runtimeEventSender.ProcessWrappedEvent(theEvent);
            }

            public void SendEvent(XmlNode node)
            {
                EventBean theEvent = _eventAdapterService.AdapterForDOM(node);
                _runtimeEventSender.ProcessWrappedEvent(theEvent);
            }

            public void SendEvent(XElement node)
            {
                EventBean theEvent = _eventAdapterService.AdapterForDOM(node);
                _runtimeEventSender.ProcessWrappedEvent(theEvent);
            }
        }
    }
}
