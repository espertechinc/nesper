///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.method.avg
{
    public class AggregationFactoryMethodAvg : AggregationFactoryMethodBase
    {
        internal readonly Type childType;
        internal readonly MathContext optionalMathContext;
        internal readonly ExprAvgNode parent;
        internal readonly Type resultType;
        private AggregatorMethod aggregator;

        public AggregationFactoryMethodAvg(
            ExprAvgNode parent,
            Type childType,
            MathContext optionalMathContext)
        {
            this.parent = parent;
            this.childType = childType;
            resultType = GetAvgAggregatorType(childType);
            this.optionalMathContext = optionalMathContext;
        }

        public override Type ResultType => resultType;

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation =>
            new AggregationPortableValidationAvg(
                parent.IsDistinct,
                parent.HasFilter,
                parent.ChildNodes[0].Forge.EvaluationType);

        public override AggregatorMethod Aggregator => aggregator;

        public override void InitMethodForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            var distinctValueType = !parent.IsDistinct ? null : childType;
            if (resultType == typeof(BigInteger) || resultType == typeof(decimal?)) {
                aggregator = new AggregatorAvgBig(
                    this,
                    col,
                    rowCtor,
                    membersColumnized,
                    classScope,
                    distinctValueType,
                    parent.HasFilter,
                    parent.OptionalFilter);
            }
            else {
                aggregator = new AggregatorAvgNonBig(
                    this,
                    col,
                    rowCtor,
                    membersColumnized,
                    classScope,
                    distinctValueType,
                    parent.HasFilter,
                    parent.OptionalFilter,
                    childType.GetBoxedType());
            }
        }

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
        }

        private Type GetAvgAggregatorType(Type type)
        {
            if (type == typeof(decimal?) || type == typeof(BigInteger)) {
                return typeof(decimal?);
            }

            return typeof(double?);
        }
    }
} // end of namespace