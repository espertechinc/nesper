///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.join.exec.composite
{
    using Map = IDictionary<object, object>;

    public interface CompositeAccessStrategy
    {
        ICollection<EventBean> Lookup(
            EventBean theEvent,
            Map parent,
            ICollection<EventBean> result,
            CompositeIndexQuery next,
            ExprEvaluatorContext context,
            IList<object> optionalKeyCollector,
            CompositeIndexQueryResultPostProcessor postProcessor);

        ICollection<EventBean> Lookup(
            EventBean[] eventPerStream,
            Map parent,
            ICollection<EventBean> result,
            CompositeIndexQuery next,
            ExprEvaluatorContext context,
            IList<object> optionalKeyCollector,
            CompositeIndexQueryResultPostProcessor postProcessor);
    }
}
