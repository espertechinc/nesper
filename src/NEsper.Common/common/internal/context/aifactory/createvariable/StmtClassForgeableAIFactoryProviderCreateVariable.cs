///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.context.aifactory.core.SAIFFInitializeSymbol;

namespace com.espertech.esper.common.@internal.context.aifactory.createvariable
{
    public class StmtClassForgeableAIFactoryProviderCreateVariable : StmtClassForgeableAIFactoryProviderBase
    {
        private readonly StatementAgentInstanceFactoryCreateVariableForge _forge;
        private readonly string _variableName;
        private readonly DataInputOutputSerdeForge _serde;

        public StmtClassForgeableAIFactoryProviderCreateVariable(
            string className,
            CodegenNamespaceScope namespaceScope,
            StatementAgentInstanceFactoryCreateVariableForge forge,
            string variableName,
            DataInputOutputSerdeForge serde)
            : base(className, namespaceScope)
        {
            _forge = forge;
            _variableName = variableName;
            _serde = serde;
        }

        protected override Type TypeOfFactory()
        {
            return typeof(StatementAgentInstanceFactoryCreateVariable);
        }

        protected override CodegenMethod CodegenConstructorInit(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var saiffInitializeSymbol = new SAIFFInitializeSymbol();
            var method = parent.MakeChildWithScope(TypeOfFactory(), GetType(), saiffInitializeSymbol, classScope)
                .AddParam<EPStatementInitServices>(REF_STMTINITSVC.Ref);
            method.Block
                .ExprDotMethod(
                    REF_STMTINITSVC,
                    "ActivateVariable",
                    Constant(_variableName),
                    _serde.Codegen(method, classScope, null))
                .MethodReturn(LocalMethod(_forge.InitializeCodegen(method, saiffInitializeSymbol, classScope)));
            return method;
        }
    }
} // end of namespace