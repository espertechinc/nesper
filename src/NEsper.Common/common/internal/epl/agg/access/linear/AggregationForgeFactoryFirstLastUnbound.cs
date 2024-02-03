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
using com.espertech.esper.common.@internal.epl.agg.method.firstlastever;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationForgeFactoryFirstLastUnbound : AggregationForgeFactoryBase
    {
        private readonly ExprAggMultiFunctionLinearAccessNode parent;
        private readonly Type resultType;
        private readonly bool hasFilter;
        private readonly DataInputOutputSerdeForge serde;
        private AggregatorMethod aggregator;

        public override Type ResultType => resultType;

        public AggregationForgeFactoryFirstLastUnbound(
            ExprAggMultiFunctionLinearAccessNode parent,
            Type resultType,
            bool hasFilter,
            DataInputOutputSerdeForge serde)
        {
            this.parent = parent;
            this.resultType = resultType;
            this.hasFilter = hasFilter;
            this.serde = serde;

            if (parent.StateType == AggregationAccessorLinearType.FIRST) {
                aggregator = new AggregatorFirstEver(null, null, hasFilter, parent.OptionalFilter, resultType, serde);
            }
            else if (parent.StateType == AggregationAccessorLinearType.LAST) {
                aggregator = new AggregatorLastEver(null, null, hasFilter, parent.OptionalFilter, resultType, serde);
            }
            else {
                throw new EPRuntimeException("Window aggregation function is not available");
            }
        }

        public override AggregatorMethod Aggregator => aggregator;

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation =>
            throw new UnsupportedOperationException(
                "Not available as linear-access first/last is not used with tables");

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
        }
    }
} // end of namespace