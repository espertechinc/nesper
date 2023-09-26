///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.order.OrderByProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.order.OrderByProcessorOrderedLimitForge;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    /// <summary>
    ///     Sorter and row limiter in one: sorts using a sorter and row limits
    /// </summary>
    public class OrderByProcessorOrderedLimit
    {
        protected internal static void SortPlainCodegenCodegen(
            OrderByProcessorOrderedLimitForge forge,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var limit1 = EqualsIdentity(ExprDotName(REF_ROWLIMITPROCESSOR, "CurrentRowLimit"), Constant(1));
            var offset0 = EqualsIdentity(ExprDotName(REF_ROWLIMITPROCESSOR, "CurrentOffset"), Constant(0));
            var haveOutgoing = And(
                NotEqualsNull(REF_OUTGOINGEVENTS),
                Relational(ArrayLength(REF_OUTGOINGEVENTS), GT, Constant(1)));
            var determineLocalMinMax = OrderByProcessorImpl.DetermineLocalMinMaxCodegen(
                forge.OrderByProcessorForge,
                classScope,
                namedMethods);

            var sortPlain = method.MakeChild(typeof(EventBean[]), typeof(OrderByProcessorOrderedLimit), classScope)
                .AddParam(SORTPLAIN_PARAMS);
            OrderByProcessorImpl.SortPlainCodegen(forge.OrderByProcessorForge, sortPlain, classScope, namedMethods);

            method.Block.ExprDotMethod(REF_ROWLIMITPROCESSOR, "DetermineCurrentLimit")
                .IfCondition(And(limit1, offset0, haveOutgoing))
                .DeclareVar<EventBean>(
                    "minmax",
                    LocalMethod(
                        determineLocalMinMax,
                        REF_OUTGOINGEVENTS,
                        REF_GENERATINGEVENTS,
                        REF_ISNEWDATA,
                        REF_EXPREVALCONTEXT,
                        MEMBER_AGGREGATIONSVC))
                .BlockReturn(NewArrayWithInit(typeof(EventBean), Ref("minmax")))
                .DeclareVar<EventBean[]>(
                    "sorted",
                    LocalMethod(
                        sortPlain,
                        REF_OUTGOINGEVENTS,
                        REF_GENERATINGEVENTS,
                        REF_ISNEWDATA,
                        REF_EXPREVALCONTEXT,
                        MEMBER_AGGREGATIONSVC))
                .MethodReturn(ExprDotMethod(REF_ROWLIMITPROCESSOR, "ApplyLimit", Ref("sorted")));
        }

        public static void SortRollupCodegen(
            OrderByProcessorOrderedLimitForge forge,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var sortRollup = method.MakeChild(typeof(EventBean[]), typeof(OrderByProcessorOrderedLimit), classScope)
                .AddParam(SORTROLLUP_PARAMS);
            OrderByProcessorImpl.SortRollupCodegen(forge.OrderByProcessorForge, sortRollup, classScope, namedMethods);
            method.Block.DeclareVar<EventBean[]>(
                    "sorted",
                    LocalMethod(
                        sortRollup,
                        REF_OUTGOINGEVENTS,
                        REF_ORDERCURRENTGENERATORS,
                        REF_ISNEWDATA,
                        MEMBER_EXPREVALCONTEXT,
                        MEMBER_AGGREGATIONSVC))
                .MethodReturn(ExprDotMethod(REF_ROWLIMITPROCESSOR, "DetermineLimitAndApply", Ref("sorted")));
        }

        protected internal static void SortWGroupKeysCodegen(
            OrderByProcessorOrderedLimitForge forge,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var sortWGroupKeys = method.MakeChild(typeof(EventBean[]), typeof(OrderByProcessorOrderedLimit), classScope)
                .AddParam(SORTWGROUPKEYS_PARAMS);
            OrderByProcessorImpl.SortWGroupKeysCodegen(
                forge.OrderByProcessorForge,
                sortWGroupKeys,
                classScope,
                namedMethods);

            method.Block.DeclareVar<EventBean[]>(
                    "sorted",
                    LocalMethod(
                        sortWGroupKeys,
                        REF_OUTGOINGEVENTS,
                        REF_GENERATINGEVENTS,
                        REF_ORDERGROUPBYKEYS,
                        REF_ISNEWDATA,
                        REF_EXPREVALCONTEXT,
                        MEMBER_AGGREGATIONSVC))
                .MethodReturn(ExprDotMethod(REF_ROWLIMITPROCESSOR, "DetermineLimitAndApply", Ref("sorted")));
        }

        protected internal static void SortTwoKeysCodegen(
            OrderByProcessorOrderedLimitForge forge,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var sortTwoKeys = method.MakeChild(typeof(EventBean[]), typeof(OrderByProcessorOrderedLimit), classScope)
                .AddParam(SORTTWOKEYS_PARAMS);
            OrderByProcessorImpl.SortTwoKeysCodegen(forge.OrderByProcessorForge, sortTwoKeys, classScope, namedMethods);

            method.Block.DeclareVar<EventBean[]>(
                    "sorted",
                    LocalMethod(
                        sortTwoKeys,
                        REF_ORDERFIRSTEVENT,
                        REF_ORDERFIRSTSORTKEY,
                        REF_ORDERSECONDEVENT,
                        REF_ORDERSECONDSORTKEY))
                .MethodReturn(ExprDotMethod(REF_ROWLIMITPROCESSOR, "DetermineLimitAndApply", Ref("sorted")));
        }

        protected internal static void SortWOrderKeysCodegen(
            OrderByProcessorOrderedLimitForge forge,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenExpression comparator =
                classScope.AddOrGetDefaultFieldSharable(forge.OrderByProcessorForge.IComparer);
            method.Block.MethodReturn(
                StaticMethod(
                    typeof(OrderByProcessorUtil),
                    "SortWOrderKeysWLimit",
                    REF_OUTGOINGEVENTS,
                    REF_ORDERKEYS,
                    comparator,
                    REF_ROWLIMITPROCESSOR));
        }
    }
} // end of namespace