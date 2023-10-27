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
using com.espertech.esper.regressionlib.support.schedule;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTIntervalOps
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithCalendarOps(execs);
            WithInvalid(execs);
            WithBeforeInSelectClause(execs);
            WithBeforeWhereClauseWithBean(execs);
            WithBeforeWhereClause(execs);
            WithAfterWhereClause(execs);
            WithCoincidesWhereClause(execs);
            WithDuringWhereClause(execs);
            WithFinishesWhereClause(execs);
            WithFinishedByWhereClause(execs);
            WithIncludesByWhereClause(execs);
            WithMeetsWhereClause(execs);
            WithMetByWhereClause(execs);
            WithOverlapsWhereClause(execs);
            WithOverlappedByWhereClause(execs);
            WithStartsWhereClause(execs);
            WithStartedByWhereClause(execs);
            WithPointInTimeWCalendarOps(execs);
            WithBeforeWVariable(execs);
            WithTimePeriodWYearNonConst(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTimePeriodWYearNonConst(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalTimePeriodWYearNonConst());
            return execs;
        }

        public static IList<RegressionExecution> WithBeforeWVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalBeforeWVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithPointInTimeWCalendarOps(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalPointInTimeWCalendarOps());
            return execs;
        }

        public static IList<RegressionExecution> WithStartedByWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalStartedByWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithStartsWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalStartsWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithOverlappedByWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalOverlappedByWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithOverlapsWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalOverlapsWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithMetByWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalMetByWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithMeetsWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalMeetsWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithIncludesByWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalIncludesByWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithFinishedByWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalFinishedByWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithFinishesWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalFinishesWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithDuringWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalDuringWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithCoincidesWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalCoincidesWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithAfterWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalAfterWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithBeforeWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalBeforeWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithBeforeWhereClauseWithBean(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalBeforeWhereClauseWithBean());
            return execs;
        }

        public static IList<RegressionExecution> WithBeforeInSelectClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalBeforeInSelectClause());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithCalendarOps(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDTIntervalCalendarOps());
            return execs;
        }

        private class ExprDTIntervalBeforeWVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create variable int somenumber = 1;\n" +
                          "@name('s0') select longdate.before(longdate, somenumber) as c0 from SupportDateTime;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(SupportDateTime.Make("2002-05-30T09:00:00.000"));
                env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { false });

                env.UndeployAll();
            }
        }

        private class ExprDTIntervalTimePeriodWYearNonConst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create variable int somenumber = 1", path);

                var epl = "@name('s0') select " +
                          "longdate.before(longdate, somenumber years) as c0," +
                          "longdate.before(longdate, somenumber month) as c1, " +
                          "longdate.before(longdate, somenumber weeks) as c2, " +
                          "longdate.before(longdate, somenumber days) as c3, " +
                          "longdate.before(longdate, somenumber hours) as c4, " +
                          "longdate.before(longdate, somenumber minutes) as c5, " +
                          "longdate.before(longdate, somenumber seconds) as c6, " +
                          "longdate.before(longdate, somenumber milliseconds) as c7, " +
                          "longdate.before(longdate, somenumber microseconds) as c8 " +
                          " from SupportDateTime";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(SupportDateTime.Make("2002-05-30T09:00:00.000"));
                env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { false });

                env.UndeployAll();
            }
        }

        private class ExprDTIntervalPointInTimeWCalendarOps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4".SplitCsv();
                var epl = "@name('s0') select " +
                          "longdate.set('month', 1).before(LongPrimitive) as c0, " +
                          "utildate.set('month', 1).before(LongPrimitive) as c1," +
                          "caldate.set('month', 1).before(LongPrimitive) as c2," +
                          "localdate.set('month', 1).before(LongPrimitive) as c3," +
                          "zoneddate.set('month', 1).before(LongPrimitive) as c4 " +
                          "from SupportDateTime unidirectional, SupportBean#lastevent";
                env.CompileDeploy(epl).AddListener("s0");

                var bean = new SupportBean();
                bean.LongPrimitive = DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:00:00.000");
                env.SendEventBean(bean);

                env.SendEventBean(SupportDateTime.Make("2002-05-30T09:00:00.000"));
                env.AssertPropsNew("s0", fields, new object[] { true, true, true, true, true });

                env.SendEventBean(SupportDateTime.Make("2003-05-30T08:00:00.000"));
                env.AssertPropsNew("s0", fields, new object[] { false, false, false, false, false });

                env.UndeployAll();
            }
        }

        private class ExprDTIntervalCalendarOps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var seedTime = "2002-05-30T09:00:00.000"; // seed is time for B

                object[][] expected = {
                    new object[] { "2999-01-01T09:00:00.001", 0, true }, // sending in A
                };
                AssertExpression(env, seedTime, 0, "a.withDate(2001, 1, 1).before(b)", expected, null);

                expected = new object[][] {
                    new object[] { "2999-01-01T10:00:00.001", 0, false },
                    new object[] { "2999-01-01T08:00:00.001", 0, true },
                };
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.withDate(2001, 1, 1).before(b.withDate(2001, 1, 1))",
                    expected,
                    null);

                // Test end-timestamp preserved when using calendar op
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 2000, false },
                };
                AssertExpression(env, seedTime, 0, "a.before(b)", expected, null);
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 2000, false },
                };
                AssertExpression(env, seedTime, 0, "a.withTime(8, 59, 59, 0).before(b)", expected, null);

                // Test end-timestamp preserved when using calendar op
                expected = new object[][] {
                    new object[] { "2002-05-30T09:00:01.000", 0, false },
                    new object[] { "2002-05-30T09:00:01.001", 0, true },
                };
                AssertExpression(env, seedTime, 1000, "a.after(b)", expected, null);

                // NOT YET SUPPORTED (a documented limitation of datetime methods)
                // assertExpression(seedTime, 0, "a.after(b.withTime(9, 0, 0, 0))", expected, null);   // the "b.withTime(...) must retain the end-timestamp correctness (a documented limitation)
            }
        }

        private class ExprDTIntervalInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // wrong 1st parameter - string
                env.TryInvalidCompile(
                    "select a.before('x') from SupportTimeStartEndA as a",
                    "Failed to validate select-clause expression 'a.before('x')': Failed to resolve enumeration method, date-time method or mapped property 'a.before('x')': For date-time method 'before' the first parameter expression returns 'String', however requires a Date, Calendar, Long-type return value or event (with timestamp)");

                // wrong 1st parameter - event not defined with timestamp expression
                env.TryInvalidCompile(
                    "select a.before(b) from SupportTimeStartEndA#lastevent as a, SupportBean#lastevent as b",
                    "Failed to validate select-clause expression 'a.before(b)': For date-time method 'before' the first parameter is event type 'SupportBean', however no timestamp property has been defined for this event type");

                // wrong 1st parameter - boolean
                env.TryInvalidCompile(
                    "select a.before(true) from SupportTimeStartEndA#lastevent as a, SupportBean#lastevent as b",
                    "Failed to validate select-clause expression 'a.before(true)': For date-time method 'before' the first parameter expression returns 'boolean', however requires a Date, Calendar, Long-type return value or event (with timestamp)");

                // wrong zero parameters
                env.TryInvalidCompile(
                    "select a.before() from SupportTimeStartEndA#lastevent as a, SupportBean#lastevent as b",
                    "Failed to validate select-clause expression 'a.before()': Parameters mismatch for date-time method 'before', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing interval start value, or an expression providing timestamp or timestamped-event and an expression providing interval start value and an expression providing interval finishes value, but receives no parameters");

                // wrong target
                env.TryInvalidCompile(
                    "select TheString.before(a) from SupportTimeStartEndA#lastevent as a, SupportBean#lastevent as b",
                    "Failed to validate select-clause expression 'TheString.before(a)': Date-time enumeration method 'before' requires either a Calendar, Date, long, LocalDateTime or ZonedDateTime value as input or events of an event type that declares a timestamp property but received String");
                env.TryInvalidCompile(
                    "select b.before(a) from SupportTimeStartEndA#lastevent as a, SupportBean#lastevent as b",
                    "Failed to validate select-clause expression 'b.before(a)': Date-time enumeration method 'before' requires either a Calendar, Date, long, LocalDateTime or ZonedDateTime value as input or events of an event type that declares a timestamp property");
                env.TryInvalidCompile(
                    "select a.get('month').before(a) from SupportTimeStartEndA#lastevent as a, SupportBean#lastevent as b",
                    "Failed to validate select-clause expression 'a.get(\"month\").before(a)': Failed to resolve method 'get': Could not find enumeration method, date-time method, instance method or property named 'get'");

                // test before/after
                env.TryInvalidCompile(
                    "select a.before(b, 'abc') from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.before(b,\"abc\")': Failed to validate date-time method 'before', expected a time-period expression or a numeric-type result for expression parameter 1 but received String ");
                env.TryInvalidCompile(
                    "select a.before(b, 1, 'def') from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.before(b,1,\"def\")': Failed to validate date-time method 'before', expected a time-period expression or a numeric-type result for expression parameter 2 but received String ");
                env.TryInvalidCompile(
                    "select a.before(b, 1, 2, 3) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.before(b,1,2,3)': Parameters mismatch for date-time method 'before', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing interval start value, or an expression providing timestamp or timestamped-event and an expression providing interval start value and an expression providing interval finishes value, but receives 4 expressions ");

                // test coincides
                env.TryInvalidCompile(
                    "select a.coincides(b, 1, 2, 3) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.coincides(b,1,2,3)': Parameters mismatch for date-time method 'coincides', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing threshold for start and end value, or an expression providing timestamp or timestamped-event and an expression providing threshold for start value and an expression providing threshold for end value, but receives 4 expressions ");
                env.TryInvalidCompile(
                    "select a.coincides(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.coincides(b,-1)': The coincides date-time method does not allow negative start and end values ");

                // test during+interval
                env.TryInvalidCompile(
                    "select a.during(b, 1, 2, 3) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.during(b,1,2,3)': Parameters mismatch for date-time method 'during', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance interval both start and end and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance start and an expression providing maximum distance start and an expression providing minimum distance end and an expression providing maximum distance end, but receives 4 expressions ");

                // test finishes+finished-by
                env.TryInvalidCompile(
                    "select a.finishes(b, 1, 2) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.finishes(b,1,2)': Parameters mismatch for date-time method 'finishes', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between end timestamps, but receives 3 expressions ");
                env.TryInvalidCompile(
                    "select a.finishes(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.finishes(b,-1)': The finishes date-time method does not allow negative threshold value ");
                env.TryInvalidCompile(
                    "select a.finishedby(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.finishedby(b,-1)': The finishedby date-time method does not allow negative threshold value ");

                // test meets+met-by
                env.TryInvalidCompile(
                    "select a.meets(b, 1, 2) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.meets(b,1,2)': Parameters mismatch for date-time method 'meets', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between start and end timestamps, but receives 3 expressions ");
                env.TryInvalidCompile(
                    "select a.meets(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.meets(b,-1)': The meets date-time method does not allow negative threshold value ");
                env.TryInvalidCompile(
                    "select a.metBy(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.metBy(b,-1)': The metBy date-time method does not allow negative threshold value ");

                // test overlaps+overlapped-by
                env.TryInvalidCompile(
                    "select a.overlaps(b, 1, 2, 3) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.overlaps(b,1,2,3)': Parameters mismatch for date-time method 'overlaps', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance interval both start and end and an expression providing maximum distance interval both start and end, but receives 4 expressions ");

                // test start/startedby
                env.TryInvalidCompile(
                    "select a.starts(b, 1, 2, 3) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.starts(b,1,2,3)': Parameters mismatch for date-time method 'starts', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between start timestamps, but receives 4 expressions ");
                env.TryInvalidCompile(
                    "select a.starts(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.starts(b,-1)': The starts date-time method does not allow negative threshold value ");
                env.TryInvalidCompile(
                    "select a.startedBy(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.startedBy(b,-1)': The startedBy date-time method does not allow negative threshold value ");
            }
        }

        private class ExprDTIntervalBeforeInSelectClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var epl = "@name('s0') select " +
                          "a.longdateStart.before(b.longdateStart) as c0," +
                          "a.before(b) as c1 " +
                          " from SupportTimeStartEndA#lastevent as a, " +
                          "      SupportTimeStartEndB#lastevent as b";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStmtTypesAllSame("s0", fields, typeof(bool?));

                env.SendEventBean(SupportTimeStartEndB.Make("B1", "2002-05-30T09:00:00.000", 0));

                env.SendEventBean(SupportTimeStartEndA.Make("A1", "2002-05-30T08:59:59.000", 0));
                AssertPropsAllValuesSame(env, fields, true);

                env.SendEventBean(SupportTimeStartEndA.Make("A2", "2002-05-30T08:59:59.950", 0));
                AssertPropsAllValuesSame(env, fields, true);

                env.UndeployAll();
            }
        }

        private class ExprDTIntervalBeforeWhereClauseWithBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new BeforeValidator(1L, long.MaxValue);
                object[][] expected = {
                    new object[] { "2002-05-30T08:59:59.000", 0, true },
                    new object[] { "2002-05-30T08:59:59.999", 0, true },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                };

                var expressions = new string[] {
                    "a.before(b)",
                    "a.before(b, 1 millisecond)",
                    "a.before(b, 1 millisecond, 1000000000L)",
                    "a.longdateStart.before(b)",
                    "a.utildateStart.before(b)",
                    "a.caldateStart.before(b)",
                    "a.before(b.longdateStart)",
                    "a.before(b.utildateStart)",
                    "a.before(b.caldateStart)",
                    "a.longdateStart.before(b.longdateStart)",
                    "a.longdateStart.before(b.longdateStart)",
                    "a.utildateStart.before(b.utildateStart)",
                    "a.caldateStart.before(b.caldateStart)",
                    "a.utildateStart.before(b.caldateStart)",
                    "a.utildateStart.before(b.longdateStart)",
                    "a.caldateStart.before(b.utildateStart)",
                    "a.caldateStart.before(b.longdateStart)",
                    "a.ldtStart.before(b.ldtStart)",
                    "a.zdtStart.before(b.zdtStart)"
                };
                var seedTime = "2002-05-30T09:00:00.000";
                foreach (var expression in expressions) {
                    AssertExpressionBean(env, seedTime, 0, expression, expected, expectedValidator);
                }
            }
        }

        private class ExprDTIntervalBeforeWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string seedTime;
                object[][] expected;
                BeforeValidator expectedValidator;

                seedTime = "2002-05-30T09:00:00.000";
                expectedValidator = new BeforeValidator(1L, long.MaxValue);
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 0, true },
                    new object[] { "2002-05-30T08:59:59.000", 999, true },
                    new object[] { "2002-05-30T08:59:59.000", 1000, false },
                    new object[] { "2002-05-30T08:59:59.000", 1001, false },
                    new object[] { "2002-05-30T08:59:59.999", 0, true },
                    new object[] { "2002-05-30T08:59:59.999", 1, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                };
                AssertExpression(env, seedTime, 0, "a.before(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 100000, "a.before(b)", expected, expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 0, true },
                    new object[] { "2002-05-30T08:59:59.899", 0, true },
                    new object[] { "2002-05-30T08:59:59.900", 0, true },
                    new object[] { "2002-05-30T08:59:59.901", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                };
                expectedValidator = new BeforeValidator(100L, long.MaxValue);
                AssertExpression(env, seedTime, 0, "a.before(b, 100 milliseconds)", expected, expectedValidator);
                AssertExpression(env, seedTime, 100000, "a.before(b, 100 milliseconds)", expected, expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 0, false },
                    new object[] { "2002-05-30T08:59:59.499", 0, false },
                    new object[] { "2002-05-30T08:59:59.499", 1, true },
                    new object[] { "2002-05-30T08:59:59.500", 0, true },
                    new object[] { "2002-05-30T08:59:59.500", 1, true },
                    new object[] { "2002-05-30T08:59:59.500", 400, true },
                    new object[] { "2002-05-30T08:59:59.500", 401, false },
                    new object[] { "2002-05-30T08:59:59.899", 0, true },
                    new object[] { "2002-05-30T08:59:59.899", 2, false },
                    new object[] { "2002-05-30T08:59:59.900", 0, true },
                    new object[] { "2002-05-30T08:59:59.900", 1, false },
                    new object[] { "2002-05-30T08:59:59.901", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                };
                expectedValidator = new BeforeValidator(100L, 500L);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.before(b, 100 milliseconds, 500 milliseconds)",
                    expected,
                    expectedValidator);
                AssertExpression(
                    env,
                    seedTime,
                    100000,
                    "a.before(b, 100 milliseconds, 500 milliseconds)",
                    expected,
                    expectedValidator);

                // test expression params
                SetVStartEndVariables(env, 100, 500);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.before(b, V_START milliseconds, V_END milliseconds)",
                    expected,
                    expectedValidator);

                SetVStartEndVariables(env, 200, 800);
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 0, false },
                    new object[] { "2002-05-30T08:59:59.199", 0, false },
                    new object[] { "2002-05-30T08:59:59.199", 1, true },
                    new object[] { "2002-05-30T08:59:59.200", 0, true },
                    new object[] { "2002-05-30T08:59:59.800", 0, true },
                    new object[] { "2002-05-30T08:59:59.801", 0, false },
                };
                expectedValidator = new BeforeValidator(200L, 800L);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.before(b, V_START milliseconds, V_END milliseconds)",
                    expected,
                    expectedValidator);

                // test negative and reversed max and min
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.500", 0, false },
                    new object[] { "2002-05-30T09:00:00.990", 0, false },
                    new object[] { "2002-05-30T09:00:00.100", 0, true },
                    new object[] { "2002-05-30T09:00:00.500", 0, true },
                    new object[] { "2002-05-30T09:00:00.501", 0, false },
                };
                expectedValidator = new BeforeValidator(-500L, -100L);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.before(b, -100 milliseconds, -500 milliseconds)",
                    expected,
                    expectedValidator);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.before(b, -500 milliseconds, -100 milliseconds)",
                    expected,
                    expectedValidator);

                // test month logic
                seedTime = "2002-03-01T09:00:00.000";
                expected = new object[][] {
                    new object[] { "2002-02-01T09:00:00.000", 0, true },
                    new object[] { "2002-02-01T09:00:00.001", 0, false }
                };
                expectedValidator = new BeforeValidator(GetMillisecForDays(28), long.MaxValue);
                AssertExpression(env, seedTime, 100, "a.before(b, 1 month)", expected, expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-01-01T08:59:59.999", 0, false },
                    new object[] { "2002-01-01T09:00:00.000", 0, true },
                    new object[] { "2002-01-11T09:00:00.000", 0, true },
                    new object[] { "2002-02-01T09:00:00.000", 0, true },
                    new object[] { "2002-02-01T09:00:00.001", 0, false }
                };
                expectedValidator = new BeforeValidator(GetMillisecForDays(28), GetMillisecForDays(28 + 31));
                AssertExpression(env, seedTime, 100, "a.before(b, 1 month, 2 month)", expected, expectedValidator);
            }
        }

        private class ExprDTIntervalAfterWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new AfterValidator(1L, long.MaxValue);
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] { "2002-05-30T08:59:59.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, true },
                };
                AssertExpression(env, seedTime, 0, "a.after(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 0, "a.after(b, 1 millisecond)", expected, expectedValidator);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.after(b, 1 millisecond, 1000000000L)",
                    expected,
                    expectedValidator);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.after(b, 1000000000L, 1 millisecond)",
                    expected,
                    expectedValidator);
                AssertExpression(env, seedTime, 0, "a.startTS.after(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 0, "a.after(b.startTS)", expected, expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                    new object[] { "2002-05-30T09:00:00.002", 0, true },
                };
                AssertExpression(env, seedTime, 1, "a.after(b)", expected, expectedValidator);
                AssertExpression(
                    env,
                    seedTime,
                    1,
                    "a.after(b, 1 millisecond, 1000000000L)",
                    expected,
                    expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.099", 0, false },
                    new object[] { "2002-05-30T09:00:00.100", 0, true },
                    new object[] { "2002-05-30T09:00:00.101", 0, true },
                };
                expectedValidator = new AfterValidator(100L, long.MaxValue);
                AssertExpression(env, seedTime, 0, "a.after(b, 100 milliseconds)", expected, expectedValidator);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.after(b, 100 milliseconds, 1000000000L)",
                    expected,
                    expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.099", 0, false },
                    new object[] { "2002-05-30T09:00:00.100", 0, true },
                    new object[] { "2002-05-30T09:00:00.500", 0, true },
                    new object[] { "2002-05-30T09:00:00.501", 0, false },
                };
                expectedValidator = new AfterValidator(100L, 500L);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.after(b, 100 milliseconds, 500 milliseconds)",
                    expected,
                    expectedValidator);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.after(b, 100 milliseconds, 500 milliseconds)",
                    expected,
                    expectedValidator);

                // test expression params
                SetVStartEndVariables(env, 100, 500);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.after(b, V_START milliseconds, V_END milliseconds)",
                    expected,
                    expectedValidator);

                SetVStartEndVariables(env, 200, 800);
                expected = new object[][] {
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.199", 0, false },
                    new object[] { "2002-05-30T09:00:00.200", 0, true },
                    new object[] { "2002-05-30T09:00:00.800", 0, true },
                    new object[] { "2002-05-30T09:00:00.801", 0, false },
                };
                expectedValidator = new AfterValidator(200L, 800L);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.after(b, V_START milliseconds, V_END milliseconds)",
                    expected,
                    expectedValidator);

                // test negative distances
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.599", 0, false },
                    new object[] { "2002-05-30T08:59:59.600", 0, true },
                    new object[] { "2002-05-30T09:00:00.000", 0, true },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                };
                expectedValidator = new AfterValidator(-500L, -100L);
                AssertExpression(
                    env,
                    seedTime,
                    100,
                    "a.after(b, -100 milliseconds, -500 milliseconds)",
                    expected,
                    expectedValidator);
                AssertExpression(
                    env,
                    seedTime,
                    100,
                    "a.after(b, -500 milliseconds, -100 milliseconds)",
                    expected,
                    expectedValidator);

                // test month logic
                seedTime = "2002-02-01T09:00:00.000";
                expected = new object[][] {
                    new object[] { "2002-03-01T09:00:00.099", 0, false },
                    new object[] { "2002-03-01T09:00:00.100", 0, true }
                };
                expectedValidator = new AfterValidator(GetMillisecForDays(28), long.MaxValue);
                AssertExpression(env, seedTime, 100, "a.after(b, 1 month)", expected, expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-03-01T09:00:00.099", 0, false },
                    new object[] { "2002-03-01T09:00:00.100", 0, true },
                    new object[] { "2002-04-01T09:00:00.100", 0, true },
                    new object[] { "2002-04-01T09:00:00.101", 0, false }
                };
                AssertExpression(env, seedTime, 100, "a.after(b, 1 month, 2 month)", expected, null);
            }
        }

        private class ExprDTIntervalCoincidesWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new CoincidesValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] { "2002-05-30T08:59:59.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, true },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                };
                AssertExpression(env, seedTime, 0, "a.coincides(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 0, "a.coincides(b, 0 millisecond)", expected, expectedValidator);
                AssertExpression(env, seedTime, 0, "a.coincides(b, 0, 0)", expected, expectedValidator);
                AssertExpression(env, seedTime, 0, "a.startTS.coincides(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 0, "a.coincides(b.startTS)", expected, expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T09:00:00.000", 1, true },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 1, false },
                };
                AssertExpression(env, seedTime, 1, "a.coincides(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 1, "a.coincides(b, 0, 0)", expected, expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.899", 0, false },
                    new object[] { "2002-05-30T08:59:59.900", 0, true },
                    new object[] { "2002-05-30T09:00:00.000", 0, true },
                    new object[] { "2002-05-30T09:00:00.000", 50, true },
                    new object[] { "2002-05-30T09:00:00.000", 100, true },
                    new object[] { "2002-05-30T09:00:00.000", 101, false },
                    new object[] { "2002-05-30T09:00:00.099", 0, true },
                    new object[] { "2002-05-30T09:00:00.100", 0, true },
                    new object[] { "2002-05-30T09:00:00.101", 0, false },
                };
                expectedValidator = new CoincidesValidator(100L);
                AssertExpression(env, seedTime, 0, "a.coincides(b, 100 milliseconds)", expected, expectedValidator);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.coincides(b, 100 milliseconds, 0.1 sec)",
                    expected,
                    expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.799", 0, false },
                    new object[] { "2002-05-30T08:59:59.800", 0, true },
                    new object[] { "2002-05-30T09:00:00.000", 0, true },
                    new object[] { "2002-05-30T09:00:00.099", 0, true },
                    new object[] { "2002-05-30T09:00:00.100", 0, true },
                    new object[] { "2002-05-30T09:00:00.200", 0, true },
                    new object[] { "2002-05-30T09:00:00.201", 0, false },
                };
                expectedValidator = new CoincidesValidator(200L, 500L);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.coincides(b, 200 milliseconds, 500 milliseconds)",
                    expected,
                    expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.799", 0, false },
                    new object[] { "2002-05-30T08:59:59.799", 200, false },
                    new object[] { "2002-05-30T08:59:59.799", 201, false },
                    new object[] { "2002-05-30T08:59:59.800", 0, false },
                    new object[] { "2002-05-30T08:59:59.800", 199, false },
                    new object[] { "2002-05-30T08:59:59.800", 200, true },
                    new object[] { "2002-05-30T08:59:59.800", 300, true },
                    new object[] { "2002-05-30T08:59:59.800", 301, false },
                    new object[] { "2002-05-30T09:00:00.050", 0, true },
                    new object[] { "2002-05-30T09:00:00.099", 0, true },
                    new object[] { "2002-05-30T09:00:00.100", 0, true },
                    new object[] { "2002-05-30T09:00:00.101", 0, false },
                };
                expectedValidator = new CoincidesValidator(200L, 50L);
                AssertExpression(
                    env,
                    seedTime,
                    50,
                    "a.coincides(b, 200 milliseconds, 50 milliseconds)",
                    expected,
                    expectedValidator);

                // test expression params
                SetVStartEndVariables(env, 200, 50);
                AssertExpression(
                    env,
                    seedTime,
                    50,
                    "a.coincides(b, V_START milliseconds, V_END milliseconds)",
                    expected,
                    expectedValidator);

                SetVStartEndVariables(env, 200, 70);
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.800", 0, false },
                    new object[] { "2002-05-30T08:59:59.800", 179, false },
                    new object[] { "2002-05-30T08:59:59.800", 180, true },
                    new object[] { "2002-05-30T08:59:59.800", 200, true },
                    new object[] { "2002-05-30T08:59:59.800", 320, true },
                    new object[] { "2002-05-30T08:59:59.800", 321, false },
                };
                expectedValidator = new CoincidesValidator(200L, 70L);
                AssertExpression(
                    env,
                    seedTime,
                    50,
                    "a.coincides(b, V_START milliseconds, V_END milliseconds)",
                    expected,
                    expectedValidator);

                // test month logic
                seedTime = "2002-02-01T09:00:00.000"; // lasts to "2002-04-01T09:00:00.000" (28+31 days)
                expected = new object[][] {
                    new object[] { "2002-02-15T09:00:00.099", GetMillisecForDays(28 + 14), true },
                    new object[] { "2002-01-01T08:00:00.000", GetMillisecForDays(28 + 30), false }
                };
                expectedValidator = new CoincidesValidator(GetMillisecForDays(28));
                AssertExpression(
                    env,
                    seedTime,
                    GetMillisecForDays(28 + 31),
                    "a.coincides(b, 1 month)",
                    expected,
                    expectedValidator);
            }
        }

        private class ExprDTIntervalDuringWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new DuringValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] { "2002-05-30T08:59:59.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, true },
                    new object[] { "2002-05-30T09:00:00.001", 98, true },
                    new object[] { "2002-05-30T09:00:00.001", 99, false },
                    new object[] { "2002-05-30T09:00:00.099", 0, true },
                    new object[] { "2002-05-30T09:00:00.099", 1, false },
                    new object[] { "2002-05-30T09:00:00.100", 0, false },
                };
                AssertExpression(env, seedTime, 100, "a.during(b)", expected, expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 1, false },
                };
                AssertExpression(env, seedTime, 0, "a.during(b)", expected, expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T09:00:00.001", 0, true },
                    new object[] { "2002-05-30T09:00:00.001", 2000000, true },
                };
                AssertExpression(
                    env,
                    seedTime,
                    100,
                    "a.startTS.during(b)",
                    expected,
                    null); // want to use null-validator here

                // test 1-parameter footprint
                expected = new object[][] {
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 100, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 83, false },
                    new object[] { "2002-05-30T09:00:00.001", 84, true },
                    new object[] { "2002-05-30T09:00:00.001", 98, true },
                    new object[] { "2002-05-30T09:00:00.001", 99, false },
                    new object[] { "2002-05-30T09:00:00.015", 69, false },
                    new object[] { "2002-05-30T09:00:00.015", 70, true },
                    new object[] { "2002-05-30T09:00:00.015", 84, true },
                    new object[] { "2002-05-30T09:00:00.015", 85, false },
                    new object[] { "2002-05-30T09:00:00.016", 80, false },
                    new object[] { "2002-05-30T09:00:00.099", 0, false },
                };
                expectedValidator = new DuringValidator(15L);
                AssertExpression(env, seedTime, 100, "a.during(b, 15 milliseconds)", expected, expectedValidator);

                // test 2-parameter footprint
                expected = new object[][] {
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 100, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 78, false },
                    new object[] { "2002-05-30T09:00:00.001", 79, false },
                    new object[] { "2002-05-30T09:00:00.004", 85, false },
                    new object[] { "2002-05-30T09:00:00.005", 74, false },
                    new object[] { "2002-05-30T09:00:00.005", 75, true },
                    new object[] { "2002-05-30T09:00:00.005", 90, true },
                    new object[] { "2002-05-30T09:00:00.005", 91, false },
                    new object[] { "2002-05-30T09:00:00.006", 83, true },
                    new object[] { "2002-05-30T09:00:00.020", 76, false },
                    new object[] { "2002-05-30T09:00:00.020", 75, true },
                    new object[] { "2002-05-30T09:00:00.020", 60, true },
                    new object[] { "2002-05-30T09:00:00.020", 59, false },
                    new object[] { "2002-05-30T09:00:00.021", 68, false },
                    new object[] { "2002-05-30T09:00:00.099", 0, false },
                };
                expectedValidator = new DuringValidator(5L, 20L);
                AssertExpression(
                    env,
                    seedTime,
                    100,
                    "a.during(b, 5 milliseconds, 20 milliseconds)",
                    expected,
                    expectedValidator);

                // test 4-parameter footprint
                expected = new object[][] {
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 100, false },
                    new object[] { "2002-05-30T09:00:00.004", 85, false },
                    new object[] { "2002-05-30T09:00:00.005", 64, false },
                    new object[] { "2002-05-30T09:00:00.005", 65, true },
                    new object[] { "2002-05-30T09:00:00.005", 85, true },
                    new object[] { "2002-05-30T09:00:00.005", 86, false },
                    new object[] { "2002-05-30T09:00:00.020", 49, false },
                    new object[] { "2002-05-30T09:00:00.020", 50, true },
                    new object[] { "2002-05-30T09:00:00.020", 70, true },
                    new object[] { "2002-05-30T09:00:00.020", 71, false },
                    new object[] { "2002-05-30T09:00:00.021", 55, false },
                };
                expectedValidator = new DuringValidator(5L, 20L, 10L, 30L);
                AssertExpression(
                    env,
                    seedTime,
                    100,
                    "a.during(b, 5 milliseconds, 20 milliseconds, 10 milliseconds, 30 milliseconds)",
                    expected,
                    expectedValidator);
            }
        }

        private class ExprDTIntervalFinishesWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new FinishesValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] { "2002-05-30T08:59:59.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 98, false },
                    new object[] { "2002-05-30T09:00:00.001", 99, true },
                    new object[] { "2002-05-30T09:00:00.001", 100, false },
                    new object[] { "2002-05-30T09:00:00.050", 50, true },
                    new object[] { "2002-05-30T09:00:00.099", 0, false },
                    new object[] { "2002-05-30T09:00:00.099", 1, true },
                    new object[] { "2002-05-30T09:00:00.100", 0, true },
                    new object[] { "2002-05-30T09:00:00.101", 0, false },
                };
                AssertExpression(env, seedTime, 100, "a.finishes(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 100, "a.finishes(b, 0)", expected, expectedValidator);
                AssertExpression(env, seedTime, 100, "a.finishes(b, 0 milliseconds)", expected, expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 99, false },
                    new object[] { "2002-05-30T09:00:00.001", 93, false },
                    new object[] { "2002-05-30T09:00:00.001", 94, true },
                    new object[] { "2002-05-30T09:00:00.001", 100, true },
                    new object[] { "2002-05-30T09:00:00.001", 104, true },
                    new object[] { "2002-05-30T09:00:00.001", 105, false },
                    new object[] { "2002-05-30T09:00:00.050", 50, true },
                    new object[] { "2002-05-30T09:00:00.104", 0, true },
                    new object[] { "2002-05-30T09:00:00.104", 1, true },
                    new object[] { "2002-05-30T09:00:00.105", 0, true },
                    new object[] { "2002-05-30T09:00:00.105", 1, false },
                };
                expectedValidator = new FinishesValidator(5L);
                AssertExpression(env, seedTime, 100, "a.finishes(b, 5 milliseconds)", expected, expectedValidator);
            }
        }

        private class ExprDTIntervalFinishedByWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new FinishedByValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] { "2002-05-30T08:59:59.000", 0, false },
                    new object[] { "2002-05-30T08:59:59.000", 1099, false },
                    new object[] { "2002-05-30T08:59:59.000", 1100, true },
                    new object[] { "2002-05-30T08:59:59.000", 1101, false },
                    new object[] { "2002-05-30T08:59:59.999", 100, false },
                    new object[] { "2002-05-30T08:59:59.999", 101, true },
                    new object[] { "2002-05-30T08:59:59.999", 102, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 50, false },
                    new object[] { "2002-05-30T09:00:00.000", 100, false },
                };
                AssertExpression(env, seedTime, 100, "a.finishedBy(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 100, "a.finishedBy(b, 0)", expected, expectedValidator);
                AssertExpression(env, seedTime, 100, "a.finishedBy(b, 0 milliseconds)", expected, expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 0, false },
                    new object[] { "2002-05-30T08:59:59.000", 1094, false },
                    new object[] { "2002-05-30T08:59:59.000", 1095, true },
                    new object[] { "2002-05-30T08:59:59.000", 1105, true },
                    new object[] { "2002-05-30T08:59:59.000", 1106, false },
                    new object[] { "2002-05-30T08:59:59.999", 95, false },
                    new object[] { "2002-05-30T08:59:59.999", 96, true },
                    new object[] { "2002-05-30T08:59:59.999", 106, true },
                    new object[] { "2002-05-30T08:59:59.999", 107, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 95, false },
                    new object[] { "2002-05-30T09:00:00.000", 100, false },
                    new object[] { "2002-05-30T09:00:00.000", 105, false },
                };
                expectedValidator = new FinishedByValidator(5L);
                AssertExpression(env, seedTime, 100, "a.finishedBy(b, 5 milliseconds)", expected, expectedValidator);
            }
        }

        private class ExprDTIntervalIncludesByWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new IncludesValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] { "2002-05-30T08:59:59.000", 1100, false },
                    new object[] { "2002-05-30T08:59:59.000", 1101, true },
                    new object[] { "2002-05-30T08:59:59.000", 3000, true },
                    new object[] { "2002-05-30T08:59:59.999", 101, false },
                    new object[] { "2002-05-30T08:59:59.999", 102, true },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 50, false },
                    new object[] { "2002-05-30T09:00:00.000", 102, false },
                };
                AssertExpression(env, seedTime, 100, "a.includes(b)", expected, expectedValidator);

                // test 1-parameter form
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 0, false },
                    new object[] { "2002-05-30T08:59:59.000", 1100, false },
                    new object[] { "2002-05-30T08:59:59.000", 1105, false },
                    new object[] { "2002-05-30T08:59:59.994", 106, false },
                    new object[] { "2002-05-30T08:59:59.994", 110, false },
                    new object[] { "2002-05-30T08:59:59.995", 105, false },
                    new object[] { "2002-05-30T08:59:59.995", 106, true },
                    new object[] { "2002-05-30T08:59:59.995", 110, true },
                    new object[] { "2002-05-30T08:59:59.995", 111, false },
                    new object[] { "2002-05-30T08:59:59.999", 101, false },
                    new object[] { "2002-05-30T08:59:59.999", 102, true },
                    new object[] { "2002-05-30T08:59:59.999", 106, true },
                    new object[] { "2002-05-30T08:59:59.999", 107, false },
                    new object[] { "2002-05-30T09:00:00.000", 105, false },
                    new object[] { "2002-05-30T09:00:00.000", 106, false },
                };
                expectedValidator = new IncludesValidator(5L);
                AssertExpression(env, seedTime, 100, "a.includes(b, 5 milliseconds)", expected, expectedValidator);

                // test 2-parameter form
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 0, false },
                    new object[] { "2002-05-30T08:59:59.000", 1100, false },
                    new object[] { "2002-05-30T08:59:59.000", 1105, false },
                    new object[] { "2002-05-30T08:59:59.979", 130, false },
                    new object[] { "2002-05-30T08:59:59.980", 124, false },
                    new object[] { "2002-05-30T08:59:59.980", 125, true },
                    new object[] { "2002-05-30T08:59:59.980", 140, true },
                    new object[] { "2002-05-30T08:59:59.980", 141, false },
                    new object[] { "2002-05-30T08:59:59.995", 109, false },
                    new object[] { "2002-05-30T08:59:59.995", 110, true },
                    new object[] { "2002-05-30T08:59:59.995", 125, true },
                    new object[] { "2002-05-30T08:59:59.995", 126, false },
                    new object[] { "2002-05-30T08:59:59.996", 112, false },
                };
                expectedValidator = new IncludesValidator(5L, 20L);
                AssertExpression(
                    env,
                    seedTime,
                    100,
                    "a.includes(b, 5 milliseconds, 20 milliseconds)",
                    expected,
                    expectedValidator);

                // test 4-parameter form
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 0, false },
                    new object[] { "2002-05-30T08:59:59.000", 1100, false },
                    new object[] { "2002-05-30T08:59:59.000", 1105, false },
                    new object[] { "2002-05-30T08:59:59.979", 150, false },
                    new object[] { "2002-05-30T08:59:59.980", 129, false },
                    new object[] { "2002-05-30T08:59:59.980", 130, true },
                    new object[] { "2002-05-30T08:59:59.980", 150, true },
                    new object[] { "2002-05-30T08:59:59.980", 151, false },
                    new object[] { "2002-05-30T08:59:59.995", 114, false },
                    new object[] { "2002-05-30T08:59:59.995", 115, true },
                    new object[] { "2002-05-30T08:59:59.995", 135, true },
                    new object[] { "2002-05-30T08:59:59.995", 136, false },
                    new object[] { "2002-05-30T08:59:59.996", 124, false },
                };
                expectedValidator = new IncludesValidator(5L, 20L, 10L, 30L);
                AssertExpression(
                    env,
                    seedTime,
                    100,
                    "a.includes(b, 5 milliseconds, 20 milliseconds, 10 milliseconds, 30 milliseconds)",
                    expected,
                    expectedValidator);
            }
        }

        private class ExprDTIntervalMeetsWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new MeetsValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] { "2002-05-30T08:59:59.000", 1000, true },
                    new object[] { "2002-05-30T08:59:59.000", 1001, false },
                    new object[] { "2002-05-30T08:59:59.998", 1, false },
                    new object[] { "2002-05-30T08:59:59.999", 1, true },
                    new object[] { "2002-05-30T09:00:00.000", 0, true },
                    new object[] { "2002-05-30T09:00:00.000", 1, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                };
                AssertExpression(env, seedTime, 0, "a.meets(b)", expected, expectedValidator);

                // test 1-parameter form
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 0, false },
                    new object[] { "2002-05-30T08:59:59.000", 994, false },
                    new object[] { "2002-05-30T08:59:59.000", 995, true },
                    new object[] { "2002-05-30T08:59:59.000", 1005, true },
                    new object[] { "2002-05-30T08:59:59.000", 1006, false },
                    new object[] { "2002-05-30T08:59:59.994", 0, false },
                    new object[] { "2002-05-30T08:59:59.994", 1, true },
                    new object[] { "2002-05-30T08:59:59.995", 0, true },
                    new object[] { "2002-05-30T08:59:59.999", 0, true },
                    new object[] { "2002-05-30T08:59:59.999", 1, true },
                    new object[] { "2002-05-30T08:59:59.999", 6, true },
                    new object[] { "2002-05-30T08:59:59.999", 7, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, true },
                    new object[] { "2002-05-30T09:00:00.000", 1, true },
                    new object[] { "2002-05-30T09:00:00.000", 5, true },
                    new object[] { "2002-05-30T09:00:00.005", 0, true },
                    new object[] { "2002-05-30T09:00:00.005", 1, false },
                };
                expectedValidator = new MeetsValidator(5L);
                AssertExpression(env, seedTime, 0, "a.meets(b, 5 milliseconds)", expected, expectedValidator);
            }
        }

        private class ExprDTIntervalMetByWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new MetByValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] { "2002-05-30T09:00:00.990", 0, false },
                    new object[] { "2002-05-30T09:00:00.100", 0, true },
                    new object[] { "2002-05-30T09:00:00.100", 500, true },
                    new object[] { "2002-05-30T09:00:00.101", 0, false },
                };
                AssertExpression(env, seedTime, 100, "a.metBy(b)", expected, expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.999", 1, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, true },
                    new object[] { "2002-05-30T09:00:00.000", 1, true },
                };
                AssertExpression(env, seedTime, 0, "a.metBy(b)", expected, expectedValidator);

                // test 1-parameter form
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.994", 0, false },
                    new object[] { "2002-05-30T08:59:59.994", 5, false },
                    new object[] { "2002-05-30T08:59:59.995", 0, true },
                    new object[] { "2002-05-30T09:00:00.000", 0, true },
                    new object[] { "2002-05-30T09:00:00.000", 20, true },
                    new object[] { "2002-05-30T09:00:00.005", 0, true },
                    new object[] { "2002-05-30T09:00:00.005", 1000, true },
                    new object[] { "2002-05-30T09:00:00.006", 0, false },
                };
                expectedValidator = new MetByValidator(5L);
                AssertExpression(env, seedTime, 0, "a.metBy(b, 5 milliseconds)", expected, expectedValidator);

                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.994", 0, false },
                    new object[] { "2002-05-30T08:59:59.994", 5, false },
                    new object[] { "2002-05-30T08:59:59.995", 0, false },
                    new object[] { "2002-05-30T09:00:00.094", 0, false },
                    new object[] { "2002-05-30T09:00:00.095", 0, true },
                    new object[] { "2002-05-30T09:00:00.105", 0, true },
                    new object[] { "2002-05-30T09:00:00.105", 5000, true },
                    new object[] { "2002-05-30T09:00:00.106", 0, false },
                };
                expectedValidator = new MetByValidator(5L);
                AssertExpression(env, seedTime, 100, "a.metBy(b, 5 milliseconds)", expected, expectedValidator);
            }
        }

        private class ExprDTIntervalOverlapsWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new OverlapsValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] { "2002-05-30T08:59:59.000", 1000, false },
                    new object[] { "2002-05-30T08:59:59.000", 1001, true },
                    new object[] { "2002-05-30T08:59:59.000", 1050, true },
                    new object[] { "2002-05-30T08:59:59.000", 1099, true },
                    new object[] { "2002-05-30T08:59:59.000", 1100, false },
                    new object[] { "2002-05-30T08:59:59.999", 1, false },
                    new object[] { "2002-05-30T08:59:59.999", 2, true },
                    new object[] { "2002-05-30T08:59:59.999", 100, true },
                    new object[] { "2002-05-30T08:59:59.999", 101, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                };
                AssertExpression(env, seedTime, 100, "a.overlaps(b)", expected, expectedValidator);

                // test 1-parameter form (overlap by not more then X msec)
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 1000, false },
                    new object[] { "2002-05-30T08:59:59.000", 1001, true },
                    new object[] { "2002-05-30T08:59:59.000", 1005, true },
                    new object[] { "2002-05-30T08:59:59.000", 1006, false },
                    new object[] { "2002-05-30T08:59:59.000", 1100, false },
                    new object[] { "2002-05-30T08:59:59.999", 1, false },
                    new object[] { "2002-05-30T08:59:59.999", 2, true },
                    new object[] { "2002-05-30T08:59:59.999", 6, true },
                    new object[] { "2002-05-30T08:59:59.999", 7, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 5, false },
                };
                expectedValidator = new OverlapsValidator(5L);
                AssertExpression(env, seedTime, 100, "a.overlaps(b, 5 milliseconds)", expected, expectedValidator);

                // test 2-parameter form (overlap by min X and not more then Y msec)
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 1004, false },
                    new object[] { "2002-05-30T08:59:59.000", 1005, true },
                    new object[] { "2002-05-30T08:59:59.000", 1010, true },
                    new object[] { "2002-05-30T08:59:59.000", 1011, false },
                    new object[] { "2002-05-30T08:59:59.999", 5, false },
                    new object[] { "2002-05-30T08:59:59.999", 6, true },
                    new object[] { "2002-05-30T08:59:59.999", 11, true },
                    new object[] { "2002-05-30T08:59:59.999", 12, false },
                    new object[] { "2002-05-30T08:59:59.999", 12, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 5, false },
                };
                expectedValidator = new OverlapsValidator(5L, 10L);
                AssertExpression(
                    env,
                    seedTime,
                    100,
                    "a.overlaps(b, 5 milliseconds, 10 milliseconds)",
                    expected,
                    expectedValidator);
            }
        }

        private class ExprDTIntervalOverlappedByWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new OverlappedByValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] { "2002-05-30T08:59:59.000", 1000, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 1, false },
                    new object[] { "2002-05-30T09:00:00.001", 99, false },
                    new object[] { "2002-05-30T09:00:00.001", 100, true },
                    new object[] { "2002-05-30T09:00:00.099", 1, false },
                    new object[] { "2002-05-30T09:00:00.099", 2, true },
                    new object[] { "2002-05-30T09:00:00.100", 0, false },
                    new object[] { "2002-05-30T09:00:00.100", 1, false },
                };
                AssertExpression(env, seedTime, 100, "a.overlappedBy(b)", expected, expectedValidator);

                // test 1-parameter form (overlap by not more then X msec)
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 1000, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 1, false },
                    new object[] { "2002-05-30T09:00:00.001", 99, false },
                    new object[] { "2002-05-30T09:00:00.094", 7, false },
                    new object[] { "2002-05-30T09:00:00.094", 100, false },
                    new object[] { "2002-05-30T09:00:00.095", 5, false },
                    new object[] { "2002-05-30T09:00:00.095", 6, true },
                    new object[] { "2002-05-30T09:00:00.095", 100, true },
                    new object[] { "2002-05-30T09:00:00.099", 1, false },
                    new object[] { "2002-05-30T09:00:00.099", 2, true },
                    new object[] { "2002-05-30T09:00:00.099", 100, true },
                    new object[] { "2002-05-30T09:00:00.100", 100, false },
                };
                expectedValidator = new OverlappedByValidator(5L);
                AssertExpression(env, seedTime, 100, "a.overlappedBy(b, 5 milliseconds)", expected, expectedValidator);

                // test 2-parameter form (overlap by min X and not more then Y msec)
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.000", 1000, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 1, false },
                    new object[] { "2002-05-30T09:00:00.001", 99, false },
                    new object[] { "2002-05-30T09:00:00.089", 14, false },
                    new object[] { "2002-05-30T09:00:00.090", 10, false },
                    new object[] { "2002-05-30T09:00:00.090", 11, true },
                    new object[] { "2002-05-30T09:00:00.090", 1000, true },
                    new object[] { "2002-05-30T09:00:00.095", 5, false },
                    new object[] { "2002-05-30T09:00:00.095", 6, true },
                    new object[] { "2002-05-30T09:00:00.096", 5, false },
                    new object[] { "2002-05-30T09:00:00.096", 100, false },
                    new object[] { "2002-05-30T09:00:00.100", 100, false },
                };
                expectedValidator = new OverlappedByValidator(5L, 10L);
                AssertExpression(
                    env,
                    seedTime,
                    100,
                    "a.overlappedBy(b, 5 milliseconds, 10 milliseconds)",
                    expected,
                    expectedValidator);
            }
        }

        private class ExprDTIntervalStartsWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new StartsValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] { "2002-05-30T08:59:59.999", 100, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, true },
                    new object[] { "2002-05-30T09:00:00.000", 1, true },
                    new object[] { "2002-05-30T09:00:00.000", 99, true },
                    new object[] { "2002-05-30T09:00:00.000", 100, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                };
                AssertExpression(env, seedTime, 100, "a.starts(b)", expected, expectedValidator);

                // test 1-parameter form (max distance between start times)
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.994", 6, false },
                    new object[] { "2002-05-30T08:59:59.995", 0, true },
                    new object[] { "2002-05-30T08:59:59.995", 104, true },
                    new object[] { "2002-05-30T08:59:59.995", 105, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, true },
                    new object[] { "2002-05-30T09:00:00.000", 1, true },
                    new object[] { "2002-05-30T09:00:00.000", 99, true },
                    new object[] { "2002-05-30T09:00:00.000", 100, false },
                    new object[] { "2002-05-30T09:00:00.001", 0, true },
                    new object[] { "2002-05-30T09:00:00.005", 94, true },
                    new object[] { "2002-05-30T09:00:00.005", 95, false },
                    new object[] { "2002-05-30T09:00:00.005", 100, false },
                };
                expectedValidator = new StartsValidator(5L);
                AssertExpression(env, seedTime, 100, "a.starts(b, 5 milliseconds)", expected, expectedValidator);
            }
        }

        private class ExprDTIntervalStartedByWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new StartedByValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] { "2002-05-30T08:59:59.999", 100, false },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 100, false },
                    new object[] { "2002-05-30T09:00:00.000", 101, true },
                    new object[] { "2002-05-30T09:00:00.001", 0, false },
                    new object[] { "2002-05-30T09:00:00.001", 101, false },
                };
                AssertExpression(env, seedTime, 100, "a.startedBy(b)", expected, expectedValidator);

                // test 1-parameter form (max distance between start times)
                expected = new object[][] {
                    new object[] { "2002-05-30T08:59:59.994", 6, false },
                    new object[] { "2002-05-30T08:59:59.995", 0, false },
                    new object[] { "2002-05-30T08:59:59.995", 105, false },
                    new object[] { "2002-05-30T08:59:59.995", 106, true },
                    new object[] { "2002-05-30T09:00:00.000", 0, false },
                    new object[] { "2002-05-30T09:00:00.000", 100, false },
                    new object[] { "2002-05-30T09:00:00.000", 101, true },
                    new object[] { "2002-05-30T09:00:00.001", 99, false },
                    new object[] { "2002-05-30T09:00:00.001", 100, true },
                    new object[] { "2002-05-30T09:00:00.005", 94, false },
                    new object[] { "2002-05-30T09:00:00.005", 95, false },
                    new object[] { "2002-05-30T09:00:00.005", 96, true },
                };
                expectedValidator = new StartedByValidator(5L);
                AssertExpression(env, seedTime, 100, "a.startedBy(b, 5 milliseconds)", expected, expectedValidator);
            }
        }

        private static void SetVStartEndVariables(
            RegressionEnvironment env,
            long vstart,
            long vend)
        {
            env.RuntimeSetVariable(null, "V_START", vstart);
            env.RuntimeSetVariable(null, "V_END", vend);
        }

        private static void AssertExpression(
            RegressionEnvironment env,
            string seedTime,
            long seedDuration,
            string whereClause,
            object[][] timestampsAndResult,
            Validator validator)
        {
            foreach (var fieldType in EnumHelper.GetValues<SupportDateTimeFieldType>()) {
                AssertExpressionForType(
                    env,
                    seedTime,
                    seedDuration,
                    whereClause,
                    timestampsAndResult,
                    validator,
                    fieldType);
            }
        }

        private static void AssertExpressionForType(
            RegressionEnvironment env,
            string seedTime,
            long seedDuration,
            string whereClause,
            object[][] timestampsAndResult,
            Validator validator,
            SupportDateTimeFieldType fieldType)
        {
            var epl = "@name('s0') select * from A_" +
                      fieldType.GetName() +
                      "#lastevent as a, B_" +
                      fieldType.GetName() +
                      "#lastevent as b " +
                      "where " +
                      whereClause;
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventObjectArray(
                new object[] { fieldType.MakeStart(seedTime), fieldType.MakeEnd(seedTime, seedDuration) },
                "B_" + fieldType.GetName());

            foreach (var test in timestampsAndResult) {
                var testtime = (string)test[0];
                var testduration = test[1].AsInt64();
                var expected = test[2].AsBoolean();

                var rightStart = DateTimeParsingFunctions.ParseDefaultMSec(seedTime);
                var rightEnd = rightStart + seedDuration;
                var leftStart = DateTimeParsingFunctions.ParseDefaultMSec(testtime);
                var leftEnd = leftStart + testduration;
                var message = "time " + testtime + " duration " + testduration + " for '" + whereClause + "'";

                if (validator != null) {
                    Assert.AreEqual(
                        expected,
                        validator.Validate(leftStart, leftEnd, rightStart, rightEnd),
                        "Validation of expected result failed for " + message);
                }

                env.SendEventObjectArray(
                    new object[] { fieldType.MakeStart(testtime), fieldType.MakeEnd(testtime, testduration) },
                    "A_" + fieldType.GetName());

                env.AssertListener(
                    "s0",
                    listener => {
                        if (!listener.IsInvoked && expected) {
                            Assert.Fail("Expected but not received for " + message);
                        }

                        if (listener.IsInvoked && !expected) {
                            Assert.Fail("Not expected but received for " + message);
                        }

                        listener.Reset();
                    });
            }

            env.UndeployAll();
        }

        private static long GetMillisecForDays(int days)
        {
            return days * 24 * 60 * 60 * 1000L;
        }

        private static void AssertExpressionBean(
            RegressionEnvironment env,
            string seedTime,
            long seedDuration,
            string whereClause,
            object[][] timestampsAndResult,
            Validator validator)
        {
            var epl =
                "@name('s0') select * from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b where " +
                whereClause;
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(SupportTimeStartEndB.Make("B", seedTime, seedDuration));

            foreach (var test in timestampsAndResult) {
                var testtime = (string)test[0];
                var testduration = test[1].AsInt64();
                var expected = test[2].AsBoolean();

                var rightStart = DateTimeParsingFunctions.ParseDefaultMSec(seedTime);
                var rightEnd = rightStart + seedDuration;
                var leftStart = DateTimeParsingFunctions.ParseDefaultMSec(testtime);
                var leftEnd = leftStart + testduration;
                var message = "time " + testtime + " duration " + testduration + " for '" + whereClause + "'";

                if (validator != null) {
                    Assert.AreEqual(
                        expected,
                        validator.Validate(leftStart, leftEnd, rightStart, rightEnd),
                        "Validation of expected result failed for " + message);
                }

                env.SendEventBean(SupportTimeStartEndA.Make("A", testtime, testduration));

                env.AssertListener(
                    "s0",
                    listener => {
                        if (!listener.IsInvoked && expected) {
                            Assert.Fail("Expected but not received for " + message);
                        }

                        if (listener.IsInvoked && !expected) {
                            Assert.Fail("Not expected but received for " + message);
                        }

                        listener.Reset();
                    });
            }

            env.UndeployAll();
        }

        public interface Validator
        {
            bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd);
        }

        public class BeforeValidator : Validator
        {
            private long? start;
            private long? end;

            public BeforeValidator(
                long? start,
                long? end)
            {
                this.start = start;
                this.end = end;
            }

            public bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd)
            {
                var delta = rightStart - leftEnd;
                return start <= delta && delta <= end;
            }
        }

        public class AfterValidator : Validator
        {
            private long? start;
            private long? end;

            public AfterValidator(
                long? start,
                long? end)
            {
                this.start = start;
                this.end = end;
            }

            public bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd)
            {
                var delta = leftStart - rightEnd;
                return start <= delta && delta <= end;
            }
        }

        public class CoincidesValidator : Validator
        {
            private readonly long? startThreshold;
            private readonly long? endThreshold;

            public CoincidesValidator()
            {
                startThreshold = 0L;
                endThreshold = 0L;
            }

            public CoincidesValidator(long? startThreshold)
            {
                this.startThreshold = startThreshold;
                this.endThreshold = startThreshold;
            }

            public CoincidesValidator(
                long? startThreshold,
                long? endThreshold)
            {
                this.startThreshold = startThreshold;
                this.endThreshold = endThreshold;
            }

            public bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd)
            {
                var startDelta = Math.Abs(leftStart - rightStart);
                var endDelta = Math.Abs(leftEnd - rightEnd);
                return startDelta <= startThreshold && endDelta <= endThreshold;
            }
        }

        private class DuringValidator : Validator
        {
            private int form;
            private long? threshold;
            private long? minThreshold;
            private long? maxThreshold;
            private long? minStartThreshold;
            private long? maxStartThreshold;
            private long? minEndThreshold;
            private long? maxEndThreshold;

            public DuringValidator()
            {
                form = 1;
            }

            public DuringValidator(long? threshold)
            {
                form = 2;
                this.threshold = threshold;
            }

            public DuringValidator(
                long? minThreshold,
                long? maxThreshold)
            {
                form = 3;
                this.minThreshold = minThreshold;
                this.maxThreshold = maxThreshold;
            }

            public DuringValidator(
                long? minStartThreshold,
                long? maxStartThreshold,
                long? minEndThreshold,
                long? maxEndThreshold)
            {
                form = 4;
                this.minStartThreshold = minStartThreshold;
                this.maxStartThreshold = maxStartThreshold;
                this.minEndThreshold = minEndThreshold;
                this.maxEndThreshold = maxEndThreshold;
            }

            public bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd)
            {
                if (form == 1) {
                    return rightStart < leftStart &&
                           leftEnd < rightEnd;
                }
                else if (form == 2) {
                    var distanceStart = leftStart - rightStart;
                    if (distanceStart <= 0 || distanceStart > threshold) {
                        return false;
                    }

                    var distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd <= 0 || distanceEnd > threshold);
                }
                else if (form == 3) {
                    var distanceStart = leftStart - rightStart;
                    if (distanceStart < minThreshold || distanceStart > maxThreshold) {
                        return false;
                    }

                    var distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd < minThreshold || distanceEnd > maxThreshold);
                }
                else if (form == 4) {
                    var distanceStart = leftStart - rightStart;
                    if (distanceStart < minStartThreshold || distanceStart > maxStartThreshold) {
                        return false;
                    }

                    var distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd < minEndThreshold || distanceEnd > maxEndThreshold);
                }

                throw new IllegalStateException("Invalid form: " + form);
            }
        }

        public class FinishesValidator : Validator
        {
            private long? threshold;

            public FinishesValidator()
            {
            }

            public FinishesValidator(long? threshold)
            {
                this.threshold = threshold;
            }

            public bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd)
            {
                if (threshold == null) {
                    return rightStart < leftStart && leftEnd == rightEnd;
                }
                else {
                    if (rightStart >= leftStart) {
                        return false;
                    }

                    var delta = Math.Abs(leftEnd - rightEnd);
                    return delta <= threshold;
                }
            }
        }

        public class FinishedByValidator : Validator
        {
            private long? threshold;

            public FinishedByValidator()
            {
            }

            public FinishedByValidator(long? threshold)
            {
                this.threshold = threshold;
            }

            public bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd)
            {
                if (threshold == null) {
                    return leftStart < rightStart && leftEnd == rightEnd;
                }
                else {
                    if (leftStart >= rightStart) {
                        return false;
                    }

                    var delta = Math.Abs(leftEnd - rightEnd);
                    return delta <= threshold;
                }
            }
        }

        public class IncludesValidator : Validator
        {
            private int form;
            private long? threshold;
            private long? minThreshold;
            private long? maxThreshold;
            private long? minStartThreshold;
            private long? maxStartThreshold;
            private long? minEndThreshold;
            private long? maxEndThreshold;

            public IncludesValidator()
            {
                form = 1;
            }

            public IncludesValidator(long? threshold)
            {
                form = 2;
                this.threshold = threshold;
            }

            public IncludesValidator(
                long? minThreshold,
                long? maxThreshold)
            {
                form = 3;
                this.minThreshold = minThreshold;
                this.maxThreshold = maxThreshold;
            }

            public IncludesValidator(
                long? minStartThreshold,
                long? maxStartThreshold,
                long? minEndThreshold,
                long? maxEndThreshold)
            {
                form = 4;
                this.minStartThreshold = minStartThreshold;
                this.maxStartThreshold = maxStartThreshold;
                this.minEndThreshold = minEndThreshold;
                this.maxEndThreshold = maxEndThreshold;
            }

            public bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd)
            {
                if (form == 1) {
                    return leftStart < rightStart &&
                           rightEnd < leftEnd;
                }
                else if (form == 2) {
                    var distanceStart = rightStart - leftStart;
                    if (distanceStart <= 0 || distanceStart > threshold) {
                        return false;
                    }

                    var distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd <= 0 || distanceEnd > threshold);
                }
                else if (form == 3) {
                    var distanceStart = rightStart - leftStart;
                    if (distanceStart < minThreshold || distanceStart > maxThreshold) {
                        return false;
                    }

                    var distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd < minThreshold || distanceEnd > maxThreshold);
                }
                else if (form == 4) {
                    var distanceStart = rightStart - leftStart;
                    if (distanceStart < minStartThreshold || distanceStart > maxStartThreshold) {
                        return false;
                    }

                    var distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd < minEndThreshold || distanceEnd > maxEndThreshold);
                }

                throw new IllegalStateException("Invalid form: " + form);
            }
        }

        public class MeetsValidator : Validator
        {
            private long? threshold;

            public MeetsValidator()
            {
            }

            public MeetsValidator(long? threshold)
            {
                this.threshold = threshold;
            }

            public bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd)
            {
                if (threshold == null) {
                    return rightStart == leftEnd;
                }
                else {
                    var delta = Math.Abs(rightStart - leftEnd);
                    return delta <= threshold;
                }
            }
        }

        public class MetByValidator : Validator
        {
            private long? threshold;

            public MetByValidator()
            {
            }

            public MetByValidator(long? threshold)
            {
                this.threshold = threshold;
            }

            public bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd)
            {
                if (threshold == null) {
                    return leftStart == rightEnd;
                }
                else {
                    var delta = Math.Abs(leftStart - rightEnd);
                    return delta <= threshold;
                }
            }
        }

        public class OverlapsValidator : Validator
        {
            private int form;
            private long? threshold;
            private long? minThreshold;
            private long? maxThreshold;

            public OverlapsValidator()
            {
                form = 1;
            }

            public OverlapsValidator(long? threshold)
            {
                form = 2;
                this.threshold = threshold;
            }

            public OverlapsValidator(
                long? minThreshold,
                long? maxThreshold)
            {
                form = 3;
                this.minThreshold = minThreshold;
                this.maxThreshold = maxThreshold;
            }

            public bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd)
            {
                var match = (leftStart < rightStart) &&
                            (rightStart < leftEnd) &&
                            (leftEnd < rightEnd);

                if (form == 1) {
                    return match;
                }
                else if (form == 2) {
                    if (!match) {
                        return false;
                    }

                    var delta = leftEnd - rightStart;
                    return 0 <= delta && delta <= threshold;
                }
                else if (form == 3) {
                    if (!match) {
                        return false;
                    }

                    var delta = leftEnd - rightStart;
                    return minThreshold <= delta && delta <= maxThreshold;
                }

                throw new ArgumentException("Invalid form " + form);
            }
        }

        public class OverlappedByValidator : Validator
        {
            private int form;
            private long? threshold;
            private long? minThreshold;
            private long? maxThreshold;

            public OverlappedByValidator()
            {
                form = 1;
            }

            public OverlappedByValidator(long? threshold)
            {
                form = 2;
                this.threshold = threshold;
            }

            public OverlappedByValidator(
                long? minThreshold,
                long? maxThreshold)
            {
                form = 3;
                this.minThreshold = minThreshold;
                this.maxThreshold = maxThreshold;
            }

            public bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd)
            {
                var match = (rightStart < leftStart) &&
                            (leftStart < rightEnd) &&
                            (rightEnd < leftEnd);

                if (form == 1) {
                    return match;
                }
                else if (form == 2) {
                    if (!match) {
                        return false;
                    }

                    var delta = rightEnd - leftStart;
                    return 0 <= delta && delta <= threshold;
                }
                else if (form == 3) {
                    if (!match) {
                        return false;
                    }

                    var delta = rightEnd - leftStart;
                    return minThreshold <= delta && delta <= maxThreshold;
                }

                throw new ArgumentException("Invalid form " + form);
            }
        }

        public class StartsValidator : Validator
        {
            private long? threshold;

            public StartsValidator()
            {
            }

            public StartsValidator(long? threshold)
            {
                this.threshold = threshold;
            }

            public bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd)
            {
                if (threshold == null) {
                    return (leftStart == rightStart) && (leftEnd < rightEnd);
                }
                else {
                    var delta = Math.Abs(leftStart - rightStart);
                    return (delta <= threshold) && (leftEnd < rightEnd);
                }
            }
        }

        public class StartedByValidator : Validator
        {
            private long? threshold;

            public StartedByValidator()
            {
            }

            public StartedByValidator(long? threshold)
            {
                this.threshold = threshold;
            }

            public bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd)
            {
                if (threshold == null) {
                    return (leftStart == rightStart) && (leftEnd > rightEnd);
                }
                else {
                    var delta = Math.Abs(leftStart - rightStart);
                    return (delta <= threshold) && (leftEnd > rightEnd);
                }
            }
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