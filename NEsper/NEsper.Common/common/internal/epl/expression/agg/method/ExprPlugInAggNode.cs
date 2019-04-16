///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.plugin;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.expression.agg.method
{
    /// <summary>
    /// Represents a custom aggregation function in an expresson tree.
    /// </summary>
    public class ExprPlugInAggNode : ExprAggregateNodeBase,
        ExprPlugInAggNodeMarker
    {
        private AggregationFunctionForge aggregationFunctionForge;
        private readonly string functionName;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        /// <param name="aggregationFunctionForge">is the base class for plug-in aggregation functions</param>
        /// <param name="functionName">is the aggregation function name</param>
        public ExprPlugInAggNode(
            bool distinct,
            AggregationFunctionForge aggregationFunctionForge,
            string functionName)
            : base(distinct)
        {
            this.aggregationFunctionForge = aggregationFunctionForge;
            this.functionName = functionName;
            aggregationFunctionForge.FunctionName = functionName;
        }

        internal override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            Type[] parameterTypes = new Type[positionalParams.Length];
            object[] constant = new object[positionalParams.Length];
            bool[] isConstant = new bool[positionalParams.Length];
            ExprNode[] expressions = new ExprNode[positionalParams.Length];

            int count = 0;
            bool hasDataWindows = true;
            foreach (ExprNode child in positionalParams) {
                if (child.Forge.ForgeConstantType == ExprForgeConstantType.COMPILETIMECONST) {
                    isConstant[count] = true;
                    constant[count] = child.Forge.ExprEvaluator.Evaluate(null, true, null);
                }

                parameterTypes[count] = child.Forge.EvaluationType;
                expressions[count] = child;

                if (!ExprNodeUtilityAggregation.HasRemoveStreamForAggregations(
                    child, validationContext.StreamTypeService, validationContext.IsResettingAggregations)) {
                    hasDataWindows = false;
                }

                if (child is ExprWildcard) {
                    ExprAggMultiFunctionUtil.CheckWildcardNotJoinOrSubquery(
                        validationContext.StreamTypeService, functionName);
                    parameterTypes[count] = validationContext.StreamTypeService.EventTypes[0].UnderlyingType;
                    isConstant[count] = false;
                    constant[count] = null;
                }

                count++;
            }

            LinkedHashMap<string, IList<ExprNode>> namedParameters = null;
            if (optionalFilter != null) {
                namedParameters = new LinkedHashMap<string, IList<ExprNode>>();
                namedParameters.Put("filter", Collections.SingletonList(optionalFilter));
                positionalParams = ExprNodeUtilityMake.AddExpression(positionalParams, optionalFilter);
            }

            AggregationFunctionValidationContext context = new AggregationFunctionValidationContext(
                parameterTypes, isConstant, constant, base.IsDistinct, hasDataWindows, expressions, namedParameters);
            try {
                // the aggregation function factory is transient, obtain if not provided
                if (aggregationFunctionForge == null) {
                    aggregationFunctionForge =
                        validationContext.ImportService.ResolveAggregationFunction(functionName);
                }

                aggregationFunctionForge.Validate(context);
            }
            catch (Exception ex) {
                throw new ExprValidationException(
                    "Plug-in aggregation function '" + functionName + "' failed validation: " + ex.Message, ex);
            }

            AggregationFunctionMode mode = aggregationFunctionForge.AggregationFunctionMode;
            if (mode == null) {
                throw new ExprValidationException("Aggregation function forge returned a null value for mode");
            }

            if (mode is AggregationFunctionModeManaged) {
                if (positionalParams.Length > 2) {
                    throw new ExprValidationException(
                        "Aggregation function forge single-value mode requires zero, one or two parameters");
                }
            }
            else if (mode is AggregationFunctionModeMultiParam || mode is AggregationFunctionModeCodeGenerated) {
            }
            else {
                throw new ExprValidationException("Aggregation function forge returned an unrecognized mode " + mode);
            }

            return new AggregationMethodFactoryPluginMethod(this, aggregationFunctionForge, mode);
        }

        public override string AggregationFunctionName {
            get => functionName;
        }

        internal override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            if (!(node is ExprPlugInAggNode)) {
                return false;
            }

            ExprPlugInAggNode other = (ExprPlugInAggNode) node;
            return other.AggregationFunctionName.Equals(this.AggregationFunctionName);
        }

        internal override bool IsFilterExpressionAsLastParameter => false;
    }
} // end of namespace