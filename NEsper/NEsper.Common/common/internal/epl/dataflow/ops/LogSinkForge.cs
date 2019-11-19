///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.dataflow.core.EPDataFlowServiceImpl;

namespace com.espertech.esper.common.@internal.epl.dataflow.ops
{
    public class LogSinkForge : DataFlowOperatorForge
    {
        [DataFlowOpParameter] private ExprNode title;

        [DataFlowOpParameter] private ExprNode layout;

        [DataFlowOpParameter] private ExprNode format;

        [DataFlowOpParameter] private ExprNode log;

        [DataFlowOpParameter] private ExprNode linefeed;

        private EventType[] eventTypes;

        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            if (!context.OutputPorts.IsEmpty()) {
                throw new ArgumentException("LogSink operator does not provide an output stream");
            }

            eventTypes = new EventType[context.InputPorts.Count];
            foreach (KeyValuePair<int, DataFlowOpInputPort> entry in context.InputPorts) {
                eventTypes[entry.Key] = entry.Value.TypeDesc.EventType;
            }

            title = DataFlowParameterValidation.Validate("title", title, typeof(string), context);
            layout = DataFlowParameterValidation.Validate("layout", layout, typeof(string), context);
            format = DataFlowParameterValidation.Validate("format", format, typeof(string), context);
            log = DataFlowParameterValidation.Validate("log", log, typeof(bool), context);
            linefeed = DataFlowParameterValidation.Validate("linefeed", linefeed, typeof(bool), context);
            return null;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return new SAIFFInitializeBuilder(
                    OP_PACKAGE_NAME + ".logsink.LogSinkFactory",
                    this.GetType(),
                    "log",
                    parent,
                    symbols,
                    classScope)
                .Exprnode("title", title)
                .Exprnode("layout", layout)
                .Exprnode("format", format)
                .Exprnode("log", log)
                .Exprnode("linefeed", linefeed)
                .EventtypesMayNull("eventTypes", eventTypes)
                .Build();
        }
    }
} // end of namespace