///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.subquery;

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistrySubselectSingle : AIRegistrySubselect, ExprSubselectStrategy {
    
        private ExprSubselectStrategy _strategy;

        public void AssignService(int num, ExprSubselectStrategy subselectStrategy) {
            _strategy = subselectStrategy;
        }
    
        public void DeassignService(int num) {
            _strategy = null;
        }
    
        public ICollection<EventBean> EvaluateMatching(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {
            return _strategy.EvaluateMatching(eventsPerStream, exprEvaluatorContext);
        }

        public int AgentInstanceCount
        {
            get { return _strategy == null ? 0 : 1; }
        }
    }
}
