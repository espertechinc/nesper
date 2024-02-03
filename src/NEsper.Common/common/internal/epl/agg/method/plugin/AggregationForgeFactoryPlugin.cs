///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat;


namespace com.espertech.esper.common.@internal.epl.agg.method.plugin
{
    public class AggregationForgeFactoryPlugin : AggregationForgeFactoryBase
    {
        protected readonly ExprPlugInAggNode parent;
        protected readonly AggregationFunctionForge aggregationFunctionForge;
        private readonly AggregationFunctionMode mode;
        private readonly Type aggregatedValueType;
        private readonly DataInputOutputSerdeForge distinctSerde;
        private readonly AggregatorMethod aggregator;

        public AggregationForgeFactoryPlugin(
            ExprPlugInAggNode parent,
            AggregationFunctionForge aggregationFunctionForge,
            AggregationFunctionMode mode,
            Type aggregatedValueType,
            DataInputOutputSerdeForge distinctSerde)
        {
            this.parent = parent;
            this.aggregationFunctionForge = aggregationFunctionForge;
            this.mode = mode;
            this.aggregatedValueType = aggregatedValueType;
            this.distinctSerde = distinctSerde;
            if (mode is AggregationFunctionModeManaged singleValue) {
                if (parent.PositionalParams.Length == 0) {
                    throw new ArgumentException(
                        nameof(AggregationFunctionModeManaged) + " requires at least one positional parameter");
                }

                var distinctType = !parent.IsDistinct ? null : aggregatedValueType;
                aggregator = new AggregatorPlugInManaged(
                    distinctType,
                    distinctSerde,
                    parent.ChildNodes.Length > 1,
                    parent.OptionalFilter,
                    singleValue);
            }
            else if (mode is AggregationFunctionModeMultiParam multiParam) {
                aggregator = new AggregatorPlugInMultiParam(multiParam);
            }
            else if (mode is AggregationFunctionModeCodeGenerated codeGenerated) {
                aggregator =
                    codeGenerated.AggregatorMethodFactory.GetAggregatorMethod(new AggregatorMethodFactoryContext(this));
            }
            else {
                throw new IllegalStateException("Received an unrecognized value for mode, the value is " + mode);
            }
        }

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
        }

        public override Type ResultType => aggregationFunctionForge.ValueType;

        public override AggregatorMethod Aggregator => aggregator;

        public override AggregationPortableValidation AggregationPortableValidation =>
            new AggregationPortableValidationPlugin(parent.IsDistinct, parent.AggregationFunctionName);

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public AggregationFunctionForge AggregationFunctionForge => aggregationFunctionForge;

        public ExprPlugInAggNode Parent => parent;

        public AggregationFunctionMode Mode => mode;

        public Type AggregatedValueType => aggregatedValueType;

        public DataInputOutputSerdeForge DistinctSerde => distinctSerde;
    }
} // end of namespace