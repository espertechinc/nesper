///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;


namespace com.espertech.esper.common.@internal.epl.agg.method.count
{
    public class AggregationForgeFactoryCount : AggregationForgeFactoryBase
    {
        protected readonly ExprCountNode parent;
        protected readonly bool ignoreNulls;
        protected readonly Type countedValueType;
        protected readonly DataInputOutputSerdeForge distinctValueSerde;
        private readonly AggregatorCount aggregator;

        public AggregationForgeFactoryCount(
            ExprCountNode parent,
            bool ignoreNulls,
            Type countedValueType,
            DataInputOutputSerdeForge distinctValueSerde)
        {
            this.parent = parent;
            this.ignoreNulls = ignoreNulls;
            this.countedValueType = countedValueType;
            this.distinctValueSerde = distinctValueSerde;
            var distinctType = !parent.IsDistinct ? null : countedValueType;
            aggregator = new AggregatorCount(
                distinctType,
                distinctValueSerde,
                parent.HasFilter,
                parent.OptionalFilter,
                false);
        }

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return GetMethodAggregationEvaluatorCountByForge(parent.PositionalParams, join, typesPerStream);
        }

        private static ExprForge[] GetMethodAggregationEvaluatorCountByForge(
            ExprNode[] childNodes,
            bool join,
            EventType[] typesPerStream)
        {
            if (childNodes[0] is ExprWildcard && childNodes.Length == 2) {
                return ExprMethodAggUtil.GetDefaultForges(new ExprNode[] { childNodes[1] }, join, typesPerStream);
            }

            if (childNodes[0] is ExprWildcard && childNodes.Length == 1) {
                return ExprNodeUtilityQuery.EMPTY_FORGE_ARRAY;
            }

            return ExprMethodAggUtil.GetDefaultForges(childNodes, join, typesPerStream);
        }

        public override Type ResultType => typeof(long?);

        public override AggregatorMethod Aggregator => aggregator;

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation => new AggregationPortableValidationCount(
            parent.IsDistinct,
            false,
            parent.IsDistinct,
            countedValueType,
            ignoreNulls);
    }
} // end of namespace