///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.statemgmtsettings;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerInitTermFactoryForge : ContextControllerForgeBase
    {
        private readonly ContextSpecInitiatedTerminated detail;
        private StateMgmtSetting distinctStateMgmtSettings;
        private StateMgmtSetting ctxStateMgmtSettings;

        public ContextControllerInitTermFactoryForge(
            ContextControllerFactoryEnv ctx,
            ContextSpecInitiatedTerminated detail) : base(ctx)
        {
            this.detail = detail;
        }

        public override void ValidateGetContextProps(
            IDictionary<string, object> props,
            string contextName,
            int controllerLevel,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            props.Put(ContextPropertyEventType.PROP_CTX_STARTTIME, typeof(long?));
            props.Put(ContextPropertyEventType.PROP_CTX_ENDTIME, typeof(long?));
            var allTags = new LinkedHashSet<string>();
            ContextPropertyEventType.AddEndpointTypes(detail.StartCondition, props, allTags);
            ContextPropertyEventType.AddEndpointTypes(detail.EndCondition, props, allTags);
        }

        public override void PlanStateSettings(
            ContextMetaData detail,
            FabricCharge fabricCharge,
            int controllerLevel,
            string nestedContextName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            distinctStateMgmtSettings = StateMgmtSettingDefault.INSTANCE;
            if (this.detail.DistinctExpressions != null && this.detail.DistinctExpressions.Length > 0) {
                distinctStateMgmtSettings = services.StateMgmtSettingsProvider.Context.ContextInitTermDistinct(
                    fabricCharge,
                    detail,
                    this,
                    statementRawInfo,
                    controllerLevel);
            }

            ctxStateMgmtSettings = services.StateMgmtSettingsProvider.Context.ContextInitTerm(
                fabricCharge,
                detail,
                this,
                statementRawInfo,
                controllerLevel);
        }

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols)
        {
            var method = parent.MakeChild(typeof(ContextControllerInitTermFactory), GetType(), classScope);
            method.Block.DeclareVar<ContextControllerInitTermFactory>(
                    "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.CONTEXTSERVICEFACTORY)
                        .Add(
                            "InitTermFactory",
                            distinctStateMgmtSettings.ToExpression(),
                            ctxStateMgmtSettings.ToExpression()))
                .ExprDotMethod(Ref("factory"), "setInitTermSpec", detail.MakeCodegen(method, symbols, classScope))
                .MethodReturn(Ref("factory"));
            return method;
        }

        public override T Accept<T>(ContextControllerFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override ContextControllerPortableInfo ValidationInfo => ContextControllerInitTermValidation.INSTANCE;

        public ContextSpecInitiatedTerminated Detail => detail;
    }
} // end of namespace