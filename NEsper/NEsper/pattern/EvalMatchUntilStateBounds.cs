///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.pattern
{
    public class EvalMatchUntilStateBounds
    {
        private readonly int? _lowerbounds;
        private readonly int? _upperbounds;
    
        public EvalMatchUntilStateBounds(int? lowerbounds, int? upperbounds)
        {
            _lowerbounds = lowerbounds;
            _upperbounds = upperbounds;
        }

        public int? Lowerbounds
        {
            get { return _lowerbounds; }
        }

        public int? Upperbounds
        {
            get { return _upperbounds; }
        }

        public static EvalMatchUntilStateBounds InitBounds(
            EvalMatchUntilFactoryNode factoryNode,
            MatchedEventMap beginState,
            PatternAgentInstanceContext context)
        {
            int? lowerbounds = null;
            int? upperbounds = null;
            var eventsPerStream = factoryNode.Convertor.Convert(beginState);
            var evaluateParams = new EvaluateParams(eventsPerStream, true, context.AgentInstanceContext);
            if (factoryNode.SingleBound != null)
            {
                var bounds = (int?) factoryNode.SingleBound.ExprEvaluator.Evaluate(evaluateParams);
                lowerbounds = bounds;
                upperbounds = bounds;
            }
            else
            {
                if (factoryNode.LowerBounds != null)
                {
                    lowerbounds = (int?) factoryNode.LowerBounds.ExprEvaluator.Evaluate(evaluateParams);
                }
                if (factoryNode.UpperBounds != null)
                {
                    upperbounds = (int?) factoryNode.UpperBounds.ExprEvaluator.Evaluate(evaluateParams);
                }
                if (upperbounds != null && lowerbounds != null)
                {
                    if (upperbounds < lowerbounds)
                    {
                        int? lbounds = lowerbounds;
                        lowerbounds = upperbounds;
                        upperbounds = lbounds;
                    }
                }
            }

            return new EvalMatchUntilStateBounds(lowerbounds, upperbounds);
        }
    }
}
