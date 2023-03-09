///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTResolution
    {
        public static IList<RegressionExecution> Executions(bool isMicrosecond)
        {
            var execs = new List<RegressionExecution>();
            WithResolutionEventTime(isMicrosecond, execs);
            WithLongProperty(isMicrosecond, execs);
            return execs;
        }

        public static IList<RegressionExecution> WithLongProperty(
            bool isMicrosecond,
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTLongProperty(isMicrosecond));
            return execs;
        }

        public static IList<RegressionExecution> WithResolutionEventTime(
            bool isMicrosecond,
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTResolutionEventTime(isMicrosecond));
            return execs;
        }

        private static void RunAssertionLongProperty(
            RegressionEnvironment env,
            long startTime,
            SupportDateTime @event,
            string select,
            string[] fields,
            object[] expected)
        {
            env.AdvanceTime(startTime);

            var epl = "@Name('s0') select " + select + " from SupportDateTime";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(@event);
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);

            env.UndeployAll();
        }

        private static void RunAssertionEventTime(
            RegressionEnvironment env,
            long tsB,
            long flipTimeEndtsA)
        {
            env.AdvanceTime(0);
            var epl =
                "@Name('s0') select * from " +
                "MyEvent(Id='A') as a unidirectional, " +
                "MyEvent(Id='B')#lastevent as b" +
                " where a.withDate(2002, 5, 30).before(b)";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventObjectArray(new object[] { "B", tsB, tsB }, "MyEvent");

            env.SendEventObjectArray(new object[] { "A", flipTimeEndtsA - 1, flipTimeEndtsA - 1 }, "MyEvent");
            Assert.IsTrue(env.Listener("s0").IsInvokedAndReset());

            env.SendEventObjectArray(new object[] { "A", flipTimeEndtsA, flipTimeEndtsA }, "MyEvent");
            Assert.IsFalse(env.Listener("s0").IsInvokedAndReset());

            env.UndeployAll();
        }

        public class ExprDTResolutionEventTime : RegressionExecution
        {
            private readonly bool isMicrosecond;

            public ExprDTResolutionEventTime(bool isMicrosecond)
            {
                this.isMicrosecond = isMicrosecond;
            }

            public void Run(RegressionEnvironment env)
            {
                var time = DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:00:00.000");
                if (!isMicrosecond) {
                    RunAssertionEventTime(env, time, time);
                }
                else {
                    RunAssertionEventTime(env, time * 1000, time * 1000);
                }
            }
        }

        internal class ExprDTLongProperty : RegressionExecution
        {
            private readonly bool isMicrosecond;

            public ExprDTLongProperty(bool isMicrosecond)
            {
                this.isMicrosecond = isMicrosecond;
            }

            public void Run(RegressionEnvironment env)
            {
                var time = DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:05:06.007");
                var dtxTime = DateTimeEx.GetInstance(TimeZoneInfo.Utc, time);

                var dtxMod = DateTimeEx.GetInstance(TimeZoneInfo.Utc, time)
                    .SetHour(1)
                    .SetMinute(2)
                    .SetSecond(3)
                    .SetMillis(4);

                var select =
                    "LongDate.withTime(1, 2, 3, 4) as c0," +
                    "LongDate.set('hour', 1).set('minute', 2).set('second', 3).set('millisecond', 4).toDateTimeEx() as c1," +
                    "LongDate.get('month') as c2," +
                    "current_timestamp.get('month') as c3," +
                    "current_timestamp.getMinuteOfHour() as c4," +
                    "current_timestamp.toDateTime() as c5," +
                    "current_timestamp.toDateTimeEx() as c6," +
                    "current_timestamp.minus(1) as c7";
                var fields = new[] { "c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7" };

                if (!isMicrosecond) {
                    RunAssertionLongProperty(
                        env,
                        time,
                        new SupportDateTime(time, null, null, null),
                        select,
                        fields,
                        new object[] {
                            dtxMod.UtcMillis, // c0
                            dtxMod, // c1 (dtx)
                            5, // c2 (month)
                            5, // c3 (month)
                            5, // c4 (month)
                            dtxTime.DateTime.DateTime, // c5 (dateTime)
                            dtxTime, // c6 (dateTimeEx)
                            time - 1 // c7
                        });
                }
                else {
                    RunAssertionLongProperty(
                        env,
                        time * 1000,
                        new SupportDateTime(time * 1000 + 123, null, null, null),
                        select,
                        fields,
                        new object[] {
                            dtxMod.UtcMillis * 1000 + 123, // c0
                            dtxMod, // c1 (dtx) 
                            5, // c2 (month)
                            5, // c3 (month)
                            5, // c4 (month)
                            dtxTime.DateTime.DateTime, // c5 (dateTime)
                            dtxTime, // c6 (dateTimeEx)
                            time * 1000 - 1000 // c7
                        });
                }
            }
        }
    }
} // end of namespace