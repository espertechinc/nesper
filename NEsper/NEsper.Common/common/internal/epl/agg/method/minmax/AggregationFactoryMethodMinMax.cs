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

namespace com.espertech.esper.common.@internal.epl.agg.method.minmax
{
    public class AggregationFactoryMethodMinMax : AggregationFactoryMethodBase
    {
        internal readonly bool hasDataWindows;
        internal readonly ExprMinMaxAggrNode parent;
        internal readonly Type type;
        private AggregatorMethod _aggregator;

        public AggregationFactoryMethodMinMax(
            ExprMinMaxAggrNode parent,
            Type type,
            bool hasDataWindows)
        {
            this.parent = parent;
            this.type = type;
            this.hasDataWindows = hasDataWindows;
        }

        public override Type ResultType => type;

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregatorMethod Aggregator {
            get => _aggregator;
        }

        public override AggregationPortableValidation AggregationPortableValidation => new AggregationPortableValidationMinMax(
            parent.IsDistinct, parent.HasFilter, parent.ChildNodes[0].Forge.EvaluationType, parent.MinMaxTypeEnum,
            hasDataWindows);

        public ExprMinMaxAggrNode Parent => parent;

        public override void InitMethodForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            var distinctType = !parent.IsDistinct ? null : type;
            if (!hasDataWindows) {
                _aggregator = new AggregatorMinMaxEver(
                    this, col, rowCtor, membersColumnized, classScope, distinctType, parent.HasFilter,
                    parent.OptionalFilter);
            }
            else {
                _aggregator = new AggregatorMinMax(
                    this, col, rowCtor, membersColumnized, classScope, distinctType, parent.HasFilter,
                    parent.OptionalFilter);
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