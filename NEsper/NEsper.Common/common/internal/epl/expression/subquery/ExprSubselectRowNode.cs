///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    /// <summary>
    ///     Represents a subselect in an expression tree.
    /// </summary>
    public class ExprSubselectRowNode : ExprSubselectNode
    {
        private SubselectForgeRow evalStrategy;
        internal EventType subselectMultirowType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="statementSpec">is the lookup statement spec from the parser, unvalidated</param>
        public ExprSubselectRowNode(StatementSpecRaw statementSpec)
            : base(statementSpec)
        {
        }

        public override bool IsAllowMultiColumnSelect => true;

        private LinkedHashMap<string, object> RowType {
            get {
                ISet<string> uniqueNames = new HashSet<string>();
                var type = new LinkedHashMap<string, object>();
                var selectClause = SelectClause;
                var selectAsNames = SelectAsNames;

                for (var i = 0; i < selectClause.Length; i++) {
                    var assignedName = selectAsNames[i];
                    if (assignedName == null) {
                        assignedName = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(selectClause[i]);
                    }

                    if (uniqueNames.Add(assignedName)) {
                        type.Put(assignedName, selectClause[i].Forge.EvaluationType);
                    }
                    else {
                        throw new ExprValidationException(
                            "Column " + i + " in subquery does not have a unique column name assigned");
                    }
                }

                return type;
            }
        }

        public override Type EvaluationType {
            get {
                var selectClause = SelectClause;
                if (selectClause == null) { // wildcards allowed
                    return RawEventType.UnderlyingType;
                }

                if (selectClause.Length == 1) {
                    return selectClause[0].Forge.EvaluationType.GetBoxedType();
                }

                return typeof(IDictionary<string, object>);
            }
        }

        public override void ValidateSubquery(ExprValidationContext validationContext)
        {
            // Strategy for subselect depends on presence of filter + presence of select clause expressions
            // the filter expression is handled elsewhere if there is any aggregation

            if (FilterExpr == null) {
                if (SelectClause == null) {
                    var table = validationContext.TableCompileTimeResolver.ResolveTableFromEventType(RawEventType);
                    if (table != null) {
                        evalStrategy = new SubselectForgeStrategyRowUnfilteredUnselectedTable(this, table);
                    }
                    else {
                        evalStrategy = new SubselectForgeStrategyRowPlain(this);
                    }
                }
                else {
                    if (StatementSpecCompiled.Raw.GroupByExpressions != null &&
                        StatementSpecCompiled.Raw.GroupByExpressions.Count > 0) {
                        if (HavingExpr != null) {
                            evalStrategy = new SubselectForgeRowUnfilteredSelectedGroupedWHaving(this);
                        }
                        else {
                            evalStrategy = new SubselectForgeRowUnfilteredSelectedGroupedNoHaving(this);
                        }
                    }
                    else {
                        if (HavingExpr != null) {
                            evalStrategy = new SubselectForgeRowHavingSelected(this);
                        }
                        else {
                            evalStrategy = new SubselectForgeStrategyRowPlain(this);
                        }
                    }
                }
            }
            else {
                if (SelectClause == null) {
                    var table = validationContext.TableCompileTimeResolver.ResolveTableFromEventType(RawEventType);
                    if (table != null) {
                        evalStrategy = new SubselectForgeStrategyRowFilteredUnselectedTable(this, table);
                    }
                    else {
                        evalStrategy = new SubselectForgeStrategyRowPlain(this);
                    }
                }
                else {
                    evalStrategy = new SubselectForgeStrategyRowPlain(this);
                }
            }
        }

        public override IDictionary<string, object> TypableGetRowProperties()
        {
            var selectClause = SelectClause;
            if (selectClause == null || selectClause.Length < 2) {
                return null;
            }

            return RowType;
        }

        public override EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            if (SelectClause == null) {
                return null;
            }

            if (SubselectAggregationType != SubqueryAggregationType.FULLY_AGGREGATED_NOPROPS) {
                return null;
            }

            return GetAssignAnonymousType(statementRawInfo, compileTimeServices);
        }

        public override EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            var rawEventType = RawEventType;
            var selectClause = SelectClause;
            if (selectClause == null) { // wildcards allowed
                return rawEventType;
            }

            // special case: selecting a single property that is itself an event
            if (selectClause.Length == 1 && selectClause[0] is ExprIdentNode) {
                var identNode = (ExprIdentNode) selectClause[0];
                var fragment = rawEventType.GetFragmentType(identNode.ResolvedPropertyName);
                if (fragment != null && !fragment.IsIndexed) {
                    return fragment.FragmentType;
                }
            }

            // select of a single value otherwise results in a collection of scalar values
            if (selectClause.Length == 1) {
                return null;
            }

            // fully-aggregated always returns zero or one row
            if (SubselectAggregationType == SubqueryAggregationType.FULLY_AGGREGATED_NOPROPS) {
                return null;
            }

            return GetAssignAnonymousType(statementRawInfo, compileTimeServices);
        }

        private EventType GetAssignAnonymousType(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            IDictionary<string, object> rowType = RowType;
            var eventTypeName =
                services.EventTypeNameGeneratorStatement.GetAnonymousTypeSubselectMultirow(SubselectNumber);
            var metadata = new EventTypeMetadata(
                eventTypeName,
                statementRawInfo.ModuleName,
                EventTypeTypeClass.SUBQDERIVED,
                EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            var maptype = BaseNestableEventUtil.MakeMapTypeCompileTime(
                metadata,
                rowType,
                null,
                null,
                null,
                null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(maptype);
            subselectMultirowType = maptype;
            return maptype;
        }

        public override Type ComponentTypeCollection {
            get {
                var selectClause = SelectClause;
                if (selectClause == null) { // wildcards allowed
                    return null;
                }

                if (selectClause.Length > 1) {
                    return null;
                }

                return selectClause[0].Forge.EvaluationType;
            }
        }

        protected override CodegenExpression EvalMatchesPlainCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(EvaluationType, GetType(), classScope);
            method.Block
                .ApplyTri(
                    new SubselectForgeCodegenUtil.ReturnIfNoMatch(ConstantNull(), ConstantNull()),
                    method,
                    symbols)
                .MethodReturn(evalStrategy.EvaluateCodegen(method, symbols, classScope));
            return LocalMethod(method);
        }

        protected override CodegenExpression EvalMatchesGetCollEventsCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent
                .MakeChild(typeof(ICollection<EventBean>), GetType(), classScope);

            method.Block
                .ApplyTri(
                    new SubselectForgeCodegenUtil.ReturnIfNoMatch(
                        ConstantNull(),
                        StaticMethod(typeof(Collections), "GetEmptyList", new [] { typeof(EventBean) })),
                    method,
                    symbols)
                .MethodReturn(evalStrategy.EvaluateGetCollEventsCodegen(method, symbols, classScope));

            return LocalMethod(method);
        }

        protected override CodegenExpression EvalMatchesGetCollScalarCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ICollection<object>), GetType(), classScope);
            method.Block
                .ApplyTri(
                    new SubselectForgeCodegenUtil.ReturnIfNoMatch(ConstantNull(), CollectionUtil.EMPTY_LIST_EXPRESSION),
                    method,
                    symbols)
                .MethodReturn(evalStrategy.EvaluateGetCollScalarCodegen(method, symbols, classScope));
            return LocalMethod(method);
        }

        protected override CodegenExpression EvalMatchesGetEventBeanCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(EventBean), GetType(), classScope);
            method.Block
                .ApplyTri(
                    new SubselectForgeCodegenUtil.ReturnIfNoMatch(ConstantNull(), ConstantNull()),
                    method,
                    symbols)
                .MethodReturn(evalStrategy.EvaluateGetBeanCodegen(method, symbols, classScope));
            return LocalMethod(method);
        }

        public CodegenMethod EvaluateRowCodegen(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var symbols = new ExprForgeCodegenSymbol(true, true);
            var method = parent.MakeChildWithScope(
                    typeof(IDictionary<string, object>),
                    typeof(CodegenLegoMethodExpression),
                    symbols,
                    classScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);

            var selectClause = SelectClause;
            var selectAsNames = SelectAsNames;

            var expressions = new CodegenExpression[selectClause.Length];
            for (var i = 0; i < selectClause.Length; i++) {
                expressions[i] = selectClause[i].Forge.EvaluateCodegen(typeof(object), method, symbols, classScope);
            }

            symbols.DerivedSymbolsCodegen(method, method.Block, classScope);

            method.Block.DeclareVar<IDictionary<string, object>>(
                "map",
                NewInstance(typeof(Dictionary<string, object>)));
            for (var i = 0; i < selectClause.Length; i++) {
                method.Block.ExprDotMethod(Ref("map"), "Put", Constant(selectAsNames[i]), expressions[i]);
            }

            method.Block.MethodReturn(Ref("map"));
            return method;
        }

        protected override CodegenExpression EvalMatchesTypableSingleCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(object[]), GetType(), classScope);
            method.Block
                .ApplyTri(
                    new SubselectForgeCodegenUtil.ReturnIfNoMatch(
                        ConstantNull(),
                        PublicConstValue(typeof(CollectionUtil), "OBJECTARRAYARRAY_EMPTY")),
                    method,
                    symbols)
                .MethodReturn(evalStrategy.EvaluateTypableSinglerowCodegen(method, symbols, classScope));
            return LocalMethod(method);
        }

        protected override CodegenExpression EvalMatchesTypableMultiCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(object[][]), GetType(), classScope);
            method.Block
                .ApplyTri(
                    new SubselectForgeCodegenUtil.ReturnIfNoMatch(
                        ConstantNull(),
                        PublicConstValue(typeof(CollectionUtil), "OBJECTARRAYARRAY_EMPTY")),
                    method,
                    symbols)
                .MethodReturn(evalStrategy.EvaluateTypableMultirowCodegen(method, symbols, classScope));
            return LocalMethod(method);
        }
    }
} // end of namespace