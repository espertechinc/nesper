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

namespace com.espertech.esper.common.@internal.context.aifactory.createcontext
{
    public class StatementAgentInstanceFactoryCreateContextForge : StatementAgentInstanceFactoryForge
    {
        private readonly string contextName;
        private readonly EventType statementEventType;

        public StatementAgentInstanceFactoryCreateContextForge(
            string contextName,
            EventType statementEventType)
        {
            this.contextName = contextName;
            this.statementEventType = statementEventType;
        }

        public CodegenMethod InitializeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols)
        {
            var method = parent.MakeChild(typeof(StatementAgentInstanceFactoryCreateContext), GetType(), classScope);
            method.Block
                .DeclareVarNewInstance<StatementAgentInstanceFactoryCreateContext>("saiff")
                .SetProperty(Ref("saiff"), "ContextName", Constant(contextName))
                .SetProperty(
                    Ref("saiff"),
                    "StatementEventType",
                    EventTypeUtility.ResolveTypeCodegen(statementEventType, symbols.GetAddInitSvc(method)))
                .ExprDotMethod(symbols.GetAddInitSvc(method), "AddReadyCallback", Ref("saiff"))
                .MethodReturn(Ref("saiff"));
            return method;
        }
    }
} // end of namespace