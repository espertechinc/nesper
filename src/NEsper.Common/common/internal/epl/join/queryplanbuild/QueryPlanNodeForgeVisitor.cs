///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.join.queryplan;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
    public interface QueryPlanNodeForgeVisitor
    {
        void Visit(QueryPlanNodeForge node);
    }

    public class ProxyQueryPlanNodeForgeVisitor : QueryPlanNodeForgeVisitor
    {
        public Action<QueryPlanNodeForge> ProcVisit;

        public void Visit(QueryPlanNodeForge node)
        {
            ProcVisit?.Invoke(node);
        }
    }
} // end of namespace