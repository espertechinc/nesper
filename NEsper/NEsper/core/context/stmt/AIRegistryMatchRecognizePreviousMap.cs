///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.rowregex;

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryMatchRecognizePreviousMap
        : AIRegistryMatchRecognizePrevious
        , RegexExprPreviousEvalStrategy
    {
        private readonly IDictionary<int, RegexExprPreviousEvalStrategy> _strategies;

        public AIRegistryMatchRecognizePreviousMap()
        {
            _strategies = new Dictionary<int, RegexExprPreviousEvalStrategy>();
        }

        public void AssignService(int num, RegexExprPreviousEvalStrategy value)
        {
            _strategies[num] = value;
        }

        public void DeassignService(int num)
        {
            _strategies.Remove(num);
        }

        public int AgentInstanceCount
        {
            get { return _strategies.Count; }
        }

        public RegexPartitionStateRandomAccess GetAccess(ExprEvaluatorContext exprEvaluatorContext)
        {
            var agentInstanceId = exprEvaluatorContext.AgentInstanceId;
            var strategy = _strategies.Get(agentInstanceId);
            return strategy.GetAccess(exprEvaluatorContext);
        }
    }
}