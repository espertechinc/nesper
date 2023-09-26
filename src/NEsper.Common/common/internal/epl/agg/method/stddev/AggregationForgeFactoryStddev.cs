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

namespace com.espertech.esper.common.@internal.epl.agg.method.stddev
{
    public class AggregationForgeFactoryStddev : AggregationForgeFactoryBase
    {
        private readonly Type _aggregatedValueType;
        private readonly DataInputOutputSerdeForge _distinctSerde;
        private readonly ExprStddevNode _parent;

        public AggregationForgeFactoryStddev(
            ExprStddevNode parent,
            Type aggregatedValueType,
            DataInputOutputSerdeForge distinctSerde)
        {
            _parent = parent;
            _aggregatedValueType = aggregatedValueType;
            _distinctSerde = distinctSerde;

            var distinctType = !parent.IsDistinct ? null : aggregatedValueType;
            Aggregator = new AggregatorStddev(distinctType, distinctSerde, parent.HasFilter, parent.OptionalFilter);
        }

        public DataInputOutputSerdeForge DistinctSerde => _distinctSerde;

        public override Type ResultType => typeof(double?);

        public override ExprAggregateNodeBase AggregationExpression => _parent;

        public override AggregatorMethod Aggregator { get; }

        public override AggregationPortableValidation AggregationPortableValidation =>
            new AggregationPortableValidationStddev(_parent.IsDistinct, _parent.HasFilter, _aggregatedValueType);

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(_parent.PositionalParams, join, typesPerStream);
        }
    }
} // end of namespace