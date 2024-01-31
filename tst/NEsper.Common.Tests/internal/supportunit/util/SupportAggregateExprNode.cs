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
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class SupportAggregateExprNode : ExprAggregateNodeBase
    {
        private static int validateCount;

        private readonly Type type;
        private object value;

        public SupportAggregateExprNode(Type type)
            : base(false)
        {
            this.type = type;
            value = null;
        }

        public SupportAggregateExprNode(object value)
            : base(false)
        {
            type = value.GetType();
            this.value = value;
        }

        public SupportAggregateExprNode(
            object value,
            Type type)
            : base(false)
        {
            this.value = value;
            this.type = type;
        }

        public int ValidateCountSnapshot { get; private set; }

        public override string AggregationFunctionName => "support";

        public object Value
        {
            set => this.value = value;
        }

        public override bool IsFilterExpressionAsLastParameter => true;

        public static int ValidateCount
        {
            set => validateCount = value;
        }

        public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            // Keep a count for if and when this was validated
            validateCount++;
            ValidateCountSnapshot = validateCount;
            return null;
        }

        public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            throw new UnsupportedOperationException("not implemented");
        }

        public void EvaluateEnter(EventBean[] eventsPerStream)
        {
        }

        public void EvaluateLeave(EventBean[] eventsPerStream)
        {
        }
    }
} // end of namespace
