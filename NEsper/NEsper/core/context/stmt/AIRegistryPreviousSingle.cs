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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prev;


namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryPreviousSingle : AIRegistryPrevious, ExprPreviousEvalStrategy
    {
        private ExprPreviousEvalStrategy _strategy;
    
        public void AssignService(int num, ExprPreviousEvalStrategy value) {
            this._strategy = value;
        }
    
        public void DeassignService(int num) {
            this._strategy = null;
        }

        public int AgentInstanceCount
        {
            get { return _strategy == null ? 0 : 1; }
        }

        public Object Evaluate(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {
            return _strategy.Evaluate(eventsPerStream, exprEvaluatorContext);
        }
    
        public ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
            return _strategy.EvaluateGetCollEvents(eventsPerStream, context);
        }
    
        public ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream,
                                                 ExprEvaluatorContext context) {
            return _strategy.EvaluateGetCollScalar(eventsPerStream, context);
        }
    
        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
            return _strategy.EvaluateGetEventBean(eventsPerStream, context);
        }
    }
}
