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


namespace com.espertech.esper.common.@internal.epl.agg.method.firstlastever
{
    public class AggregationForgeFactoryFirstLastEver : AggregationForgeFactoryBase
    {
        protected readonly ExprFirstLastEverNode parent;
        protected readonly Type childType;
        protected readonly DataInputOutputSerdeForge serde;
        private readonly AggregatorMethod aggregator;

        public AggregationForgeFactoryFirstLastEver(
            ExprFirstLastEverNode parent,
            Type childType,
            DataInputOutputSerdeForge serde)
        {
            this.parent = parent;
            this.childType = childType;
            this.serde = serde;
            if (parent.IsFirst) {
                aggregator = new AggregatorFirstEver(
                    null,
                    null,
                    parent.HasFilter,
                    parent.OptionalFilter,
                    childType,
                    serde);
            }
            else {
                aggregator = new AggregatorLastEver(
                    null,
                    null,
                    parent.HasFilter,
                    parent.OptionalFilter,
                    childType,
                    serde);
            }
        }

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
        }

        public override Type ResultType => childType;

        public override AggregatorMethod Aggregator => aggregator;

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation =>
            new AggregationPortableValidationFirstLastEver(
                parent.IsDistinct,
                parent.HasFilter,
                childType,
                parent.IsFirst);
    }
} // end of namespace