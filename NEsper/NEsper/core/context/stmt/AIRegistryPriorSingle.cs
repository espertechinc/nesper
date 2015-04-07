///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prior;


namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryPriorSingle : AIRegistryPrior, ExprPriorEvalStrategy {
    
        private ExprPriorEvalStrategy strategy;
    
        public AIRegistryPriorSingle() {
        }
    
        public void AssignService(int num, ExprPriorEvalStrategy value) {
            strategy = value;
        }
    
        public void DeassignService(int num) {
            strategy = null;
        }

        public int AgentInstanceCount
        {
            get { return strategy == null ? 0 : 1; }
        }

        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext, int streamNumber, ExprEvaluator evaluator, int constantIndexNumber) {
            return strategy.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext, streamNumber, evaluator, constantIndexNumber);
        }
    }
}
