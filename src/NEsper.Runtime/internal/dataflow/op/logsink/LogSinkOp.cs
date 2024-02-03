///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.runtime.@internal.dataflow.op.logsink
{
    public class LogSinkOp : DataFlowOperator
    {
        private static readonly ILog LOGME = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string dataFlowInstanceId;

        private readonly LogSinkFactory factory;
        private readonly string layout;
        private readonly bool linefeed;
        private readonly bool log;
        private readonly ConsoleOpRenderer renderer;

        private readonly EventBeanSPI[] shellPerStream;
        private readonly string title;

        public LogSinkOp(
            LogSinkFactory factory,
            string dataFlowInstanceId,
            ConsoleOpRenderer renderer,
            string title,
            string layout,
            bool log,
            bool linefeed)
        {
            this.factory = factory;
            this.dataFlowInstanceId = dataFlowInstanceId;
            this.renderer = renderer;
            this.title = title;
            this.layout = layout;
            this.log = log;
            this.linefeed = linefeed;

            shellPerStream = new EventBeanSPI[factory.EventTypes.Length];
            for (var i = 0; i < factory.EventTypes.Length; i++) {
                EventType eventType = factory.EventTypes[i];
                if (eventType != null) {
                    shellPerStream[i] = EventTypeUtility.GetShellForType(eventType);
                }
            }
        }

        public void OnInput(
            int port,
            object theEvent)
        {
            string line;
            if (layout == null) {
                var writer = new StringWriter();

                writer.Write("[");
                writer.Write(factory.DataflowName);
                writer.Write("] ");

                if (title != null) {
                    writer.Write("[");
                    writer.Write(title);
                    writer.Write("] ");
                }

                if (dataFlowInstanceId != null) {
                    writer.Write("[");
                    writer.Write(dataFlowInstanceId);
                    writer.Write("] ");
                }

                writer.Write("[port ");
                writer.Write(Convert.ToString(port));
                writer.Write("] ");

                GetEventOut(port, theEvent, writer);
                line = writer.ToString();
            }
            else {
                var result = layout.Replace("%df", factory.DataflowName).Replace("%p", Convert.ToString(port));
                if (dataFlowInstanceId != null) {
                    result = result.Replace("%i", dataFlowInstanceId);
                }

                if (title != null) {
                    result = result.Replace("%t", title);
                }

                var writer = new StringWriter();
                GetEventOut(port, theEvent, writer);
                result = result.Replace("%e", writer.ToString());

                line = result;
            }

            if (!linefeed) {
                line = line.Replace("\n", "").Replace("\r", "");
            }

            // output
            if (log) {
                LOGME.Info(line);
            }
            else {
                Console.Out.WriteLine(line);
            }
        }

        private void GetEventOut(
            int port,
            object theEvent,
            TextWriter writer)
        {
            if (theEvent is EventBean) {
                renderer.Render((EventBean) theEvent, writer);
                return;
            }

            if (shellPerStream[port] != null) {
                lock (this) {
                    shellPerStream[port].Underlying = theEvent;
                    renderer.Render(shellPerStream[port], writer);
                }

                return;
            }

            writer.Write("Unrecognized underlying: ");
            writer.Write(theEvent.ToString());
        }
    }
} // end of namespace