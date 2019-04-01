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
    /// <summary>
    ///     Interface for visiting each element in the evaluation node tree for an event expression (see Visitor pattern).
    /// </summary>
    public interface EvalStateNodeVisitor
    {
        void VisitGuard(EvalGuardFactoryNode factoryNode, EvalStateNode stateNode, Guard guard);

        void VisitFollowedBy(EvalFollowedByFactoryNode factoryNode, EvalStateNode stateNode, params object[] stateFlat);

        void VisitFilter(
            EvalFilterFactoryNode factoryNode, EvalStateNode stateNode, EPStatementHandleCallbackFilter handle,
            MatchedEventMap beginState);

        void VisitMatchUntil(EvalMatchUntilFactoryNode factoryNode, EvalStateNode stateNode, params object[] stateDeep);

        void VisitObserver(EvalObserverFactoryNode factoryNode, EvalStateNode stateNode, EventObserver eventObserver);

        void VisitNot(EvalNotFactoryNode factoryNode, EvalStateNode stateNode);

        void VisitOr(EvalOrFactoryNode factoryNode, EvalStateNode stateNode);

        void VisitRoot(EvalStateNode stateNode);

        void VisitAnd(EvalAndFactoryNode factoryNode, EvalStateNode stateNode, params object[] stateDeep);

        void VisitEvery(
            EvalEveryFactoryNode factoryNode, EvalStateNode stateNode, MatchedEventMap beginState,
            params object[] stateFlat);

        void VisitEveryDistinct(
            EvalEveryDistinctFactoryNode factoryNode, EvalStateNode stateNode, MatchedEventMap beginState,
            ICollection<object> keySetCollection);

        void VisitAudit();
    }
} // end of namespace