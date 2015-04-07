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

    public interface CompositeAccessStrategy
    {
        ISet<EventBean> Lookup(EventBean theEvent, Map parent, ISet<EventBean> result, CompositeIndexQuery next, ExprEvaluatorContext context, IList<object> optionalKeyCollector);
        ISet<EventBean> Lookup(EventBean[] eventPerStream, Map parent, ISet<EventBean> result, CompositeIndexQuery next, ExprEvaluatorContext context, IList<object> optionalKeyCollector);
    }
}
