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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.table
{
    public interface ExprTableAccessEvalStrategy
    {
        object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);
    
        ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
    
        EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
    
        ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
    
        object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
    }
}
