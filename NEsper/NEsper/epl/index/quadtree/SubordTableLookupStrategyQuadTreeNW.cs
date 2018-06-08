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
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.index.quadtree
{
    public class SubordTableLookupStrategyQuadTreeNW : SubordTableLookupStrategyQuadTreeBase, SubordTableLookupStrategy
    {
        public SubordTableLookupStrategyQuadTreeNW(EventTableQuadTree index, SubordTableLookupStrategyFactoryQuadTree factory)
            : base(index, factory)
        {
        }
    
        public ICollection<EventBean> Lookup(EventBean[] events, ExprEvaluatorContext context) {
            return base.LookupInternal(events, context);
        }
    }
} // end of namespace
