///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.methodagg
{
	/// <summary>
	/// Represents a custom aggregation function in an expresson tree.
	/// </summary>
    [Serializable]
    public class ExprPlugInAggNode 
        : ExprAggregateNodeBase 
        , ExprAggregationPlugInNodeMarker
	{
        [NonSerialized]
	    private AggregationFunctionFactory _aggregationFunctionFactory;
	    private readonly string _functionName;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
	    /// <param name="aggregationFunctionFactory">is the base class for plug-in aggregation functions</param>
	    /// <param name="functionName">is the aggregation function name</param>
	    public ExprPlugInAggNode(bool distinct, AggregationFunctionFactory aggregationFunctionFactory, string functionName)
	        : base(distinct)
	    {
	        _functionName = functionName;
	        _aggregationFunctionFactory = aggregationFunctionFactory;
	        _aggregationFunctionFactory.FunctionName = functionName;
	    }

	    protected override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
	    {
	        var positionalParams = PositionalParams;
	        var parameterTypes = new Type[positionalParams.Length];
            var constant = new object[positionalParams.Length];
            var isConstant = new bool[positionalParams.Length];
            var expressions = new ExprNode[positionalParams.Length];

	        var count = 0;
	        var hasDataWindows = true;
            var evaluateParams = new EvaluateParams(null, true, validationContext.ExprEvaluatorContext);

            foreach (var child in positionalParams)
	        {
	            if (child.IsConstantResult)
	            {
	                isConstant[count] = true;
	                constant[count] = child.ExprEvaluator.Evaluate(evaluateParams);
	            }
	            parameterTypes[count] = child.ExprEvaluator.ReturnType;
	            expressions[count] = child;

	            if (!ExprNodeUtility.HasRemoveStreamForAggregations(child, validationContext.StreamTypeService, validationContext.IsResettingAggregations)) {
	                hasDataWindows = false;
	            }

	            if (child is ExprWildcard) {
	                ExprAggMultiFunctionUtil.CheckWildcardNotJoinOrSubquery(validationContext.StreamTypeService, _functionName);
	                parameterTypes[count] = validationContext.StreamTypeService.EventTypes[0].UnderlyingType;
	                isConstant[count] = false;
	                constant[count] = null;
	            }

	            count++;
	        }

	        IDictionary<string, IList<ExprNode>> namedParameters = null;
	        if (OptionalFilter != null)
	        {
	            namedParameters = new Dictionary<string, IList<ExprNode>>();
	            namedParameters.Put("filter", Collections.SingletonList(OptionalFilter));
	            positionalParams = ExprNodeUtility.AddExpression(positionalParams, OptionalFilter);
	        }

            var context = new AggregationValidationContext(parameterTypes, isConstant, constant, base.IsDistinct, hasDataWindows, expressions, namedParameters);
	        try
	        {
	            // the aggregation function factory is transient, obtain if not provided
	            if (_aggregationFunctionFactory == null) {
	                _aggregationFunctionFactory = validationContext.EngineImportService.ResolveAggregationFactory(_functionName);
	            }

	            _aggregationFunctionFactory.Validate(context);
	        }
	        catch (Exception ex)
	        {
	            throw new ExprValidationException("Plug-in aggregation function '" + _functionName + "' failed validation: " + ex.Message, ex);
	        }

	        Type childType = null;
            if (positionalParams.Length > 0)
	        {
                childType = positionalParams[0].ExprEvaluator.ReturnType;
	        }

	        return validationContext.EngineImportService.AggregationFactoryFactory.MakePlugInMethod(validationContext.StatementExtensionSvcContext, this, _aggregationFunctionFactory, childType);
	    }

	    public override string AggregationFunctionName => _functionName;

	    protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
	    {
	        if (node is ExprPlugInAggNode other) {
	            return other.AggregationFunctionName == AggregationFunctionName;
	        }

	        return false;
	    }

	    protected override bool IsFilterExpressionAsLastParameter => true;
	}
} // end of namespace
