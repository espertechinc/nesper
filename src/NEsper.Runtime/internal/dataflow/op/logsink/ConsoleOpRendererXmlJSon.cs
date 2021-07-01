///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.render;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.dataflow.op.logsink
{
    public class ConsoleOpRendererXmlJSon : ConsoleOpRenderer
    {
        private readonly LogSinkOutputFormat format;
        private readonly EPRenderEventService runtimeRenderEvent;

        private readonly IDictionary<EventType, JSONEventRenderer> jsonRendererCache = new Dictionary<EventType, JSONEventRenderer>();
        private readonly IDictionary<EventType, XMLEventRenderer> xmlRendererCache = new Dictionary<EventType, XMLEventRenderer>();

        public ConsoleOpRendererXmlJSon(LogSinkOutputFormat format, EPRenderEventService runtimeRenderEvent)
        {
            this.format = format;
            this.runtimeRenderEvent = runtimeRenderEvent;
        }

        public void Render(EventBean theEvent, StringWriter writer)
        {
            string result;
            if (format == LogSinkOutputFormat.json)
            {
                JSONEventRenderer renderer = jsonRendererCache.Get(theEvent.EventType);
                if (renderer == null)
                {
                    renderer = GetJsonRenderer(theEvent.EventType);
                    jsonRendererCache.Put(theEvent.EventType, renderer);
                }
                result = renderer.Render(theEvent.EventType.Name, theEvent);
            }
            else
            {
                XMLEventRenderer renderer = xmlRendererCache.Get(theEvent.EventType);
                if (renderer == null)
                {
                    renderer = GetXmlRenderer(theEvent.EventType);
                    xmlRendererCache.Put(theEvent.EventType, renderer);
                }
                result = renderer.Render(theEvent.EventType.Name, theEvent);
            }
            writer.Write(result);
        }

        protected JSONEventRenderer GetJsonRenderer(EventType eventType)
        {
            return runtimeRenderEvent.GetJSONRenderer(eventType, RenderingOptions.JsonOptions);
        }

        protected XMLEventRenderer GetXmlRenderer(EventType eventType)
        {
            return runtimeRenderEvent.GetXMLRenderer(eventType, RenderingOptions.XmlOptions);
        }
    }
} // end of namespace