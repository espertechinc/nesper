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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.context.aifactory.core.SAIFFInitializeSymbol;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public class StmtClassForgeableAIFactoryProviderSelect : StmtClassForgeableAIFactoryProviderBase
    {
        private readonly StatementAgentInstanceFactoryForge _forge;

        public StmtClassForgeableAIFactoryProviderSelect(
            string className,
            CodegenNamespaceScope namespaceScope,
            StatementAgentInstanceFactoryForge forge)
            : base(className, namespaceScope)
        {
            this._forge = forge;
        }

        protected override Type TypeOfFactory()
        {
            return typeof(StatementAgentInstanceFactorySelect);
        }

        protected override CodegenMethod CodegenConstructorInit(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var saiffInitializeSymbol = new SAIFFInitializeSymbol();
            var method = parent.MakeChildWithScope(TypeOfFactory(), GetType(), saiffInitializeSymbol, classScope)
                .AddParam(typeof(EPStatementInitServices), REF_STMTINITSVC.Ref);
            method.Block.MethodReturn(LocalMethod(_forge.InitializeCodegen(classScope, method, saiffInitializeSymbol)));
            return method;
        }
    }
} // end of namespace