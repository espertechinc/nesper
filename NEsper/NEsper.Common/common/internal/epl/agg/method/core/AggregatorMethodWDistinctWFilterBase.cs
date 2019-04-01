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
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.serde.CodegenSharableSerdeClassTyped.CodegenSharableSerdeName;

namespace com.espertech.esper.common.@internal.epl.agg.method.core
{
    public abstract class AggregatorMethodWDistinctWFilterBase : AggregatorMethod
    {
        internal readonly CodegenExpressionRef distinct;
        private readonly CodegenExpressionField distinctSerde;

        internal readonly bool hasFilter; // this flag can be true and "optionalFilter" can still be null when declaring a table column

        internal readonly Type optionalDistinctValueType;
        internal readonly ExprNode optionalFilter;

        public abstract void GetValueCodegen(CodegenMethod method, CodegenClassScope classScope);

        public AggregatorMethodWDistinctWFilterBase(
            AggregationForgeFactory factory,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            Type optionalDistinctValueType,
            bool hasFilter,
            ExprNode optionalFilter)
        {
            this.optionalDistinctValueType = optionalDistinctValueType;
            this.optionalFilter = optionalFilter;
            this.hasFilter = hasFilter;

            if (optionalDistinctValueType != null) {
                distinct = membersColumnized.AddMember(col, typeof(RefCountedSet<object>), "distinctSet");
                rowCtor.Block.AssignRef(distinct, NewInstance(typeof(RefCountedSet<object>)));
                distinctSerde = classScope.AddOrGetFieldSharable(
                    new CodegenSharableSerdeClassTyped(REFCOUNTEDSET, optionalDistinctValueType));
            }
            else {
                distinct = null;
                distinctSerde = null;
            }
        }

        public void ApplyEvalEnterCodegen(
            CodegenMethod method, ExprForgeCodegenSymbol symbols, ExprForge[] forges, CodegenClassScope classScope)
        {
            if (optionalFilter != null) {
                PrefixWithFilterCheck(optionalFilter.Forge, method, symbols, classScope);
            }

            ApplyEvalEnterFiltered(method, symbols, forges, classScope);
        }

        public void ApplyTableEnterCodegen(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            if (hasFilter) {
                method.Block
                    .DeclareVar(typeof(object[]), "in", Cast(typeof(object[]), value))
                    .DeclareVar(typeof(bool), "pass", Cast(typeof(bool?), ArrayAtIndex(Ref("in"), Constant(1))))
                    .IfCondition(Not(Ref("pass"))).BlockReturnNoValue()
                    .DeclareVar(typeof(object), "filtered", ArrayAtIndex(Ref("in"), Constant(0)));
                ApplyTableEnterFiltered(Ref("filtered"), evaluationTypes, method, classScope);
            }
            else {
                ApplyTableEnterFiltered(value, evaluationTypes, method, classScope);
            }
        }

        public virtual void ApplyEvalLeaveCodegen(
            CodegenMethod method, ExprForgeCodegenSymbol symbols, ExprForge[] forges, CodegenClassScope classScope)
        {
            if (optionalFilter != null) {
                PrefixWithFilterCheck(optionalFilter.Forge, method, symbols, classScope);
            }

            ApplyEvalLeaveFiltered(method, symbols, forges, classScope);
        }

        public virtual void ApplyTableLeaveCodegen(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            if (hasFilter) {
                method.Block
                    .DeclareVar(typeof(object[]), "in", Cast(typeof(object[]), value))
                    .DeclareVar(typeof(bool), "pass", Cast(typeof(bool?), ArrayAtIndex(Ref("in"), Constant(1))))
                    .IfCondition(Not(Ref("pass"))).BlockReturnNoValue()
                    .DeclareVar(typeof(object), "filtered", ArrayAtIndex(Ref("in"), Constant(0)));
                ApplyTableLeaveFiltered(Ref("filtered"), evaluationTypes, method, classScope);
            }
            else {
                ApplyTableLeaveFiltered(value, evaluationTypes, method, classScope);
            }
        }

        public void ClearCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            if (distinct != null) {
                method.Block.ExprDotMethod(distinct, "clear");
            }

            ClearWODistinct(method, classScope);
        }

        public void WriteCodegen(
            CodegenExpressionRef row, int col, CodegenExpressionRef output, CodegenExpressionRef unitKey,
            CodegenExpressionRef writer, CodegenMethod method, CodegenClassScope classScope)
        {
            if (distinct != null) {
                method.Block.ExprDotMethod(distinctSerde, "write", RowDotRef(row, distinct), output, unitKey, writer);
            }

            WriteWODistinct(row, col, output, unitKey, writer, method, classScope);
        }

        public void ReadCodegen(
            CodegenExpressionRef row, int col, CodegenExpressionRef input, CodegenExpressionRef unitKey,
            CodegenMethod method, CodegenClassScope classScope)
        {
            if (distinct != null) {
                method.Block.AssignRef(
                    RowDotRef(row, distinct),
                    Cast(typeof(RefCountedSet<object>), ExprDotMethod(distinctSerde, "read", input, unitKey)));
            }

            ReadWODistinct(row, col, input, unitKey, method, classScope);
        }

        protected abstract void ApplyEvalEnterFiltered(
            CodegenMethod method, ExprForgeCodegenSymbol symbols, ExprForge[] forges, CodegenClassScope classScope);

        protected abstract void ApplyEvalLeaveFiltered(
            CodegenMethod method, ExprForgeCodegenSymbol symbols, ExprForge[] forges, CodegenClassScope classScope);

        protected abstract void ApplyTableEnterFiltered(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope);

        protected abstract void ApplyTableLeaveFiltered(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope);

        protected abstract void ClearWODistinct(CodegenMethod method, CodegenClassScope classScope);

        protected abstract void WriteWODistinct(
            CodegenExpressionRef row, int col, CodegenExpressionRef output, CodegenExpressionRef unitKey,
            CodegenExpressionRef writer, CodegenMethod method, CodegenClassScope classScope);

        protected abstract void ReadWODistinct(
            CodegenExpressionRef row, int col, CodegenExpressionRef input, CodegenExpressionRef unitKey,
            CodegenMethod method, CodegenClassScope classScope);
    }
} // end of namespace