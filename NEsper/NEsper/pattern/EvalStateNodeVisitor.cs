///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.service;
using com.espertech.esper.pattern.guard;
using com.espertech.esper.pattern.observer;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Interface for visiting each element in the evaluation node tree for an event expression (see Visitor pattern).
    /// </summary>
    public interface EvalStateNodeVisitor
    {
        void VisitGuard(EvalGuardFactoryNode factoryNode, EvalStateNode stateNode, Guard guard);
        void VisitFollowedBy(EvalFollowedByFactoryNode factoryNode, EvalStateNode stateNode, params object[] stateFlat);
        void VisitFilter(EvalFilterFactoryNode factoryNode, EvalStateNode stateNode, EPStatementHandleCallback handle, MatchedEventMap beginState);
        void VisitMatchUntil(EvalMatchUntilFactoryNode factoryNode, EvalStateNode stateNode, params object[] stateDeep);
        void VisitObserver(EvalObserverFactoryNode factoryNode, EvalStateNode stateNode, EventObserver eventObserver);
        void VisitNot(EvalNotFactoryNode factoryNode, EvalStateNode stateNode);
        void VisitOr(EvalOrFactoryNode factoryNode, EvalStateNode stateNode);
        void VisitRoot(EvalStateNode stateNode);
        void VisitAnd(EvalAndFactoryNode factoryNode, EvalStateNode stateNode, params object[] stateDeep);
        void VisitEvery(EvalEveryFactoryNode factoryNode, EvalStateNode stateNode, MatchedEventMap beginState, params object[]stateFlat);
        void VisitEveryDistinct(EvalEveryDistinctFactoryNode factoryNode, EvalStateNode stateNode, MatchedEventMap beginState, IEnumerable<object> keySetCollection);
        void VisitAudit();
    }
}
