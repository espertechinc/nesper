///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.module;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashFactoryForge : ContextControllerForgeBase
    {
        private readonly ContextSpecHash _detail;
        private StateMgmtSetting _stateMgmtSettings;

        public ContextControllerHashFactoryForge(
            ContextControllerFactoryEnv ctx,
            ContextSpecHash detail)
            : base(ctx)
        {
            this._detail = detail;
        }

        public override void ValidateGetContextProps(
            IDictionary<string, object> props,
            string contextName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            ContextControllerHashUtil.ValidateContextDesc(contextName, _detail, statementRawInfo, services);
            _stateMgmtSettings = services.StateMgmtSettingsProvider.GetContext(statementRawInfo, contextName, AppliesTo.CONTEXT_HASH);
        }

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols)
        {
            CodegenMethod method = parent.MakeChild(typeof(ContextControllerHashFactory), GetType(), classScope);
            method.Block
                .DeclareVar<ContextControllerHashFactory>(
                    "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.CONTEXTSERVICEFACTORY)
                        .Add("HashFactory", _stateMgmtSettings.ToExpression()))
                .SetProperty(Ref("factory"), "HashSpec", _detail.MakeCodegen(method, symbols, classScope))
                .MethodReturn(Ref("factory"));
            return method;
        }

        public override ContextControllerPortableInfo ValidationInfo {
            get {
                ContextControllerHashValidationItem[] items =
                    new ContextControllerHashValidationItem[_detail.Items.Count];
                for (int i = 0; i < _detail.Items.Count; i++) {
                    ContextSpecHashItem props = _detail.Items[i];
                    items[i] = new ContextControllerHashValidationItem(props.FilterSpecCompiled.FilterForEventType);
                }

                return new ContextControllerHashValidation(items);
            }
        }
    }
} // end of namespace