///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.pattern.and;
using com.espertech.esper.common.@internal.epl.pattern.every;
using com.espertech.esper.common.@internal.epl.pattern.everydistinct;
using com.espertech.esper.common.@internal.epl.pattern.filter;
using com.espertech.esper.common.@internal.epl.pattern.followedby;
using com.espertech.esper.common.@internal.epl.pattern.guard;
using com.espertech.esper.common.@internal.epl.pattern.matchuntil;
using com.espertech.esper.common.@internal.epl.pattern.not;
using com.espertech.esper.common.@internal.epl.pattern.observer;
using com.espertech.esper.common.@internal.epl.pattern.or;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    public interface EvalFactoryNodeVisitor
    {
        void Visit(EvalRootFactoryNode root);

        void Visit(EvalOrFactoryNode or);

        void Visit(EvalNotFactoryNode not);

        void Visit(EvalEveryDistinctFactoryNode everyDistinct);

        void Visit(EvalMatchUntilFactoryNode matchUntil);

        void Visit(EvalFollowedByFactoryNode followedBy);

        void Visit(EvalAndFactoryNode and);

        void Visit(EvalObserverFactoryNode observer);

        void Visit(EvalEveryFactoryNode every);

        void Visit(EvalGuardFactoryNode guard);

        void Visit(EvalFilterFactoryNode filter);
    }

    public class ProxyEvalFactoryNodeVisitor : EvalFactoryNodeVisitor
    {
        public Action<EvalRootFactoryNode> ProcRootFactoryNode;

        public void Visit(EvalRootFactoryNode root)
        {
            ProcRootFactoryNode?.Invoke(root);
        }

        public Action<EvalOrFactoryNode> ProcOrFactoryNode;

        public void Visit(EvalOrFactoryNode or)
        {
            ProcOrFactoryNode?.Invoke(or);
        }

        public Action<EvalNotFactoryNode> ProcNotFactoryNode;

        public void Visit(EvalNotFactoryNode not)
        {
            ProcNotFactoryNode?.Invoke(not);
        }

        public Action<EvalEveryDistinctFactoryNode> ProcEveryDistinctFactoryNode;

        public void Visit(EvalEveryDistinctFactoryNode everyDistinct)
        {
            ProcEveryDistinctFactoryNode?.Invoke(everyDistinct);
        }

        public Action<EvalMatchUntilFactoryNode> ProcMatchUntilFactoryNode;

        public void Visit(EvalMatchUntilFactoryNode matchUntil)
        {
            ProcMatchUntilFactoryNode?.Invoke(matchUntil);
        }

        public Action<EvalFollowedByFactoryNode> ProcFollowedByFactoryNode;

        public void Visit(EvalFollowedByFactoryNode followedBy)
        {
            ProcFollowedByFactoryNode?.Invoke(followedBy);
        }

        public Action<EvalAndFactoryNode> ProcAndFactoryNode;

        public void Visit(EvalAndFactoryNode and)
        {
            ProcAndFactoryNode?.Invoke(and);
        }

        public Action<EvalObserverFactoryNode> ProcObserverFactoryNode;

        public void Visit(EvalObserverFactoryNode observer)
        {
            ProcObserverFactoryNode?.Invoke(observer);
        }

        public Action<EvalEveryFactoryNode> ProcEveryFactoryNode;

        public void Visit(EvalEveryFactoryNode every)
        {
            ProcEveryFactoryNode?.Invoke(every);
        }

        public Action<EvalGuardFactoryNode> ProcGuardFactoryNode;

        public void Visit(EvalGuardFactoryNode guard)
        {
            ProcGuardFactoryNode?.Invoke(guard);
        }

        public Action<EvalFilterFactoryNode> ProcFilterFactoryNode;

        public void Visit(EvalFilterFactoryNode filter)
        {
            ProcFilterFactoryNode?.Invoke(filter);
        }
    }
} // end of namespace