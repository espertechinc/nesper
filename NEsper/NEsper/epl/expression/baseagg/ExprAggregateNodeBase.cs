///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.baseagg
{
    /// <summary>
    /// Base expression node that represents an aggregation function such as 'sum' or 'count'.
    /// <para>
    /// In terms of validation each concrete aggregation node must implement it's own validation.
    /// </para>
    /// <para>
    /// In terms of evaluation this base class will ask the assigned <seealso cref="com.espertech.esper.epl.agg.service.AggregationResultFuture" /> for the current state,
    /// using a column number assigned to the node.
    /// </para>
    /// <para>
    /// Concrete subclasses must supply an aggregation state prototype node <seealso cref="com.espertech.esper.epl.agg.aggregator.AggregationMethod" /> that reflects
    /// each group's (there may be group-by critera) current aggregation state.
    /// </para>
    /// </summary>
    [Serializable]
    public abstract class ExprAggregateNodeBase : ExprNodeBase
        , ExprEvaluator
        , ExprAggregateNode
    {
        [NonSerialized]
        private AggregationResultFuture _aggregationResultFuture;
        private int _column;
        [NonSerialized]
        private AggregationMethodFactory _aggregationMethodFactory;
        private ExprAggregateLocalGroupByDesc _optionalAggregateLocalGroupByDesc;
        private ExprNode _optionalFilter;
        private ExprNode[] _positionalParams;

        /// <summary>
        /// Indicator for whether the aggregation is distinct - i.e. only unique values are considered.
        /// </summary>
        private readonly bool _isDistinct;

        /// <summary>
        /// Returns the aggregation function name for representation in a generate expression string.
        /// </summary>
        /// <value>aggregation function name</value>
        public abstract string AggregationFunctionName { get; }

        protected abstract bool IsFilterExpressionAsLastParameter { get; }

        protected virtual int MaxPositionalParams => 1;

        public AggregationResultFuture AggregationResultFuture => _aggregationResultFuture;

        public int Column => _column;

        /// <summary>
        /// Return true if a expression aggregate node semantically equals the current node, or false if not.
        /// <para>
        /// For use by the equalsNode implementation which compares the distinct flag.
        /// </para>
        /// </summary>
        /// <param name="node">to compare to</param>
        /// <returns>true if semantically equal, or false if not equals</returns>
        protected abstract bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node);
    
        /// <summary>
        /// Gives the aggregation node a chance to validate the sub-expression types.
        /// </summary>
        /// <param name="validationContext">validation information</param>
        /// <exception cref="com.espertech.esper.epl.expression.core.ExprValidationException">when expression validation failed</exception>
        /// <returns>aggregation function factory to use</returns>
        protected abstract AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext);
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="distinct">- sets the flag indicatating whether only unique values should be aggregated</param>
        protected ExprAggregateNodeBase(bool distinct) {
            _isDistinct = distinct;
        }

        public ExprNode[] PositionalParams
        {
            get => _positionalParams;
            set => _positionalParams = value;
        }

        public override ExprEvaluator ExprEvaluator => this;

        public override bool IsConstantResult => false;

        public override ExprNode Validate(ExprValidationContext validationContext) {
            ValidatePositionals();
            _aggregationMethodFactory = ValidateAggregationChild(validationContext);
            if (validationContext.ExprEvaluatorContext.StatementType == StatementType.CREATE_TABLE &&
                    (_optionalAggregateLocalGroupByDesc != null || _optionalFilter != null)) {
                throw new ExprValidationException("The 'group_by' and 'filter' parameter is not allowed in create-table statements");
            }
            return null;
        }
    
        public void ValidatePositionals() {
            ExprAggregateNodeParamDesc paramDesc = ExprAggregateNodeUtil.GetValidatePositionalParams(ChildNodes, !(this is ExprAggregationPlugInNodeMarker));
            _optionalAggregateLocalGroupByDesc = paramDesc.OptLocalGroupBy;
            _optionalFilter = paramDesc.OptionalFilter;
            if (_optionalAggregateLocalGroupByDesc != null) {
                ExprNodeUtility.ValidateNoSpecialsGroupByExpressions(_optionalAggregateLocalGroupByDesc.PartitionExpressions);
            }
            if (_optionalFilter != null) {
                ExprNodeUtility.ValidateNoSpecialsGroupByExpressions(new ExprNode[] {_optionalFilter});
            }
            if (_optionalFilter != null && IsFilterExpressionAsLastParameter) {
                if (paramDesc.PositionalParams.Length > MaxPositionalParams) {
                    throw new ExprValidationException("Only a single filter expression can be provided");
                }
                _positionalParams = ExprNodeUtility.AddExpression(paramDesc.PositionalParams, _optionalFilter);
            } else {
                _positionalParams = paramDesc.PositionalParams;
            }
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
        /// <value>
        ///   prototype aggregation state as a factory for aggregation states per group-by key value
        /// </value>
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
        /// <param name="aggregationResultFuture">- future containing state</param>
        /// <param name="column">- column to hand to future for easy access</param>
        public void SetAggregationResultFuture(AggregationResultFuture aggregationResultFuture, int column) {
            _aggregationResultFuture = aggregationResultFuture;
            _column = column;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED)
            {
                object value = _aggregationResultFuture.GetValue(
                    _column, evaluateParams.ExprEvaluatorContext.AgentInstanceId, evaluateParams);
                InstrumentationHelper.Get().QaExprAggValue(this, value);
                return value;
            }

            return _aggregationResultFuture.GetValue(
                _column, evaluateParams.ExprEvaluatorContext.AgentInstanceId, evaluateParams);
        }

        /// <summary>
        /// Returns true if the aggregation node is only aggregatig distinct values, or false if
        /// aggregating all values.
        /// </summary>
        /// <value>true if 'distinct' keyword was given, false if not</value>
        public bool IsDistinct => _isDistinct;

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (node is ExprAggregateNode other)
            {
                return other.IsDistinct == _isDistinct && EqualsNodeAggregateMethodOnly(other);
            }

            return false;
        }
    
        /// <summary>
        /// For use by implementing classes, validates the aggregation node expecting
        /// a single numeric-type child node.
        /// </summary>
        /// <param name="hasFilter">for filter indication</param>
        /// <exception cref="com.espertech.esper.epl.expression.core.ExprValidationException">if the validation failed</exception>
        /// <returns>numeric type of single child</returns>
        protected Type ValidateNumericChildAllowFilter(bool hasFilter)
        {
            if (_positionalParams.Length == 0 || _positionalParams.Length > 2) {
                throw MakeExceptionExpectedParamNum(1, 2);
            }
    
            // validate child expression (filter expression is actually always the first expression)
            ExprNode child = _positionalParams[0];
            if (hasFilter) {
                ValidateFilter(_positionalParams[1].ExprEvaluator);
            }
    
            Type childType = child.ExprEvaluator.ReturnType;
            if (!childType.IsNumeric()) {
                throw new ExprValidationException("Implicit conversion from datatype '" +
                        (childType == null ? "null" : childType.Name) +
                        "' to numeric is not allowed for aggregation function '" + AggregationFunctionName + "'");
            }
    
            return childType;
        }
    
        protected ExprValidationException MakeExceptionExpectedParamNum(int lower, int upper) {
            string message = "The '" + AggregationFunctionName + "' function expects ";
            if (lower == 0 && upper == 0) {
                message += "no parameters";
            } else if (lower == upper) {
                message += lower + " parameters";
            } else {
                message += "at least " + lower + " and up to " + upper + " parameters";
            }
            return new ExprValidationException(message);
        }
    
        public override void ToPrecedenceFreeEPL(TextWriter writer) {
            writer.Write(AggregationFunctionName);
            writer.Write('(');
    
            if (_isDistinct) {
                writer.Write("distinct ");
            }
    
            if (ChildNodes.Count > 0) {
                ChildNodes[0].ToEPL(writer, Precedence);
    
                string delimiter = ",";
                for (int i = 1; i < ChildNodes.Count; i++) {
                    writer.Write(delimiter);
                    delimiter = ",";
                    ChildNodes[i].ToEPL(writer, Precedence);
                }
            } else {
                if (IsExprTextWildcardWhenNoParams) {
                    writer.Write('*');
                }
            }
    
            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.MINIMUM;

        public void ValidateFilter(ExprEvaluator filterEvaluator) {
            if (filterEvaluator.ReturnType.GetBoxedType() != typeof(bool?))
            {
                throw new ExprValidationException(
                    "Invalid filter expression parameter to the aggregation function '" +
                    AggregationFunctionName +
                    "' is expected to return a bool value but returns " +
                    filterEvaluator.ReturnType.GetCleanName());
            }
        }

        public ExprAggregateLocalGroupByDesc OptionalLocalGroupBy => _optionalAggregateLocalGroupByDesc;

        public ExprNode OptionalFilter => _optionalFilter;

        protected virtual bool IsExprTextWildcardWhenNoParams => true;
    }
} // end of namespace
