///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.timer;
using com.espertech.esper.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTIntervalOps : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            // register types for testing
            foreach (SupportDateTimeFieldType fieldType in EnumHelper.GetValues<SupportDateTimeFieldType>()) {
                RegisterType(epService, fieldType.GetName(), fieldType.GetDateTimeFieldType());   // registers types "A_MSEC" and "B_MSEC" as object-array types
            }
    
            epService.EPAdministrator.CreateEPL("create variable long V_START");
            epService.EPAdministrator.CreateEPL("create variable long V_END");
    
            RunAssertionCalendarOps(epService);
            RunAssertionInvalid(epService);
            RunAssertionBeforeInSelectClause(epService);
            RunAssertionBeforeWhereClauseWithBean(epService);
            RunAssertionBeforeWhereClause(epService);
            RunAssertionAfterWhereClause(epService);
            RunAssertionCoincidesWhereClause(epService);
            RunAssertionDuringWhereClause(epService);
            RunAssertionFinishesWhereClause(epService);
            RunAssertionFinishedByWhereClause(epService);
            RunAssertionIncludesByWhereClause(epService);
            RunAssertionMeetsWhereClause(epService);
            RunAssertionMetByWhereClause(epService);
            RunAssertionOverlapsWhereClause(epService);
            RunAssertionOverlappedByWhereClause(epService);
            RunAssertionStartsWhereClause(epService);
            RunAssertionStartedByWhereClause(epService);
        }
    
        private void RunAssertionCalendarOps(EPServiceProvider epService) {
            string seedTime = "2002-05-30T09:00:00.000"; // seed is time for B
    
            object[][] expected = {
                    new object[] {"2999-01-01T09:00:00.001", 0, true},       // sending in A
            };
            AssertExpression(epService, seedTime, 0, "a.withDate(2001, 1, 1).before(b)", expected, null);
    
            expected = new[] {
                    new object[] {"2999-01-01T10:00:00.001", 0, false},
                    new object[] {"2999-01-01T08:00:00.001", 0, true},
            };
            AssertExpression(epService, seedTime, 0, "a.withDate(2001, 1, 1).before(b.withDate(2001, 1, 1))", expected, null);
    
            // Test end-timestamp preserved when using calendar ops
            expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 2000, false},
            };
            AssertExpression(epService, seedTime, 0, "a.before(b)", expected, null);
            expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 2000, false},
            };
            AssertExpression(epService, seedTime, 0, "a.withTime(8, 59, 59, 0).before(b)", expected, null);
    
            // Test end-timestamp preserved when using calendar ops
            expected = new[] {
                    new object[] {"2002-05-30T09:00:01.000", 0, false},
                    new object[] {"2002-05-30T09:00:01.001", 0, true},
            };
            AssertExpression(epService, seedTime, 1000, "a.after(b)", expected, null);
    
            // NOT YET SUPPORTED (a documented limitation of datetime methods)
            // AssertExpression(seedTime, 0, "a.after(b.withTime(9, 0, 0, 0))", expected, null);   // the "b.withTime(...) must retain the end-timestamp correctness (a documented limitation)
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            RegisterBeanType(epService);
    
            // wrong 1st parameter - string
            TryInvalid(epService, "select a.before('x') from A as a",
                    "Error starting statement: Failed to validate select-clause expression 'a.before('x')': Failed to resolve enumeration method, date-time method or mapped property 'a.before('x')': For date-time method 'before' the first parameter expression returns 'System.String', however requires a DateTime or Long-type return value or event (with timestamp) [select a.before('x') from A as a]");
    
            // wrong 1st parameter - event not defined with timestamp expression
            TryInvalid(epService, "select a.before(b) from A#lastevent as a, SupportBean#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.before(b)': For date-time method 'before' the first parameter is event type 'SupportBean', however no timestamp property has been defined for this event type [select a.before(b) from A#lastevent as a, SupportBean#lastevent as b]");
    
            // wrong 1st parameter - boolean
            TryInvalid(epService, "select a.before(true) from A#lastevent as a, SupportBean#lastevent as b",
                    string.Format("Error starting statement: Failed to validate select-clause expression 'a.before(true)': For date-time method 'before' the first parameter expression returns '{0}', however requires a DateTime or Long-type return value or event (with timestamp) [select a.before(true) from A#lastevent as a, SupportBean#lastevent as b]", 
                        typeof(bool).GetCleanName()));
    
            // wrong zero parameters
            TryInvalid(epService, "select a.before() from A#lastevent as a, SupportBean#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.before()': Parameters mismatch for date-time method 'before', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing interval start value, or an expression providing timestamp or timestamped-event and an expression providing interval start value and an expression providing interval finishes value, but receives no parameters [select a.before() from A#lastevent as a, SupportBean#lastevent as b]");
    
            // wrong target
            TryInvalid(epService, "select TheString.before(a) from A#lastevent as a, SupportBean#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'TheString.before(a)': Date-time enumeration method 'before' requires either a DateTime, DateTimeEx or long value as input or events of an event type that declares a timestamp property but received System.String [select TheString.before(a) from A#lastevent as a, SupportBean#lastevent as b]");
            TryInvalid(epService, "select b.before(a) from A#lastevent as a, SupportBean#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'b.before(a)': Date-time enumeration method 'before' requires either a DateTime, DateTimeEx or long value as input or events of an event type that declares a timestamp property [select b.before(a) from A#lastevent as a, SupportBean#lastevent as b]");
            TryInvalid(epService, "select a.get('month').before(a) from A#lastevent as a, SupportBean#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.get(\"month\").before(a)': Invalid input for date-time method 'before' [select a.get('month').before(a) from A#lastevent as a, SupportBean#lastevent as b]");
    
            // test before/after
            TryInvalid(epService, "select a.before(b, 'abc') from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.before(b,\"abc\")': Error validating date-time method 'before', expected a time-period expression or a numeric-type result for expression parameter 1 but received System.String [select a.before(b, 'abc') from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select a.before(b, 1, 'def') from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.before(b,1,\"def\")': Error validating date-time method 'before', expected a time-period expression or a numeric-type result for expression parameter 2 but received System.String [select a.before(b, 1, 'def') from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select a.before(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.before(b,1,2,3)': Parameters mismatch for date-time method 'before', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing interval start value, or an expression providing timestamp or timestamped-event and an expression providing interval start value and an expression providing interval finishes value, but receives 4 expressions [select a.before(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b]");
    
            // test coincides
            TryInvalid(epService, "select a.coincides(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.coincides(b,1,2,3)': Parameters mismatch for date-time method 'coincides', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing threshold for start and end value, or an expression providing timestamp or timestamped-event and an expression providing threshold for start value and an expression providing threshold for end value, but receives 4 expressions [select a.coincides(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select a.coincides(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.coincides(b,-1)': The coincides date-time method does not allow negative start and end values [select a.coincides(b, -1) from A#lastevent as a, B#lastevent as b]");
    
            // test during+interval
            TryInvalid(epService, "select a.during(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.during(b,1,2,3)': Parameters mismatch for date-time method 'during', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance interval both start and end and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance start and an expression providing maximum distance start and an expression providing minimum distance end and an expression providing maximum distance end, but receives 4 expressions [select a.during(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b]");
    
            // test finishes+finished-by
            TryInvalid(epService, "select a.finishes(b, 1, 2) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.finishes(b,1,2)': Parameters mismatch for date-time method 'finishes', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between end timestamps, but receives 3 expressions [select a.finishes(b, 1, 2) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select a.finishes(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.finishes(b,-1)': The finishes date-time method does not allow negative threshold value [select a.finishes(b, -1) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select a.finishedby(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.finishedby(b,-1)': The finishedby date-time method does not allow negative threshold value [select a.finishedby(b, -1) from A#lastevent as a, B#lastevent as b]");
    
            // test meets+met-by
            TryInvalid(epService, "select a.meets(b, 1, 2) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.meets(b,1,2)': Parameters mismatch for date-time method 'meets', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between start and end timestamps, but receives 3 expressions [select a.meets(b, 1, 2) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select a.meets(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.meets(b,-1)': The meets date-time method does not allow negative threshold value [select a.meets(b, -1) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select a.metBy(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.metBy(b,-1)': The metBy date-time method does not allow negative threshold value [select a.metBy(b, -1) from A#lastevent as a, B#lastevent as b]");
    
            // test overlaps+overlapped-by
            TryInvalid(epService, "select a.overlaps(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.overlaps(b,1,2,3)': Parameters mismatch for date-time method 'overlaps', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance interval both start and end and an expression providing maximum distance interval both start and end, but receives 4 expressions [select a.overlaps(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b]");
    
            // test start/startedby
            TryInvalid(epService, "select a.starts(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.starts(b,1,2,3)': Parameters mismatch for date-time method 'starts', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between start timestamps, but receives 4 expressions [select a.starts(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select a.starts(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.starts(b,-1)': The starts date-time method does not allow negative threshold value [select a.starts(b, -1) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select a.startedBy(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.startedBy(b,-1)': The startedBy date-time method does not allow negative threshold value [select a.startedBy(b, -1) from A#lastevent as a, B#lastevent as b]");
        }
    
        private void RunAssertionBeforeInSelectClause(EPServiceProvider epService) {
    
            RegisterBeanType(epService);
    
            string[] fields = "c0,c1".Split(',');
            string epl =
                    "select " +
                            "a.longdateStart.before(b.longdateStart) as c0," +
                            "a.before(b) as c1 " +
                            " from A#lastevent as a, " +
                            "      B#lastevent as b";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            LambdaAssertionUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(bool?));
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndB.Make("B1", "2002-05-30T09:00:00.000", 0));
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("A1", "2002-05-30T08:59:59.000", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(listener.AssertOneGetNewAndReset(), fields, true);
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("A2", "2002-05-30T08:59:59.950", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(listener.AssertOneGetNewAndReset(), fields, true);
        }
    
        private void RunAssertionBeforeWhereClauseWithBean(EPServiceProvider epService) {
    
            RegisterBeanType(epService);
    
            var expectedValidator = new BeforeValidator(1L, Int64.MaxValue);
            object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 0, true},
                    new object[] {"2002-05-30T08:59:59.999", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
            };
    
            var expressions = new[]{
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
                    "a.caldateStart.before(b.longdateStart)"
            };
            string seedTime = "2002-05-30T09:00:00.000";
            foreach (string expression in expressions) {
                AssertExpressionBean(epService, seedTime, 0, expression, expected, expectedValidator);
            }
        }
    
        private void RunAssertionBeforeWhereClause(EPServiceProvider epService) {
    
            string seedTime = "2002-05-30T09:00:00.000";
            var expectedValidator = new BeforeValidator(1L, Int64.MaxValue);
            var expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, true},
                    new object[] {"2002-05-30T08:59:59.000", 999, true},
                    new object[] {"2002-05-30T08:59:59.000", 1000, false},
                    new object[] {"2002-05-30T08:59:59.000", 1001, false},
                    new object[] {"2002-05-30T08:59:59.999", 0, true},
                    new object[] {"2002-05-30T08:59:59.999", 1, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
            };
            AssertExpression(epService, seedTime, 0, "a.before(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100000, "a.before(b)", expected, expectedValidator);
    
            expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, true},
                    new object[] {"2002-05-30T08:59:59.899", 0, true},
                    new object[] {"2002-05-30T08:59:59.900", 0, true},
                    new object[] {"2002-05-30T08:59:59.901", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
            };
            expectedValidator = new BeforeValidator(100L, Int64.MaxValue);
            AssertExpression(epService, seedTime, 0, "a.before(b, 100 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100000, "a.before(b, 100 milliseconds)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
            };
            expectedValidator = new BeforeValidator(100L, 500L);
            AssertExpression(epService, seedTime, 0, "a.before(b, 100 milliseconds, 500 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100000, "a.before(b, 100 milliseconds, 500 milliseconds)", expected, expectedValidator);
    
            // test expression params
            SetVStartEndVariables(epService, 100, 500);
            AssertExpression(epService, seedTime, 0, "a.before(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            SetVStartEndVariables(epService, 200, 800);
            expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T08:59:59.199", 0, false},
                    new object[] {"2002-05-30T08:59:59.199", 1, true},
                    new object[] {"2002-05-30T08:59:59.200", 0, true},
                    new object[] {"2002-05-30T08:59:59.800", 0, true},
                    new object[] {"2002-05-30T08:59:59.801", 0, false},
            };
            expectedValidator = new BeforeValidator(200L, 800L);
            AssertExpression(epService, seedTime, 0, "a.before(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            // test negative and reversed max and min
            expected = new[] {
                    new object[] {"2002-05-30T08:59:59.500", 0, false},
                    new object[] {"2002-05-30T09:00:00.990", 0, false},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.500", 0, true},
                    new object[] {"2002-05-30T09:00:00.501", 0, false},
            };
            expectedValidator = new BeforeValidator(-500L, -100L);
            AssertExpression(epService, seedTime, 0, "a.before(b, -100 milliseconds, -500 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.before(b, -500 milliseconds, -100 milliseconds)", expected, expectedValidator);
    
            // test month logic
            seedTime = "2002-03-01T09:00:00.000";
            expected = new[] {
                    new object[] {"2002-02-01T09:00:00.000", 0, true},
                    new object[] {"2002-02-01T09:00:00.001", 0, false}
            };
            expectedValidator = new BeforeValidator(GetMillisecForDays(28), Int64.MaxValue);
            AssertExpression(epService, seedTime, 100, "a.before(b, 1 month)", expected, expectedValidator);
    
            expected = new[] {
                    new object[] {"2002-01-01T08:59:59.999", 0, false},
                    new object[] {"2002-01-01T09:00:00.000", 0, true},
                    new object[] {"2002-01-11T09:00:00.000", 0, true},
                    new object[] {"2002-02-01T09:00:00.000", 0, true},
                    new object[] {"2002-02-01T09:00:00.001", 0, false}
            };
            expectedValidator = new BeforeValidator(GetMillisecForDays(28), GetMillisecForDays(28 + 31));
            AssertExpression(epService, seedTime, 100, "a.before(b, 1 month, 2 month)", expected, expectedValidator);
        }
    
        private void RunAssertionAfterWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new AfterValidator(1L, Int64.MaxValue);
            string seedTime = "2002-05-30T09:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, true},
            };
            AssertExpression(epService, seedTime, 0, "a.after(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.after(b, 1 millisecond)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.after(b, 1 millisecond, 1000000000L)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.after(b, 1000000000L, 1 millisecond)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.startTS.after(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.after(b.startTS)", expected, expectedValidator);
    
            expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.002", 0, true},
            };
            AssertExpression(epService, seedTime, 1, "a.after(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 1, "a.after(b, 1 millisecond, 1000000000L)", expected, expectedValidator);
    
            expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.099", 0, false},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.101", 0, true},
            };
            expectedValidator = new AfterValidator(100L, Int64.MaxValue);
            AssertExpression(epService, seedTime, 0, "a.after(b, 100 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.after(b, 100 milliseconds, 1000000000L)", expected, expectedValidator);
    
            expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.099", 0, false},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.500", 0, true},
                    new object[] {"2002-05-30T09:00:00.501", 0, false},
            };
            expectedValidator = new AfterValidator(100L, 500L);
            AssertExpression(epService, seedTime, 0, "a.after(b, 100 milliseconds, 500 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.after(b, 100 milliseconds, 500 milliseconds)", expected, expectedValidator);
    
            // test expression params
            SetVStartEndVariables(epService, 100, 500);
            AssertExpression(epService, seedTime, 0, "a.after(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            SetVStartEndVariables(epService, 200, 800);
            expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.199", 0, false},
                    new object[] {"2002-05-30T09:00:00.200", 0, true},
                    new object[] {"2002-05-30T09:00:00.800", 0, true},
                    new object[] {"2002-05-30T09:00:00.801", 0, false},
            };
            expectedValidator = new AfterValidator(200L, 800L);
            AssertExpression(epService, seedTime, 0, "a.after(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            // test negative distances
            expected = new[] {
                    new object[] {"2002-05-30T08:59:59.599", 0, false},
                    new object[] {"2002-05-30T08:59:59.600", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
            };
            expectedValidator = new AfterValidator(-500L, -100L);
            AssertExpression(epService, seedTime, 100, "a.after(b, -100 milliseconds, -500 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100, "a.after(b, -500 milliseconds, -100 milliseconds)", expected, expectedValidator);
    
            // test month logic
            seedTime = "2002-02-01T09:00:00.000";
            expected = new[] {
                    new object[] {"2002-03-01T09:00:00.099", 0, false},
                    new object[] {"2002-03-01T09:00:00.100", 0, true}
            };
            expectedValidator = new AfterValidator(GetMillisecForDays(28), Int64.MaxValue);
            AssertExpression(epService, seedTime, 100, "a.after(b, 1 month)", expected, expectedValidator);
    
            expected = new[] {
                    new object[] {"2002-03-01T09:00:00.099", 0, false},
                    new object[] {"2002-03-01T09:00:00.100", 0, true},
                    new object[] {"2002-04-01T09:00:00.100", 0, true},
                    new object[] {"2002-04-01T09:00:00.101", 0, false}
            };
            AssertExpression(epService, seedTime, 100, "a.after(b, 1 month, 2 month)", expected, null);
        }
    
        private void RunAssertionCoincidesWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new CoincidesValidator();
            string seedTime = "2002-05-30T09:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
            };
            AssertExpression(epService, seedTime, 0, "a.coincides(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.coincides(b, 0 millisecond)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.coincides(b, 0, 0)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.startTS.coincides(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.coincides(b.startTS)", expected, expectedValidator);
    
            expected = new[] {
                    new object[] {"2002-05-30T09:00:00.000", 1, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 1, false},
            };
            AssertExpression(epService, seedTime, 1, "a.coincides(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 1, "a.coincides(b, 0, 0)", expected, expectedValidator);
    
            expected = new[] {
                    new object[] {"2002-05-30T08:59:59.899", 0, false},
                    new object[] {"2002-05-30T08:59:59.900", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 50, true},
                    new object[] {"2002-05-30T09:00:00.000", 100, true},
                    new object[] {"2002-05-30T09:00:00.000", 101, false},
                    new object[] {"2002-05-30T09:00:00.099", 0, true},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.101", 0, false},
            };
            expectedValidator = new CoincidesValidator(100L);
            AssertExpression(epService, seedTime, 0, "a.coincides(b, 100 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.coincides(b, 100 milliseconds, 0.1 sec)", expected, expectedValidator);
    
            expected = new[] {
                    new object[] {"2002-05-30T08:59:59.799", 0, false},
                    new object[] {"2002-05-30T08:59:59.800", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.099", 0, true},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.200", 0, true},
                    new object[] {"2002-05-30T09:00:00.201", 0, false},
            };
            expectedValidator = new CoincidesValidator(200L, 500L);
            AssertExpression(epService, seedTime, 0, "a.coincides(b, 200 milliseconds, 500 milliseconds)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.101", 0, false},
            };
            expectedValidator = new CoincidesValidator(200L, 50L);
            AssertExpression(epService, seedTime, 50, "a.coincides(b, 200 milliseconds, 50 milliseconds)", expected, expectedValidator);
    
            // test expression params
            SetVStartEndVariables(epService, 200, 50);
            AssertExpression(epService, seedTime, 50, "a.coincides(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            SetVStartEndVariables(epService, 200, 70);
            expected = new[] {
                    new object[] {"2002-05-30T08:59:59.800", 0, false},
                    new object[] {"2002-05-30T08:59:59.800", 179, false},
                    new object[] {"2002-05-30T08:59:59.800", 180, true},
                    new object[] {"2002-05-30T08:59:59.800", 200, true},
                    new object[] {"2002-05-30T08:59:59.800", 320, true},
                    new object[] {"2002-05-30T08:59:59.800", 321, false},
            };
            expectedValidator = new CoincidesValidator(200L, 70L);
            AssertExpression(epService, seedTime, 50, "a.coincides(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            // test month logic
            seedTime = "2002-02-01T09:00:00.000";    // lasts to "2002-04-01T09:00:00.000" (28+31 days)
            expected = new[] {
                    new object[] {"2002-02-15T09:00:00.099", GetMillisecForDays(28 + 14), true},
                    new object[] {"2002-01-01T08:00:00.000", GetMillisecForDays(28 + 30), false}
            };
            expectedValidator = new CoincidesValidator(GetMillisecForDays(28));
            AssertExpression(epService, seedTime, GetMillisecForDays(28 + 31), "a.coincides(b, 1 month)", expected, expectedValidator);
        }
    
        private void RunAssertionDuringWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new DuringValidator();
            string seedTime = "2002-05-30T09:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, true},
                    new object[] {"2002-05-30T09:00:00.001", 98, true},
                    new object[] {"2002-05-30T09:00:00.001", 99, false},
                    new object[] {"2002-05-30T09:00:00.099", 0, true},
                    new object[] {"2002-05-30T09:00:00.099", 1, false},
                    new object[] {"2002-05-30T09:00:00.100", 0, false},
            };
            AssertExpression(epService, seedTime, 100, "a.during(b)", expected, expectedValidator);
    
            expected = new[] {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 1, false},
            };
            AssertExpression(epService, seedTime, 0, "a.during(b)", expected, expectedValidator);
    
            expected = new[] {
                    new object[] {"2002-05-30T09:00:00.001", 0, true},
                    new object[] {"2002-05-30T09:00:00.001", 2000000, true},
            };
            AssertExpression(epService, seedTime, 100, "a.startTS.during(b)", expected, null);    // want to use null-validator here
    
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
                    new object[] {"2002-05-30T09:00:00.099", 0, false},
            };
            expectedValidator = new DuringValidator(15L);
            AssertExpression(epService, seedTime, 100, "a.during(b, 15 milliseconds)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.099", 0, false},
            };
            expectedValidator = new DuringValidator(5L, 20L);
            AssertExpression(epService, seedTime, 100, "a.during(b, 5 milliseconds, 20 milliseconds)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.021", 55, false},
            };
            expectedValidator = new DuringValidator(5L, 20L, 10L, 30L);
            AssertExpression(epService, seedTime, 100, "a.during(b, 5 milliseconds, 20 milliseconds, 10 milliseconds, 30 milliseconds)", expected, expectedValidator);
        }
    
        private void RunAssertionFinishesWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new FinishesValidator();
            string seedTime = "2002-05-30T09:00:00.000";
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
                    new object[] {"2002-05-30T09:00:00.101", 0, false},
            };
            AssertExpression(epService, seedTime, 100, "a.finishes(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100, "a.finishes(b, 0)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100, "a.finishes(b, 0 milliseconds)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.105", 1, false},
            };
            expectedValidator = new FinishesValidator(5L);
            AssertExpression(epService, seedTime, 100, "a.finishes(b, 5 milliseconds)", expected, expectedValidator);
        }
    
        private void RunAssertionFinishedByWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new FinishedByValidator();
            string seedTime = "2002-05-30T09:00:00.000";
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
                    new object[] {"2002-05-30T09:00:00.000", 100, false},
            };
            AssertExpression(epService, seedTime, 100, "a.finishedBy(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100, "a.finishedBy(b, 0)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100, "a.finishedBy(b, 0 milliseconds)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.000", 105, false},
            };
            expectedValidator = new FinishedByValidator(5L);
            AssertExpression(epService, seedTime, 100, "a.finishedBy(b, 5 milliseconds)", expected, expectedValidator);
        }
    
        private void RunAssertionIncludesByWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new IncludesValidator();
            string seedTime = "2002-05-30T09:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 1100, false},
                    new object[] {"2002-05-30T08:59:59.000", 1101, true},
                    new object[] {"2002-05-30T08:59:59.000", 3000, true},
                    new object[] {"2002-05-30T08:59:59.999", 101, false},
                    new object[] {"2002-05-30T08:59:59.999", 102, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 50, false},
                    new object[] {"2002-05-30T09:00:00.000", 102, false},
            };
            AssertExpression(epService, seedTime, 100, "a.includes(b)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.000", 106, false},
            };
            expectedValidator = new IncludesValidator(5L);
            AssertExpression(epService, seedTime, 100, "a.includes(b, 5 milliseconds)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T08:59:59.996", 112, false},
            };
            expectedValidator = new IncludesValidator(5L, 20L);
            AssertExpression(epService, seedTime, 100, "a.includes(b, 5 milliseconds, 20 milliseconds)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T08:59:59.996", 124, false},
            };
            expectedValidator = new IncludesValidator(5L, 20L, 10L, 30L);
            AssertExpression(epService, seedTime, 100, "a.includes(b, 5 milliseconds, 20 milliseconds, 10 milliseconds, 30 milliseconds)", expected, expectedValidator);
        }
    
        private void RunAssertionMeetsWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new MeetsValidator();
            string seedTime = "2002-05-30T09:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 1000, true},
                    new object[] {"2002-05-30T08:59:59.000", 1001, false},
                    new object[] {"2002-05-30T08:59:59.998", 1, false},
                    new object[] {"2002-05-30T08:59:59.999", 1, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 1, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
            };
            AssertExpression(epService, seedTime, 0, "a.meets(b)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.005", 1, false},
            };
            expectedValidator = new MeetsValidator(5L);
            AssertExpression(epService, seedTime, 0, "a.meets(b, 5 milliseconds)", expected, expectedValidator);
        }
    
        private void RunAssertionMetByWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new MetByValidator();
            string seedTime = "2002-05-30T09:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T09:00:00.990", 0, false},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.100", 500, true},
                    new object[] {"2002-05-30T09:00:00.101", 0, false},
            };
            AssertExpression(epService, seedTime, 100, "a.metBy(b)", expected, expectedValidator);
    
            expected = new[] {
                    new object[] {"2002-05-30T08:59:59.999", 1, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 1, true},
            };
            AssertExpression(epService, seedTime, 0, "a.metBy(b)", expected, expectedValidator);
    
            // test 1-parameter form
            expected = new[] {
                    new object[] {"2002-05-30T08:59:59.994", 0, false},
                    new object[] {"2002-05-30T08:59:59.994", 5, false},
                    new object[] {"2002-05-30T08:59:59.995", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 20, true},
                    new object[] {"2002-05-30T09:00:00.005", 0, true},
                    new object[] {"2002-05-30T09:00:00.005", 1000, true},
                    new object[] {"2002-05-30T09:00:00.006", 0, false},
            };
            expectedValidator = new MetByValidator(5L);
            AssertExpression(epService, seedTime, 0, "a.metBy(b, 5 milliseconds)", expected, expectedValidator);
    
            expected = new[] {
                    new object[] {"2002-05-30T08:59:59.994", 0, false},
                    new object[] {"2002-05-30T08:59:59.994", 5, false},
                    new object[] {"2002-05-30T08:59:59.995", 0, false},
                    new object[] {"2002-05-30T09:00:00.094", 0, false},
                    new object[] {"2002-05-30T09:00:00.095", 0, true},
                    new object[] {"2002-05-30T09:00:00.105", 0, true},
                    new object[] {"2002-05-30T09:00:00.105", 5000, true},
                    new object[] {"2002-05-30T09:00:00.106", 0, false},
            };
            expectedValidator = new MetByValidator(5L);
            AssertExpression(epService, seedTime, 100, "a.metBy(b, 5 milliseconds)", expected, expectedValidator);
        }
    
        private void RunAssertionOverlapsWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new OverlapsValidator();
            string seedTime = "2002-05-30T09:00:00.000";
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
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
            };
            AssertExpression(epService, seedTime, 100, "a.overlaps(b)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.000", 5, false},
            };
            expectedValidator = new OverlapsValidator(5L);
            AssertExpression(epService, seedTime, 100, "a.overlaps(b, 5 milliseconds)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.000", 5, false},
            };
            expectedValidator = new OverlapsValidator(5L, 10L);
            AssertExpression(epService, seedTime, 100, "a.overlaps(b, 5 milliseconds, 10 milliseconds)", expected, expectedValidator);
        }
    
        private void RunAssertionOverlappedByWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new OverlappedByValidator();
            string seedTime = "2002-05-30T09:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 1000, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 1, false},
                    new object[] {"2002-05-30T09:00:00.001", 99, false},
                    new object[] {"2002-05-30T09:00:00.001", 100, true},
                    new object[] {"2002-05-30T09:00:00.099", 1, false},
                    new object[] {"2002-05-30T09:00:00.099", 2, true},
                    new object[] {"2002-05-30T09:00:00.100", 0, false},
                    new object[] {"2002-05-30T09:00:00.100", 1, false},
            };
            AssertExpression(epService, seedTime, 100, "a.overlappedBy(b)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.100", 100, false},
            };
            expectedValidator = new OverlappedByValidator(5L);
            AssertExpression(epService, seedTime, 100, "a.overlappedBy(b, 5 milliseconds)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.100", 100, false},
            };
            expectedValidator = new OverlappedByValidator(5L, 10L);
            AssertExpression(epService, seedTime, 100, "a.overlappedBy(b, 5 milliseconds, 10 milliseconds)", expected, expectedValidator);
        }
    
        private void RunAssertionStartsWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new StartsValidator();
            string seedTime = "2002-05-30T09:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.999", 100, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 1, true},
                    new object[] {"2002-05-30T09:00:00.000", 99, true},
                    new object[] {"2002-05-30T09:00:00.000", 100, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
            };
            AssertExpression(epService, seedTime, 100, "a.starts(b)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.005", 100, false},
            };
            expectedValidator = new StartsValidator(5L);
            AssertExpression(epService, seedTime, 100, "a.starts(b, 5 milliseconds)", expected, expectedValidator);
        }
    
        private void RunAssertionStartedByWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new StartedByValidator();
            string seedTime = "2002-05-30T09:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.999", 100, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 100, false},
                    new object[] {"2002-05-30T09:00:00.000", 101, true},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 101, false},
            };
            AssertExpression(epService, seedTime, 100, "a.startedBy(b)", expected, expectedValidator);
    
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
                    new object[] {"2002-05-30T09:00:00.005", 96, true},
            };
            expectedValidator = new StartedByValidator(5L);
            AssertExpression(epService, seedTime, 100, "a.startedBy(b, 5 milliseconds)", expected, expectedValidator);
        }
    
        private void SetVStartEndVariables(EPServiceProvider epService, long vstart, long vend) {
            epService.EPRuntime.SetVariableValue("V_START", vstart);
            epService.EPRuntime.SetVariableValue("V_END", vend);
        }
    
        private void RegisterType(EPServiceProvider epService, string suffix, string timestampType) {
            string props = "(startTS " + timestampType + ", endTS " + timestampType + ") starttimestamp startTS endtimestamp endTS";
            epService.EPAdministrator.CreateEPL("create objectarray schema A_" + suffix + " as " + props);
            epService.EPAdministrator.CreateEPL("create objectarray schema B_" + suffix + " as " + props);
        }
    
        private void RegisterBeanType(EPServiceProvider epService) {
            var configBean = new ConfigurationEventTypeLegacy();
            configBean.StartTimestampPropertyName = "longdateStart";
            configBean.EndTimestampPropertyName = "longdateEnd";
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportTimeStartEndA), configBean);
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportTimeStartEndB), configBean);
        }
    
        private void AssertExpression(EPServiceProvider epService, string seedTime, long seedDuration, string whereClause, object[][] timestampsAndResult, Validator validator) {
            foreach (SupportDateTimeFieldType fieldType in EnumHelper.GetValues<SupportDateTimeFieldType>()) {
                AssertExpressionForType(epService, seedTime, seedDuration, whereClause, timestampsAndResult, validator, fieldType);
            }
        }
    
        private void AssertExpressionForType(EPServiceProvider epService, string seedTime, long seedDuration, string whereClause, object[][] timestampsAndResult, Validator validator, SupportDateTimeFieldType fieldType) {
    
            string epl = "select * from A_" + fieldType.GetName() + "#lastevent as a, B_" + fieldType.GetName() + "#lastevent as b " +
                    "where " + whereClause;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new[]{fieldType.MakeStart(seedTime), fieldType.MakeEnd(seedTime, seedDuration)}, "B_" + fieldType.GetName());
    
            foreach (object[] test in timestampsAndResult) {
                string testtime = (string) test[0];
                long testduration = test[1].AsLong();
                bool expected = test[2].AsBoolean();
    
                long rightStart = DateTimeParser.ParseDefaultMSec(seedTime);
                long rightEnd = rightStart + seedDuration;
                long leftStart = DateTimeParser.ParseDefaultMSec(testtime);
                long leftEnd = leftStart + testduration;
                string message = "time " + testtime + " duration " + testduration + " for '" + whereClause + "'";
    
                if (validator != null) {
                    Assert.AreEqual(expected, validator.Validate(leftStart, leftEnd, rightStart, rightEnd), "Validation of expected result failed for " + message);
                }
    
                epService.EPRuntime.SendEvent(new[]{fieldType.MakeStart(testtime), fieldType.MakeEnd(testtime, testduration)}, "A_" + fieldType.GetName());
    
                if (!listener.IsInvoked && expected) {
                    Assert.Fail("Expected but not received for " + message);
                }
                if (listener.IsInvoked && !expected) {
                    Assert.Fail("Not expected but received for " + message);
                }
                listener.Reset();
            }
    
            stmt.Dispose();
        }
    
        private static long GetMillisecForDays(int days) {
            return days * 24 * 60 * 60 * 1000L;
        }
    
        private void AssertExpressionBean(EPServiceProvider epService, string seedTime, long seedDuration, string whereClause, object[][] timestampsAndResult, Validator validator) {
    
            string epl = "select * from A#lastevent as a, B#lastevent as b " +
                    "where " + whereClause;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndB.Make("B", seedTime, seedDuration));
    
            foreach (object[] test in timestampsAndResult) {
                string testtime = (string) test[0];
                long testduration = test[1].AsLong();
                bool expected = test[2].AsBoolean();
    
                long rightStart = DateTimeParser.ParseDefaultMSec(seedTime);
                long rightEnd = rightStart + seedDuration;
                long leftStart = DateTimeParser.ParseDefaultMSec(testtime);
                long leftEnd = leftStart + testduration;
                string message = "time " + testtime + " duration " + testduration + " for '" + whereClause + "'";
    
                if (validator != null) {
                    Assert.AreEqual(expected, validator.Validate(leftStart, leftEnd, rightStart, rightEnd), "Validation of expected result failed for " + message);
                }
    
                epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("A", testtime, testduration));
    
                if (!listener.IsInvoked && expected) {
                    Assert.Fail("Expected but not received for " + message);
                }
                if (listener.IsInvoked && !expected) {
                    Assert.Fail("Not expected but received for " + message);
                }
                listener.Reset();
            }
    
            stmt.Dispose();
        }

        public interface Validator {
            bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd);
        }

        public class BeforeValidator : Validator {
            private readonly long _start;
            private readonly long _end;

            public BeforeValidator(long start, long end) {
                _start = start;
                _end = end;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                long delta = rightStart - leftEnd;
                return _start <= delta && delta <= _end;
            }
        }

        public class AfterValidator : Validator {
            private readonly long _start;
            private readonly long _end;

            public AfterValidator(long start, long end) {
                _start = start;
                _end = end;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                long delta = leftStart - rightEnd;
                return _start <= delta && delta <= _end;
            }
        }

        public class CoincidesValidator : Validator {
            private readonly long _startThreshold;
            private readonly long _endThreshold;

            public CoincidesValidator() {
                _startThreshold = 0L;
                _endThreshold = 0L;
            }

            public CoincidesValidator(long startThreshold) {
                _startThreshold = startThreshold;
                _endThreshold = startThreshold;
            }

            public CoincidesValidator(long startThreshold, long endThreshold) {
                _startThreshold = startThreshold;
                _endThreshold = endThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                long startDelta = Math.Abs(leftStart - rightStart);
                long endDelta = Math.Abs(leftEnd - rightEnd);
                return startDelta <= _startThreshold && endDelta <= _endThreshold;
            }
        }

        public class DuringValidator : Validator {
    
            private readonly int _form;
            private readonly long _threshold;
            private readonly long _minThreshold;
            private readonly long _maxThreshold;
            private readonly long _minStartThreshold;
            private readonly long _maxStartThreshold;
            private readonly long _minEndThreshold;
            private readonly long _maxEndThreshold;

            public DuringValidator() {
                _form = 1;
            }

            public DuringValidator(long threshold) {
                _form = 2;
                _threshold = threshold;
            }

            public DuringValidator(long minThreshold, long maxThreshold) {
                _form = 3;
                _minThreshold = minThreshold;
                _maxThreshold = maxThreshold;
            }

            public DuringValidator(long minStartThreshold, long maxStartThreshold, long minEndThreshold, long maxEndThreshold) {
                _form = 4;
                _minStartThreshold = minStartThreshold;
                _maxStartThreshold = maxStartThreshold;
                _minEndThreshold = minEndThreshold;
                _maxEndThreshold = maxEndThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                if (_form == 1) {
                    return rightStart < leftStart &&
                            leftEnd < rightEnd;
                } else if (_form == 2) {
                    long distanceStart = leftStart - rightStart;
                    if (distanceStart <= 0 || distanceStart > _threshold) {
                        return false;
                    }
                    long distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd <= 0 || distanceEnd > _threshold);
                } else if (_form == 3) {
                    long distanceStart = leftStart - rightStart;
                    if (distanceStart < _minThreshold || distanceStart > _maxThreshold) {
                        return false;
                    }
                    long distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd < _minThreshold || distanceEnd > _maxThreshold);
                } else if (_form == 4) {
                    long distanceStart = leftStart - rightStart;
                    if (distanceStart < _minStartThreshold || distanceStart > _maxStartThreshold) {
                        return false;
                    }
                    long distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd < _minEndThreshold || distanceEnd > _maxEndThreshold);
                }
                throw new IllegalStateException("Invalid form: " + _form);
            }
        }

        public class FinishesValidator : Validator {
            private readonly long? _threshold;

            public FinishesValidator() {
            }

            public FinishesValidator(long threshold) {
                _threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                if (_threshold == null) {
                    return rightStart < leftStart && leftEnd == rightEnd;
                } else {
                    if (rightStart >= leftStart) {
                        return false;
                    }
                    long delta = Math.Abs(leftEnd - rightEnd);
                    return delta <= _threshold;
                }
            }
        }

        public class FinishedByValidator : Validator {
            private readonly long? _threshold;

            public FinishedByValidator() {
            }

            public FinishedByValidator(long threshold) {
                _threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                if (_threshold == null) {
                    return leftStart < rightStart && leftEnd == rightEnd;
                } else {
                    if (leftStart >= rightStart) {
                        return false;
                    }
                    long delta = Math.Abs(leftEnd - rightEnd);
                    return delta <= _threshold;
                }
            }
        }

        public class IncludesValidator : Validator {
            private readonly int _form;
            private readonly long _threshold;
            private readonly long _minThreshold;
            private readonly long _maxThreshold;
            private readonly long _minStartThreshold;
            private readonly long _maxStartThreshold;
            private readonly long _minEndThreshold;
            private readonly long _maxEndThreshold;
    
            public IncludesValidator() {
                _form = 1;
            }

            public IncludesValidator(long threshold) {
                _form = 2;
                _threshold = threshold;
            }

            public IncludesValidator(long minThreshold, long maxThreshold) {
                _form = 3;
                _minThreshold = minThreshold;
                _maxThreshold = maxThreshold;
            }

            public IncludesValidator(long minStartThreshold, long maxStartThreshold, long minEndThreshold, long maxEndThreshold) {
                _form = 4;
                _minStartThreshold = minStartThreshold;
                _maxStartThreshold = maxStartThreshold;
                _minEndThreshold = minEndThreshold;
                _maxEndThreshold = maxEndThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                if (_form == 1) {
                    return leftStart < rightStart &&
                            rightEnd < leftEnd;
                } else if (_form == 2) {
                    long distanceStart = rightStart - leftStart;
                    if (distanceStart <= 0 || distanceStart > _threshold) {
                        return false;
                    }
                    long distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd <= 0 || distanceEnd > _threshold);
                } else if (_form == 3) {
                    long distanceStart = rightStart - leftStart;
                    if (distanceStart < _minThreshold || distanceStart > _maxThreshold) {
                        return false;
                    }
                    long distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd < _minThreshold || distanceEnd > _maxThreshold);
                } else if (_form == 4) {
                    long distanceStart = rightStart - leftStart;
                    if (distanceStart < _minStartThreshold || distanceStart > _maxStartThreshold) {
                        return false;
                    }
                    long distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd < _minEndThreshold || distanceEnd > _maxEndThreshold);
                }
                throw new IllegalStateException("Invalid form: " + _form);
            }
        }

        public class MeetsValidator : Validator {
            private readonly long? _threshold;

            public MeetsValidator() {
            }

            public MeetsValidator(long threshold) {
                _threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                if (_threshold == null) {
                    return rightStart == leftEnd;
                } else {
                    long delta = Math.Abs(rightStart - leftEnd);
                    return delta <= _threshold;
                }
            }
        }

        public class MetByValidator : Validator {
            private readonly long? _threshold;

            public MetByValidator() {
            }

            public MetByValidator(long threshold) {
                _threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                if (_threshold == null) {
                    return leftStart == rightEnd;
                } else {
                    long delta = Math.Abs(leftStart - rightEnd);
                    return delta <= _threshold;
                }
            }
        }

        public class OverlapsValidator : Validator {
            private readonly int _form;
            private readonly long _threshold;
            private readonly long _minThreshold;
            private readonly long _maxThreshold;

            public OverlapsValidator() {
                _form = 1;
            }

            public OverlapsValidator(long threshold) {
                _form = 2;
                _threshold = threshold;
            }

            public OverlapsValidator(long minThreshold, long maxThreshold) {
                _form = 3;
                _minThreshold = minThreshold;
                _maxThreshold = maxThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                bool match = (leftStart < rightStart) &&
                        (rightStart < leftEnd) &&
                        (leftEnd < rightEnd);
    
                if (_form == 1) {
                    return match;
                } else if (_form == 2) {
                    if (!match) {
                        return false;
                    }
                    long delta = leftEnd - rightStart;
                    return 0 <= delta && delta <= _threshold;
                } else if (_form == 3) {
                    if (!match) {
                        return false;
                    }
                    long delta = leftEnd - rightStart;
                    return _minThreshold <= delta && delta <= _maxThreshold;
                }
                throw new ArgumentException("Invalid form " + _form);
            }
        }

        public class OverlappedByValidator : Validator {
            private readonly int _form;
            private readonly long _threshold;
            private readonly long _minThreshold;
            private readonly long _maxThreshold;

            public OverlappedByValidator() {
                _form = 1;
            }

            public OverlappedByValidator(long threshold) {
                _form = 2;
                _threshold = threshold;
            }

            public OverlappedByValidator(long minThreshold, long maxThreshold) {
                _form = 3;
                _minThreshold = minThreshold;
                _maxThreshold = maxThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                bool match = (rightStart < leftStart) &&
                        (leftStart < rightEnd) &&
                        (rightEnd < leftEnd);
    
                if (_form == 1) {
                    return match;
                } else if (_form == 2) {
                    if (!match) {
                        return false;
                    }
                    long delta = rightEnd - leftStart;
                    return 0 <= delta && delta <= _threshold;
                } else if (_form == 3) {
                    if (!match) {
                        return false;
                    }
                    long delta = rightEnd - leftStart;
                    return _minThreshold <= delta && delta <= _maxThreshold;
                }
                throw new ArgumentException("Invalid form " + _form);
            }
        }

        public class StartsValidator : Validator {
            private readonly long? _threshold;

            public StartsValidator() {
            }

            public StartsValidator(long threshold) {
                _threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                if (_threshold == null) {
                    return (leftStart == rightStart) && (leftEnd < rightEnd);
                } else {
                    long delta = Math.Abs(leftStart - rightStart);
                    return (delta <= _threshold) && (leftEnd < rightEnd);
                }
            }
        }

        public class StartedByValidator : Validator {
            private readonly long? _threshold;

            public StartedByValidator() {
            }

            public StartedByValidator(long threshold) {
                _threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                if (_threshold == null) {
                    return (leftStart == rightStart) && (leftEnd > rightEnd);
                } else {
                    long delta = Math.Abs(leftStart - rightStart);
                    return (delta <= _threshold) && (leftEnd > rightEnd);
                }
            }
        }
    }
} // end of namespace
