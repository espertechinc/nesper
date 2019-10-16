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
using com.espertech.esper.common.@internal.epl.agg.method.firstlastever;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationFactoryMethodFirstLastUnbound : AggregationFactoryMethodBase
    {
        private readonly EventType collectionEventType;
        protected internal readonly bool hasFilter;
        protected internal readonly ExprAggMultiFunctionLinearAccessNode parent;
        private readonly int streamNum;
        private AggregatorMethod _aggregator;

        public AggregationFactoryMethodFirstLastUnbound(
            ExprAggMultiFunctionLinearAccessNode parent,
            EventType collectionEventType,
            Type resultType,
            int streamNum,
            bool hasFilter)
        {
            this.parent = parent;
            this.collectionEventType = collectionEventType;
            ResultType = resultType.GetBoxedType();
            this.streamNum = streamNum;
            this.hasFilter = hasFilter;
        }

        public override Type ResultType { get; }

        public override AggregatorMethod Aggregator {
            get => _aggregator;
        }

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation =>
            throw new UnsupportedOperationException(
                "Not available as linear-access first/last is not used with tables");

        public override void InitMethodForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            if (parent.StateType == AggregationAccessorLinearType.FIRST) {
                _aggregator = new AggregatorFirstEver(
                    this,
                    col,
                    rowCtor,
                    membersColumnized,
                    classScope,
                    null,
                    hasFilter,
                    parent.OptionalFilter,
                    ResultType);
            }
            else if (parent.StateType == AggregationAccessorLinearType.LAST) {
                _aggregator = new AggregatorLastEver(
                    this,
                    col,
                    rowCtor,
                    membersColumnized,
                    classScope,
                    null,
                    hasFilter,
                    parent.OptionalFilter,
                    ResultType);
            }
            else {
                throw new EPRuntimeException("Window aggregation function is not available");
            }
        }

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
        }
    }
} // end of namespace