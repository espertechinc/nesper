///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.baseagg
{
    /// <summary>
    /// Base expression node that represents an aggregation function such as 'sum' or 'count'.
    /// <para/>
    /// In terms of validation each concrete aggregation node must implement it's own validation.
    /// <para/>
    /// In terms of evaluation this base class will ask the assigned <seealso cref="agg.service.AggregationResultFuture"/> for the current state,
    /// using a column number assigned to the node.
    /// <para/>
    /// Concrete subclasses must supply an aggregation state Prototype node <seealso cref="AggregationMethod"/> that reflects
    /// each group's (there may be group-by critera) current aggregation state.
    /// </summary>
	[Serializable]
    public abstract class ExprAggregateNodeBase 
        : ExprNodeBase 
        , ExprEvaluator
        , ExprAggregateNode
	{
	    [NonSerialized] protected AggregationResultFuture AggregationResultFuture;
		protected int Column;
	    [NonSerialized] private AggregationMethodFactory _aggregationMethodFactory;

	    /// <summary>
	    /// Indicator for whether the aggregation is distinct - i.e. only unique values are considered.
	    /// </summary>
	    private bool _isDistinct;

        private ExprAggregateLocalGroupByDesc _optionalAggregateLocalGroupByDesc;
        private ExprNode[] _positionalParams;

        /// <summary>
	    /// Returns the aggregation function name for representation in a generate expression string.
	    /// </summary>
	    /// <value>aggregation function name</value>
	    public abstract string AggregationFunctionName { get; }

	    /// <summary>
	    /// Return true if a expression aggregate node semantically equals the current node, or false if not.
	    /// <para />
	    /// For use by the equalsNode implementation which compares the distinct flag.
	    /// </summary>
	    /// <param name="node">to compare to</param>
	    /// <returns>true if semantically equal, or false if not equals</returns>
	    protected abstract bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node);

        /// <summary>
        /// Gives the aggregation node a chance to validate the sub-expression types.
        /// </summary>
        /// <param name="validationContext">validation information</param>
        /// <returns>aggregation function factory to use</returns>
        /// <throws>com.espertech.esper.epl.expression.core.ExprValidationException when expression validation failed</throws>
        public abstract AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext);

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="distinct">sets the flag indicatating whether only unique values should be aggregated</param>
	    protected ExprAggregateNodeBase(bool distinct)
	    {
	        _isDistinct = distinct;
	    }

        /// <summary>
        /// Gets the positional parameters.
        /// </summary>
        /// <value>
        /// The positional parameters.
        /// </value>
        public ExprNode[] PositionalParams
        {
            get { return _positionalParams; }
            private set { _positionalParams = value; }
        }

        public override ExprEvaluator ExprEvaluator
	    {
	        get { return this; }
	    }

	    public override bool IsConstantResult
	    {
	        get { return false; }
	    }

	    public override ExprNode Validate(ExprValidationContext validationContext)
	    {
            ValidatePositionals();
            _aggregationMethodFactory = ValidateAggregationChild(validationContext);
            if (validationContext.ExprEvaluatorContext.StatementType == StatementType.CREATE_TABLE && _optionalAggregateLocalGroupByDesc != null)
            {
                throw new ExprValidationException("The 'group_by' parameter is not allowed in create-table statements");
            }
            return null;
        }

        public void ValidatePositionals()
        {
            ExprAggregateNodeParamDesc paramDesc = ExprAggregateNodeUtil.GetValidatePositionalParams(ChildNodes, !(this is ExprAggregationPlugInNodeMarker));
            _optionalAggregateLocalGroupByDesc = paramDesc.OptLocalGroupBy;
            if (_optionalAggregateLocalGroupByDesc != null) {
                ExprNodeUtility.ValidateNoSpecialsGroupByExpressions(_optionalAggregateLocalGroupByDesc.PartitionExpressions);
            }

            _positionalParams = paramDesc.PositionalParams;
        }

        public virtual Type ReturnType
        {
            get
            {
                if (_aggregationMethodFactory == null)
                {
                    throw new IllegalStateException("Aggregation method has not been set");
                }
                return _aggregationMethodFactory.ResultType;
            }
        }

        /// <summary>
	    /// Returns the aggregation state factory for use in grouping aggregation states per group-by keys.
	    /// </summary>
	    /// <value>Prototype aggregation state as a factory for aggregation states per group-by key value</value>
	    public AggregationMethodFactory Factory
	    {
	        get
	        {
	            if (_aggregationMethodFactory == null)
	            {
	                throw new IllegalStateException("Aggregation method has not been set");
	            }
	            return _aggregationMethodFactory;
	        }
	    }

	    /// <summary>
	    /// Assigns to the node the future which can be queried for the current aggregation state at evaluation time.
	    /// </summary>
	    /// <param name="aggregationResultFuture">future containing state</param>
	    /// <param name="column">column to hand to future for easy access</param>
		public void SetAggregationResultFuture(AggregationResultFuture aggregationResultFuture, int column)
	    {
	        AggregationResultFuture = aggregationResultFuture;
	        Column = column;
	    }

        public virtual object Evaluate(EvaluateParams evaluateParams)
        {
            var events = evaluateParams.EventsPerStream;
            var isNewData = evaluateParams.IsNewData;
            var exprEvaluatorContext = evaluateParams.ExprEvaluatorContext;

	        if (InstrumentationHelper.ENABLED) {
	            object value = AggregationResultFuture.GetValue(Column, exprEvaluatorContext.AgentInstanceId, events, isNewData, exprEvaluatorContext);
	            InstrumentationHelper.Get().QaExprAggValue(this, value);
	            return value;
	        }
	        return AggregationResultFuture.GetValue(Column, exprEvaluatorContext.AgentInstanceId, events, isNewData, exprEvaluatorContext);
		}

	    /// <summary>
	    /// Returns true if the aggregation node is only aggregatig distinct values, or false if
	    /// aggregating all values.
	    /// </summary>
	    /// <value>true if 'distinct' keyword was given, false if not</value>
	    public bool IsDistinct
	    {
	        get { return _isDistinct; }
	    }

	    public override bool EqualsNode(ExprNode node)
	    {
	        var other = node as ExprAggregateNode;
	        if (other == null)
	        {
	            return false;
	        }

	        return other.IsDistinct == this._isDistinct && this.EqualsNodeAggregateMethodOnly(other);
	    }

        /// <summary>
        /// For use by implementing classes, validates the aggregation node expecting
        /// a single numeric-type child node.
        /// </summary>
        /// <param name="hasFilter">if set to <c>true</c> [has filter].</param>
        /// <returns>
        /// numeric type of single child
        /// </returns>
	    protected Type ValidateNumericChildAllowFilter(bool hasFilter)
	    {
            if (_positionalParams.Length == 0 || _positionalParams.Length > 2)
            {
                throw MakeExceptionExpectedParamNum(1, 2);
            }

            // validate child expression (filter expression is actually always the first expression)
            var child = _positionalParams[0];
            if (hasFilter)
            {
                ValidateFilter(_positionalParams[1].ExprEvaluator);
            }

            var childType = child.ExprEvaluator.ReturnType;
            if (childType.IsNumeric() == false)
            {
                throw new ExprValidationException(string.Format(
                    "Implicit conversion from datatype '{0}' to numeric is not allowed for aggregation function '{1}'",
                    childType == null ? "null" : childType.Name,
                    AggregationFunctionName));
            }

            return childType;
        }

        protected ExprValidationException MakeExceptionExpectedParamNum(int lower, int upper)
        {
            var message = "The '" + AggregationFunctionName + "' function expects ";
            if (lower == 0 && upper == 0)
            {
                message += "no parameters";
            }
            else if (lower == upper)
            {
                message += lower + " parameters";
            }
            else
            {
                message += "at least " + lower + " and up to " + upper + " parameters";
            }
            return new ExprValidationException(message);
        }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
	        writer.Write(AggregationFunctionName);
	        writer.Write('(');

	        if (_isDistinct) {
	            writer.Write("distinct ");
	        }

	        if (this.ChildNodes.Length > 0) {
	            this.ChildNodes[0].ToEPL(writer, Precedence);

	            string delimiter = ",";
	            for (int i = 1 ; i < this.ChildNodes.Length; i++) {
	                writer.Write(delimiter);
	                delimiter = ",";
	                this.ChildNodes[i].ToEPL(writer, Precedence);
	            }
	        }
	        else {
	            if (IsExprTextWildcardWhenNoParams) {
	                writer.Write('*');
	            }
	        }

	        writer.Write(')');
	    }

	    public override ExprPrecedenceEnum Precedence
	    {
	        get { return ExprPrecedenceEnum.MINIMUM; }
	    }

	    public void ValidateFilter(ExprEvaluator filterEvaluator)
        {
	        if (filterEvaluator.ReturnType.GetBoxedType() != typeof(bool?))
            {
	            throw new ExprValidationException("Invalid filter expression parameter to the aggregation function '" +
	                    AggregationFunctionName +
	                    "' is expected to return a boolean value but returns " + TypeHelper.GetTypeNameFullyQualPretty(filterEvaluator.ReturnType));
	        }
	    }

        public ExprAggregateLocalGroupByDesc OptionalLocalGroupBy
        {
            get { return _optionalAggregateLocalGroupByDesc; }
            internal set { _optionalAggregateLocalGroupByDesc = value; }
        }

        protected virtual bool IsExprTextWildcardWhenNoParams
	    {
	        get { return true; }
	    }
	}
} // end of namespace
