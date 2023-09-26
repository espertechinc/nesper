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

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.core
{
    public class StmtClassForgeableAIFactoryProviderOnTrigger : StmtClassForgeableAIFactoryProviderBase
    {
        private readonly StatementAgentInstanceFactoryOnTriggerBaseForge forge;

        public StmtClassForgeableAIFactoryProviderOnTrigger(
            string className,
            CodegenNamespaceScope namespaceScope,
            StatementAgentInstanceFactoryOnTriggerBaseForge forge)
            : base(className, namespaceScope)
        {
            this.forge = forge;
        }

        protected override Type TypeOfFactory()
        {
            return typeof(StatementAgentInstanceFactoryOnTriggerBase);
        }

        protected override CodegenMethod CodegenConstructorInit(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var saiffInitializeSymbol = new SAIFFInitializeSymbol();
            var method = parent.MakeChildWithScope(TypeOfFactory(), GetType(), saiffInitializeSymbol, classScope)
                .AddParam<EPStatementInitServices>(REF_STMTINITSVC.Ref);
            method.Block.MethodReturn(LocalMethod(forge.InitializeCodegen(method, saiffInitializeSymbol, classScope)));
            return method;
        }
    }
} // end of namespace