///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

        public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            var parameterTypes = new Type[positionalParams.Length];
            var constant = new object[positionalParams.Length];
            var isConstant = new bool[positionalParams.Length];
            var expressions = new ExprNode[positionalParams.Length];

            var count = 0;
            var hasDataWindows = true;
            foreach (var child in positionalParams) {
                if (child.Forge.ForgeConstantType == ExprForgeConstantType.COMPILETIMECONST) {
                    isConstant[count] = true;
                    constant[count] = child.Forge.ExprEvaluator.Evaluate(null, true, null);
                }

                parameterTypes[count] = child.Forge.EvaluationType;
                expressions[count] = child;

                if (!ExprNodeUtilityAggregation.HasRemoveStreamForAggregations(
                        child,
                        validationContext.StreamTypeService,
                        validationContext.IsResettingAggregations)) {
                    hasDataWindows = false;
                }

                if (child is ExprWildcard && validationContext.StreamTypeService.EventTypes.Length > 0) {
                    ExprAggMultiFunctionUtil.CheckWildcardNotJoinOrSubquery(
                        validationContext.StreamTypeService,
                        functionName);
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

            var context = new AggregationFunctionValidationContext(
                parameterTypes,
                isConstant,
                constant,
                IsDistinct,
                hasDataWindows,
                expressions,
                namedParameters);
            try {
                // the aggregation function factory is transient, obtain if not provided
                if (aggregationFunctionForge == null) {
                    aggregationFunctionForge = validationContext.ImportService.ResolveAggregationFunction(
                        functionName,
                        validationContext.ClassProvidedExtension);
                }

                aggregationFunctionForge.Validate(context);
            }
            catch (Exception ex) {
                throw new ExprValidationException(
                    "Plug-in aggregation function '" + functionName + "' failed validation: " + ex.Message,
                    ex);
            }

            var mode = aggregationFunctionForge.AggregationFunctionMode;
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

            var aggregatedValueType = PositionalParams.Length == 0 ? null : PositionalParams[0].Forge.EvaluationType;
            var distinctForge = isDistinct
                ? validationContext.SerdeResolver.SerdeForAggregationDistinct(
                    aggregatedValueType,
                    validationContext.StatementRawInfo)
                : null;
            return new AggregationForgeFactoryPlugin(
                this,
                aggregationFunctionForge,
                mode,
                aggregatedValueType,
                distinctForge);
        }

        public override string AggregationFunctionName => functionName;

        public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            if (!(node is ExprPlugInAggNode other)) {
                return false;
            }

            return other.AggregationFunctionName.Equals(AggregationFunctionName);
        }

        public override bool IsFilterExpressionAsLastParameter => false;
    }
} // end of namespace