///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    public interface CompositeAccessStrategy
    {
        ICollection<EventBean> Lookup(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> parent,
            ICollection<EventBean> result,
            CompositeIndexQuery next,
            ExprEvaluatorContext context,
            ICollection<object> optionalKeyCollector,
            CompositeIndexQueryResultPostProcessor postProcessor);

        ICollection<EventBean> Lookup(
            EventBean[] eventsPerStream,
            IDictionary<object, CompositeIndexEntry> parent,
            ICollection<EventBean> result,
            CompositeIndexQuery next,
            ExprEvaluatorContext context,
            ICollection<object> optionalKeyCollector,
            CompositeIndexQueryResultPostProcessor postProcessor);
    }
} // end of namespace