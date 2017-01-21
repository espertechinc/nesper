///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prior;

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryPriorMultiPerm
        : AIRegistryPrior
        , ExprPriorEvalStrategy
    {
        private readonly ArrayWrap<ExprPriorEvalStrategy> _strategies;
        private int _count;

        public AIRegistryPriorMultiPerm()
        {
            _strategies = new ArrayWrap<ExprPriorEvalStrategy>(10);
        }

        #region AIRegistryPrior Members

        public void AssignService(int num, ExprPriorEvalStrategy value)
        {
            AIRegistryUtil.CheckExpand(num, _strategies);
            _strategies.Array[num] = value;
            _count++;
        }

        public void DeassignService(int num)
        {
            _strategies.Array[num] = null;
            _count--;
        }

        public int AgentInstanceCount
        {
            get { return _count; }
        }

        public Object Evaluate(EventBean[] eventsPerStream,
                               bool isNewData,
                               ExprEvaluatorContext exprEvaluatorContext,
                               int streamNumber,
                               ExprEvaluator evaluator,
                               int constantIndexNumber)
        {
            int agentInstanceId = exprEvaluatorContext.AgentInstanceId;
            ExprPriorEvalStrategy strategy = _strategies.Array[agentInstanceId];
            return strategy.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext, streamNumber, evaluator,
                                     constantIndexNumber);
        }

        #endregion
    }
}