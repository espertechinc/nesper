///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.dataflow.op.logsink
{
    public class LogSinkFactory : DataFlowOperatorFactory
    {
        public EventType[] EventTypes { set; get; }

        public string DataflowName { get; private set; }

        public ExprEvaluator Title { get; set; }

        public ExprEvaluator Layout { get; set; }

        public ExprEvaluator Format { get; set; }

        public ExprEvaluator Log { get; set; }

        public ExprEvaluator Linefeed { get; set; }

        public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
        {
            DataflowName = context.DataFlowName;
        }

        public DataFlowOperator Operator(DataFlowOpInitializeContext context)
        {
            var titleText = DataFlowParameterResolution.ResolveStringOptional("title", Title, context);
            var layoutText = DataFlowParameterResolution.ResolveStringOptional("layout", Layout, context);
            var logFlag = DataFlowParameterResolution.ResolveWithDefault<bool>("log", Log, true, context);
            var linefeedFlag = DataFlowParameterResolution.ResolveWithDefault<bool>("linefeed", Linefeed, true, context);

            ConsoleOpRenderer renderer;
            var formatText = DataFlowParameterResolution.ResolveStringOptional("format", Format, context);
            if (formatText == null) {
                renderer = new ConsoleOpRendererSummary();
            }
            else {
                LogSinkOutputFormat formatEnum = EnumHelper.Parse<LogSinkOutputFormat>(formatText);
                if (formatEnum == LogSinkOutputFormat.summary) {
                    renderer = new ConsoleOpRendererSummary();
                }
                else if (formatEnum == LogSinkOutputFormat.json || formatEnum == LogSinkOutputFormat.xml) {
                    renderer = new ConsoleOpRendererXmlJSon(formatEnum, context.AgentInstanceContext.EPRuntimeRenderEvent);
                }
                else {
                    throw new EPException(
                        "Format '" + formatText + "' is not supported, expecting any of " + CompatExtensions.RenderAny(
                            EnumHelper.GetValues<LogSinkOutputFormat>()));
                }
            }

            return new LogSinkOp(this, context.DataFlowInstanceId, renderer, titleText, layoutText, logFlag, linefeedFlag);
        }
    }
} // end of namespace