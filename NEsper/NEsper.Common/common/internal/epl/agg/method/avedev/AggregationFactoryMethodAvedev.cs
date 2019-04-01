///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.method.avedev
{
    public class AggregationFactoryMethodAvedev : AggregationFactoryMethodBase
    {
        internal readonly Type aggregatedValueType;
        internal readonly ExprAvedevNode parent;
        internal readonly ExprNode[] positionalParameters;
        private AggregatorMethod aggregator;

        public AggregationFactoryMethodAvedev(
            ExprAvedevNode parent, Type aggregatedValueType, ExprNode[] positionalParameters)
        {
            this.parent = parent;
            this.aggregatedValueType = aggregatedValueType;
            this.positionalParameters = positionalParameters;
        }

        public override Type ResultType => typeof(double?);

        public override AggregatorMethod Aggregator => aggregator;

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation =>
            new AggregationPortableValidationAvedev(parent.IsDistinct, parent.HasFilter, aggregatedValueType);

        public override void InitMethodForge(
            int col, CodegenCtor rowCtor, CodegenMemberCol membersColumnized, CodegenClassScope classScope)
        {
            var distinctType = !parent.IsDistinct ? null : aggregatedValueType;
            aggregator = new AggregatorAvedev(
                this, col, rowCtor, membersColumnized, classScope, distinctType, parent.HasFilter,
                parent.OptionalFilter);
        }

        public override ExprForge[] GetMethodAggregationForge(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
        }
    }
} // end of namespace