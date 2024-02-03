///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.order.OrderByProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.order.OrderByProcessorOrderedLimitForge;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    /// <summary>
    ///     An order-by processor that sorts events according to the expressions
    ///     in the order_by clause.
    /// </summary>
    public class OrderByProcessorRowLimitOnly
    {
        public static void SortPlainCodegen(CodegenMethod method)
        {
            DetermineLimitAndApplyCodegen(method);
        }

        protected internal static void SortWGroupKeysCodegen(CodegenMethod method)
        {
            DetermineLimitAndApplyCodegen(method);
        }

        protected internal static void SortRollupCodegen(CodegenMethod method)
        {
            DetermineLimitAndApplyCodegen(method);
        }

        protected internal static void SortTwoKeysCodegen(CodegenMethod method)
        {
            method.Block.MethodReturn(
                ExprDotMethod(
                    REF_ROWLIMITPROCESSOR,
                    "DetermineApplyLimit2Events",
                    REF_ORDERFIRSTEVENT,
                    REF_ORDERSECONDEVENT));
        }

        protected internal static void SortWOrderKeysCodegen(CodegenMethod method)
        {
            DetermineLimitAndApplyCodegen(method);
        }

        private static void DetermineLimitAndApplyCodegen(CodegenMethod method)
        {
            method.Block.MethodReturn(
                ExprDotMethod(REF_ROWLIMITPROCESSOR, "DetermineLimitAndApply", REF_OUTGOINGEVENTS));
        }
    }
} // end of namespace