///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using static com.espertech.esper.common.@internal.epl.expression.subquery.ExprSubselectNode.SubselectEvaluationType;

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

        public static readonly ExprSubselectNode[] EMPTY_SUBSELECT_ARRAY = new ExprSubselectNode[0];

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

        /// <summary>
        ///     Returns the compiled statement spec.
        /// </summary>
        /// <returns>compiled statement</returns>
        public StatementSpecCompiled StatementSpecCompiled { get; private set; }

        /// <summary>
        ///     Returns the uncompiled statement spec.
        /// </summary>
        /// <returns>statement spec uncompiled</returns>
        public StatementSpecRaw StatementSpecRaw { get; }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        /// <summary>
        ///     Returns the select clause or null if none.
        /// </summary>
        /// <returns>clause</returns>
        public ExprNode[] SelectClause { get; set; }

        /// <summary>
        ///     Returns the event type.
        /// </summary>
        /// <returns>type</returns>
        public EventType RawEventType { get; set; }

        /// <summary>
        ///     Return stream types.
        /// </summary>
        /// <returns>types</returns>
        public StreamTypeService FilterSubqueryStreamTypes { get; set; }

        public SubqueryAggregationType SubselectAggregationType { get; set; }

        public int SubselectNumber { get; set; } = -1;

        public bool IsFilterStreamSubselect { get; set; }

        /// <summary>
        ///     Supplies the name of the select expression as-tag
        /// </summary>
        /// <value>is the as-name(s)</value>
        public string[] SelectAsNames { get; set; }

        /// <summary>
        ///     Sets the validated filter expression, or null if there is none.
        /// </summary>
        /// <value>is the filter</value>
        public ExprForge FilterExpr { get; set; }

        public ExprForge HavingExpr { get; set; }

        public abstract bool IsAllowMultiColumnSelect { get; }

        public abstract Type ComponentTypeCollection { get; }

        public abstract EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices);

        public abstract EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices);

        public ExprNodeRenderable EnumForgeRenderable => this;
        public ExprNodeRenderable ExprForgeRenderable => this;

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(GETEVENTCOLL, this, typeof(ICollection<EventBean>), parent, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(
                GETSCALARCOLL,
                this,
                typeof(ICollection<object>),
                parent,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(GETEVENT, this, typeof(EventBean), parent, exprSymbol, codegenClassScope);
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
            return MakeEvaluate(PLAIN, this, EvaluationType, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public abstract Type EvaluationType { get; }

        public IDictionary<string, object> RowProperties => TypableGetRowProperties();

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

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateTypableSingleCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return MakeEvaluate(
                TYPABLESINGLE,
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
                TYPABLEMULTI,
                this,
                typeof(object[][]),
                parent,
                exprSymbol,
                codegenClassScope);
        }

        public bool? IsMultirow => true; // subselect can always return multiple rows

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

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (SelectAsNames != null && SelectAsNames[0] != null) {
                writer.Write(SelectAsNames[0]);
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
            Type resultType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(resultType, typeof(ExprSubselectNode), classScope);

            CodegenExpression eps = symbols.GetAddEPS(method);
            var newData = symbols.GetAddIsNewData(method);
            CodegenExpression evalCtx = symbols.GetAddExprEvalCtx(method);

            // get matching events
            CodegenExpression future = classScope.NamespaceScope.AddOrGetFieldWellKnown(
                new CodegenFieldNameSubqueryResult(subselectNode.SubselectNumber),
                typeof(SubordTableLookupStrategy));
            var evalMatching = ExprDotMethod(future, "Lookup", eps, evalCtx);
            method.Block.DeclareVar<ICollection<EventBean>>(NAME_MATCHINGEVENTS, evalMatching);

            // process matching events
            var evalMatchSymbol = new ExprSubselectEvalMatchSymbol();
            var processMethod = method
                .MakeChildWithScope(resultType, typeof(ExprSubselectNode), evalMatchSymbol, classScope)
                .AddParam(typeof(ICollection<EventBean>), NAME_MATCHINGEVENTS)
                .AddParam(ExprForgeCodegenNames.PARAMS);
            CodegenExpression process;
            if (evaluationType == PLAIN) {
                process = subselectNode.EvalMatchesPlainCodegen(processMethod, evalMatchSymbol, classScope);
            }
            else if (evaluationType == GETEVENTCOLL) {
                process = subselectNode.EvalMatchesGetCollEventsCodegen(processMethod, evalMatchSymbol, classScope);
            }
            else if (evaluationType == GETSCALARCOLL) {
                process = subselectNode.EvalMatchesGetCollScalarCodegen(processMethod, evalMatchSymbol, classScope);
            }
            else if (evaluationType == GETEVENT) {
                process = subselectNode.EvalMatchesGetEventBeanCodegen(processMethod, evalMatchSymbol, classScope);
            }
            else if (evaluationType == TYPABLESINGLE) {
                process = subselectNode.EvalMatchesTypableSingleCodegen(processMethod, evalMatchSymbol, classScope);
            }
            else if (evaluationType == TYPABLEMULTI) {
                process = subselectNode.EvalMatchesTypableMultiCodegen(processMethod, evalMatchSymbol, classScope);
            }
            else {
                throw new IllegalStateException("Unrecognized evaluation type " + evaluationType);
            }

            evalMatchSymbol.DerivedSymbolsCodegen(processMethod, processMethod.Block, classScope);
            processMethod.Block.MethodReturn(process);

            method.Block.MethodReturn(LocalMethod(processMethod, REF_MATCHINGEVENTS, eps, newData, evalCtx));

            return LocalMethod(method);
        }

        internal enum SubselectEvaluationType
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