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
    public interface CompositeIndexQuery
    {
        void Add(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> value,
            ICollection<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor);

        void Add(
            EventBean[] eventsPerStream,
            IDictionary<object, CompositeIndexEntry> value,
            ICollection<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor);

        ICollection<EventBean> Get(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> parent,
            ExprEvaluatorContext context,
            CompositeIndexQueryResultPostProcessor postProcessor);

        ICollection<EventBean> Get(
            EventBean[] eventsPerStream,
            IDictionary<object, CompositeIndexEntry> parent,
            ExprEvaluatorContext context,
            CompositeIndexQueryResultPostProcessor postProcessor);

        ICollection<EventBean> GetCollectKeys(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> parent,
            ExprEvaluatorContext context,
            ICollection<object> keys,
            CompositeIndexQueryResultPostProcessor postProcessor);

        ICollection<EventBean> GetCollectKeys(
            EventBean[] eventsPerStream,
            IDictionary<object, CompositeIndexEntry> parent,
            ExprEvaluatorContext context,
            ICollection<object> keys,
            CompositeIndexQueryResultPostProcessor postProcessor);

        CompositeIndexQuery SetNext(CompositeIndexQuery next);
    }
} // end of namespace