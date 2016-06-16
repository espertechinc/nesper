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

namespace com.espertech.esper.epl.join.exec.composite
{
    using Map = IDictionary<object, object>;
    public interface CompositeIndexQuery
    {
        void Add(EventBean theEvent, Map value, ICollection<EventBean> result, CompositeIndexQueryResultPostProcessor postProcessor);
        void Add(EventBean[] eventsPerStream, Map value, ICollection<EventBean> result, CompositeIndexQueryResultPostProcessor postProcessor);
        ICollection<EventBean> Get(EventBean theEvent, Map parent, ExprEvaluatorContext context, CompositeIndexQueryResultPostProcessor postProcessor);
        ICollection<EventBean> Get(EventBean[] eventsPerStream, Map parent, ExprEvaluatorContext context, CompositeIndexQueryResultPostProcessor postProcessor);
        ICollection<EventBean> GetCollectKeys(EventBean theEvent, Map parent, ExprEvaluatorContext context, IList<object> keys, CompositeIndexQueryResultPostProcessor postProcessor);
        ICollection<EventBean> GetCollectKeys(EventBean[] eventsPerStream, Map parent, ExprEvaluatorContext context, IList<object> keys, CompositeIndexQueryResultPostProcessor postProcessor);
        void SetNext(CompositeIndexQuery value);
    }
}
