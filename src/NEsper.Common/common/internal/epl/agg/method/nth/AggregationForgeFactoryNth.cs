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


namespace com.espertech.esper.common.@internal.epl.agg.method.nth
{
    public class AggregationForgeFactoryNth : AggregationForgeFactoryBase
    {
        protected readonly ExprNthAggNode parent;
        protected readonly Type childType;
        protected readonly DataInputOutputSerdeForge serde;
        protected readonly DataInputOutputSerdeForge distinctSerde;
        protected readonly int size;
        protected readonly AggregatorNth aggregator;

        public DataInputOutputSerdeForge Serde => serde;

        public AggregationForgeFactoryNth(
            ExprNthAggNode parent,
            Type childType,
            DataInputOutputSerdeForge serde,
            DataInputOutputSerdeForge distinctSerde,
            int size)
        {
            this.parent = parent;
            this.childType = childType;
            this.serde = serde;
            this.distinctSerde = distinctSerde;
            this.size = size;
            var distinctValueType = !parent.IsDistinct ? null : childType;
            aggregator = new AggregatorNth(this, distinctValueType, distinctSerde, false, parent.OptionalFilter);
        }

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
        }

        public override Type ResultType => childType;

        public override AggregatorMethod Aggregator => aggregator;

        public ExprNthAggNode Parent => parent;

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation => new AggregationPortableValidationNth(
            parent.IsDistinct,
            parent.OptionalFilter != null,
            childType,
            size);

        public Type ChildType => childType;

        public int SizeOfBuf => size + 1;
    }
} // end of namespace