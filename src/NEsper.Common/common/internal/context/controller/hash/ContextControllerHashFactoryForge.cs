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
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashFactoryForge : ContextControllerForgeBase
    {
        private readonly ContextSpecHash detail;
        private StateMgmtSetting stateMgmtSettings;

        public ContextControllerHashFactoryForge(
            ContextControllerFactoryEnv ctx,
            ContextSpecHash detail) : base(ctx)
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
            // no props
        }

        public override void PlanStateSettings(
            ContextMetaData detail,
            FabricCharge fabricCharge,
            int controllerLevel,
            string nestedContextName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            stateMgmtSettings = services.StateMgmtSettingsProvider.Context.ContextHash(
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
            var method = parent.MakeChild(typeof(ContextControllerHashFactory), GetType(), classScope);
            method.Block.DeclareVar<ContextControllerHashFactory>("factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.CONTEXTSERVICEFACTORY)
                        .Add("HashFactory", stateMgmtSettings.ToExpression()))
                .SetProperty(Ref("factory"), "HashSpec", detail.MakeCodegen(method, symbols, classScope))
                .MethodReturn(Ref("factory"));
            return method;
        }

        public bool IsPreallocate()
        {
            return detail.IsPreallocate;
        }

        public override T Accept<T>(ContextControllerFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override ContextControllerPortableInfo ValidationInfo {
            get {
                var items = new ContextControllerHashValidationItem[detail.Items.Count];
                for (var i = 0; i < detail.Items.Count; i++) {
                    var props = detail.Items[i];
                    items[i] = new ContextControllerHashValidationItem(props.FilterSpecCompiled.FilterForEventType);
                }

                return new ContextControllerHashValidation(items);
            }
        }
    }
} // end of namespace