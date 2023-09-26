///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

using static
    com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // LocalMethod
using static com.espertech.esper.common.@internal.context.aifactory.core.SAIFFInitializeSymbol; // REF_STMTINITSVC

namespace com.espertech.esper.common.@internal.context.aifactory.createclass
{
    public class StmtClassForgeableAIFactoryProviderCreateClass : StmtClassForgeableAIFactoryProviderBase
    {
        private readonly StatementAgentInstanceFactoryCreateClassForge forge;

        public StmtClassForgeableAIFactoryProviderCreateClass(
            string className,
            CodegenNamespaceScope namespaceScope,
            StatementAgentInstanceFactoryCreateClassForge forge)
            : base(className, namespaceScope)
        {
            this.forge = forge;
        }

        protected override Type TypeOfFactory()
        {
            return typeof(StatementAgentInstanceFactoryCreateClass);
        }

        protected override CodegenMethod CodegenConstructorInit(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var saiffInitializeSymbol = new SAIFFInitializeSymbol();
            var method = parent.MakeChildWithScope(TypeOfFactory(), GetType(), saiffInitializeSymbol, classScope)
                .AddParam<EPStatementInitServices>(REF_STMTINITSVC.Ref);
            method.Block
                .MethodReturn(LocalMethod(forge.InitializeCodegen(method, saiffInitializeSymbol, classScope)));
            return method;
        }
    }
} // end of namespace