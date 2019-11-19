///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.core
{
    public interface QuadTreeCollector<TT>
    {
        void CollectInto(
            EventBean @event,
            object value,
            TT target);
    }

    public class ProxyQuadTreeCollector<TT> : QuadTreeCollector<TT>
    {
        public Action<EventBean, object, TT> ProcCollectInto { get; set; }

        public ProxyQuadTreeCollector(Action<EventBean, object, TT> procCollectInto)
        {
            ProcCollectInto = procCollectInto;
        }

        public ProxyQuadTreeCollector()
        {
        }

        public void CollectInto(
            EventBean @event,
            object value,
            TT target)
        {
            ProcCollectInto.Invoke(@event, value, target);
        }
    }
} // end of namespace