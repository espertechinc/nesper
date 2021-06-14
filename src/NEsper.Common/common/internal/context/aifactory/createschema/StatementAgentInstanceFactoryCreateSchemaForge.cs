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

namespace com.espertech.esper.common.@internal.context.aifactory.createschema
{
    public class StatementAgentInstanceFactoryCreateSchemaForge
    {
        private readonly EventType eventType;

        public StatementAgentInstanceFactoryCreateSchemaForge(EventType eventType)
        {
            this.eventType = eventType;
        }

        public CodegenMethod InitializeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(
                typeof(StatementAgentInstanceFactoryCreateSchema),
                this.GetType(),
                classScope);
            method.Block
                .DeclareVar<StatementAgentInstanceFactoryCreateSchema>(
                    "saiff",
                    NewInstance(typeof(StatementAgentInstanceFactoryCreateSchema)))
                .SetProperty(
                    Ref("saiff"),
                    "EventType",
                    EventTypeUtility.ResolveTypeCodegen(eventType, symbols.GetAddInitSvc(method)))
                .MethodReturn(Ref("saiff"));
            return method;
        }
    }
} // end of namespace