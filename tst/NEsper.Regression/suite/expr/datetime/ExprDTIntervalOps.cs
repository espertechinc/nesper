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
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTIntervalOps
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprDTIntervalCalendarOps());
            executions.Add(new ExprDTIntervalInvalid());
            executions.Add(new ExprDTIntervalBeforeInSelectClause());
            executions.Add(new ExprDTIntervalBeforeWhereClauseWithBean());
            executions.Add(new ExprDTIntervalBeforeWhereClause());
            executions.Add(new ExprDTIntervalAfterWhereClause());
            executions.Add(new ExprDTIntervalCoincidesWhereClause());
            executions.Add(new ExprDTIntervalDuringWhereClause());
            executions.Add(new ExprDTIntervalFinishesWhereClause());
            executions.Add(new ExprDTIntervalFinishedByWhereClause());
            executions.Add(new ExprDTIntervalIncludesByWhereClause());
            executions.Add(new ExprDTIntervalMeetsWhereClause());
            executions.Add(new ExprDTIntervalMetByWhereClause());
            executions.Add(new ExprDTIntervalOverlapsWhereClause());
            executions.Add(new ExprDTIntervalOverlappedByWhereClause());
            executions.Add(new ExprDTIntervalStartsWhereClause());
            executions.Add(new ExprDTIntervalStartedByWhereClause());
            executions.Add(new ExprDTIntervalPointInTimeWCalendarOps());
            executions.Add(new ExprDTIntervalBeforeWVariable());
            executions.Add(new ExprDTIntervalTimePeriodWYearNonConst());
            return executions;
        }

        private static void SetVStartEndVariables(
            RegressionEnvironment env,
            long vstart,
            long vend)
        {
            env.Runtime.VariableService.SetVariableValue(null, "V_START", vstart);
            env.Runtime.VariableService.SetVariableValue(null, "V_END", vend);
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
            var epl = "@Name('s0') select * from A_" +
                      fieldType.GetName() +
                      "#lastevent as a, B_" +
                      fieldType.GetName() +
                      "#lastevent as b " +
                      "where " +
                      whereClause;
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventObjectArray(
                new[] {fieldType.MakeStart(seedTime), fieldType.MakeEnd(seedTime, seedDuration)},
                "B_" + fieldType.GetName());

            foreach (var test in timestampsAndResult) {
                var testtime = (string) test[0];
                var testduration = test[1].AsInt64();
                var expected = (bool) test[2];

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
                    new[] {
                        fieldType.MakeStart(testtime),
                        fieldType.MakeEnd(testtime, testduration)
                    },
                    "A_" + fieldType.GetName());

                if (!env.Listener("s0").IsInvoked && expected) {
                    Assert.Fail("Expected but not received for " + message);
                }

                if (env.Listener("s0").IsInvoked && !expected) {
                    Assert.Fail("Not expected but received for " + message);
                }

                env.Listener("s0").Reset();
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
                "@Name('s0') select * from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b where " +
                whereClause;
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(SupportTimeStartEndB.Make("B", seedTime, seedDuration));

            foreach (var test in timestampsAndResult) {
                var testtime = (string) test[0];
                var testduration = test[1].AsInt64();
                var expected = (bool) test[2];

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

                if (!env.Listener("s0").IsInvoked && expected) {
                    Assert.Fail("Expected but not received for " + message);
                }

                if (env.Listener("s0").IsInvoked && !expected) {
                    Assert.Fail("Not expected but received for " + message);
                }

                env.Listener("s0").Reset();
            }

            env.UndeployAll();
        }

        internal class ExprDTIntervalBeforeWVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create variable int somenumber = 1;\n" +
                          "@Name('s0') select LongDate.before(LongDate, somenumber) as c0 from SupportDateTime;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(SupportDateTime.Make("2002-05-30T09:00:00.000"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "c0" },
                    new object[] {false});

                env.UndeployAll();
            }
        }

        internal class ExprDTIntervalTimePeriodWYearNonConst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create variable int somenumber = 1", path);

                var epl = "@Name('s0') select " +
                          "LongDate.before(LongDate, somenumber years) as c0," +
                          "LongDate.before(LongDate, somenumber month) as c1, " +
                          "LongDate.before(LongDate, somenumber weeks) as c2, " +
                          "LongDate.before(LongDate, somenumber days) as c3, " +
                          "LongDate.before(LongDate, somenumber hours) as c4, " +
                          "LongDate.before(LongDate, somenumber minutes) as c5, " +
                          "LongDate.before(LongDate, somenumber seconds) as c6, " +
                          "LongDate.before(LongDate, somenumber milliseconds) as c7, " +
                          "LongDate.before(LongDate, somenumber microseconds) as c8 " +
                          " from SupportDateTime";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(SupportDateTime.Make("2002-05-30T09:00:00.000"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "c0" },
                    new object[] {false});

                env.UndeployAll();
            }
        }

        internal class ExprDTIntervalPointInTimeWCalendarOps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1", "c2" };
                var epl = "@Name('s0') select " +
                          "LongDate.set('month', 1).before(LongPrimitive) as c0, " +
                          "DateTimeOffset.set('month', 1).before(LongPrimitive) as c1," +
                          "DateTimeEx.set('month', 1).before(LongPrimitive) as c2 " +
                          "from SupportDateTime unidirectional, SupportBean#lastevent";
                env.CompileDeploy(epl).AddListener("s0");

                var bean = new SupportBean();
                bean.LongPrimitive = DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:00:00.000");
                env.SendEventBean(bean);

                env.SendEventBean(SupportDateTime.Make("2002-05-30T09:00:00.000"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true, true});

                env.SendEventBean(SupportDateTime.Make("2003-05-30T08:00:00.000"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false, false});

                env.UndeployAll();
            }
        }

        internal class ExprDTIntervalCalendarOps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var seedTime = "2002-05-30T09:00:00.000"; // seed is time for B

                object[][] expected = {
                    new object[] {"2999-01-01T09:00:00.001", 0, true} // sending in A
                };
                AssertExpression(env, seedTime, 0, "a.withDate(2001, 1, 1).before(b)", expected, null);

                expected = new[] {
                    new object[] {"2999-01-01T10:00:00.001", 0, false},
                    new object[] {"2999-01-01T08:00:00.001", 0, true}
                };
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.withDate(2001, 1, 1).before(b.withDate(2001, 1, 1))",
                    expected,
                    null);

                // Test end-timestamp preserved when using calendar op
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 2000, false}
                };
                AssertExpression(env, seedTime, 0, "a.before(b)", expected, null);
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 2000, false}
                };
                AssertExpression(env, seedTime, 0, "a.withTime(8, 59, 59, 0).before(b)", expected, null);

                // Test end-timestamp preserved when using calendar op
                expected = new[] {
                    new object[] {"2002-05-30T09:00:01.000", 0, false},
                    new object[] {"2002-05-30T09:00:01.001", 0, true}
                };
                AssertExpression(env, seedTime, 1000, "a.after(b)", expected, null);

                // NOT YET SUPPORTED (a documented limitation of datetime methods)
                // assertExpression(seedTime, 0, "a.after(b.withTime(9, 0, 0, 0))", expected, null);   // the "b.withTime(...) must retain the end-timestamp correctness (a documented limitation)
            }
        }

        internal class ExprDTIntervalInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // wrong 1st parameter - string
                TryInvalidCompile(
                    env,
                    "select a.before('x') from SupportTimeStartEndA as a",
                    "Failed to validate select-clause expression 'a.before('x')': Failed to resolve enumeration method, date-time method or mapped property 'a.before('x')': For date-time method 'before' the first parameter expression returns 'System.String', however requires a Date, DateTimeEx, Long-type return value or event (with timestamp)");

                // wrong 1st parameter - event not defined with timestamp expression
                TryInvalidCompile(
                    env,
                    "select a.before(b) from SupportTimeStartEndA#lastevent as a, SupportBean#lastevent as b",
                    "Failed to validate select-clause expression 'a.before(b)': For date-time method 'before' the first parameter is event type 'SupportBean', however no timestamp property has been defined for this event type");

                // wrong 1st parameter - boolean
                TryInvalidCompile(
                    env,
                    "select a.before(true) from SupportTimeStartEndA#lastevent as a, SupportBean#lastevent as b",
                    "Failed to validate select-clause expression 'a.before(true)': For date-time method 'before' the first parameter expression returns 'System.Boolean', however requires a Date, DateTimeEx, Long-type return value or event (with timestamp)");

                // wrong zero parameters
                TryInvalidCompile(
                    env,
                    "select a.before() from SupportTimeStartEndA#lastevent as a, SupportBean#lastevent as b",
                    "Failed to validate select-clause expression 'a.before()': Parameters mismatch for date-time method 'before', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing interval start value, or an expression providing timestamp or timestamped-event and an expression providing interval start value and an expression providing interval finishes value, but receives no parameters");

                // wrong target
                TryInvalidCompile(
                    env,
                    "select TheString.before(a) from SupportTimeStartEndA#lastevent as a, SupportBean#lastevent as b",
                    "Failed to validate select-clause expression 'TheString.before(a)': Date-time enumeration method 'before' requires either a DateTimeEx, DateTimeOffset, DateTime, or long value as input or events of an event type that declares a timestamp property but received System.String");
                TryInvalidCompile(
                    env,
                    "select b.before(a) from SupportTimeStartEndA#lastevent as a, SupportBean#lastevent as b",
                    "Failed to validate select-clause expression 'b.before(a)': Date-time enumeration method 'before' requires either a DateTimeEx, DateTimeOffset, DateTime, or long value as input or events of an event type that declares a timestamp property");
                TryInvalidCompile(
                    env,
                    "select a.get('month').before(a) from SupportTimeStartEndA#lastevent as a, SupportBean#lastevent as b",
                    "Failed to validate select-clause expression 'a.get(\"month\").before(a)': Failed to resolve method 'get': Could not find enumeration method, date-time method, instance method or property named 'get'");
                
                // test before/after
                TryInvalidCompile(
                    env,
                    "select a.before(b, 'abc') from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.before(b,\"abc\")': Failed to validate date-time method 'before', expected a time-period expression or a numeric-type result for expression parameter 1 but received System.String ");
                TryInvalidCompile(
                    env,
                    "select a.before(b, 1, 'def') from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.before(b,1,\"def\")': Failed to validate date-time method 'before', expected a time-period expression or a numeric-type result for expression parameter 2 but received System.String ");
                TryInvalidCompile(
                    env,
                    "select a.before(b, 1, 2, 3) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.before(b,1,2,3)': Parameters mismatch for date-time method 'before', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing interval start value, or an expression providing timestamp or timestamped-event and an expression providing interval start value and an expression providing interval finishes value, but receives 4 expressions ");

                // test coincides
                TryInvalidCompile(
                    env,
                    "select a.coincides(b, 1, 2, 3) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.coincides(b,1,2,3)': Parameters mismatch for date-time method 'coincides', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing threshold for start and end value, or an expression providing timestamp or timestamped-event and an expression providing threshold for start value and an expression providing threshold for end value, but receives 4 expressions ");
                TryInvalidCompile(
                    env,
                    "select a.coincides(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.coincides(b,-1)': The coincides date-time method does not allow negative start and end values ");

                // test during+interval
                TryInvalidCompile(
                    env,
                    "select a.during(b, 1, 2, 3) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.during(b,1,2,3)': Parameters mismatch for date-time method 'during', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance interval both start and end and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance start and an expression providing maximum distance start and an expression providing minimum distance end and an expression providing maximum distance end, but receives 4 expressions ");

                // test finishes+finished-by
                TryInvalidCompile(
                    env,
                    "select a.finishes(b, 1, 2) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.finishes(b,1,2)': Parameters mismatch for date-time method 'finishes', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between end timestamps, but receives 3 expressions ");
                TryInvalidCompile(
                    env,
                    "select a.finishes(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.finishes(b,-1)': The finishes date-time method does not allow negative threshold value ");
                TryInvalidCompile(
                    env,
                    "select a.finishedby(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.finishedby(b,-1)': The finishedby date-time method does not allow negative threshold value ");

                // test meets+met-by
                TryInvalidCompile(
                    env,
                    "select a.meets(b, 1, 2) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.meets(b,1,2)': Parameters mismatch for date-time method 'meets', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between start and end timestamps, but receives 3 expressions ");
                TryInvalidCompile(
                    env,
                    "select a.meets(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.meets(b,-1)': The meets date-time method does not allow negative threshold value ");
                TryInvalidCompile(
                    env,
                    "select a.metBy(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.metBy(b,-1)': The metBy date-time method does not allow negative threshold value ");

                // test overlaps+overlapped-by
                TryInvalidCompile(
                    env,
                    "select a.overlaps(b, 1, 2, 3) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.overlaps(b,1,2,3)': Parameters mismatch for date-time method 'overlaps', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance interval both start and end and an expression providing maximum distance interval both start and end, but receives 4 expressions ");

                // test start/startedby
                TryInvalidCompile(
                    env,
                    "select a.starts(b, 1, 2, 3) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.starts(b,1,2,3)': Parameters mismatch for date-time method 'starts', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between start timestamps, but receives 4 expressions ");
                TryInvalidCompile(
                    env,
                    "select a.starts(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.starts(b,-1)': The starts date-time method does not allow negative threshold value ");
                TryInvalidCompile(
                    env,
                    "select a.startedBy(b, -1) from SupportTimeStartEndA#lastevent as a, SupportTimeStartEndB#lastevent as b",
                    "Failed to validate select-clause expression 'a.startedBy(b,-1)': The startedBy date-time method does not allow negative threshold value ");
            }
        }

        internal class ExprDTIntervalBeforeInSelectClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1" };
                var epl = "@Name('s0') select " +
                          "a.LongdateStart.before(b.LongdateStart) as c0," +
                          "a.before(b) as c1 " +
                          " from SupportTimeStartEndA#lastevent as a, " +
                          "      SupportTimeStartEndB#lastevent as b";
                env.CompileDeploy(epl).AddListener("s0");
                SupportEventPropUtil.AssertTypesAllSame(env.Statement("s0").EventType, fields, typeof(bool?));

                env.SendEventBean(SupportTimeStartEndB.Make("B1", "2002-05-30T09:00:00.000", 0));

                env.SendEventBean(SupportTimeStartEndA.Make("A1", "2002-05-30T08:59:59.000", 0));
                EPAssertionUtil.AssertPropsAllValuesSame(env.Listener("s0").AssertOneGetNewAndReset(), fields, true);

                env.SendEventBean(SupportTimeStartEndA.Make("A2", "2002-05-30T08:59:59.950", 0));
                EPAssertionUtil.AssertPropsAllValuesSame(env.Listener("s0").AssertOneGetNewAndReset(), fields, true);

                env.UndeployAll();
            }
        }

        internal class ExprDTIntervalBeforeWhereClauseWithBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new BeforeValidator(1L, long.MaxValue);
                object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 0, true},
                    new object[] {"2002-05-30T08:59:59.999", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false}
                };

                string[] expressions = {
                    "a.before(b)",
                    "a.before(b, 1 millisecond)",
                    "a.before(b, 1 millisecond, 1000000000L)",
                    "a.LongdateStart.before(b)",
                    "a.DateTimeStart.before(b)",
                    "a.DateTimeExStart.before(b)",
                    "a.before(b.LongdateStart)",
                    "a.before(b.DateTimeStart)",
                    "a.before(b.DateTimeExStart)",
                    "a.LongdateStart.before(b.LongdateStart)",
                    "a.LongdateStart.before(b.LongdateStart)",
                    "a.DateTimeStart.before(b.DateTimeStart)",
                    "a.DateTimeExStart.before(b.DateTimeExStart)",
                    "a.DateTimeStart.before(b.DateTimeExStart)",
                    "a.DateTimeStart.before(b.LongdateStart)",
                    "a.DateTimeExStart.before(b.DateTimeStart)",
                    "a.DateTimeExStart.before(b.LongdateStart)"
                };
                var seedTime = "2002-05-30T09:00:00.000";
                foreach (var expression in expressions) {
                    AssertExpressionBean(env, seedTime, 0, expression, expected, expectedValidator);
                }
            }
        }

        internal class ExprDTIntervalBeforeWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string seedTime;
                object[][] expected;
                BeforeValidator expectedValidator;

                seedTime = "2002-05-30T09:00:00.000";
                expectedValidator = new BeforeValidator(1L, long.MaxValue);
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, true},
                    new object[] {"2002-05-30T08:59:59.000", 999, true},
                    new object[] {"2002-05-30T08:59:59.000", 1000, false},
                    new object[] {"2002-05-30T08:59:59.000", 1001, false},
                    new object[] {"2002-05-30T08:59:59.999", 0, true},
                    new object[] {"2002-05-30T08:59:59.999", 1, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false}
                };
                AssertExpression(env, seedTime, 0, "a.before(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 100000, "a.before(b)", expected, expectedValidator);

                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, true},
                    new object[] {"2002-05-30T08:59:59.899", 0, true},
                    new object[] {"2002-05-30T08:59:59.900", 0, true},
                    new object[] {"2002-05-30T08:59:59.901", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false}
                };
                expectedValidator = new BeforeValidator(100L, long.MaxValue);
                AssertExpression(env, seedTime, 0, "a.before(b, 100 milliseconds)", expected, expectedValidator);
                AssertExpression(env, seedTime, 100000, "a.before(b, 100 milliseconds)", expected, expectedValidator);

                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T08:59:59.499", 0, false},
                    new object[] {"2002-05-30T08:59:59.499", 1, true},
                    new object[] {"2002-05-30T08:59:59.500", 0, true},
                    new object[] {"2002-05-30T08:59:59.500", 1, true},
                    new object[] {"2002-05-30T08:59:59.500", 400, true},
                    new object[] {"2002-05-30T08:59:59.500", 401, false},
                    new object[] {"2002-05-30T08:59:59.899", 0, true},
                    new object[] {"2002-05-30T08:59:59.899", 2, false},
                    new object[] {"2002-05-30T08:59:59.900", 0, true},
                    new object[] {"2002-05-30T08:59:59.900", 1, false},
                    new object[] {"2002-05-30T08:59:59.901", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false}
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
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T08:59:59.199", 0, false},
                    new object[] {"2002-05-30T08:59:59.199", 1, true},
                    new object[] {"2002-05-30T08:59:59.200", 0, true},
                    new object[] {"2002-05-30T08:59:59.800", 0, true},
                    new object[] {"2002-05-30T08:59:59.801", 0, false}
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
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.500", 0, false},
                    new object[] {"2002-05-30T09:00:00.990", 0, false},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.500", 0, true},
                    new object[] {"2002-05-30T09:00:00.501", 0, false}
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
                expected = new[] {
                    new object[] {"2002-02-01T09:00:00.000", 0, true},
                    new object[] {"2002-02-01T09:00:00.001", 0, false}
                };
                expectedValidator = new BeforeValidator(GetMillisecForDays(28), long.MaxValue);
                AssertExpression(env, seedTime, 100, "a.before(b, 1 month)", expected, expectedValidator);

                expected = new[] {
                    new object[] {"2002-01-01T08:59:59.999", 0, false},
                    new object[] {"2002-01-01T09:00:00.000", 0, true},
                    new object[] {"2002-01-11T09:00:00.000", 0, true},
                    new object[] {"2002-02-01T09:00:00.000", 0, true},
                    new object[] {"2002-02-01T09:00:00.001", 0, false}
                };
                expectedValidator = new BeforeValidator(GetMillisecForDays(28), GetMillisecForDays(28 + 31));
                AssertExpression(env, seedTime, 100, "a.before(b, 1 month, 2 month)", expected, expectedValidator);
            }
        }

        internal class ExprDTIntervalAfterWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new AfterValidator(1L, long.MaxValue);
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, true}
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

                expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.002", 0, true}
                };
                AssertExpression(env, seedTime, 1, "a.after(b)", expected, expectedValidator);
                AssertExpression(
                    env,
                    seedTime,
                    1,
                    "a.after(b, 1 millisecond, 1000000000L)",
                    expected,
                    expectedValidator);

                expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.099", 0, false},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.101", 0, true}
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

                expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.099", 0, false},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.500", 0, true},
                    new object[] {"2002-05-30T09:00:00.501", 0, false}
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
                expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.199", 0, false},
                    new object[] {"2002-05-30T09:00:00.200", 0, true},
                    new object[] {"2002-05-30T09:00:00.800", 0, true},
                    new object[] {"2002-05-30T09:00:00.801", 0, false}
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
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.599", 0, false},
                    new object[] {"2002-05-30T08:59:59.600", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.001", 0, false}
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
                expected = new[] {
                    new object[] {"2002-03-01T09:00:00.099", 0, false},
                    new object[] {"2002-03-01T09:00:00.100", 0, true}
                };
                expectedValidator = new AfterValidator(GetMillisecForDays(28), long.MaxValue);
                AssertExpression(env, seedTime, 100, "a.after(b, 1 month)", expected, expectedValidator);

                expected = new[] {
                    new object[] {"2002-03-01T09:00:00.099", 0, false},
                    new object[] {"2002-03-01T09:00:00.100", 0, true},
                    new object[] {"2002-04-01T09:00:00.100", 0, true},
                    new object[] {"2002-04-01T09:00:00.101", 0, false}
                };
                AssertExpression(env, seedTime, 100, "a.after(b, 1 month, 2 month)", expected, null);
            }
        }

        internal class ExprDTIntervalCoincidesWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new CoincidesValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.001", 0, false}
                };
                AssertExpression(env, seedTime, 0, "a.coincides(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 0, "a.coincides(b, 0 millisecond)", expected, expectedValidator);
                AssertExpression(env, seedTime, 0, "a.coincides(b, 0, 0)", expected, expectedValidator);
                AssertExpression(env, seedTime, 0, "a.startTS.coincides(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 0, "a.coincides(b.startTS)", expected, expectedValidator);

                expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 1, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 1, false}
                };
                AssertExpression(env, seedTime, 1, "a.coincides(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 1, "a.coincides(b, 0, 0)", expected, expectedValidator);

                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.899", 0, false},
                    new object[] {"2002-05-30T08:59:59.900", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 50, true},
                    new object[] {"2002-05-30T09:00:00.000", 100, true},
                    new object[] {"2002-05-30T09:00:00.000", 101, false},
                    new object[] {"2002-05-30T09:00:00.099", 0, true},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.101", 0, false}
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

                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.799", 0, false},
                    new object[] {"2002-05-30T08:59:59.800", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.099", 0, true},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.200", 0, true},
                    new object[] {"2002-05-30T09:00:00.201", 0, false}
                };
                expectedValidator = new CoincidesValidator(200L, 500L);
                AssertExpression(
                    env,
                    seedTime,
                    0,
                    "a.coincides(b, 200 milliseconds, 500 milliseconds)",
                    expected,
                    expectedValidator);

                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.799", 0, false},
                    new object[] {"2002-05-30T08:59:59.799", 200, false},
                    new object[] {"2002-05-30T08:59:59.799", 201, false},
                    new object[] {"2002-05-30T08:59:59.800", 0, false},
                    new object[] {"2002-05-30T08:59:59.800", 199, false},
                    new object[] {"2002-05-30T08:59:59.800", 200, true},
                    new object[] {"2002-05-30T08:59:59.800", 300, true},
                    new object[] {"2002-05-30T08:59:59.800", 301, false},
                    new object[] {"2002-05-30T09:00:00.050", 0, true},
                    new object[] {"2002-05-30T09:00:00.099", 0, true},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.101", 0, false}
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
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.800", 0, false},
                    new object[] {"2002-05-30T08:59:59.800", 179, false},
                    new object[] {"2002-05-30T08:59:59.800", 180, true},
                    new object[] {"2002-05-30T08:59:59.800", 200, true},
                    new object[] {"2002-05-30T08:59:59.800", 320, true},
                    new object[] {"2002-05-30T08:59:59.800", 321, false}
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
                expected = new[] {
                    new object[] {"2002-02-15T09:00:00.099", GetMillisecForDays(28 + 14), true},
                    new object[] {"2002-01-01T08:00:00.000", GetMillisecForDays(28 + 30), false}
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

        internal class ExprDTIntervalDuringWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new DuringValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, true},
                    new object[] {"2002-05-30T09:00:00.001", 98, true},
                    new object[] {"2002-05-30T09:00:00.001", 99, false},
                    new object[] {"2002-05-30T09:00:00.099", 0, true},
                    new object[] {"2002-05-30T09:00:00.099", 1, false},
                    new object[] {"2002-05-30T09:00:00.100", 0, false}
                };
                AssertExpression(env, seedTime, 100, "a.during(b)", expected, expectedValidator);

                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 1, false}
                };
                AssertExpression(env, seedTime, 0, "a.during(b)", expected, expectedValidator);

                expected = new[] {
                    new object[] {"2002-05-30T09:00:00.001", 0, true},
                    new object[] {"2002-05-30T09:00:00.001", 2000000, true}
                };
                AssertExpression(
                    env,
                    seedTime,
                    100,
                    "a.startTS.during(b)",
                    expected,
                    null); // want to use null-validator here

                // test 1-parameter footprint
                expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 100, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 83, false},
                    new object[] {"2002-05-30T09:00:00.001", 84, true},
                    new object[] {"2002-05-30T09:00:00.001", 98, true},
                    new object[] {"2002-05-30T09:00:00.001", 99, false},
                    new object[] {"2002-05-30T09:00:00.015", 69, false},
                    new object[] {"2002-05-30T09:00:00.015", 70, true},
                    new object[] {"2002-05-30T09:00:00.015", 84, true},
                    new object[] {"2002-05-30T09:00:00.015", 85, false},
                    new object[] {"2002-05-30T09:00:00.016", 80, false},
                    new object[] {"2002-05-30T09:00:00.099", 0, false}
                };
                expectedValidator = new DuringValidator(15L);
                AssertExpression(env, seedTime, 100, "a.during(b, 15 milliseconds)", expected, expectedValidator);

                // test 2-parameter footprint
                expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 100, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 78, false},
                    new object[] {"2002-05-30T09:00:00.001", 79, false},
                    new object[] {"2002-05-30T09:00:00.004", 85, false},
                    new object[] {"2002-05-30T09:00:00.005", 74, false},
                    new object[] {"2002-05-30T09:00:00.005", 75, true},
                    new object[] {"2002-05-30T09:00:00.005", 90, true},
                    new object[] {"2002-05-30T09:00:00.005", 91, false},
                    new object[] {"2002-05-30T09:00:00.006", 83, true},
                    new object[] {"2002-05-30T09:00:00.020", 76, false},
                    new object[] {"2002-05-30T09:00:00.020", 75, true},
                    new object[] {"2002-05-30T09:00:00.020", 60, true},
                    new object[] {"2002-05-30T09:00:00.020", 59, false},
                    new object[] {"2002-05-30T09:00:00.021", 68, false},
                    new object[] {"2002-05-30T09:00:00.099", 0, false}
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
                expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 100, false},
                    new object[] {"2002-05-30T09:00:00.004", 85, false},
                    new object[] {"2002-05-30T09:00:00.005", 64, false},
                    new object[] {"2002-05-30T09:00:00.005", 65, true},
                    new object[] {"2002-05-30T09:00:00.005", 85, true},
                    new object[] {"2002-05-30T09:00:00.005", 86, false},
                    new object[] {"2002-05-30T09:00:00.020", 49, false},
                    new object[] {"2002-05-30T09:00:00.020", 50, true},
                    new object[] {"2002-05-30T09:00:00.020", 70, true},
                    new object[] {"2002-05-30T09:00:00.020", 71, false},
                    new object[] {"2002-05-30T09:00:00.021", 55, false}
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

        internal class ExprDTIntervalFinishesWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new FinishesValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 98, false},
                    new object[] {"2002-05-30T09:00:00.001", 99, true},
                    new object[] {"2002-05-30T09:00:00.001", 100, false},
                    new object[] {"2002-05-30T09:00:00.050", 50, true},
                    new object[] {"2002-05-30T09:00:00.099", 0, false},
                    new object[] {"2002-05-30T09:00:00.099", 1, true},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.101", 0, false}
                };
                AssertExpression(env, seedTime, 100, "a.finishes(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 100, "a.finishes(b, 0)", expected, expectedValidator);
                AssertExpression(env, seedTime, 100, "a.finishes(b, 0 milliseconds)", expected, expectedValidator);

                expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 99, false},
                    new object[] {"2002-05-30T09:00:00.001", 93, false},
                    new object[] {"2002-05-30T09:00:00.001", 94, true},
                    new object[] {"2002-05-30T09:00:00.001", 100, true},
                    new object[] {"2002-05-30T09:00:00.001", 104, true},
                    new object[] {"2002-05-30T09:00:00.001", 105, false},
                    new object[] {"2002-05-30T09:00:00.050", 50, true},
                    new object[] {"2002-05-30T09:00:00.104", 0, true},
                    new object[] {"2002-05-30T09:00:00.104", 1, true},
                    new object[] {"2002-05-30T09:00:00.105", 0, true},
                    new object[] {"2002-05-30T09:00:00.105", 1, false}
                };
                expectedValidator = new FinishesValidator(5L);
                AssertExpression(env, seedTime, 100, "a.finishes(b, 5 milliseconds)", expected, expectedValidator);
            }
        }

        internal class ExprDTIntervalFinishedByWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new FinishedByValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T08:59:59.000", 1099, false},
                    new object[] {"2002-05-30T08:59:59.000", 1100, true},
                    new object[] {"2002-05-30T08:59:59.000", 1101, false},
                    new object[] {"2002-05-30T08:59:59.999", 100, false},
                    new object[] {"2002-05-30T08:59:59.999", 101, true},
                    new object[] {"2002-05-30T08:59:59.999", 102, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 50, false},
                    new object[] {"2002-05-30T09:00:00.000", 100, false}
                };
                AssertExpression(env, seedTime, 100, "a.finishedBy(b)", expected, expectedValidator);
                AssertExpression(env, seedTime, 100, "a.finishedBy(b, 0)", expected, expectedValidator);
                AssertExpression(env, seedTime, 100, "a.finishedBy(b, 0 milliseconds)", expected, expectedValidator);

                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T08:59:59.000", 1094, false},
                    new object[] {"2002-05-30T08:59:59.000", 1095, true},
                    new object[] {"2002-05-30T08:59:59.000", 1105, true},
                    new object[] {"2002-05-30T08:59:59.000", 1106, false},
                    new object[] {"2002-05-30T08:59:59.999", 95, false},
                    new object[] {"2002-05-30T08:59:59.999", 96, true},
                    new object[] {"2002-05-30T08:59:59.999", 106, true},
                    new object[] {"2002-05-30T08:59:59.999", 107, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 95, false},
                    new object[] {"2002-05-30T09:00:00.000", 100, false},
                    new object[] {"2002-05-30T09:00:00.000", 105, false}
                };
                expectedValidator = new FinishedByValidator(5L);
                AssertExpression(env, seedTime, 100, "a.finishedBy(b, 5 milliseconds)", expected, expectedValidator);
            }
        }

        internal class ExprDTIntervalIncludesByWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new IncludesValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 1100, false},
                    new object[] {"2002-05-30T08:59:59.000", 1101, true},
                    new object[] {"2002-05-30T08:59:59.000", 3000, true},
                    new object[] {"2002-05-30T08:59:59.999", 101, false},
                    new object[] {"2002-05-30T08:59:59.999", 102, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 50, false},
                    new object[] {"2002-05-30T09:00:00.000", 102, false}
                };
                AssertExpression(env, seedTime, 100, "a.includes(b)", expected, expectedValidator);

                // test 1-parameter form
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T08:59:59.000", 1100, false},
                    new object[] {"2002-05-30T08:59:59.000", 1105, false},
                    new object[] {"2002-05-30T08:59:59.994", 106, false},
                    new object[] {"2002-05-30T08:59:59.994", 110, false},
                    new object[] {"2002-05-30T08:59:59.995", 105, false},
                    new object[] {"2002-05-30T08:59:59.995", 106, true},
                    new object[] {"2002-05-30T08:59:59.995", 110, true},
                    new object[] {"2002-05-30T08:59:59.995", 111, false},
                    new object[] {"2002-05-30T08:59:59.999", 101, false},
                    new object[] {"2002-05-30T08:59:59.999", 102, true},
                    new object[] {"2002-05-30T08:59:59.999", 106, true},
                    new object[] {"2002-05-30T08:59:59.999", 107, false},
                    new object[] {"2002-05-30T09:00:00.000", 105, false},
                    new object[] {"2002-05-30T09:00:00.000", 106, false}
                };
                expectedValidator = new IncludesValidator(5L);
                AssertExpression(env, seedTime, 100, "a.includes(b, 5 milliseconds)", expected, expectedValidator);

                // test 2-parameter form
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T08:59:59.000", 1100, false},
                    new object[] {"2002-05-30T08:59:59.000", 1105, false},
                    new object[] {"2002-05-30T08:59:59.979", 130, false},
                    new object[] {"2002-05-30T08:59:59.980", 124, false},
                    new object[] {"2002-05-30T08:59:59.980", 125, true},
                    new object[] {"2002-05-30T08:59:59.980", 140, true},
                    new object[] {"2002-05-30T08:59:59.980", 141, false},
                    new object[] {"2002-05-30T08:59:59.995", 109, false},
                    new object[] {"2002-05-30T08:59:59.995", 110, true},
                    new object[] {"2002-05-30T08:59:59.995", 125, true},
                    new object[] {"2002-05-30T08:59:59.995", 126, false},
                    new object[] {"2002-05-30T08:59:59.996", 112, false}
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
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T08:59:59.000", 1100, false},
                    new object[] {"2002-05-30T08:59:59.000", 1105, false},
                    new object[] {"2002-05-30T08:59:59.979", 150, false},
                    new object[] {"2002-05-30T08:59:59.980", 129, false},
                    new object[] {"2002-05-30T08:59:59.980", 130, true},
                    new object[] {"2002-05-30T08:59:59.980", 150, true},
                    new object[] {"2002-05-30T08:59:59.980", 151, false},
                    new object[] {"2002-05-30T08:59:59.995", 114, false},
                    new object[] {"2002-05-30T08:59:59.995", 115, true},
                    new object[] {"2002-05-30T08:59:59.995", 135, true},
                    new object[] {"2002-05-30T08:59:59.995", 136, false},
                    new object[] {"2002-05-30T08:59:59.996", 124, false}
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

        internal class ExprDTIntervalMeetsWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new MeetsValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 1000, true},
                    new object[] {"2002-05-30T08:59:59.000", 1001, false},
                    new object[] {"2002-05-30T08:59:59.998", 1, false},
                    new object[] {"2002-05-30T08:59:59.999", 1, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 1, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false}
                };
                AssertExpression(env, seedTime, 0, "a.meets(b)", expected, expectedValidator);

                // test 1-parameter form
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T08:59:59.000", 994, false},
                    new object[] {"2002-05-30T08:59:59.000", 995, true},
                    new object[] {"2002-05-30T08:59:59.000", 1005, true},
                    new object[] {"2002-05-30T08:59:59.000", 1006, false},
                    new object[] {"2002-05-30T08:59:59.994", 0, false},
                    new object[] {"2002-05-30T08:59:59.994", 1, true},
                    new object[] {"2002-05-30T08:59:59.995", 0, true},
                    new object[] {"2002-05-30T08:59:59.999", 0, true},
                    new object[] {"2002-05-30T08:59:59.999", 1, true},
                    new object[] {"2002-05-30T08:59:59.999", 6, true},
                    new object[] {"2002-05-30T08:59:59.999", 7, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 1, true},
                    new object[] {"2002-05-30T09:00:00.000", 5, true},
                    new object[] {"2002-05-30T09:00:00.005", 0, true},
                    new object[] {"2002-05-30T09:00:00.005", 1, false}
                };
                expectedValidator = new MeetsValidator(5L);
                AssertExpression(env, seedTime, 0, "a.meets(b, 5 milliseconds)", expected, expectedValidator);
            }
        }

        internal class ExprDTIntervalMetByWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new MetByValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] {"2002-05-30T09:00:00.990", 0, false},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.100", 500, true},
                    new object[] {"2002-05-30T09:00:00.101", 0, false}
                };
                AssertExpression(env, seedTime, 100, "a.metBy(b)", expected, expectedValidator);

                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.999", 1, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 1, true}
                };
                AssertExpression(env, seedTime, 0, "a.metBy(b)", expected, expectedValidator);

                // test 1-parameter form
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.994", 0, false},
                    new object[] {"2002-05-30T08:59:59.994", 5, false},
                    new object[] {"2002-05-30T08:59:59.995", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 20, true},
                    new object[] {"2002-05-30T09:00:00.005", 0, true},
                    new object[] {"2002-05-30T09:00:00.005", 1000, true},
                    new object[] {"2002-05-30T09:00:00.006", 0, false}
                };
                expectedValidator = new MetByValidator(5L);
                AssertExpression(env, seedTime, 0, "a.metBy(b, 5 milliseconds)", expected, expectedValidator);

                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.994", 0, false},
                    new object[] {"2002-05-30T08:59:59.994", 5, false},
                    new object[] {"2002-05-30T08:59:59.995", 0, false},
                    new object[] {"2002-05-30T09:00:00.094", 0, false},
                    new object[] {"2002-05-30T09:00:00.095", 0, true},
                    new object[] {"2002-05-30T09:00:00.105", 0, true},
                    new object[] {"2002-05-30T09:00:00.105", 5000, true},
                    new object[] {"2002-05-30T09:00:00.106", 0, false}
                };
                expectedValidator = new MetByValidator(5L);
                AssertExpression(env, seedTime, 100, "a.metBy(b, 5 milliseconds)", expected, expectedValidator);
            }
        }

        internal class ExprDTIntervalOverlapsWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new OverlapsValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 1000, false},
                    new object[] {"2002-05-30T08:59:59.000", 1001, true},
                    new object[] {"2002-05-30T08:59:59.000", 1050, true},
                    new object[] {"2002-05-30T08:59:59.000", 1099, true},
                    new object[] {"2002-05-30T08:59:59.000", 1100, false},
                    new object[] {"2002-05-30T08:59:59.999", 1, false},
                    new object[] {"2002-05-30T08:59:59.999", 2, true},
                    new object[] {"2002-05-30T08:59:59.999", 100, true},
                    new object[] {"2002-05-30T08:59:59.999", 101, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false}
                };
                AssertExpression(env, seedTime, 100, "a.overlaps(b)", expected, expectedValidator);

                // test 1-parameter form (overlap by not more then X msec)
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 1000, false},
                    new object[] {"2002-05-30T08:59:59.000", 1001, true},
                    new object[] {"2002-05-30T08:59:59.000", 1005, true},
                    new object[] {"2002-05-30T08:59:59.000", 1006, false},
                    new object[] {"2002-05-30T08:59:59.000", 1100, false},
                    new object[] {"2002-05-30T08:59:59.999", 1, false},
                    new object[] {"2002-05-30T08:59:59.999", 2, true},
                    new object[] {"2002-05-30T08:59:59.999", 6, true},
                    new object[] {"2002-05-30T08:59:59.999", 7, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 5, false}
                };
                expectedValidator = new OverlapsValidator(5L);
                AssertExpression(env, seedTime, 100, "a.overlaps(b, 5 milliseconds)", expected, expectedValidator);

                // test 2-parameter form (overlap by min X and not more then Y msec)
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 1004, false},
                    new object[] {"2002-05-30T08:59:59.000", 1005, true},
                    new object[] {"2002-05-30T08:59:59.000", 1010, true},
                    new object[] {"2002-05-30T08:59:59.000", 1011, false},
                    new object[] {"2002-05-30T08:59:59.999", 5, false},
                    new object[] {"2002-05-30T08:59:59.999", 6, true},
                    new object[] {"2002-05-30T08:59:59.999", 11, true},
                    new object[] {"2002-05-30T08:59:59.999", 12, false},
                    new object[] {"2002-05-30T08:59:59.999", 12, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 5, false}
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

        internal class ExprDTIntervalOverlappedByWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new OverlappedByValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 1000, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 1, false},
                    new object[] {"2002-05-30T09:00:00.001", 99, false},
                    new object[] {"2002-05-30T09:00:00.001", 100, true},
                    new object[] {"2002-05-30T09:00:00.099", 1, false},
                    new object[] {"2002-05-30T09:00:00.099", 2, true},
                    new object[] {"2002-05-30T09:00:00.100", 0, false},
                    new object[] {"2002-05-30T09:00:00.100", 1, false}
                };
                AssertExpression(env, seedTime, 100, "a.overlappedBy(b)", expected, expectedValidator);

                // test 1-parameter form (overlap by not more then X msec)
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 1000, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 1, false},
                    new object[] {"2002-05-30T09:00:00.001", 99, false},
                    new object[] {"2002-05-30T09:00:00.094", 7, false},
                    new object[] {"2002-05-30T09:00:00.094", 100, false},
                    new object[] {"2002-05-30T09:00:00.095", 5, false},
                    new object[] {"2002-05-30T09:00:00.095", 6, true},
                    new object[] {"2002-05-30T09:00:00.095", 100, true},
                    new object[] {"2002-05-30T09:00:00.099", 1, false},
                    new object[] {"2002-05-30T09:00:00.099", 2, true},
                    new object[] {"2002-05-30T09:00:00.099", 100, true},
                    new object[] {"2002-05-30T09:00:00.100", 100, false}
                };
                expectedValidator = new OverlappedByValidator(5L);
                AssertExpression(env, seedTime, 100, "a.overlappedBy(b, 5 milliseconds)", expected, expectedValidator);

                // test 2-parameter form (overlap by min X and not more then Y msec)
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 1000, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 1, false},
                    new object[] {"2002-05-30T09:00:00.001", 99, false},
                    new object[] {"2002-05-30T09:00:00.089", 14, false},
                    new object[] {"2002-05-30T09:00:00.090", 10, false},
                    new object[] {"2002-05-30T09:00:00.090", 11, true},
                    new object[] {"2002-05-30T09:00:00.090", 1000, true},
                    new object[] {"2002-05-30T09:00:00.095", 5, false},
                    new object[] {"2002-05-30T09:00:00.095", 6, true},
                    new object[] {"2002-05-30T09:00:00.096", 5, false},
                    new object[] {"2002-05-30T09:00:00.096", 100, false},
                    new object[] {"2002-05-30T09:00:00.100", 100, false}
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

        internal class ExprDTIntervalStartsWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new StartsValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.999", 100, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 1, true},
                    new object[] {"2002-05-30T09:00:00.000", 99, true},
                    new object[] {"2002-05-30T09:00:00.000", 100, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false}
                };
                AssertExpression(env, seedTime, 100, "a.starts(b)", expected, expectedValidator);

                // test 1-parameter form (max distance between start times)
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.994", 6, false},
                    new object[] {"2002-05-30T08:59:59.995", 0, true},
                    new object[] {"2002-05-30T08:59:59.995", 104, true},
                    new object[] {"2002-05-30T08:59:59.995", 105, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 1, true},
                    new object[] {"2002-05-30T09:00:00.000", 99, true},
                    new object[] {"2002-05-30T09:00:00.000", 100, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, true},
                    new object[] {"2002-05-30T09:00:00.005", 94, true},
                    new object[] {"2002-05-30T09:00:00.005", 95, false},
                    new object[] {"2002-05-30T09:00:00.005", 100, false}
                };
                expectedValidator = new StartsValidator(5L);
                AssertExpression(env, seedTime, 100, "a.starts(b, 5 milliseconds)", expected, expectedValidator);
            }
        }

        internal class ExprDTIntervalStartedByWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Validator expectedValidator = new StartedByValidator();
                var seedTime = "2002-05-30T09:00:00.000";
                object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.999", 100, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 100, false},
                    new object[] {"2002-05-30T09:00:00.000", 101, true},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 101, false}
                };
                AssertExpression(env, seedTime, 100, "a.startedBy(b)", expected, expectedValidator);

                // test 1-parameter form (max distance between start times)
                expected = new[] {
                    new object[] {"2002-05-30T08:59:59.994", 6, false},
                    new object[] {"2002-05-30T08:59:59.995", 0, false},
                    new object[] {"2002-05-30T08:59:59.995", 105, false},
                    new object[] {"2002-05-30T08:59:59.995", 106, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 100, false},
                    new object[] {"2002-05-30T09:00:00.000", 101, true},
                    new object[] {"2002-05-30T09:00:00.001", 99, false},
                    new object[] {"2002-05-30T09:00:00.001", 100, true},
                    new object[] {"2002-05-30T09:00:00.005", 94, false},
                    new object[] {"2002-05-30T09:00:00.005", 95, false},
                    new object[] {"2002-05-30T09:00:00.005", 96, true}
                };
                expectedValidator = new StartedByValidator(5L);
                AssertExpression(env, seedTime, 100, "a.startedBy(b, 5 milliseconds)", expected, expectedValidator);
            }
        }

        private interface Validator
        {
            bool Validate(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd);
        }

        internal class BeforeValidator : Validator
        {
            private readonly long? end;
            private readonly long? start;

            internal BeforeValidator(
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

        internal class AfterValidator : Validator
        {
            private readonly long? end;
            private readonly long? start;

            internal AfterValidator(
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

        internal class CoincidesValidator : Validator
        {
            private readonly long? endThreshold;
            private readonly long? startThreshold;

            internal CoincidesValidator()
            {
                startThreshold = 0L;
                endThreshold = 0L;
            }

            internal CoincidesValidator(long? startThreshold)
            {
                this.startThreshold = startThreshold;
                endThreshold = startThreshold;
            }

            internal CoincidesValidator(
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

        internal class DuringValidator : Validator
        {
            private readonly int form;
            private readonly long? maxEndThreshold;
            private readonly long? maxStartThreshold;
            private readonly long? maxThreshold;
            private readonly long? minEndThreshold;
            private readonly long? minStartThreshold;
            private readonly long? minThreshold;
            private readonly long? threshold;

            internal DuringValidator()
            {
                form = 1;
            }

            internal DuringValidator(long? threshold)
            {
                form = 2;
                this.threshold = threshold;
            }

            internal DuringValidator(
                long? minThreshold,
                long? maxThreshold)
            {
                form = 3;
                this.minThreshold = minThreshold;
                this.maxThreshold = maxThreshold;
            }

            internal DuringValidator(
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

                if (form == 2) {
                    var distanceStart = leftStart - rightStart;
                    if (distanceStart <= 0 || distanceStart > threshold) {
                        return false;
                    }

                    var distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd <= 0 || distanceEnd > threshold);
                }

                if (form == 3) {
                    var distanceStart = leftStart - rightStart;
                    if (distanceStart < minThreshold || distanceStart > maxThreshold) {
                        return false;
                    }

                    var distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd < minThreshold || distanceEnd > maxThreshold);
                }

                if (form == 4) {
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

        internal class FinishesValidator : Validator
        {
            private readonly long? threshold;

            internal FinishesValidator()
            {
            }

            internal FinishesValidator(long? threshold)
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

                if (rightStart >= leftStart) {
                    return false;
                }

                var delta = Math.Abs(leftEnd - rightEnd);
                return delta <= threshold;
            }
        }

        internal class FinishedByValidator : Validator
        {
            private readonly long? threshold;

            internal FinishedByValidator()
            {
            }

            internal FinishedByValidator(long? threshold)
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

                if (leftStart >= rightStart) {
                    return false;
                }

                var delta = Math.Abs(leftEnd - rightEnd);
                return delta <= threshold;
            }
        }

        internal class IncludesValidator : Validator
        {
            private readonly int form;
            private readonly long? maxEndThreshold;
            private readonly long? maxStartThreshold;
            private readonly long? maxThreshold;
            private readonly long? minEndThreshold;
            private readonly long? minStartThreshold;
            private readonly long? minThreshold;
            private readonly long? threshold;

            internal IncludesValidator()
            {
                form = 1;
            }

            internal IncludesValidator(long? threshold)
            {
                form = 2;
                this.threshold = threshold;
            }

            internal IncludesValidator(
                long? minThreshold,
                long? maxThreshold)
            {
                form = 3;
                this.minThreshold = minThreshold;
                this.maxThreshold = maxThreshold;
            }

            internal IncludesValidator(
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

                if (form == 2) {
                    var distanceStart = rightStart - leftStart;
                    if (distanceStart <= 0 || distanceStart > threshold) {
                        return false;
                    }

                    var distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd <= 0 || distanceEnd > threshold);
                }

                if (form == 3) {
                    var distanceStart = rightStart - leftStart;
                    if (distanceStart < minThreshold || distanceStart > maxThreshold) {
                        return false;
                    }

                    var distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd < minThreshold || distanceEnd > maxThreshold);
                }

                if (form == 4) {
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

        internal class MeetsValidator : Validator
        {
            private readonly long? threshold;

            internal MeetsValidator()
            {
            }

            internal MeetsValidator(long? threshold)
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

                var delta = Math.Abs(rightStart - leftEnd);
                return delta <= threshold;
            }
        }

        internal class MetByValidator : Validator
        {
            private readonly long? threshold;

            internal MetByValidator()
            {
            }

            internal MetByValidator(long? threshold)
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

                var delta = Math.Abs(leftStart - rightEnd);
                return delta <= threshold;
            }
        }

        internal class OverlapsValidator : Validator
        {
            private readonly int form;
            private readonly long? maxThreshold;
            private readonly long? minThreshold;
            private readonly long? threshold;

            internal OverlapsValidator()
            {
                form = 1;
            }

            internal OverlapsValidator(long? threshold)
            {
                form = 2;
                this.threshold = threshold;
            }

            internal OverlapsValidator(
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
                var match = leftStart < rightStart &&
                            rightStart < leftEnd &&
                            leftEnd < rightEnd;

                if (form == 1) {
                    return match;
                }

                if (form == 2) {
                    if (!match) {
                        return false;
                    }

                    var delta = leftEnd - rightStart;
                    return 0 <= delta && delta <= threshold;
                }

                if (form == 3) {
                    if (!match) {
                        return false;
                    }

                    var delta = leftEnd - rightStart;
                    return minThreshold <= delta && delta <= maxThreshold;
                }

                throw new ArgumentException("Invalid form " + form);
            }
        }

        internal class OverlappedByValidator : Validator
        {
            private readonly int form;
            private readonly long? maxThreshold;
            private readonly long? minThreshold;
            private readonly long? threshold;

            internal OverlappedByValidator()
            {
                form = 1;
            }

            internal OverlappedByValidator(long? threshold)
            {
                form = 2;
                this.threshold = threshold;
            }

            internal OverlappedByValidator(
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
                var match = rightStart < leftStart &&
                            leftStart < rightEnd &&
                            rightEnd < leftEnd;

                if (form == 1) {
                    return match;
                }

                if (form == 2) {
                    if (!match) {
                        return false;
                    }

                    var delta = rightEnd - leftStart;
                    return 0 <= delta && delta <= threshold;
                }

                if (form == 3) {
                    if (!match) {
                        return false;
                    }

                    var delta = rightEnd - leftStart;
                    return minThreshold <= delta && delta <= maxThreshold;
                }

                throw new ArgumentException("Invalid form " + form);
            }
        }

        internal class StartsValidator : Validator
        {
            private readonly long? threshold;

            internal StartsValidator()
            {
            }

            internal StartsValidator(long? threshold)
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
                    return leftStart == rightStart && leftEnd < rightEnd;
                }

                var delta = Math.Abs(leftStart - rightStart);
                return delta <= threshold && leftEnd < rightEnd;
            }
        }

        internal class StartedByValidator : Validator
        {
            private readonly long? threshold;

            internal StartedByValidator()
            {
            }

            internal StartedByValidator(long? threshold)
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
                    return leftStart == rightStart && leftEnd > rightEnd;
                }

                var delta = Math.Abs(leftStart - rightStart);
                return delta <= threshold && leftEnd > rightEnd;
            }
        }
    }
} // end of namespace