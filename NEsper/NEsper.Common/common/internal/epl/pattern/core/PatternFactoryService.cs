///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
    public interface PatternFactoryService
    {
        EvalRootFactoryNode Root();

        EvalObserverFactoryNode Observer();

        EvalGuardFactoryNode Guard();

        EvalAndFactoryNode And();

        EvalOrFactoryNode Or();

        EvalFilterFactoryNode Filter();

        EvalEveryFactoryNode Every();

        EvalNotFactoryNode Not();

        EvalFollowedByFactoryNode Followedby();

        EvalMatchUntilFactoryNode MatchUntil();

        EvalEveryDistinctFactoryNode EveryDistinct();

        TimerIntervalObserverFactory ObserverTimerInterval();

        TimerAtObserverFactory ObserverTimerAt();

        TimerScheduleObserverFactory ObserverTimerSchedule();

        TimerWithinGuardFactory GuardTimerWithin();

        TimerWithinOrMaxCountGuardFactory GuardTimerWithinOrMax();

        ExpressionGuardFactory GuardWhile();
    }
} // end of namespace