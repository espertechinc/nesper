///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class SubordTableLookupStrategyQuadTreeSubq : SubordTableLookupStrategyQuadTreeBase,
        SubordTableLookupStrategy
    {
        private readonly EventBean[] events;

        public SubordTableLookupStrategyQuadTreeSubq(
            EventTableQuadTree index,
            SubordTableLookupStrategyFactoryQuadTree factory,
            int numStreamsOuter)
            : base(
                index, factory)
        {
            events = new EventBean[numStreamsOuter + 1];
        }

        public ICollection<EventBean> Lookup(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            Array.Copy(eventsPerStream, 0, events, 1, eventsPerStream.Length);
            return LookupInternal(events, context, index, this);
        }
    }
} // end of namespace