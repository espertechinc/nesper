///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.supportunit.epl
{
    [Serializable]
    public class SupportAggregateExprNode : ExprAggregateNodeBase
    {
        private static int _validateCount;
    
        private readonly Type _type;
        private object _value;
        private int _validateCountSnapshot;
    
        public static void SetValidateCount(int validateCount)
        {
            _validateCount = validateCount;
        }
    
        public SupportAggregateExprNode(Type type)
            : base(false)
        {
            _type = type;
            _value = null;
        }
    
        public SupportAggregateExprNode(object value)
            : base(false)
        {
            _type = value.GetType().GetBoxedType();
            _value = value;
        }

        public SupportAggregateExprNode(object value, Type type)
            : base(false)
        {
            _value = value;
            _type = type;
        }

        protected override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            // Keep a count for if and when this was validated
            _validateCount++;
            _validateCountSnapshot = _validateCount;
            return null;
        }

        public override Type ReturnType => _type;

        public int ValidateCountSnapshot => _validateCountSnapshot;

        public AggregationMethod AggregationFunction => null;

        public override string AggregationFunctionName => "support";

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            throw new UnsupportedOperationException("not implemented");
        }
    
        public void EvaluateEnter(EventBean[] eventsPerStream)
        {
        }
    
        public void EvaluateLeave(EventBean[] eventsPerStream)
        {
        }
    
        public void SetValue(object value)
        {
            _value = value;
        }

        protected override bool IsFilterExpressionAsLastParameter => true;
    }
}
