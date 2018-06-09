///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.dataflow.ops
{
    [DataFlowOperator]
    public class LogSink : DataFlowOpLifecycle
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

#pragma warning disable CS0649
        [DataFlowOpParameter] private string title;
        [DataFlowOpParameter] private string layout;
        [DataFlowOpParameter] private string format;
        [DataFlowOpParameter] private bool log = true;
        [DataFlowOpParameter] private bool linefeed = true;
#pragma warning restore CS0649

        private readonly object _lock = new object();

        private String _dataflowName;
        private String _dataFlowInstanceId;
        private ConsoleOpRenderer _renderer;

        private EventBeanSPI[] _shellPerStream;

        public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context)
        {
            if (!context.OutputPorts.IsEmpty())
            {
                throw new ArgumentException("LogSink operator does not provide an output stream");
            }

            _dataflowName = context.DataflowName;
            _dataFlowInstanceId = context.DataflowInstanceId;

            _shellPerStream = new EventBeanSPI[context.InputPorts.Count];
            foreach (KeyValuePair<int, DataFlowOpInputPort> entry in context.InputPorts)
            {
                EventType eventType = entry.Value.TypeDesc.EventType;
                if (eventType != null)
                {
                    _shellPerStream[entry.Key] = context.StatementContext.EventAdapterService.GetShellForType(eventType);
                }
            }

            if (format == null)
            {
                _renderer = new ConsoleOpRendererSummary();
            }
            else
            {
                try
                {
                    var formatEnum = EnumHelper.Parse<LogSinkOutputFormat>(format.Trim());
                    if (formatEnum == LogSinkOutputFormat.summary)
                    {
                        _renderer = new ConsoleOpRendererSummary();
                    }
                    else
                    {
                        _renderer = new ConsoleOpRendererXmlJSon(formatEnum, context.Engine.EPRuntime);
                    }
                }
                catch (Exception)
                {
                    throw new ExprValidationException("Format '" + format + "' is not supported, expecting any of " + EnumHelper.GetValues<LogSinkOutputFormat>().Render());
                }
            }

            return null;
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
        }

        public void OnInput(int port, Object theEvent)
        {

            String line;
            if (layout == null)
            {

                var writer = new StringWriter();

                writer.Write("[");
                writer.Write(_dataflowName);
                writer.Write("] ");

                if (title != null)
                {
                    writer.Write("[");
                    writer.Write(title);
                    writer.Write("] ");
                }

                if (_dataFlowInstanceId != null)
                {
                    writer.Write("[");
                    writer.Write(_dataFlowInstanceId);
                    writer.Write("] ");
                }

                writer.Write("[port ");
                writer.Write(Convert.ToString(port));
                writer.Write("] ");

                GetEventOut(port, theEvent, writer);
                line = writer.ToString();
            }
            else
            {
                String result = layout.Replace("%df", _dataflowName).Replace("%p", Convert.ToString(port));
                if (_dataFlowInstanceId != null)
                {
                    result = result.Replace("%i", _dataFlowInstanceId);
                }
                if (title != null)
                {
                    result = result.Replace("%t", title);
                }

                var writer = new StringWriter();
                GetEventOut(port, theEvent, writer);
                result = result.Replace("%e", writer.ToString());

                line = result;
            }

            if (!linefeed)
            {
                line = line.Replace("\n", "").Replace("\r", "");
            }

            // output
            if (log)
            {
                Logger.Info(line);
            }
            else
            {
                Console.Out.WriteLine(line);
            }
        }

        private void GetEventOut(int port, Object theEvent, TextWriter writer)
        {

            if (theEvent is EventBean)
            {
                _renderer.Render((EventBean)theEvent, writer);
                return;
            }

            if (_shellPerStream[port] != null)
            {
                lock (_lock)
                {
                    _shellPerStream[port].Underlying = theEvent;
                    _renderer.Render(_shellPerStream[port], writer);
                }
                return;
            }

            writer.Write("Unrecognized underlying: ");
            writer.Write(theEvent.ToString());
        }

        public enum LogSinkOutputFormat
        {
            json,
            xml,
            summary
        }

        public interface ConsoleOpRenderer
        {
            void Render(EventBean eventBean, TextWriter writer);
        }

        public class ConsoleOpRendererSummary : ConsoleOpRenderer
        {
            public void Render(EventBean theEvent, TextWriter writer)
            {
                EventBeanUtility.Summarize(theEvent, writer);
            }
        }

        public class ConsoleOpRendererXmlJSon : ConsoleOpRenderer
        {
            private readonly LogSinkOutputFormat _format;
            private readonly EPRuntime _runtime;

            private readonly IDictionary<EventType, JSONEventRenderer> _jsonRendererCache = new Dictionary<EventType, JSONEventRenderer>();
            private readonly IDictionary<EventType, XMLEventRenderer> _xmlRendererCache = new Dictionary<EventType, XMLEventRenderer>();

            public ConsoleOpRendererXmlJSon(LogSinkOutputFormat format, EPRuntime runtime)
            {
                _format = format;
                _runtime = runtime;
            }

            public void Render(EventBean theEvent, TextWriter writer)
            {
                String result;
                if (_format == LogSinkOutputFormat.json)
                {
                    JSONEventRenderer renderer = _jsonRendererCache.Get(theEvent.EventType);
                    if (renderer == null)
                    {
                        renderer = GetJsonRenderer(theEvent.EventType);
                        _jsonRendererCache.Put(theEvent.EventType, renderer);
                    }
                    result = renderer.Render(theEvent.EventType.Name, theEvent);
                }
                else
                {
                    XMLEventRenderer renderer = _xmlRendererCache.Get(theEvent.EventType);
                    if (renderer == null)
                    {
                        renderer = GetXmlRenderer(theEvent.EventType);
                        _xmlRendererCache.Put(theEvent.EventType, renderer);
                    }
                    result = renderer.Render(theEvent.EventType.Name, theEvent);
                }
                writer.Write(result);
            }

            protected JSONEventRenderer GetJsonRenderer(EventType eventType)
            {
                return _runtime.EventRenderer.GetJSONRenderer(eventType, RenderingOptions.JsonOptions);
            }

            protected XMLEventRenderer GetXmlRenderer(EventType eventType)
            {
                return _runtime.EventRenderer.GetXMLRenderer(eventType, RenderingOptions.XmlOptions);
            }
        }

        public static class RenderingOptions
        {
            public static XMLRenderingOptions XmlOptions;
            public static JSONRenderingOptions JsonOptions;

            static RenderingOptions()
            {
                XmlOptions = new XMLRenderingOptions();
                XmlOptions.PreventLooping = true;
                XmlOptions.Renderer = ConsoleOpEventPropertyRenderer.INSTANCE;

                JsonOptions = new JSONRenderingOptions();
                JsonOptions.PreventLooping = true;
                JsonOptions.Renderer = ConsoleOpEventPropertyRenderer.INSTANCE;
            }

            public static XMLRenderingOptions GetXmlOptions()
            {
                return XmlOptions;
            }

            public static void SetXmlOptions(XMLRenderingOptions xmlOptions)
            {
                RenderingOptions.XmlOptions = xmlOptions;
            }

            public static JSONRenderingOptions GetJsonOptions()
            {
                return JsonOptions;
            }

            public static void SetJsonOptions(JSONRenderingOptions jsonOptions)
            {
                RenderingOptions.JsonOptions = jsonOptions;
            }
        }

        public class ConsoleOpEventPropertyRenderer : EventPropertyRenderer
        {
            public static ConsoleOpEventPropertyRenderer INSTANCE = new ConsoleOpEventPropertyRenderer();

            public void Render(EventPropertyRendererContext context)
            {
                if (context.PropertyValue is Object[])
                {
                    context.StringBuilder.Append(CompatExtensions.Render(((Object[])context.PropertyValue)));
                }
                else
                {
                    context.DefaultRenderer.Render(context.PropertyValue, context.StringBuilder);
                }
            }
        }
    }
}
