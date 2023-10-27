///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.context.aifactory.core.SAIFFInitializeSymbol;

namespace com.espertech.esper.common.@internal.context.aifactory.createcontext
{
    public class StmtClassForgeableAIFactoryProviderCreateContext : StmtClassForgeableAIFactoryProviderBase
    {
        private readonly string contextName;
        private readonly ContextControllerFactoryForge[] forges;
        private readonly EventType eventTypeContextProperties;
        private readonly StatementAgentInstanceFactoryCreateContextForge forge;
        private readonly StateMgmtSetting partitionIdSvcStateMgmtSettings;

        public StmtClassForgeableAIFactoryProviderCreateContext(
            string className,
            CodegenNamespaceScope namespaceScope,
            string contextName,
            ContextControllerFactoryForge[] forges,
            EventType eventTypeContextProperties,
            StatementAgentInstanceFactoryCreateContextForge forge,
            StateMgmtSetting partitionIdSvcStateMgmtSettings) : base(className, namespaceScope)
        {
            this.contextName = contextName;
            this.forges = forges;
            this.eventTypeContextProperties = eventTypeContextProperties;
            this.forge = forge;
            this.partitionIdSvcStateMgmtSettings = partitionIdSvcStateMgmtSettings;
        }

        protected override Type TypeOfFactory()
        {
            return typeof(StatementAgentInstanceFactoryCreateContext);
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
                    "ActivateContext",
                    Constant(contextName),
                    GetDefinition(method, saiffInitializeSymbol, classScope))
                .MethodReturn(LocalMethod(forge.InitializeCodegen(classScope, method, saiffInitializeSymbol)));
            return method;
        }

        private CodegenExpression GetDefinition(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContextDefinition), GetType(), classScope);

            // controllers
            method.Block.DeclareVar<ContextControllerFactory[]>(
                "controllers",
                NewArrayByLength(typeof(ContextControllerFactory), Constant(forges.Length)));
            for (var i = 0; i < forges.Length; i++) {
                method.Block.AssignArrayElement(
                        "controllers",
                        Constant(i),
                        LocalMethod(forges[i].MakeCodegen(classScope, method, symbols)))
                    .SetProperty(
                        ArrayAtIndex(Ref("controllers"), Constant(i)),
                        "FactoryEnv",
                        forges[i].FactoryEnv.ToExpression());
            }

            method.Block.DeclareVarNewInstance(typeof(ContextDefinition), "def")
                .SetProperty(Ref("def"), "ContextName", Constant(contextName))
                .SetProperty(Ref("def"), "ControllerFactories", Ref("controllers"))
                .SetProperty(
                    Ref("def"),
                    "EventTypeContextProperties",
                    EventTypeUtility.ResolveTypeCodegen(eventTypeContextProperties, EPStatementInitServicesConstants.REF))
                .SetProperty(
                    Ref("def"),
                    "PartitionIdSvcStateMgmtSettings",
                    partitionIdSvcStateMgmtSettings.ToExpression())
                .MethodReturn(Ref("def"));
            return LocalMethod(method);
        }
    }
} // end of namespace