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
    public class PatternFactoryServiceImpl : PatternFactoryService
    {
        public static readonly PatternFactoryServiceImpl INSTANCE = new PatternFactoryServiceImpl();

        private PatternFactoryServiceImpl()
        {
        }

        public EvalRootFactoryNode Root()
        {
            return new EvalRootFactoryNode();
        }

        public EvalObserverFactoryNode Observer()
        {
            return new EvalObserverFactoryNode();
        }

        public EvalGuardFactoryNode Guard()
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

        public EvalAndFactoryNode And()
        {
            return new EvalAndFactoryNode();
        }

        public EvalOrFactoryNode Or()
        {
            return new EvalOrFactoryNode();
        }

        public EvalFilterFactoryNode Filter()
        {
            return new EvalFilterFactoryNode();
        }

        public EvalEveryFactoryNode Every()
        {
            return new EvalEveryFactoryNode();
        }

        public EvalNotFactoryNode Not()
        {
            return new EvalNotFactoryNode();
        }

        public EvalFollowedByFactoryNode Followedby()
        {
            return new EvalFollowedByFactoryNode();
        }

        public EvalMatchUntilFactoryNode MatchUntil()
        {
            return new EvalMatchUntilFactoryNode();
        }

        public TimerWithinOrMaxCountGuardFactory GuardTimerWithinOrMax()
        {
            return new TimerWithinOrMaxCountGuardFactory();
        }

        public EvalEveryDistinctFactoryNode EveryDistinct()
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