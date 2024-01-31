///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.dataflow.core.EPDataFlowServiceImpl;

namespace com.espertech.esper.common.@internal.epl.dataflow.ops
{
    public class EventBusSinkForge : DataFlowOperatorForge
    {
#pragma warning disable 649
        [DataFlowOpParameter] private readonly IDictionary<string, object> collector;
#pragma warning restore 649

        private EventType[] eventTypes;

        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            if (!context.OutputPorts.IsEmpty()) {
                throw new ArgumentException("EventBusSink operator does not provide an output stream");
            }

            eventTypes = new EventType[context.InputPorts.Count];
            for (var i = 0; i < eventTypes.Length; i++) {
                eventTypes[i] = context.InputPorts[i].TypeDesc.EventType;
            }

            return null;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return new SAIFFInitializeBuilder(
                    OP_PACKAGE_NAME + ".eventbussink.EventBusSinkFactory",
                    GetType(),
                    "sink",
                    parent,
                    symbols,
                    classScope)
                .EventtypesMayNull("eventTypes", eventTypes)
                .Map("collector", collector)
                .Build();
        }
    }
} // end of namespace