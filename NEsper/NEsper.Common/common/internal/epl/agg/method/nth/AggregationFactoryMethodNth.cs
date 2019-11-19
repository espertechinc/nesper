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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.method.nth
{
    public class AggregationFactoryMethodNth : AggregationFactoryMethodBase
    {
        internal readonly Type childType;
        internal readonly ExprNthAggNode parent;
        internal readonly int size;
        internal AggregatorNth aggregator;

        public AggregationFactoryMethodNth(
            ExprNthAggNode parent,
            Type childType,
            int size)
        {
            this.parent = parent;
            this.childType = childType;
            this.size = size;
        }

        public override Type ResultType => childType.GetBoxedType();

        public override AggregatorMethod Aggregator => aggregator;

        public ExprNthAggNode Parent => parent;

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation =>
            new AggregationPortableValidationNth(parent.IsDistinct, parent.OptionalFilter != null, childType, size);

        public int SizeOfBuf => size + 1;

        public override void InitMethodForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            var distinctValueType = !parent.IsDistinct ? null : childType;
            aggregator = new AggregatorNth(
                this,
                col,
                rowCtor,
                membersColumnized,
                classScope,
                distinctValueType,
                false,
                parent.OptionalFilter);
        }

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
        }
    }
} // end of namespace