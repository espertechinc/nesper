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


namespace com.espertech.esper.common.@internal.epl.agg.method.minmax
{
    public class AggregationForgeFactoryMinMax : AggregationForgeFactoryBase
    {
        private readonly ExprMinMaxAggrNode _parent;
        private readonly Type _resultType;
        private readonly bool _hasDataWindows;
        private readonly DataInputOutputSerdeForge _serde;
        private readonly DataInputOutputSerdeForge _distinctSerde;
        private readonly AggregatorMethod aggregator;

        public DataInputOutputSerdeForge Serde => _serde;

        public DataInputOutputSerdeForge DistinctSerde => _distinctSerde;

        public AggregationForgeFactoryMinMax(
            ExprMinMaxAggrNode parent,
            Type resultType,
            bool hasDataWindows,
            DataInputOutputSerdeForge serde,
            DataInputOutputSerdeForge distinctSerde)
        {
            _parent = parent;
            _resultType = resultType;
            _hasDataWindows = hasDataWindows;
            _serde = serde;
            _distinctSerde = distinctSerde;

            var distinctType = !parent.IsDistinct ? null : resultType;
            if (!hasDataWindows) {
                aggregator = new AggregatorMinMaxEver(
                    this,
                    distinctType,
                    distinctSerde,
                    parent.HasFilter,
                    parent.OptionalFilter,
                    serde);
            }
            else {
                aggregator = new AggregatorMinMax(
                    this,
                    distinctType,
                    distinctSerde,
                    parent.HasFilter,
                    parent.OptionalFilter);
            }
        }

        public override Type ResultType => _resultType;

        public override ExprAggregateNodeBase AggregationExpression => _parent;

        public override AggregatorMethod Aggregator => aggregator;

        public override AggregationPortableValidation AggregationPortableValidation =>
            new AggregationPortableValidationMinMax(
                _parent.IsDistinct,
                _parent.HasFilter,
                _parent.ChildNodes[0].Forge.EvaluationType,
                _parent.MinMaxTypeEnum,
                _hasDataWindows);

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(_parent.PositionalParams, join, typesPerStream);
        }

        public ExprMinMaxAggrNode Parent => _parent;
    }
} // end of namespace