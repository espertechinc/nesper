///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.order.OrderByProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    public class OrderByProcessorOrderedLimitForge : OrderByProcessorFactoryForge
    {
        public static readonly CodegenExpressionRef REF_ROWLIMITPROCESSOR = Ref("rowLimitProcessor");

        private readonly OrderByProcessorForgeImpl orderByProcessorForge;
        private readonly RowLimitProcessorFactoryForge rowLimitProcessorFactoryForge;

        public OrderByProcessorOrderedLimitForge(
            OrderByProcessorForgeImpl orderByProcessorForge,
            RowLimitProcessorFactoryForge rowLimitProcessorFactoryForge)
        {
            this.orderByProcessorForge = orderByProcessorForge;
            this.rowLimitProcessorFactoryForge = rowLimitProcessorFactoryForge;
        }

        public void InstantiateCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            var rowLimitFactory = classScope.AddDefaultFieldUnshared(
                true,
                typeof(RowLimitProcessorFactory),
                rowLimitProcessorFactoryForge.Make(classScope.NamespaceScope.InitMethod, classScope));
            method.Block.DeclareVar<RowLimitProcessor>(
                    REF_ROWLIMITPROCESSOR.Ref,
                    ExprDotMethod(rowLimitFactory, "Instantiate", REF_AGENTINSTANCECONTEXT))
                .MethodReturn(
                    CodegenExpressionBuilder.NewInstance(CLASSNAME_ORDERBYPROCESSOR, Ref("o"), REF_ROWLIMITPROCESSOR));
        }

        public void CtorCodegen(
            CodegenCtor ctor,
            IList<CodegenTypedParam> members,
            CodegenClassScope classScope)
        {
            ctor.CtorParams.Add(new CodegenTypedParam(typeof(RowLimitProcessor), REF_ROWLIMITPROCESSOR.Ref));
        }

        public void SortPlainCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorOrderedLimit.SortPlainCodegenCodegen(this, method, classScope, namedMethods);
        }

        public void SortWGroupKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorOrderedLimit.SortWGroupKeysCodegen(this, method, classScope, namedMethods);
        }

        public void SortRollupCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            if (orderByProcessorForge.OrderByRollup == null) {
                method.Block.MethodThrowUnsupported();
                return;
            }

            OrderByProcessorOrderedLimit.SortRollupCodegen(this, method, classScope, namedMethods);
        }

        public void GetSortKeyCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorImpl.GetSortKeyCodegen(orderByProcessorForge, method, classScope, namedMethods);
        }

        public void GetSortKeyRollupCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            if (orderByProcessorForge.OrderByRollup == null) {
                method.Block.MethodThrowUnsupported();
                return;
            }

            OrderByProcessorImpl.GetSortKeyRollupCodegen(orderByProcessorForge, method, classScope, namedMethods);
        }

        public void SortWOrderKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorOrderedLimit.SortWOrderKeysCodegen(this, method, classScope, namedMethods);
        }

        public void SortTwoKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorOrderedLimit.SortTwoKeysCodegen(this, method, classScope, namedMethods);
        }

        protected internal OrderByProcessorForgeImpl OrderByProcessorForge {
            get { return orderByProcessorForge; }
        }
    }
} // end of namespace