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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTBetween
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithIncludeEndpoints(execs);
            WithExcludeEndpoints(execs);
            WithTypes(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTBetweenTypes());
            return execs;
        }

        public static IList<RegressionExecution> WithExcludeEndpoints(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTBetweenExcludeEndpoints());
            return execs;
        }

        public static IList<RegressionExecution> WithIncludeEndpoints(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTBetweenIncludeEndpoints());
            return execs;
        }

        private class ExprDTBetweenTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4".SplitCsv();
                var eplCurrentTS = "@name('s0') select " +
                                   "longdate.between(longPrimitive, longBoxed) as c0, " +
                                   "utildate.between(longPrimitive, longBoxed) as c1, " +
                                   "caldate.between(longPrimitive, longBoxed) as c2," +
                                   "localdate.between(longPrimitive, longBoxed) as c3," +
                                   "zoneddate.between(longPrimitive, longBoxed) as c4 " +
                                   " from SupportDateTime unidirectional, SupportBean#lastevent";
                env.CompileDeploy(eplCurrentTS).AddListener("s0");

                var bean = new SupportBean();
                bean.LongPrimitive = 10;
                bean.LongBoxed = 20L;
                env.SendEventBean(bean);

                env.SendEventBean(SupportDateTime.Make("2002-05-30T09:01:02.003"));
                env.AssertPropsNew("s0", fields, new object[] { false, false, false, false, false });

                bean = new SupportBean();
                bean.LongPrimitive = 0;
                bean.LongBoxed = long.MaxValue;
                env.SendEventBean(bean);

                env.SendEventBean(SupportDateTime.Make("2002-05-30T09:01:02.003"));
                env.AssertPropsNew("s0", fields, new object[] { true, true, true, true, true });

                env.UndeployAll();
            }
        }

        private class ExprDTBetweenIncludeEndpoints : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var startTime = "2002-05-30T09:00:00.000";
                env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

                var fieldsCurrentTs = "val0,val1,val2,val3,val4,val5,val6,val7,val8".SplitCsv();
                var eplCurrentTS = "@name('s0') select " +
                                   "current_timestamp.after(longdateStart) as val0, " +
                                   "current_timestamp.between(longdateStart, longdateEnd) as val1, " +
                                   "current_timestamp.between(utildateStart, caldateEnd) as val2, " +
                                   "current_timestamp.between(caldateStart, utildateEnd) as val3, " +
                                   "current_timestamp.between(utildateStart, utildateEnd) as val4, " +
                                   "current_timestamp.between(caldateStart, caldateEnd) as val5, " +
                                   "current_timestamp.between(caldateEnd, caldateStart) as val6, " +
                                   "current_timestamp.between(ldtStart, ldtEnd) as val7, " +
                                   "current_timestamp.between(zdtStart, zdtEnd) as val8 " +
                                   "from SupportTimeStartEndA";
                env.CompileDeploy(eplCurrentTS).AddListener("s0");
                env.AssertStmtTypesAllSame("s0", fieldsCurrentTs, typeof(bool?));

                env.SendEventBean(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 0));
                env.AssertPropsNew(
                    "s0",
                    fieldsCurrentTs,
                    new object[] { true, false, false, false, false, false, false, false, false });

                env.SendEventBean(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 1));
                env.AssertPropsNew(
                    "s0",
                    fieldsCurrentTs,
                    new object[] { true, true, true, true, true, true, true, true, true });

                env.SendEventBean(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 100));
                env.AssertPropsNew(
                    "s0",
                    fieldsCurrentTs,
                    new object[] { true, true, true, true, true, true, true, true, true });

                env.SendEventBean(SupportTimeStartEndA.Make("E1", "2002-05-30T09:00:00.000", 0));
                env.AssertPropsNew(
                    "s0",
                    fieldsCurrentTs,
                    new object[] { false, true, true, true, true, true, true, true, true });

                env.SendEventBean(SupportTimeStartEndA.Make("E1", "2002-05-30T09:00:00.000", 100));
                env.AssertPropsNew(
                    "s0",
                    fieldsCurrentTs,
                    new object[] { false, true, true, true, true, true, true, true, true });

                env.SendEventBean(SupportTimeStartEndA.Make("E1", "2002-05-30T09:00:00.001", 100));
                env.AssertPropsNew(
                    "s0",
                    fieldsCurrentTs,
                    new object[] { false, false, false, false, false, false, false, false, false });
                env.UndeployAll();

                // test calendar field and constants
                var fieldsConstants = "val0,val1,val2,val3,val4,val5".SplitCsv();
                var eplConstants = "@name('s0') select " +
                                   "longdateStart.between(DateTime.toCalendar('2002-05-30T09:00:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), DateTime.toCalendar('2002-05-30T09:01:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\")) as val0, " +
                                   "utildateStart.between(DateTime.toCalendar('2002-05-30T09:00:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), DateTime.toCalendar('2002-05-30T09:01:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\")) as val1, " +
                                   "caldateStart.between(DateTime.toCalendar('2002-05-30T09:00:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), DateTime.toCalendar('2002-05-30T09:01:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\")) as val2, " +
                                   "ldtStart.between(DateTime.toCalendar('2002-05-30T09:00:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), DateTime.toCalendar('2002-05-30T09:01:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\")) as val3, " +
                                   "zdtStart.between(DateTime.toCalendar('2002-05-30T09:00:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), DateTime.toCalendar('2002-05-30T09:01:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\")) as val4, " +
                                   "longdateStart.between(DateTime.toCalendar('2002-05-30T09:01:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), DateTime.toCalendar('2002-05-30T09:00:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\")) as val5 " +
                                   "from SupportTimeStartEndA";
                env.CompileDeployAddListenerMile(eplConstants, "s0", 1);
                env.AssertStmtTypesAllSame("s0", fieldsConstants, typeof(bool?));

                env.SendEventBean(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 0));
                AssertPropsAllValuesSame(env, fieldsConstants, false);

                env.SendEventBean(SupportTimeStartEndA.Make("E2", "2002-05-30T09:00:00.000", 0));
                AssertPropsAllValuesSame(env, fieldsConstants, true);

                env.SendEventBean(SupportTimeStartEndA.Make("E2", "2002-05-30T09:00:05.000", 0));
                AssertPropsAllValuesSame(env, fieldsConstants, true);

                env.SendEventBean(SupportTimeStartEndA.Make("E2", "2002-05-30T09:00:59.999", 0));
                AssertPropsAllValuesSame(env, fieldsConstants, true);

                env.SendEventBean(SupportTimeStartEndA.Make("E2", "2002-05-30T09:01:00.000", 0));
                AssertPropsAllValuesSame(env, fieldsConstants, true);

                env.SendEventBean(SupportTimeStartEndA.Make("E2", "2002-05-30T09:01:00.001", 0));
                AssertPropsAllValuesSame(env, fieldsConstants, false);

                env.UndeployAll();
            }
        }

        private class ExprDTBetweenExcludeEndpoints : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var startTime = "2002-05-30T09:00:00.000";
                env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(startTime));

                var path = new RegressionPath();
                env.CompileDeploy("@public create variable boolean VAR_TRUE = true", path);
                env.CompileDeploy("@public create variable boolean VAR_FALSE = false", path);

                TryAssertionExcludeEndpoints(env, path, "longdateStart, longdateEnd", milestone);
                TryAssertionExcludeEndpoints(env, path, "utildateStart, utildateEnd", milestone);
                TryAssertionExcludeEndpoints(env, path, "caldateStart, caldateEnd", milestone);
                TryAssertionExcludeEndpoints(env, path, "ldtStart, ldtEnd", milestone);
                TryAssertionExcludeEndpoints(env, path, "zdtStart, zdtEnd", milestone);

                env.UndeployAll();
            }
        }

        private static void TryAssertionExcludeEndpoints(
            RegressionEnvironment env,
            RegressionPath path,
            string fields,
            AtomicLong milestone)
        {
            var fieldsCurrentTs = "val0,val1,val2,val3,val4,val5,val6,val7".SplitCsv();
            var eplCurrentTS = "@name('s0') select " +
                               "current_timestamp.between(" +
                               fields +
                               ", true, true) as val0, " +
                               "current_timestamp.between(" +
                               fields +
                               ", true, false) as val1, " +
                               "current_timestamp.between(" +
                               fields +
                               ", false, true) as val2, " +
                               "current_timestamp.between(" +
                               fields +
                               ", false, false) as val3, " +
                               "current_timestamp.between(" +
                               fields +
                               ", VAR_TRUE, VAR_TRUE) as val4, " +
                               "current_timestamp.between(" +
                               fields +
                               ", VAR_TRUE, VAR_FALSE) as val5, " +
                               "current_timestamp.between(" +
                               fields +
                               ", VAR_FALSE, VAR_TRUE) as val6, " +
                               "current_timestamp.between(" +
                               fields +
                               ", VAR_FALSE, VAR_FALSE) as val7 " +
                               "from SupportTimeStartEndA";
            env.CompileDeploy(eplCurrentTS, path).AddListener("s0");
            env.AssertStmtTypesAllSame("s0", fieldsCurrentTs, typeof(bool?));

            env.SendEventBean(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 0));
            AssertPropsAllValuesSame(env, fieldsCurrentTs, false);

            env.SendEventBean(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 1));
            env.AssertPropsNew(
                "s0",
                fieldsCurrentTs,
                new object[] { true, false, true, false, true, false, true, false });

            env.MilestoneInc(milestone);

            env.SendEventBean(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 2));
            AssertPropsAllValuesSame(env, fieldsCurrentTs, true);

            env.SendEventBean(SupportTimeStartEndA.Make("E1", "2002-05-30T09:00:00.000", 1));
            env.AssertPropsNew(
                "s0",
                fieldsCurrentTs,
                new object[] { true, true, false, false, true, true, false, false });

            env.UndeployModuleContaining("s0");

            // test calendar field and constants
            var fieldsConstants = "val0,val1,val2,val3".SplitCsv();
            var eplConstants = "@name('s0') select " +
                               "longdateStart.between(DateTime.toCalendar('2002-05-30T09:00:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), DateTime.toCalendar('2002-05-30T09:01:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), true, true) as val0, " +
                               "longdateStart.between(DateTime.toCalendar('2002-05-30T09:00:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), DateTime.toCalendar('2002-05-30T09:01:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), true, false) as val1, " +
                               "longdateStart.between(DateTime.toCalendar('2002-05-30T09:00:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), DateTime.toCalendar('2002-05-30T09:01:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), false, true) as val2, " +
                               "longdateStart.between(DateTime.toCalendar('2002-05-30T09:00:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), DateTime.toCalendar('2002-05-30T09:01:00.000', \"yyyy-MM-dd'T'HH:mm:ss.SSS\"), false, false) as val3 " +
                               "from SupportTimeStartEndA";
            env.CompileDeploy(eplConstants).AddListener("s0");
            env.AssertStmtTypesAllSame("s0", fieldsConstants, typeof(bool?));

            env.SendEventBean(SupportTimeStartEndA.Make("E1", "2002-05-30T08:59:59.999", 0));
            env.AssertPropsNew("s0", fieldsConstants, new object[] { false, false, false, false });

            env.SendEventBean(SupportTimeStartEndA.Make("E2", "2002-05-30T09:00:00.000", 0));
            env.AssertPropsNew("s0", fieldsConstants, new object[] { true, true, false, false });

            env.SendEventBean(SupportTimeStartEndA.Make("E2", "2002-05-30T09:00:05.000", 0));
            env.AssertPropsNew("s0", fieldsConstants, new object[] { true, true, true, true });

            env.MilestoneInc(milestone);

            env.SendEventBean(SupportTimeStartEndA.Make("E2", "2002-05-30T09:00:59.999", 0));
            env.AssertPropsNew("s0", fieldsConstants, new object[] { true, true, true, true });

            env.SendEventBean(SupportTimeStartEndA.Make("E2", "2002-05-30T09:01:00.000", 0));
            env.AssertPropsNew("s0", fieldsConstants, new object[] { true, false, true, false });

            env.SendEventBean(SupportTimeStartEndA.Make("E2", "2002-05-30T09:01:00.001", 0));
            env.AssertPropsNew("s0", fieldsConstants, new object[] { false, false, false, false });

            env.UndeployModuleContaining("s0");
        }

        private static void AssertPropsAllValuesSame(
            RegressionEnvironment env,
            string[] fields,
            bool expected)
        {
            env.AssertEventNew("s0", @event => EPAssertionUtil.AssertPropsAllValuesSame(@event, fields, expected));
        }
    }
} // end of namespace