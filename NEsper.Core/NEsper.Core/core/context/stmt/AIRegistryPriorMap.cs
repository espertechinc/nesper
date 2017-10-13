///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prior;

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryPriorMap
        : AIRegistryPrior
        , ExprPriorEvalStrategy
    {
        private readonly IDictionary<int, ExprPriorEvalStrategy> _strategies;

        public AIRegistryPriorMap()
        {
            _strategies = new Dictionary<int, ExprPriorEvalStrategy>();
        }

        #region AIRegistryPrior Members

        public void AssignService(int num,
                                  ExprPriorEvalStrategy value)
        {
            _strategies.Put(num, value);
        }

        public void DeassignService(int num)
        {
            _strategies.Remove(num);
        }

        public int AgentInstanceCount
        {
            get { return _strategies.Count; }
        }

        public Object Evaluate(EventBean[] eventsPerStream,
                               bool isNewData,
                               ExprEvaluatorContext exprEvaluatorContext,
                               int streamNumber,
                               ExprEvaluator evaluator,
                               int constantIndexNumber)
        {
            int agentInstanceId = exprEvaluatorContext.AgentInstanceId;
            ExprPriorEvalStrategy strategy = _strategies.Get(agentInstanceId);
            return strategy.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext, streamNumber, evaluator,
                                     constantIndexNumber);
        }

        #endregion
    }
}