///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.serde.compiletime.sharable;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.bytecodemodel.util.CodegenFieldSharableComparator.
    CodegenSharableSerdeName;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.serde.compiletime.sharable.CodegenSharableSerdeClassArrayTyped.
    CodegenSharableSerdeName;
using static com.espertech.esper.common.@internal.serde.compiletime.sharable.CodegenSharableSerdeClassTyped.
    CodegenSharableSerdeName;
using static com.espertech.esper.common.@internal.serde.compiletime.sharable.CodegenSharableSerdeEventTyped.
    CodegenSharableSerdeName;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    /// <summary>
    /// Implementation of access function for single-stream (not joins).
    /// </summary>
    public class AggregatorAccessSortedMinMaxByEver : AggregatorAccessWFilterBase,
        AggregatorAccessSorted
    {
        private readonly AggregationStateMinMaxByEverForge forge;
        private CodegenExpressionMember currentMinMaxBean;
        private CodegenExpressionInstanceField currentMinMaxBeanSerde;
        private CodegenExpressionMember currentMinMax;
        private CodegenExpressionInstanceField currentMinMaxSerde;
        private CodegenExpressionInstanceField comparator;

        public AggregatorAccessSortedMinMaxByEver(
            AggregationStateMinMaxByEverForge forge,
            ExprNode optionalFilter) : base(optionalFilter)
        {
            this.forge = forge;
        }

        public override void InitAccessForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            currentMinMaxBean = membersColumnized.AddMember(col, typeof(EventBean), "currentMinMaxBean");
            currentMinMaxBeanSerde = classScope.AddOrGetDefaultFieldSharable(
                new CodegenSharableSerdeEventTyped(NULLABLEEVENTMAYCOLLATE, forge.Spec.StreamEventType));
            currentMinMax = membersColumnized.AddMember(col, typeof(object), "currentMinMax");
            if (forge.Spec.Criteria.Length == 1) {
                currentMinMaxSerde = classScope.AddOrGetDefaultFieldSharable(
                    new CodegenSharableSerdeClassTyped(
                        VALUE_NULLABLE,
                        forge.Spec.CriteriaTypes[0],
                        forge.Spec.CriteriaSerdes[0],
                        classScope));
            }
            else {
                currentMinMaxSerde = classScope.AddOrGetDefaultFieldSharable(
                    new CodegenSharableSerdeClassArrayTyped(
                        OBJECTARRAYMAYNULLNULL,
                        forge.Spec.CriteriaTypes,
                        forge.Spec.CriteriaSerdes,
                        classScope));
            }

            comparator = classScope.AddOrGetDefaultFieldSharable(
                new CodegenFieldSharableComparator(
                    COMPARATOROBJECTARRAYNONHASHABLE,
                    forge.Spec.CriteriaTypes,
                    forge.Spec.IsSortUsingCollator,
                    forge.Spec.SortDescending));
        }

        internal override void ApplyEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenExpression eps = symbols.GetAddEps(method);
            CodegenExpression ctx = symbols.GetAddExprEvalCtx(method);
            method.Block.DeclareVar<EventBean>("theEvent", ArrayAtIndex(eps, Constant(forge.Spec.StreamNum)))
                .IfCondition(EqualsNull(Ref("theEvent")))
                .BlockReturnNoValue()
                .LocalMethod(AddEventCodegen(method, namedMethods, classScope), Ref("theEvent"), eps, ctx);
        }

        internal override void ApplyLeaveFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            // this is an ever-type aggregation
        }

        public override void ClearCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(currentMinMaxBean, ConstantNull()).AssignRef(currentMinMax, ConstantNull());
        }

        public override void WriteCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef output,
            CodegenExpressionRef unitKey,
            CodegenExpressionRef writer,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block
                .ExprDotMethod(currentMinMaxSerde, "Write", RowDotMember(row, currentMinMax), output, unitKey, writer)
                .ExprDotMethod(
                    currentMinMaxBeanSerde,
                    "Write",
                    RowDotMember(row, currentMinMaxBean),
                    output,
                    unitKey,
                    writer);
        }

        public override void ReadCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenMethod method,
            CodegenExpressionRef unitKey,
            CodegenClassScope classScope)
        {
            method.Block
                .AssignRef(
                    RowDotMember(row, currentMinMax),
                    Cast(typeof(object), ExprDotMethod(currentMinMaxSerde, "Read", input, unitKey)))
                .AssignRef(
                    RowDotMember(row, currentMinMaxBean),
                    Cast(typeof(EventBean), ExprDotMethod(currentMinMaxBeanSerde, "Read", input, unitKey)));
        }

        public override void CollectFabricType(FabricTypeCollector collector)
        {
            if (forge.Spec.Criteria.Length == 1) {
                collector.Serde(forge.Spec.CriteriaSerdes[0]);
            }
            else {
                collector.SerdeObjectArrayMayNullNull(forge.Spec.CriteriaSerdes);
            }

            collector.SerdeNullableEvent(forge.Spec.StreamEventType);
        }

        public CodegenExpression GetFirstValueCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            if (forge.Spec.IsMax) {
                method.Block.MethodThrowUnsupported();
            }

            return currentMinMaxBean;
        }

        public CodegenExpression GetLastValueCodegen(
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            if (!forge.Spec.IsMax) {
                method.Block.MethodThrowUnsupported();
            }

            return currentMinMaxBean;
        }

        public CodegenExpression SizeCodegen()
        {
            throw new UnsupportedOperationException("Not supported for this state");
        }

        public CodegenExpression ReverseEnumeratorCodegen()
        {
            throw new UnsupportedOperationException("Not supported for this state");
        }

        public CodegenExpression EnumeratorCodegen()
        {
            throw new UnsupportedOperationException("Not supported for this state");
        }

        public CodegenExpression CollectionReadOnlyCodegen()
        {
            throw new UnsupportedOperationException("Not supported for this state");
        }

        private CodegenMethod AddEventCodegen(
            CodegenMethod parent,
            CodegenNamedMethods namedMethods,
            CodegenClassScope classScope)
        {
            var comparable = GetComparableWObjectArrayKeyCodegen(
                forge.Spec.Criteria,
                currentMinMaxBean,
                namedMethods,
                classScope);
            var methodNode = parent.MakeChild(typeof(void), GetType(), classScope)
                .AddParam<EventBean>("theEvent")
                .AddParam<EventBean[]>(NAME_EPS)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            methodNode.Block
                .DeclareVar<object>("comparable", LocalMethod(comparable, REF_EPS, ConstantTrue(), REF_EXPREVALCONTEXT))
                .IfCondition(EqualsNull(currentMinMax))
                .AssignRef(currentMinMax, Ref("comparable"))
                .AssignRef(currentMinMaxBean, Ref("theEvent"))
                .IfElse()
                .DeclareVar<int>("compareResult", ExprDotMethod(comparator, "Compare", currentMinMax, Ref("comparable")))
                .IfCondition(Relational(Ref("compareResult"), forge.Spec.IsMax ? LT : GT, Constant(0)))
                .AssignRef(currentMinMax, Ref("comparable"))
                .AssignRef(currentMinMaxBean, Ref("theEvent"));
            return methodNode;
        }

        private static CodegenMethod GetComparableWObjectArrayKeyCodegen(
            ExprNode[] criteria,
            CodegenExpressionMember member,
            CodegenNamedMethods namedMethods,
            CodegenClassScope classScope)
        {
            var methodName = "getComparable_" + member.Ref;
            Consumer<CodegenMethod> code = method => {
                if (criteria.Length == 1) {
                    method.Block.MethodReturn(
                        LocalMethod(
                            CodegenLegoMethodExpression.CodegenExpression(criteria[0].Forge, method, classScope),
                            REF_EPS,
                            REF_ISNEWDATA,
                            REF_EXPREVALCONTEXT));
                }
                else {
                    var exprSymbol = new ExprForgeCodegenSymbol(true, null);
                    var expressions = new CodegenExpression[criteria.Length];
                    for (var i = 0; i < criteria.Length; i++) {
                        expressions[i] =
                            criteria[i].Forge.EvaluateCodegen(typeof(object), method, exprSymbol, classScope);
                    }

                    exprSymbol.DerivedSymbolsCodegen(method, method.Block, classScope);
                    method.Block.DeclareVar<object[]>(
                        "result",
                        NewArrayByLength(typeof(object), Constant(criteria.Length)));
                    for (var i = 0; i < criteria.Length; i++) {
                        method.Block.AssignArrayElement(Ref("result"), Constant(i), expressions[i]);
                    }

                    method.Block.MethodReturn(Ref("result"));
                }
            };
            return namedMethods.AddMethod(
                typeof(object),
                methodName,
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    NAME_EPS,
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT),
                typeof(AggregatorAccessSortedImpl),
                classScope,
                code);
        }

        public static CodegenExpression CodegenGetAccessTableState(
            int column,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(EventBean), typeof(AggregatorAccessSortedMinMaxByEver), classScope);
            method.Block.MethodReturn(MemberCol("currentMinMaxBean", column));
            return LocalMethod(method);
        }
    }
} // end of namespace