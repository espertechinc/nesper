///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.prev;

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryPreviousMultiPerm
        : AIRegistryPrevious
        , ExprPreviousEvalStrategy
    {
        private readonly ArrayWrap<ExprPreviousEvalStrategy> _strategies;
        private int _count;

        public AIRegistryPreviousMultiPerm()
        {
            _strategies = new ArrayWrap<ExprPreviousEvalStrategy>(10);
        }

        public void AssignService(int num, ExprPreviousEvalStrategy value)
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

        public Object Evaluate(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            int agentInstanceId = exprEvaluatorContext.AgentInstanceId;
            ExprPreviousEvalStrategy strategy = _strategies.Array[agentInstanceId];
            return strategy.Evaluate(eventsPerStream, exprEvaluatorContext);
        }

        public ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            int agentInstanceId = context.AgentInstanceId;
            ExprPreviousEvalStrategy strategy = _strategies.Array[agentInstanceId];
            return strategy.EvaluateGetCollEvents(eventsPerStream, context);
        }

        public ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream,
                                                 ExprEvaluatorContext context)
        {
            int agentInstanceId = context.AgentInstanceId;
            ExprPreviousEvalStrategy strategy = _strategies.Array[agentInstanceId];
            return strategy.EvaluateGetCollScalar(eventsPerStream, context);
        }

        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            int agentInstanceId = context.AgentInstanceId;
            ExprPreviousEvalStrategy strategy = _strategies.Array[agentInstanceId];
            return strategy.EvaluateGetEventBean(eventsPerStream, context);
        }
    }
}