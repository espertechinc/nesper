///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.createexpression
{
    public class StatementAgentInstanceFactoryCreateExpressionForge
    {
        private readonly EventType statementEventType;
        private readonly string expressionName;

        public StatementAgentInstanceFactoryCreateExpressionForge(
            EventType statementEventType,
            string expressionName)
        {
            this.statementEventType = statementEventType;
            this.expressionName = expressionName;
        }

        public CodegenMethod InitializeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(StatementAgentInstanceFactoryCreateExpression), this.GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(StatementAgentInstanceFactoryCreateExpression), "saiff",
                    NewInstance(typeof(StatementAgentInstanceFactoryCreateExpression)))
                .SetProperty(Ref("saiff"), "StatementEventType", EventTypeUtility.ResolveTypeCodegen(statementEventType, symbols.GetAddInitSvc(method)))
                .SetProperty(Ref("saiff"), "ExpressionName", Constant(expressionName))
                .MethodReturn(@Ref("saiff"));
            return method;
        }
    }
} // end of namespace