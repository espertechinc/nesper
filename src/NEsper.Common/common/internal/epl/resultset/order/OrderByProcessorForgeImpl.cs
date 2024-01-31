///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.order.OrderByProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    public class OrderByProcessorForgeImpl : OrderByProcessorFactoryForge
    {
        public OrderByProcessorForgeImpl(
            OrderByElementForge[] orderBy,
            bool needsGroupByKeys,
            OrderByElementForge[][] orderByRollup,
            CodegenFieldSharable comparator)
        {
            OrderBy = orderBy;
            IsNeedsGroupByKeys = needsGroupByKeys;
            OrderByRollup = orderByRollup;
            IComparer = comparator;
        }

        public OrderByElementForge[] OrderBy { get; }

        public bool IsNeedsGroupByKeys { get; }

        public OrderByElementForge[][] OrderByRollup { get; }

        public CodegenFieldSharable IComparer { get; }

        public string[] ExpressionTexts {
            get {
                var expressions = new string[OrderBy.Length];
                for (var i = 0; i < OrderBy.Length; i++) {
                    expressions[i] = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(OrderBy[i].ExprNode);
                }

                return expressions;
            }
        }

        public bool[] DescendingFlags {
            get {
                var descending = new bool[OrderBy.Length];
                for (var i = 0; i < OrderBy.Length; i++) {
                    descending[i] = OrderBy[i].IsDescending;
                }

                return descending;
            }
        }

        public void InstantiateCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(NewInstanceInner(CLASSNAME_ORDERBYPROCESSOR, Ref("o")));
        }

        public void CtorCodegen(
            CodegenCtor ctor,
            IList<CodegenTypedParam> members,
            CodegenClassScope classScope)
        {
        }

        public void SortPlainCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorImpl.SortPlainCodegen(this, method, classScope, namedMethods);
        }

        public void SortWGroupKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorImpl.SortWGroupKeysCodegen(this, method, classScope, namedMethods);
        }

        public void SortRollupCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            if (OrderByRollup == null) {
                method.Block.MethodThrowUnsupported();
                return;
            }

            OrderByProcessorImpl.SortRollupCodegen(this, method, classScope, namedMethods);
        }

        public void GetSortKeyCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorImpl.GetSortKeyCodegen(this, method, classScope, namedMethods);
        }

        public void GetSortKeyRollupCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            if (OrderByRollup == null) {
                method.Block.MethodThrowUnsupported();
                return;
            }

            OrderByProcessorImpl.GetSortKeyRollupCodegen(this, method, classScope, namedMethods);
        }

        public void SortWOrderKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorImpl.SortWOrderKeysCodegen(this, method, classScope);
        }

        public void SortTwoKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            OrderByProcessorImpl.SortTwoKeysCodegen(this, method, classScope, namedMethods);
        }
    }
} // end of namespace