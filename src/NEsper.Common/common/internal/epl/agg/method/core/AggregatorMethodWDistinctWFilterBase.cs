///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.compiletime.sharable;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.core
{
    public abstract class AggregatorMethodWDistinctWFilterBase : AggregatorMethod
    {
        private CodegenExpressionMember _distinct;
        private readonly Type _optionalDistinctValueType;
        private readonly DataInputOutputSerdeForge _optionalDistinctSerde;

        // this flag can be true and "optionalFilter" can still be null when declaring a table column
        private readonly bool _hasFilter;

        private readonly ExprNode _optionalFilter;
        private CodegenExpressionInstanceField _distinctSerde;

        public ExprNode OptionalFilter => _optionalFilter;

        public CodegenExpressionMember Distinct => _distinct;

        public abstract void InitForgeFiltered(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope);

        public AggregatorMethodWDistinctWFilterBase(
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter)
        {
            _optionalDistinctValueType = optionalDistinctValueType;
            _optionalDistinctSerde = optionalDistinctSerde;
            _optionalFilter = optionalFilter;
            _hasFilter = hasFilter;
        }

        public void InitForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            if (_optionalDistinctValueType != null) {
                _distinct = membersColumnized.AddMember(col, typeof(RefCountedSet<object>), "distinctSet");
                rowCtor.Block.AssignRef(_distinct, NewInstance(typeof(RefCountedSet<object>)));
                _distinctSerde = classScope.AddOrGetDefaultFieldSharable(
                    new CodegenSharableSerdeClassTyped(
                        CodegenSharableSerdeClassTyped.CodegenSharableSerdeName.REFCOUNTEDSET,
                        _optionalDistinctValueType,
                        _optionalDistinctSerde,
                        classScope));
            }
            else {
                _distinct = null;
                _distinctSerde = null;
            }

            InitForgeFiltered(col, rowCtor, membersColumnized, classScope);
        }

        public void ApplyEvalEnterCodegen(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            if (_optionalFilter != null) {
                PrefixWithFilterCheck(_optionalFilter.Forge, method, symbols, classScope);
            }

            ApplyEvalEnterFiltered(method, symbols, forges, classScope);
        }

        public void ApplyTableEnterCodegen(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (_hasFilter) {
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
            if (_optionalFilter != null) {
                PrefixWithFilterCheck(_optionalFilter.Forge, method, symbols, classScope);
            }

            ApplyEvalLeaveFiltered(method, symbols, forges, classScope);
        }

        public virtual void ApplyTableLeaveCodegen(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (_hasFilter) {
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
            if (_distinct != null) {
                method.Block.ExprDotMethod(_distinct, "Clear");
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
            if (_distinct != null) {
                method.Block.ExprDotMethod(
                    _distinctSerde,
                    "Write",
                    RowDotMember(row, _distinct),
                    output,
                    unitKey,
                    writer);
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
            if (_distinct != null) {
                method.Block.AssignRef(
                    RowDotMember(row, _distinct),
                    Cast(typeof(RefCountedSet<object>), ExprDotMethod(_distinctSerde, "Read", input, unitKey)));
            }

            ReadWODistinct(row, col, input, unitKey, method, classScope);
        }

        public void CollectFabricType(FabricTypeCollector collector)
        {
            if (_optionalDistinctSerde != null) {
                collector.RefCountedSet(_optionalDistinctSerde);
            }

            AppendFormatWODistinct(collector);
        }

        protected CodegenExpression ToDistinctValueKey(CodegenExpression distinctValue)
        {
            if (_optionalDistinctValueType == null) {
                return ConstantNull();
            }

            var inner = (Type)_optionalDistinctValueType;
            if (!inner.IsArray) {
                return distinctValue;
            }

            Type component = inner.GetComponentType();
            var mktype = MultiKeyPlanner.GetMKClassForComponentType(component);
            return NewInstance(mktype, Cast((Type)_optionalDistinctValueType, distinctValue));
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

        protected abstract void AppendFormatWODistinct(FabricTypeCollector collector);

        public abstract void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope);
    }
} // end of namespace