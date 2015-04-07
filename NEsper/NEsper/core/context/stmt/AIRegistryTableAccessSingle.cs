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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryTableAccessSingle : AIRegistryTableAccess, ExprTableAccessEvalStrategy {
    
        private ExprTableAccessEvalStrategy strategy;
    
        public void AssignService(int num, ExprTableAccessEvalStrategy value) {
            this.strategy = value;
        }
    
        public void DeassignService(int num) {
            this.strategy = null;
        }

        public int AgentInstanceCount
        {
            get { return strategy == null ? 0 : 1; }
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            return strategy.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
        }
    
        public object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return strategy.EvaluateTypableSingle(eventsPerStream, isNewData, context);
        }
    
        public ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return strategy.EvaluateGetROCollectionEvents(eventsPerStream, isNewData, context);
        }
    
        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return strategy.EvaluateGetEventBean(eventsPerStream, isNewData, context);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return strategy.EvaluateGetROCollectionScalar(eventsPerStream, isNewData, context);
        }
    }
}
