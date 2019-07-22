///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.dataflow.core.EPDataFlowServiceImpl;

namespace com.espertech.esper.common.@internal.epl.dataflow.ops
{
    public class FilterForge : DataFlowOperatorForge
    {
        [DataFlowOpParameter] private ExprNode filter;

        private EventType eventType;
        private bool singleOutputPort;

        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            if (context.InputPorts.Count != 1) {
                throw new ExprValidationException("Filter requires single input port");
            }

            if (filter == null) {
                throw new ExprValidationException(
                    "Required parameter 'filter' providing the filter expression is not provided");
            }

            if (context.OutputPorts.IsEmpty() || context.OutputPorts.Count > 2) {
                throw new ArgumentException(
                    "Filter operator requires one or two output stream(s) but produces " +
                    context.OutputPorts.Count +
                    " streams");
            }

            eventType = context.InputPorts[0].TypeDesc.EventType;
            singleOutputPort = context.OutputPorts.Count == 1;

            filter = DataFlowParameterValidation.Validate("filter", filter, eventType, typeof(bool?), context);

            GraphTypeDesc[] typesPerPort = new GraphTypeDesc[context.OutputPorts.Count];
            for (int i = 0; i < typesPerPort.Length; i++) {
                typesPerPort[i] = new GraphTypeDesc(false, true, eventType);
            }

            return new DataFlowOpForgeInitializeResult(typesPerPort);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return new SAIFFInitializeBuilder(
                    OP_PACKAGE_NAME + ".filter.FilterFactory",
                    this.GetType(),
                    "filter",
                    parent,
                    symbols,
                    classScope)
                .Exprnode("filter", filter)
                .Eventtype("eventType", eventType)
                .Constant("singleOutputPort", singleOutputPort)
                .Build();
        }
    }
} // end of namespace