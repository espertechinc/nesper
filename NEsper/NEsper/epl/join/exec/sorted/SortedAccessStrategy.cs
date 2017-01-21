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
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.exec.sorted
{
    public interface SortedAccessStrategy
    {
        ICollection<EventBean> Lookup(EventBean theEvent, PropertySortedEventTable index, ExprEvaluatorContext context);
        ICollection<EventBean> LookupCollectKeys(EventBean theEvent, PropertySortedEventTable index, ExprEvaluatorContext context, IList<object> keys);
        ICollection<EventBean> Lookup(EventBean[] eventsPerStream, PropertySortedEventTable index, ExprEvaluatorContext context);
        ICollection<EventBean> LookupCollectKeys(EventBean[] eventsPerStream, PropertySortedEventTable index, ExprEvaluatorContext context, IList<object> keys);
        String ToQueryPlan();
    }
}
