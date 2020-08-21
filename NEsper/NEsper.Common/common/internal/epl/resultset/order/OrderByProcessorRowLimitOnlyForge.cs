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
using static com.espertech.esper.common.@internal.epl.resultset.order.OrderByProcessorOrderedLimitForge;


namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    /// <summary>
    /// An order-by processor that sorts events according to the expressions
    /// in the order_by clause.
    /// </summary>
    public class OrderByProcessorRowLimitOnlyForge : OrderByProcessorFactoryForge
    {
        private readonly RowLimitProcessorFactoryForge rowLimitProcessorFactoryForge;

        public OrderByProcessorRowLimitOnlyForge(RowLimitProcessorFactoryForge rowLimitProcessorFactoryForge)
        {
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
                    ExprDotMethod(rowLimitFactory, "Instantiate", MEMBER_AGENTINSTANCECONTEXT))
                .MethodReturn(
                    CodegenExpressionBuilder.NewInstanceInner(CLASSNAME_ORDERBYPROCESSOR, Ref("o"), REF_ROWLIMITPROCESSOR));
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
            OrderByProcessorRowLimitOnly.SortPlainCodegen(method);
        }

        public void SortWGroupKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorRowLimitOnly.SortWGroupKeysCodegen(method);
        }

        public void SortRollupCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorRowLimitOnly.SortRollupCodegen(method);
        }

        public void GetSortKeyCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(ConstantNull());
        }

        public void GetSortKeyRollupCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(ConstantNull());
        }

        public void SortWOrderKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorRowLimitOnly.SortWOrderKeysCodegen(method);
        }

        public void SortTwoKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorRowLimitOnly.SortTwoKeysCodegen(method);
        }
    }
} // end of namespace