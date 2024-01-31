///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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


namespace com.espertech.esper.common.@internal.epl.agg.method.avedev
{
    public class AggregationForgeFactoryAvedev : AggregationForgeFactoryBase
    {
        protected readonly ExprAvedevNode parent;
        protected readonly Type aggregatedValueType;
        protected readonly DataInputOutputSerdeForge distinctSerde;
        protected readonly ExprNode[] positionalParameters;
        private readonly AggregatorMethod aggregator;

        public AggregationForgeFactoryAvedev(
            ExprAvedevNode parent,
            Type aggregatedValueType,
            DataInputOutputSerdeForge distinctSerde,
            ExprNode[] positionalParameters)
        {
            this.parent = parent;
            this.aggregatedValueType = aggregatedValueType;
            this.distinctSerde = distinctSerde;
            this.positionalParameters = positionalParameters;
            var distinctType = !parent.IsDistinct ? null : aggregatedValueType;
            aggregator = new AggregatorAvedev(distinctType, distinctSerde, parent.HasFilter, parent.OptionalFilter);
        }

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
        }

        public override Type ResultType => typeof(double?);

        public override AggregatorMethod Aggregator => aggregator;

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation => new AggregationPortableValidationAvedev(
            parent.IsDistinct,
            parent.HasFilter,
            aggregatedValueType);
    }
} // end of namespace