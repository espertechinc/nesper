///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.collection;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.baseagg
{
    [Serializable]
    public class ExprAggregateNodeGroupKey
        : ExprNodeBase
        , ExprEvaluator
    {
        private readonly int _groupKeyIndex;
        private readonly Type _returnType;

        private AggregationResultFuture _future;

        public ExprAggregateNodeGroupKey(int groupKeyIndex, Type returnType)
        {
            _groupKeyIndex = groupKeyIndex;
            _returnType = returnType;
        }

        public void AssignFuture(AggregationResultFuture future)
        {
            _future = future;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var groupKey = _future.GetGroupKey(evaluateParams.ExprEvaluatorContext.AgentInstanceId);
            if (groupKey is MultiKeyUntyped)
            {
                return ((MultiKeyUntyped) groupKey).Keys[_groupKeyIndex];
            }
            return groupKey;
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public String ToExpressionString(ExprPrecedenceEnum precedence)
        {
            return null;
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return false;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // not required
            return null;
        }
    }
}