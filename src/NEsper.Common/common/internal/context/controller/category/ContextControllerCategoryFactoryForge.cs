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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.category
{
    public class ContextControllerCategoryFactoryForge : ContextControllerForgeBase
    {
        private readonly ContextSpecCategory detail;
        private StateMgmtSetting stateMgmtSettings;

        public ContextControllerCategoryFactoryForge(
            ContextControllerFactoryEnv ctx,
            ContextSpecCategory detail) : base(ctx)
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
            if (detail.Items.IsEmpty()) {
                throw new ExprValidationException("Empty list of partition items");
            }

            props.Put(ContextPropertyEventType.PROP_CTX_LABEL, typeof(string));
        }

        public override void PlanStateSettings(
            ContextMetaData detail,
            FabricCharge fabricCharge,
            int controllerLevel,
            string nestedContextName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            stateMgmtSettings = services.StateMgmtSettingsProvider.Context.ContextCategory(
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
            var method = parent.MakeChild(
                typeof(ContextControllerCategoryFactory),
                typeof(ContextControllerCategoryFactoryForge),
                classScope);
            method.Block
                .DeclareVar<ContextControllerCategoryFactory>(
                    "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.CONTEXTSERVICEFACTORY)
                        .Add("CategoryFactory", stateMgmtSettings.ToExpression()))
                .SetProperty(Ref("factory"), "ContextName", Constant(Context.ContextName))
                .SetProperty(Ref("factory"), "CategorySpec", detail.MakeCodegen(method, symbols, classScope))
                .MethodReturn(Ref("factory"));
            return method;
        }

        public override T Accept<T>(ContextControllerFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override ContextControllerPortableInfo ValidationInfo =>
            new ContextControllerCategoryValidation(detail.FilterSpecCompiled.FilterForEventType);
    }
} // end of namespace