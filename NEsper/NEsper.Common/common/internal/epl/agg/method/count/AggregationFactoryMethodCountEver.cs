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
    public class AggregationFactoryMethodCountEver : AggregationFactoryMethodBase
    {
        internal readonly bool ignoreNulls;
        internal readonly ExprCountEverNode parent;
        private AggregatorCount aggregator;

        public AggregationFactoryMethodCountEver(
            ExprCountEverNode parent,
            bool ignoreNulls)
        {
            this.parent = parent;
            this.ignoreNulls = ignoreNulls;
        }

        public override Type ResultType => typeof(long);

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregatorMethod Aggregator => aggregator;

        public override AggregationPortableValidation AggregationPortableValidation {
            get {
                var distinctType = !parent.IsDistinct ? null : parent.ChildNodes[0].Forge.EvaluationType;
                return new AggregationPortableValidationCount(
                    parent.IsDistinct, parent.OptionalFilter != null, true, distinctType, ignoreNulls);
            }
        }

        public override void InitMethodForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            var distinctType = !parent.IsDistinct ? null : parent.ChildNodes[0].Forge.EvaluationType;
            aggregator = new AggregatorCount(
                this, col, rowCtor, membersColumnized, classScope, distinctType, parent.OptionalFilter != null,
                parent.OptionalFilter, true);
        }

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
        }
    }
} // end of namespace