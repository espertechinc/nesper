///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.subquery;

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistrySubselectMultiPerm 
        : AIRegistrySubselect
        , ExprSubselectStrategy
    {
        private readonly ArrayWrap<ExprSubselectStrategy> _strategies;
        private int _count;

        public AIRegistrySubselectMultiPerm()
        {
            _strategies = new ArrayWrap<ExprSubselectStrategy>(10);
        }

        public void AssignService(int num, ExprSubselectStrategy subselectStrategy)
        {
            AIRegistryUtil.CheckExpand(num, _strategies);
            _strategies.Array[num] = subselectStrategy;
            _count++;
        }

        public void DeassignService(int num)
        {
            _strategies.Array[num] = null;
            _count--;
        }

        public ICollection<EventBean> EvaluateMatching(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            int agentInstanceId = exprEvaluatorContext.AgentInstanceId;
            ExprSubselectStrategy strategy = _strategies.Array[agentInstanceId];
            return strategy.EvaluateMatching(eventsPerStream, exprEvaluatorContext);
        }

        public int AgentInstanceCount
        {
            get { return _count; }
        }
    }
}
