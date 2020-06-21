///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.core
{
    public interface QuadTreeCollector<TT>
    {
        void CollectInto(
            EventBean @event,
            object value,
            TT target,
            ExprEvaluatorContext ctx);
    }

    public class ProxyQuadTreeCollector<TT> : QuadTreeCollector<TT>
    {
        public Action<EventBean, object, TT, ExprEvaluatorContext> ProcCollectInto { get; set; }

        public ProxyQuadTreeCollector(Action<EventBean, object, TT, ExprEvaluatorContext> procCollectInto)
        {
            ProcCollectInto = procCollectInto;
        }

        public ProxyQuadTreeCollector()
        {
        }

        public void CollectInto(
            EventBean @event,
            object value,
            TT target,
            ExprEvaluatorContext ctx)
        {
            ProcCollectInto.Invoke(@event, value, target, ctx);
        }
    }
} // end of namespace