///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
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
    public class PatternFactoryServiceImpl : PatternFactoryService
    {
        public static readonly PatternFactoryServiceImpl INSTANCE = new PatternFactoryServiceImpl();

        private PatternFactoryServiceImpl()
        {
        }

        public EvalRootFactoryNode Root(StateMgmtSetting stateMgmtSettings)
        {
            return new EvalRootFactoryNode();
        }

        public EvalObserverFactoryNode Observer(StateMgmtSetting stateMgmtSettings)
        {
            return new EvalObserverFactoryNode();
        }

        public EvalGuardFactoryNode Guard(StateMgmtSetting stateMgmtSettings)
        {
            return new EvalGuardFactoryNode();
        }

        public TimerWithinGuardFactory GuardTimerWithin()
        {
            return new TimerWithinGuardFactory();
        }

        public TimerIntervalObserverFactory ObserverTimerInterval()
        {
            return new TimerIntervalObserverFactory();
        }

        public EvalAndFactoryNode And(StateMgmtSetting stateMgmtSettings)
        {
            return new EvalAndFactoryNode();
        }

        public EvalOrFactoryNode Or(StateMgmtSetting stateMgmtSettings)
        {
            return new EvalOrFactoryNode();
        }

        public EvalFilterFactoryNode Filter(StateMgmtSetting stateMgmtSettings)
        {
            return new EvalFilterFactoryNode();
        }

        public EvalEveryFactoryNode Every(StateMgmtSetting stateMgmtSettings)
        {
            return new EvalEveryFactoryNode();
        }

        public EvalNotFactoryNode Not(StateMgmtSetting stateMgmtSettings)
        {
            return new EvalNotFactoryNode();
        }

        public EvalFollowedByFactoryNode Followedby(StateMgmtSetting stateMgmtSettings)
        {
            return new EvalFollowedByFactoryNode();
        }

        public EvalMatchUntilFactoryNode MatchUntil(StateMgmtSetting stateMgmtSettings)
        {
            return new EvalMatchUntilFactoryNode();
        }

        public TimerWithinOrMaxCountGuardFactory GuardTimerWithinOrMax()
        {
            return new TimerWithinOrMaxCountGuardFactory();
        }

        public EvalEveryDistinctFactoryNode EveryDistinct(StateMgmtSetting stateMgmtSettings)
        {
            return new EvalEveryDistinctFactoryNode();
        }

        public TimerAtObserverFactory ObserverTimerAt()
        {
            return new TimerAtObserverFactory();
        }

        public TimerScheduleObserverFactory ObserverTimerSchedule()
        {
            return new TimerScheduleObserverFactory();
        }

        public ExpressionGuardFactory GuardWhile()
        {
            return new ExpressionGuardFactory();
        }
    }
} // end of namespace