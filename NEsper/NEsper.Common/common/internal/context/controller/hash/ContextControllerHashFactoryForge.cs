///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashFactoryForge : ContextControllerForgeBase
    {
        private readonly ContextSpecHash detail;

        public ContextControllerHashFactoryForge(
            ContextControllerFactoryEnv ctx,
            ContextSpecHash detail)
            : base(ctx)
        {
            this.detail = detail;
        }

        public override void ValidateGetContextProps(
            LinkedHashMap<string, object> props,
            string contextName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            ContextControllerHashUtil.ValidateContextDesc(contextName, detail, statementRawInfo, services);
        }

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols)
        {
            CodegenMethod method = parent.MakeChild(typeof(ContextControllerHashFactory), this.GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(ContextControllerHashFactory), "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add(EPStatementInitServicesConstants.GETCONTEXTSERVICEFACTORY)
                        .Add("hashFactory"))
                .ExprDotMethod(@Ref("factory"), "setHashSpec", detail.MakeCodegen(method, symbols, classScope))
                .MethodReturn(@Ref("factory"));
            return method;
        }

        public override ContextControllerPortableInfo ValidationInfo {
            get {
                ContextControllerHashValidationItem[] items = new ContextControllerHashValidationItem[detail.Items.Count];
                for (int i = 0; i < detail.Items.Count; i++) {
                    ContextSpecHashItem props = detail.Items[i];
                    items[i] = new ContextControllerHashValidationItem(props.FilterSpecCompiled.FilterForEventType);
                }

                return new ContextControllerHashValidation(items);
            }
        }
    }
} // end of namespace