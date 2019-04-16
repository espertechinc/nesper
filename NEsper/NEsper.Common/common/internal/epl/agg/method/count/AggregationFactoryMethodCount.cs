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

namespace com.espertech.esper.common.@internal.epl.agg.method.count
{
    public class AggregationFactoryMethodCount : AggregationFactoryMethodBase
    {
        internal readonly Type countedValueType;
        internal readonly bool ignoreNulls;
        internal readonly ExprCountNode parent;

        private AggregatorCount aggregator;

        public AggregationFactoryMethodCount(
            ExprCountNode parent,
            bool ignoreNulls,
            Type countedValueType)
        {
            this.parent = parent;
            this.ignoreNulls = ignoreNulls;
            this.countedValueType = countedValueType;
        }

        public override Type ResultType => typeof(long);

        public override AggregatorMethod Aggregator => aggregator;

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation => new AggregationPortableValidationCount(
            parent.IsDistinct, false, parent.IsDistinct, countedValueType, ignoreNulls);

        public override void InitMethodForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            var distinctType = !parent.IsDistinct ? null : countedValueType;
            aggregator = new AggregatorCount(
                this, col, rowCtor, membersColumnized, classScope, distinctType, parent.HasFilter,
                parent.OptionalFilter, false);
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
                return ExprMethodAggUtil.GetDefaultForges(new[] {childNodes[1]}, join, typesPerStream);
            }

            if (childNodes[0] is ExprWildcard && childNodes.Length == 1) {
                return ExprNodeUtilityQuery.EMPTY_FORGE_ARRAY;
            }

            return ExprMethodAggUtil.GetDefaultForges(childNodes, join, typesPerStream);
        }
    }
} // end of namespace