///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.dataflow.core.EPDataFlowServiceImpl;

namespace com.espertech.esper.common.@internal.epl.dataflow.ops
{
    public class EventBusSourceForge : DataFlowOperatorForge
    {
        [DataFlowOpParameter] protected EPDataFlowEventBeanCollector collector;

        [DataFlowOpParameter] protected ExprNode filter;

        private bool submitEventBean;

        public FilterSpecCompiled FilterSpecCompiled { get; private set; }

        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            if (context.OutputPorts.Count != 1) {
                throw new ArgumentException(
                    "EventBusSource operator requires one output stream but produces " + context.OutputPorts.Count + " streams");
            }

            var portZero = context.OutputPorts[0];
            if (portZero.OptionalDeclaredType == null || portZero.OptionalDeclaredType.EventType == null) {
                throw new ArgumentException("EventBusSource operator requires an event type declated for the output stream");
            }

            var eventType = portZero.OptionalDeclaredType.EventType;
            if (!portZero.OptionalDeclaredType.IsUnderlying) {
                submitEventBean = true;
            }

            DataFlowParameterValidation.Validate("filter", filter, eventType, typeof(bool), context);

            try {
                IList<ExprNode> filters = new EmptyList<ExprNode>();
                if (filter != null) {
                    filters = Collections.SingletonList(filter);
                }

                var streamTypeService = new StreamTypeServiceImpl(eventType, eventType.Name, true);
                FilterSpecCompiled = FilterSpecCompiler.MakeFilterSpec(
                    eventType, eventType.Name, filters, null,
                    null, null, streamTypeService, null, context.StatementRawInfo, context.Services);
            }
            catch (ExprValidationException ex) {
                throw new ExprValidationException("Failed to obtain filter parameters: " + ex.Message, ex);
            }

            return null;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var builder = new SAIFFInitializeBuilder(
                OP_PACKAGE_NAME + ".eventbussource.EventBusSourceFactory", GetType(), "eventbussource", parent, symbols, classScope);
            builder.Expression("filterSpecActivatable", 
                    LocalMethod(
                        FilterSpecCompiled.MakeCodegen(
                            builder.Method(), symbols, classScope)))
                .Constant("submitEventBean", submitEventBean);
            return builder.Build();
        }
    }
} // end of namespace