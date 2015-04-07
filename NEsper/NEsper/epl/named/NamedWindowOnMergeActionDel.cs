///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.named
{
    public class NamedWindowOnMergeActionDel : NamedWindowOnMergeAction
    {
        public NamedWindowOnMergeActionDel(ExprEvaluator optionalFilter)
            : base(optionalFilter)
        {
        }
    
        public override void Apply(EventBean matchingEvent, EventBean[] eventsPerStream, OneEventCollection newData, OneEventCollection oldData, ExprEvaluatorContext exprEvaluatorContext) {
            oldData.Add(matchingEvent);
        }
    
        public override String GetName()
        {
            return "delete";
        }
    }
}
