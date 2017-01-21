///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.subquery;

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistrySubselectMap : AIRegistrySubselect, ExprSubselectStrategy
    {
        private readonly IDictionary<int, ExprSubselectStrategy> _strategies;

        public AIRegistrySubselectMap()
        {
            _strategies = new Dictionary<int, ExprSubselectStrategy>();
        }

        public void AssignService(int num, ExprSubselectStrategy subselectStrategy)
        {
            _strategies.Put(num, subselectStrategy);
        }

        public void DeassignService(int num)
        {
            _strategies.Remove(num);
        }

        public ICollection<EventBean> EvaluateMatching(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            int agentInstanceId = exprEvaluatorContext.AgentInstanceId;
            ExprSubselectStrategy strategy = _strategies.Get(agentInstanceId);
            return strategy.EvaluateMatching(eventsPerStream, exprEvaluatorContext);
        }

        public int AgentInstanceCount
        {
            get { return _strategies.Count; }
        }
    }
}