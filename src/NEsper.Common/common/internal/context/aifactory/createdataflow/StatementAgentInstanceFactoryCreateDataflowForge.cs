///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.createdataflow
{
    public class StatementAgentInstanceFactoryCreateDataflowForge
    {
        private readonly DataflowDescForge dataflowForge;

        private readonly EventType eventType;

        public StatementAgentInstanceFactoryCreateDataflowForge(
            EventType eventType,
            DataflowDescForge dataflowForge)
        {
            this.eventType = eventType;
            this.dataflowForge = dataflowForge;
        }

        public CodegenMethod InitializeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(StatementAgentInstanceFactoryCreateDataflow), GetType(), classScope);
            method.Block
                .DeclareVarNewInstance<StatementAgentInstanceFactoryCreateDataflow>("saiff")
                .SetProperty(
                    Ref("saiff"),
                    "EventType",
                    EventTypeUtility.ResolveTypeCodegen(eventType, symbols.GetAddInitSvc(method)))
                .SetProperty(Ref("saiff"), "Dataflow", dataflowForge.Make(method, symbols, classScope))
                .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add("AddReadyCallback", Ref("saiff")))
                .MethodReturn(Ref("saiff"));
            return method;
        }
    }
} // end of namespace