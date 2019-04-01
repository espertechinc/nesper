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

namespace com.espertech.esper.common.@internal.epl.agg.method.firstlastever
{
    public class AggregationFactoryMethodFirstLastEver : AggregationFactoryMethodBase
    {
        internal readonly Type childType;
        internal readonly ExprFirstLastEverNode parent;
        private AggregatorMethod aggregator;

        public AggregationFactoryMethodFirstLastEver(ExprFirstLastEverNode parent, Type childType)
        {
            this.parent = parent;
            this.childType = childType;
        }

        public override Type ResultType => childType;

        public override AggregatorMethod Aggregator => aggregator;

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation =>
            new AggregationPortableValidationFirstLastEver(
                parent.IsDistinct, parent.HasFilter, childType, parent.IsFirst);

        public override void InitMethodForge(
            int col, CodegenCtor rowCtor, CodegenMemberCol membersColumnized, CodegenClassScope classScope)
        {
            if (parent.IsFirst) {
                aggregator = new AggregatorFirstEver(
                    this, col, rowCtor, membersColumnized, classScope, null, parent.HasFilter, parent.OptionalFilter,
                    childType);
            }
            else {
                aggregator = new AggregatorLastEver(
                    this, col, rowCtor, membersColumnized, classScope, null, parent.HasFilter, parent.OptionalFilter,
                    childType);
            }
        }

        public override ExprForge[] GetMethodAggregationForge(bool join, EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
        }
    }
} // end of namespace