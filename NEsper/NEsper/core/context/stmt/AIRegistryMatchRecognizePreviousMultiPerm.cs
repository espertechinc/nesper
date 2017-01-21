///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.rowregex;

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryMatchRecognizePreviousMultiPerm
        : AIRegistryMatchRecognizePrevious
        , RegexExprPreviousEvalStrategy
    {
        private readonly ArrayWrap<RegexExprPreviousEvalStrategy> _strategies;
        private int _count;

        public AIRegistryMatchRecognizePreviousMultiPerm()
        {
            _strategies = new ArrayWrap<RegexExprPreviousEvalStrategy>(10);
        }

        public void AssignService(int num, RegexExprPreviousEvalStrategy value)
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

        public RegexPartitionStateRandomAccess GetAccess(ExprEvaluatorContext exprEvaluatorContext)
        {
            int agentInstanceId = exprEvaluatorContext.AgentInstanceId;
            RegexExprPreviousEvalStrategy strategy = _strategies.Array[agentInstanceId];
            return strategy.GetAccess(exprEvaluatorContext);
        }
    }
}