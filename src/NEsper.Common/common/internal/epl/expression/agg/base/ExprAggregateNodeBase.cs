///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.name;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.codegen;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.agg.@base
{
    /// <summary>
    /// Base expression node that represents an aggregation function such as 'sum' or 'count'.
    /// <para />In terms of validation each concrete aggregation node must implement it's own validation.
    /// <para />In terms of evaluation this base class will ask the assigned <seealso cref="AggregationResultFuture" /> for the current state,
    /// using a column number assigned to the node.
    /// <para />Concrete subclasses must supply an aggregation state prototype node that reflects
    /// each group's (there may be group-by critera) current aggregation state.
    /// </summary>
    public abstract class ExprAggregateNodeBase : ExprNodeBase,
        ExprEvaluator,
        ExprAggregateNode,
        ExprForgeInstrumentable
    {
        protected int column = -1;
        private AggregationForgeFactory aggregationForgeFactory;
        protected ExprAggregateLocalGroupByDesc optionalAggregateLocalGroupByDesc;
        protected ExprNode optionalFilter;
        protected ExprNode[] positionalParams;
        protected CodegenFieldName aggregationResultFutureMemberName;

        /// <summary>
        /// Indicator for whether the aggregation is distinct - i.e. only unique values are considered.
        /// </summary>
        protected bool isDistinct;

        /// <summary>
        /// Returns the aggregation function name for representation in a generate expression string.
        /// </summary>
        /// <value>aggregation function name</value>
        public abstract string AggregationFunctionName { get; }

        public abstract bool IsFilterExpressionAsLastParameter { get; }

        /// <summary>
        /// Return true if a expression aggregate node semantically equals the current node, or false if not.
        /// <para />For use by the equalsNode implementation which compares the distinct flag.
        /// </summary>
        /// <param name="node">to compare to</param>
        /// <returns>true if semantically equal, or false if not equals</returns>
        public abstract bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node);

        /// <summary>
        /// Gives the aggregation node a chance to validate the sub-expression types.
        /// </summary>
        /// <param name="validationContext">validation information</param>
        /// <returns>aggregation function factory to use</returns>
        /// <throws>ExprValidationException when expression validation failed</throws>
        public abstract AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext);

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="distinct">sets the flag indicatating whether only unique values should be aggregated</param>
        protected ExprAggregateNodeBase(bool distinct)
        {
            isDistinct = distinct;
        }

        public ExprNode[] PositionalParams => positionalParams;

        public ExprEvaluator ExprEvaluator => this;

        public bool IsConstantResult => false;

        public ExprNode ForgeRenderable => this;
        
        public ExprNodeRenderable EnumForgeRenderable => ForgeRenderable;

        public ExprNodeRenderable ExprForgeRenderable => ForgeRenderable;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            ValidatePositionals(validationContext);
            aggregationForgeFactory = ValidateAggregationChild(validationContext);
            if (!validationContext.IsAggregationFutureNameAlreadySet) {
                aggregationResultFutureMemberName = validationContext.MemberNames.AggregationResultFutureRef();
            }
            else {
                if (aggregationResultFutureMemberName == null) {
                    throw new ExprValidationException("Aggregation future not set");
                }
            }

            return null;
        }

        public void ValidatePositionals(ExprValidationContext validationContext)
        {
            var paramDesc =
                ExprAggregateNodeUtil.GetValidatePositionalParams(ChildNodes, true);
            if (validationContext.StatementRawInfo.StatementType == StatementType.CREATE_TABLE &&
                (paramDesc.OptLocalGroupBy != null || paramDesc.OptionalFilter != null)) {
                throw new ExprValidationException(
                    "The 'group_by' and 'filter' parameter is not allowed in create-table statements");
            }

            optionalAggregateLocalGroupByDesc = paramDesc.OptLocalGroupBy;
            optionalFilter = paramDesc.OptionalFilter;
            if (optionalAggregateLocalGroupByDesc != null) {
                ExprNodeUtilityValidate.ValidateNoSpecialsGroupByExpressions(
                    optionalAggregateLocalGroupByDesc.PartitionExpressions);
            }

            if (optionalFilter != null) {
                ExprNodeUtilityValidate.ValidateNoSpecialsGroupByExpressions(new ExprNode[] { optionalFilter });
            }

            if (optionalFilter != null && IsFilterExpressionAsLastParameter) {
                if (paramDesc.PositionalParams.Length > 1) {
                    throw new ExprValidationException("Only a single filter expression can be provided");
                }

                positionalParams = ExprNodeUtilityMake.AddExpression(paramDesc.PositionalParams, optionalFilter);
            }
            else {
                positionalParams = paramDesc.PositionalParams;
            }
        }

        /// <summary>
        /// Returns the aggregation state factory for use in grouping aggregation states per group-by keys.
        /// </summary>
        /// <value>prototype aggregation state as a factory for aggregation states per group-by key value</value>
        public AggregationForgeFactory Factory {
            get {
                if (aggregationForgeFactory == null) {
                    throw new IllegalStateException("Aggregation method has not been set");
                }

                return aggregationForgeFactory;
            }
        }

        public object Evaluate(
            EventBean[] events,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var future = GetAggFuture(codegenClassScope);
            var eval = ExprDotMethod(
                future,
                "GetValue",
                Constant(column),
                ExprDotName(exprSymbol.GetAddExprEvalCtx(parent), "AgentInstanceId"),
                exprSymbol.GetAddEps(parent),
                exprSymbol.GetAddIsNewData(parent),
                exprSymbol.GetAddExprEvalCtx(parent));
            if (requiredType == typeof(object)) {
                return eval;
            }

            return CodegenLegoCast.CastSafeFromObjectType(EvaluationType, eval);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(),
                this,
                "ExprAggValue",
                requiredType,
                parent,
                exprSymbol,
                codegenClassScope).Build();
        }

        public Type EvaluationType {
            get {
                if (aggregationForgeFactory == null) {
                    throw new IllegalStateException("Aggregation method has not been set");
                }

                var resultType = aggregationForgeFactory.ResultType;
                return resultType;
            }
        }

        public override ExprForge Forge => this;

        /// <summary>
        /// Returns true if the aggregation node is only aggregatig distinct values, or false if
        /// aggregating all values.
        /// </summary>
        /// <value>true if 'distinct' keyword was given, false if not</value>
        public bool IsDistinct => isDistinct;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprAggregateNode other)) {
                return false;
            }

            if (other.IsDistinct != isDistinct) {
                return false;
            }

            return EqualsNodeAggregateMethodOnly(other);
        }

        public int Column {
            get => column;
            set => column = value;
        }

        protected Type ValidateNumericChildAllowFilter(bool hasFilter)
        {
            if (positionalParams.Length == 0 || positionalParams.Length > 2) {
                throw MakeExceptionExpectedParamNum(1, 2);
            }

            // validate child expression (filter expression is actually always the first expression)
            var child = positionalParams[0];
            if (hasFilter) {
                ValidateFilter(positionalParams[1]);
            }

            var childType = child.Forge.EvaluationType;
            ExprNodeUtilityValidate.ValidateReturnsNumeric(
                child.Forge,
                () =>
                    "Implicit conversion from datatype '" +
                    childType.CleanName() +
                    "' to numeric is not allowed for aggregation function '" +
                    AggregationFunctionName +
                    "'");

            return childType;
        }

        protected ExprValidationException MakeExceptionExpectedParamNum(
            int lower,
            int upper)
        {
            var message = "The '" + AggregationFunctionName + "' function expects ";
            if (lower == 0 && upper == 0) {
                message += "no parameters";
            }
            else if (lower == upper) {
                message += lower + " parameters";
            }
            else {
                message += "at least " + lower + " and up to " + upper + " parameters";
            }

            return new ExprValidationException(message);
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(AggregationFunctionName);
            writer.Write('(');

            if (isDistinct) {
                writer.Write("distinct ");
            }

            if (ChildNodes.Length > 0) {
                ChildNodes[0].ToEPL(writer, Precedence, flags);

                var delimiter = ",";
                for (var i = 1; i < ChildNodes.Length; i++) {
                    writer.Write(delimiter);
                    delimiter = ",";
                    ChildNodes[i].ToEPL(writer, Precedence, flags);
                }
            }
            else {
                if (IsExprTextWildcardWhenNoParams) {
                    writer.Write('*');
                }
            }

            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.MINIMUM;

        public void ValidateFilter(ExprNode filterEvaluator)
        {
            if (!TypeHelper.IsTypeBoolean(filterEvaluator.Forge.EvaluationType)) {
                throw new ExprValidationException(
                    "Invalid filter expression parameter to the aggregation function '" +
                    AggregationFunctionName +
                    "' is expected to return a boolean value but returns " +
                    filterEvaluator.Forge.EvaluationType.CleanName());
            }
        }

        public ExprAggregateLocalGroupByDesc OptionalLocalGroupBy => optionalAggregateLocalGroupByDesc;

        public ExprNode OptionalFilter => optionalFilter;

        protected virtual bool IsExprTextWildcardWhenNoParams => true;

        public CodegenExpression GetAggFuture(CodegenClassScope codegenClassScope)
        {
            var fieldExpression = codegenClassScope.NamespaceScope.AddOrGetDefaultFieldWellKnown(
                aggregationResultFutureMemberName,
                typeof(AggregationResultFuture));
            return fieldExpression;
        }
    }
} // end of namespace