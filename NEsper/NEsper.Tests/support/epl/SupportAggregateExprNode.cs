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

namespace com.espertech.esper.support.epl
{
    [Serializable]
    public class SupportAggregateExprNode : ExprAggregateNodeBase
    {
        private static int _validateCount;
    
        private readonly Type _type;
        private Object _value;
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
    
        public SupportAggregateExprNode(Object value)
            : base(false)
        {
            _type = value.GetType().GetBoxedType();
            _value = value;
        }

        public SupportAggregateExprNode(Object value, Type type)
            : base(false)
        {
            _value = value;
            _type = type;
        }

        public override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            // Keep a count for if and when this was validated
            _validateCount++;
            _validateCountSnapshot = _validateCount;
            return null;
        }

        public override Type ReturnType
        {
            get { return _type; }
        }

        public int ValidateCountSnapshot
        {
            get { return _validateCountSnapshot; }
        }

        public AggregationMethod AggregationFunction
        {
            get { return null; }
        }

        public override string AggregationFunctionName
        {
            get { return "support"; }
        }

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
    
        public void SetValue(Object value)
        {
            _value = value;
        }
    }
}
