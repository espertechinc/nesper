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
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.index.quadtree
{
    public class SubordTableLookupStrategyQuadTreeSubq : SubordTableLookupStrategyQuadTreeBase,
        SubordTableLookupStrategy
    {
        private readonly EventBean[] _events;

        public SubordTableLookupStrategyQuadTreeSubq(
            EventTableQuadTree index, SubordTableLookupStrategyFactoryQuadTree factory, int numStreamsOuter)
            : base(index, factory)
        {
            _events = new EventBean[numStreamsOuter + 1];
        }

        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
            return LookupInternal(_events, context);
        }
    }
} // end of namespace