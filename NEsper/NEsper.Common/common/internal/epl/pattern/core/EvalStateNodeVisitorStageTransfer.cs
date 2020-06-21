///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.util;
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
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    public class EvalStateNodeVisitorStageTransfer : EvalStateNodeVisitor
    {
        private readonly AgentInstanceTransferServices services;

        public EvalStateNodeVisitorStageTransfer(AgentInstanceTransferServices services)
        {
            this.services = services;
        }

        public void VisitGuard(
            EvalGuardFactoryNode factoryNode,
            EvalStateNode stateNode,
            Guard guard)
        {
            stateNode.Transfer(services);
        }

        public void VisitFilter(
            EvalFilterFactoryNode factoryNode,
            EvalStateNode stateNode,
            EPStatementHandleCallbackFilter handle,
            MatchedEventMap beginState)
        {
            stateNode.Transfer(services);
        }

        public void VisitObserver(
            EvalObserverFactoryNode factoryNode,
            EvalStateNode stateNode,
            EventObserver eventObserver)
        {
            stateNode.Transfer(services);
        }

        public void VisitFollowedBy(
            EvalFollowedByFactoryNode factoryNode,
            EvalStateNode stateNode,
            params object[] stateFlat)
        {
            // no action
        }

        public void VisitMatchUntil(
            EvalMatchUntilFactoryNode factoryNode,
            EvalStateNode stateNode,
            params object[] stateDeep)
        {
            // no action
        }

        public void VisitNot(
            EvalNotFactoryNode factoryNode,
            EvalStateNode stateNode)
        {
            // no action
        }

        public void VisitOr(
            EvalOrFactoryNode factoryNode,
            EvalStateNode stateNode)
        {
            // no action
        }

        public void VisitRoot(EvalStateNode stateNode)
        {
            // no action
        }

        public void VisitAnd(
            EvalAndFactoryNode factoryNode,
            EvalStateNode stateNode,
            params object[] stateDeep)
        {
            // no action
        }

        public void VisitEvery(
            EvalEveryFactoryNode factoryNode,
            EvalStateNode stateNode,
            MatchedEventMap beginState,
            params object[] stateFlat)
        {
            // no action
        }

        public void VisitEveryDistinct(
            EvalEveryDistinctFactoryNode factoryNode,
            EvalStateNode stateNode,
            MatchedEventMap beginState,
            ICollection<object> keySetCollection)
        {
            // no action
        }
    }
}