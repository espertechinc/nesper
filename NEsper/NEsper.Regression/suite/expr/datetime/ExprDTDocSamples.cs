///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTDocSamples : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy("select timeTaken.format() as timeTakenStr from RFIDEvent");
            env.CompileDeploy("select timeTaken.get('month') as timeTakenMonth from RFIDEvent");
            env.CompileDeploy("select timeTaken.getMonthOfYear() as timeTakenMonth from RFIDEvent");
            env.CompileDeploy("select timeTaken.minus(2 minutes) as timeTakenMinus2Min from RFIDEvent");
            env.CompileDeploy("select timeTaken.minus(2*60*1000) as timeTakenMinus2Min from RFIDEvent");
            env.CompileDeploy("select timeTaken.plus(2 minutes) as timeTakenMinus2Min from RFIDEvent");
            env.CompileDeploy("select timeTaken.plus(2*60*1000) as timeTakenMinus2Min from RFIDEvent");
            env.CompileDeploy("select timeTaken.roundCeiling('min') as timeTakenRounded from RFIDEvent");
            env.CompileDeploy("select timeTaken.roundFloor('min') as timeTakenRounded from RFIDEvent");
            env.CompileDeploy("select timeTaken.set('month', 3) as timeTakenMonth from RFIDEvent");
            env.CompileDeploy("select timeTaken.withDate(2002, 4, 30) as timeTakenDated from RFIDEvent");
            env.CompileDeploy("select timeTaken.withMax('sec') as timeTakenMaxSec from RFIDEvent");
            env.CompileDeploy("select timeTaken.toDateTimeEx() as timeTakenCal from RFIDEvent");
            env.CompileDeploy("select timeTaken.toDateTime() as timeTakenDate from RFIDEvent");
            env.CompileDeploy("select timeTaken.toMillisec() as timeTakenLong from RFIDEvent");

            // test pattern use
            var milestone = new AtomicLong();
            TryRun(
                env,
                "a.LongdateStart.after(b)",
                "2002-05-30T09:00:00.000",
                "2002-05-30T08:59:59.999",
                true,
                milestone);
            TryRun(
                env,
                "a.after(b.LongdateStart)",
                "2002-05-30T09:00:00.000",
                "2002-05-30T08:59:59.999",
                true,
                milestone);
            TryRun(
                env,
                "a.after(b)",
                "2002-05-30T09:00:00.000",
                "2002-05-30T08:59:59.999",
                true,
                milestone);
            TryRun(
                env,
                "a.after(b)",
                "2002-05-30T08:59:59.999",
                "2002-05-30T09:00:00.000",
                false,
                milestone);
        }

        private void TryRun(
            RegressionEnvironment env,
            string condition,
            string tsa,
            string tsb,
            bool IsInvoked,
            AtomicLong milestone)
        {
            var epl = "@name('s0') select * from pattern [a=A -> b=B] as abc where " + condition;
            env.CompileDeploy(epl).AddListener("s0").MilestoneInc(milestone);

            env.SendEventBean(SupportTimeStartEndA.Make("E1", tsa, 0), "A");
            env.SendEventBean(SupportTimeStartEndB.Make("E2", tsb, 0), "B");
            Assert.AreEqual(IsInvoked, env.Listener("s0").GetAndClearIsInvoked());

            env.UndeployAll();
        }

        public class MyEvent
        {
            public string Get()
            {
                return "abc";
            }
        }
    }
} // end of namespace