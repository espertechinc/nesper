///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.minmax;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.agg.method
{
    /// <summary>
    ///     Represents the min/max(distinct? ...) aggregate function is an expression tree.
    /// </summary>
    public class ExprMinMaxAggrNode : ExprAggregateNodeBase
    {
        private readonly bool isFFunc;

        public ExprMinMaxAggrNode(
            bool distinct,
            MinMaxTypeEnum minMaxTypeEnum,
            bool isFFunc,
            bool isEver)
            : base(distinct)
        {
            MinMaxTypeEnum = minMaxTypeEnum;
            this.isFFunc = isFFunc;
            IsEver = isEver;
        }

        /// <summary>
        ///     Returns the indicator for minimum or maximum.
        /// </summary>
        /// <returns>min/max indicator</returns>
        public MinMaxTypeEnum MinMaxTypeEnum { get; }

        public bool HasFilter { get; private set; }

        public override string AggregationFunctionName => MinMaxTypeEnum.GetExpressionText();

        public bool IsEver { get; }

        public override bool IsFilterExpressionAsLastParameter => true;

        public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            if (positionalParams.Length == 0 || positionalParams.Length > 2) {
                throw new ExprValidationException(MinMaxTypeEnum + " node must have either 1 or 2 parameters");
            }

            var child = positionalParams[0];
            bool hasDataWindows;
            if (IsEver) {
                hasDataWindows = false;
            }
            else {
                if (validationContext.StatementType == StatementType.CREATE_TABLE) {
                    hasDataWindows = true;
                }
                else {
                    hasDataWindows = ExprNodeUtilityAggregation.HasRemoveStreamForAggregations(
                        child,
                        validationContext.StreamTypeService,
                        validationContext.IsResettingAggregations);
                }
            }

            if (isFFunc) {
                if (positionalParams.Length < 2) {
                    throw new ExprValidationException(
                        MinMaxTypeEnum +
                        "-filtered aggregation function must have a filter expression as a second parameter");
                }

                ValidateFilter(positionalParams[1]);
            }

            HasFilter = positionalParams.Length == 2;
            if (HasFilter) {
                optionalFilter = positionalParams[1];
            }

            var evaluationType = child.Forge.EvaluationType;
            var serde = validationContext.SerdeResolver.SerdeForAggregation(
                evaluationType,
                validationContext.StatementRawInfo);
            var distinctSerde = isDistinct
                ? validationContext.SerdeResolver.SerdeForAggregationDistinct(
                    evaluationType,
                    validationContext.StatementRawInfo)
                : null;
            return new AggregationForgeFactoryMinMax(this, evaluationType, hasDataWindows, serde, distinctSerde);
        }

        public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            if (node is ExprMinMaxAggrNode other) {
                return other.MinMaxTypeEnum == MinMaxTypeEnum && other.IsEver == IsEver;
            }

            return false;
        }
    }
} // end of namespace