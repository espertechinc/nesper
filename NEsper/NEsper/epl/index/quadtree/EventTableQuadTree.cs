///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.@join.table;

namespace com.espertech.esper.epl.index.quadtree
{
    public interface EventTableQuadTree : EventTable
    {
        ICollection<EventBean> QueryRange(double x, double y, double width, double height);
    }
} // end of namespace