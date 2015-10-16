///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;
using com.espertech.esper.pattern.observer;

namespace com.espertech.esper.support.pattern
{
    public class SupportObserverFactory : ObserverFactory
    {
        public void SetObserverParameters(IList<ExprNode> observerParameters, MatchedEventConvertor convertor, ExprValidationContext validationContext)
        {
        }

        public EventObserver MakeObserver(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator,
            EvalStateNodeNumber stateNodeId,
            Object observerState,
            bool isFilterchildNonQuitting)
        {
            return null;
        }

        public bool IsNonRestarting()
        {
            return false;
        }
    }
}
