///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.compiletime.sharable;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.core
{
    public abstract class AggregatorMethodWDistinctWFilterBase : AggregatorMethod
    {
        internal readonly CodegenExpressionMember distinct;
        private readonly CodegenExpressionInstanceField distinctSerde;

        // this flag can be true and "optionalFilter" can still be null when declaring a table column
        private readonly bool hasFilter;

        private readonly Type optionalDistinctValueType;
        private readonly ExprNode optionalFilter;

        public Type OptionalDistinctValueType => optionalDistinctValueType;

        public ExprNode OptionalFilter => optionalFilter;

        public abstract void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope);

        public AggregatorMethodWDistinctWFilterBase(
            AggregationForgeFactory factory,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter)
        {
            this.optionalDistinctValueType = optionalDistinctValueType;
            this.optionalFilter = optionalFilter;
            this.hasFilter = hasFilter;

            if (optionalDistinctValueType != null) {
                distinct = membersColumnized.AddMember(col, typeof(RefCountedSet<object>), "distinctSet");
                rowCtor.Block.AssignRef(distinct, NewInstance(typeof(RefCountedSet<object>)));
                distinctSerde = classScope.AddOrGetDefaultFieldSharable(
                    new CodegenSharableSerdeClassTyped(
                        CodegenSharableSerdeClassTyped.CodegenSharableSerdeName.REFCOUNTEDSET,
                        optionalDistinctValueType,
                        optionalDistinctSerde,
                        classScope));
            }
            else {
                distinct = null;
                distinctSerde = null;
            }
        }

        public void ApplyEvalEnterCodegen(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            if (optionalFilter != null) {
                PrefixWithFilterCheck(optionalFilter.Forge, method, symbols, classScope);
            }

            ApplyEvalEnterFiltered(method, symbols, forges, classScope);
        }

        public void ApplyTableEnterCodegen(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (hasFilter) {
                method.Block
                    .DeclareVar<object[]>("vin", Cast(typeof(object[]), value))
                    .DeclareVar<bool?>("pass", Cast(typeof(bool?), ArrayAtIndex(Ref("vin"), Constant(1))))
                    .IfCondition(Not(Unbox(Ref("pass"))))
                    .BlockReturnNoValue()
                    .DeclareVar<object>("filtered", ArrayAtIndex(Ref("vin"), Constant(0)));
                ApplyTableEnterFiltered(Ref("filtered"), evaluationTypes, method, classScope);
            }
            else {
                ApplyTableEnterFiltered(value, evaluationTypes, method, classScope);
            }
        }

        public virtual void ApplyEvalLeaveCodegen(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            if (optionalFilter != null) {
                PrefixWithFilterCheck(optionalFilter.Forge, method, symbols, classScope);
            }

            ApplyEvalLeaveFiltered(method, symbols, forges, classScope);
        }

        public virtual void ApplyTableLeaveCodegen(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (hasFilter) {
                method.Block
                    .DeclareVar<object[]>("vin", Cast(typeof(object[]), value))
                    .DeclareVar<bool?>("pass", Cast(typeof(bool?), ArrayAtIndex(Ref("vin"), Constant(1))))
                    .IfCondition(Not(Unbox(Ref("pass"))))
                    .BlockReturnNoValue()
                    .DeclareVar<object>("filtered", ArrayAtIndex(Ref("vin"), Constant(0)));
                ApplyTableLeaveFiltered(Ref("filtered"), evaluationTypes, method, classScope);
            }
            else {
                ApplyTableLeaveFiltered(value, evaluationTypes, method, classScope);
            }
        }

        public void ClearCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (distinct != null) {
                method.Block.ExprDotMethod(distinct, "Clear");
            }

            ClearWODistinct(method, classScope);
        }

        public void WriteCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef output,
            CodegenExpressionRef unitKey,
            CodegenExpressionRef writer,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (distinct != null) {
                method.Block.ExprDotMethod(distinctSerde, "Write", RowDotMember(row, distinct), output, unitKey, writer);
            }

            WriteWODistinct(row, col, output, unitKey, writer, method, classScope);
        }

        public void ReadCodegen(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenExpressionRef unitKey,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (distinct != null) {
                method.Block.AssignRef(
                    RowDotMember(row, distinct),
                    Cast(typeof(RefCountedSet<object>), ExprDotMethod(distinctSerde, "Read", input, unitKey)));
            }

            ReadWODistinct(row, col, input, unitKey, method, classScope);
        }

        protected CodegenExpression ToDistinctValueKey(CodegenExpression distinctValue)
        {
            if (!optionalDistinctValueType.IsArray) {
                return distinctValue;
            }

            var mktype = MultiKeyPlanner.GetMKClassForComponentType(optionalDistinctValueType.GetElementType());
            return NewInstance(mktype, Cast(optionalDistinctValueType, distinctValue));
        }

        protected abstract void ApplyEvalEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope);

        protected abstract void ApplyEvalLeaveFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope);

        protected abstract void ApplyTableEnterFiltered(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope);

        protected abstract void ApplyTableLeaveFiltered(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope);

        protected abstract void ClearWODistinct(
            CodegenMethod method,
            CodegenClassScope classScope);

        protected abstract void WriteWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef output,
            CodegenExpressionRef unitKey,
            CodegenExpressionRef writer,
            CodegenMethod method,
            CodegenClassScope classScope);

        protected abstract void ReadWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenExpressionRef unitKey,
            CodegenMethod method,
            CodegenClassScope classScope);
    }
} // end of namespace