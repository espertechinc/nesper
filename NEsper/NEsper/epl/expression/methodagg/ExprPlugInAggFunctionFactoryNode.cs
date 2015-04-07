///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.hook;
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
    public class ExprPlugInAggFunctionFactoryNode
        : ExprAggregateNodeBase
        , ExprAggregationPlugInNodeMarker
    {
        private AggregationFunctionFactory _aggregationFunctionFactory;
        private readonly String _functionName;

        /// <summary>Ctor. </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        /// <param name="aggregationFunctionFactory">is the base class for plug-in aggregation functions</param>
        /// <param name="functionName">is the aggregation function name</param>
        public ExprPlugInAggFunctionFactoryNode(bool distinct, AggregationFunctionFactory aggregationFunctionFactory, String functionName)
            : base(distinct)
        {
            _aggregationFunctionFactory = aggregationFunctionFactory;
            _functionName = functionName;
            aggregationFunctionFactory.FunctionName = functionName;
        }

        public override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            var positionalParams = PositionalParams;
            var parameterTypes = new Type[positionalParams.Length];
            var constant = new Object[positionalParams.Length];
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

                if (!ExprNodeUtility.HasRemoveStreamForAggregations(child, validationContext.StreamTypeService, validationContext.IsResettingAggregations))
                {
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
    
            var context = new AggregationValidationContext(parameterTypes, isConstant, constant, base.IsDistinct, hasDataWindows, expressions);
            try
            {
                // the aggregation function factory is transient, obtain if not provided
                if (_aggregationFunctionFactory == null) {
                    _aggregationFunctionFactory = validationContext.MethodResolutionService.EngineImportService.ResolveAggregationFactory(_functionName);
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
    
            return new ExprPlugInAggFunctionFactory(this, _aggregationFunctionFactory, childType);
        }

        public override string AggregationFunctionName
        {
            get { return _functionName; }
        }

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            var other = node as ExprPlugInAggFunctionFactoryNode;
            if (other == null)
                return false;

            return other.AggregationFunctionName == AggregationFunctionName;
        }
    }
}
