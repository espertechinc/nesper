///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.subquery.ExprSubselectEvalMatchSymbol;
using static com.espertech.esper.common.@internal.epl.util.EPTypeCollectionConst;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    /// <summary>
    ///     Represents a subselect in an expression tree.
    /// </summary>
    public abstract class ExprSubselectNode : ExprNodeBase,
        ExprEvaluator,
        ExprEnumerationForge,
        ExprTypableReturnForge,
        ExprForgeInstrumentable
    {
        public enum SubqueryAggregationType
        {
            NONE,
            FULLY_AGGREGATED_NOPROPS,
            FULLY_AGGREGATED_WPROPS
        }

        public static readonly ExprSubselectNode[] EMPTY_SUBSELECT_ARRAY = Array.Empty<ExprSubselectNode>();

        internal ExprForge filterExpr;
        internal ExprForge havingExpr;
        internal EventType rawEventType;
        internal string[] selectAsNames;
        internal ExprNode[] selectClause;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="statementSpec">is the lookup statement spec from the parser, unvalidated</param>
        protected ExprSubselectNode(StatementSpecRaw statementSpec)
        {
            StatementSpecRaw = statementSpec;
        }

        public bool IsConstantResult => false;

        public override ExprForge Forge => this;

        public ExprNodeRenderable ForgeRenderable => this;
        
        /// <summary>
        ///     Returns the compiled statement spec.
        /// </summary>
        /// <value>compiled statement</value>
        public StatementSpecCompiled StatementSpecCompiled { get; private set; }

        /// <summary>
        ///     Returns the uncompiled statement spec.
        /// </summary>
        /// <value>statement spec uncompiled</value>
        public StatementSpecRaw StatementSpecRaw { get; }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        /// <summary>
        ///     Returns the select clause or null if none.
        /// </summary>
        /// <value>clause</value>
        public ExprNode[] SelectClause {
            get => selectClause;
            set => selectClause = value;
        }

        /// <summary>
        ///     Returns the event type.
        /// </summary>
        /// <value>type</value>
        public EventType RawEventType {
            get => rawEventType;
            set => rawEventType = value;
        }

        /// <summary>
        ///     Return stream types.
        /// </summary>
        /// <value>types</value>
        public StreamTypeService FilterSubqueryStreamTypes { get; set; }

        public SubqueryAggregationType SubselectAggregationType { get; set; }

        public int SubselectNumber { get; private set; } = -1;

        public bool IsFilterStreamSubselect { set; get; }

        public ExprValidationContext FilterStreamExprValidationContext { get; private set; }

        /// <summary>
        ///     Supplies the name of the select expression as-tag
        /// </summary>
        /// <value>is the as-name(s)</value>
        public string[] SelectAsNames {
            get => selectAsNames;
            set => selectAsNames = value;
        }

        /// <summary>
        ///     Sets the validated filter expression, or null if there is none.
        /// </summary>
        /// <value>is the filter</value>
        public ExprForge FilterExpr {
            get => filterExpr;
            set => filterExpr = value;
        }

        public ExprForge HavingExpr {
            get => havingExpr;
            set => havingExpr = value;
        }

        public abstract bool IsAllowMultiColumnSelect { get; }

        public abstract Type ComponentTypeCollection { get; }

        public virtual ExprNodeRenderable EnumForgeRenderable => ForgeRenderable;
        public virtual ExprNodeRenderable ExprForgeRenderable => ForgeRenderable;

        public abstract EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices);

        public abstract EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices);

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(
                SubselectEvaluationType.GETEVENTCOLL,
                this,
                typeof(FlexCollection),
                parent,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(
                SubselectEvaluationType.GETSCALARCOLL,
                this,
                typeof(FlexCollection),
                parent,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(
                SubselectEvaluationType.GETEVENT,
                this,
                typeof(EventBean),
                parent,
                exprSymbol,
                codegenClassScope);
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(
                SubselectEvaluationType.PLAIN,
                this,
                EvaluationType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public abstract Type EvaluationType { get; }

        public IDictionary<string, object> RowProperties => TypableGetRowProperties();

        public bool? IsMultirow => true; // subselect can always return multiple rows

        public ExprEvaluator ExprEvaluator => this;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(),
                this,
                "ExprSubselect",
                requiredType,
                parent,
                exprSymbol,
                codegenClassScope).Build();
        }

        public CodegenExpression EvaluateTypableSingleCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(
                SubselectEvaluationType.TYPABLESINGLE,
                this,
                typeof(object[]),
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression EvaluateTypableMultiCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(
                SubselectEvaluationType.TYPABLEMULTI,
                this,
                typeof(object[][]),
                parent,
                exprSymbol,
                codegenClassScope);
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public abstract void ValidateSubquery(ExprValidationContext validationContext);

        public abstract IDictionary<string, object> TypableGetRowProperties();

        protected abstract CodegenExpression EvalMatchesPlainCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope);

        protected abstract CodegenExpression EvalMatchesGetCollEventsCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope);

        protected abstract CodegenExpression EvalMatchesGetCollScalarCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope);

        protected abstract CodegenExpression EvalMatchesGetEventBeanCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope);

        protected abstract CodegenExpression EvalMatchesTypableSingleCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope);

        protected abstract CodegenExpression EvalMatchesTypableMultiCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope);

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            ValidateSubquery(validationContext);
            if (IsFilterStreamSubselect) {
                FilterStreamExprValidationContext = validationContext;
            }

            return null;
        }

        /// <summary>
        ///     Supplies a compiled statement spec.
        /// </summary>
        /// <param name="statementSpecCompiled">compiled validated filters</param>
        /// <param name="subselectNumber">subselect assigned number</param>
        public void SetStatementSpecCompiled(
            StatementSpecCompiled statementSpecCompiled,
            int subselectNumber)
        {
            StatementSpecCompiled = statementSpecCompiled;
            SubselectNumber = subselectNumber;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            if (selectAsNames != null && selectAsNames[0] != null) {
                writer.Write(selectAsNames[0]);
                return;
            }

            writer.Write("subselect_");
            writer.Write(SubselectNumber + 1); // Error-reporting starts at 1, internally we start at zero
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return false; // 2 subselects are never equivalent
        }

        public static ExprSubselectNode[] ToArray(IList<ExprSubselectNode> subselectNodes)
        {
            if (subselectNodes.IsEmpty()) {
                return EMPTY_SUBSELECT_ARRAY;
            }

            return subselectNodes.ToArray();
        }

        private static CodegenExpression MakeEvaluate(
            SubselectEvaluationType evaluationType,
            ExprSubselectNode subselectNode,
            Type resultTypeMayNull,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            if (resultTypeMayNull == null) {
                return ConstantNull();
            }

            var resultType = resultTypeMayNull;
            var method = parent.MakeChild(resultType, typeof(ExprSubselectNode), classScope);

            CodegenExpression eps = symbols.GetAddEPS(method);
            var newData = symbols.GetAddIsNewData(method);
            CodegenExpression evalCtx = symbols.GetAddExprEvalCtx(method);

            // get matching events
            CodegenExpression future = classScope.NamespaceScope.AddOrGetFieldWellKnown(
                new CodegenFieldNameSubqueryResult(subselectNode.SubselectNumber),
                typeof(SubordTableLookupStrategy));
            var evalMatching = ExprDotMethod(future, "lookup", eps, evalCtx);
            method.Block.DeclareVar(typeof(ICollection<EventBean>), NAME_MATCHINGEVENTS, evalMatching);

            // process matching events
            var evalMatchSymbol = new ExprSubselectEvalMatchSymbol();
            var processMethod = method
                .MakeChildWithScope(resultType, typeof(ExprSubselectNode), evalMatchSymbol, classScope)
                .AddParam(EPTYPE_COLLECTION_EVENTBEAN, NAME_MATCHINGEVENTS)
                .AddParam(ExprForgeCodegenNames.PARAMS);
            var process = evaluationType switch {
                SubselectEvaluationType.PLAIN => subselectNode.EvalMatchesPlainCodegen(
                    processMethod,
                    evalMatchSymbol,
                    classScope),
                SubselectEvaluationType.GETEVENTCOLL => subselectNode.EvalMatchesGetCollEventsCodegen(
                    processMethod,
                    evalMatchSymbol,
                    classScope),
                SubselectEvaluationType.GETSCALARCOLL => subselectNode.EvalMatchesGetCollScalarCodegen(
                    processMethod,
                    evalMatchSymbol,
                    classScope),
                SubselectEvaluationType.GETEVENT => subselectNode.EvalMatchesGetEventBeanCodegen(
                    processMethod,
                    evalMatchSymbol,
                    classScope),
                SubselectEvaluationType.TYPABLESINGLE => subselectNode.EvalMatchesTypableSingleCodegen(
                    processMethod,
                    evalMatchSymbol,
                    classScope),
                SubselectEvaluationType.TYPABLEMULTI => subselectNode.EvalMatchesTypableMultiCodegen(
                    processMethod,
                    evalMatchSymbol,
                    classScope),
                _ => throw new IllegalStateException("Unrecognized evaluation type " + evaluationType)
            };

            evalMatchSymbol.DerivedSymbolsCodegen(processMethod, processMethod.Block, classScope);
            processMethod.Block.MethodReturn(process);

            method.Block.MethodReturn(LocalMethod(processMethod, REF_MATCHINGEVENTS, eps, newData, evalCtx));

            return LocalMethod(method);
        }

        private enum SubselectEvaluationType
        {
            PLAIN,
            GETEVENTCOLL,
            GETSCALARCOLL,
            GETEVENT,
            TYPABLESINGLE,
            TYPABLEMULTI
        }
    }
} // end of namespace