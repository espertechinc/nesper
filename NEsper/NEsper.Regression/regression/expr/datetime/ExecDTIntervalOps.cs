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
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.timer;

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
            AssertExpression(epService, seedTime, 0, "a.WithDate(2001, 1, 1).Before(b)", expected, null);
    
            expected = new object[][]{
                    new object[] {"2999-01-01T10:00:00.001", 0, false},
                    new object[] {"2999-01-01T08:00:00.001", 0, true},
            };
            AssertExpression(epService, seedTime, 0, "a.WithDate(2001, 1, 1).Before(b.WithDate(2001, 1, 1))", expected, null);
    
            // Test end-timestamp preserved when using calendar ops
            expected = new object[][]{
                    new object[] {"2002-05-30T08:59:59.000", 2000, false},
            };
            AssertExpression(epService, seedTime, 0, "a.Before(b)", expected, null);
            expected = new object[][]{
                    new object[] {"2002-05-30T08:59:59.000", 2000, false},
            };
            AssertExpression(epService, seedTime, 0, "a.WithTime(8, 59, 59, 0).Before(b)", expected, null);
    
            // Test end-timestamp preserved when using calendar ops
            expected = new object[][]{
                    new object[] {"2002-05-30T09:00:01.000", 0, false},
                    new object[] {"2002-05-30T09:00:01.001", 0, true},
            };
            AssertExpression(epService, seedTime, 1000, "a.After(b)", expected, null);
    
            // NOT YET SUPPORTED (a documented limitation of datetime methods)
            // AssertExpression(seedTime, 0, "a.After(b.WithTime(9, 0, 0, 0))", expected, null);   // the "b.WithTime(...) must retain the end-timestamp correctness (a documented limitation)
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean).FullName);
            RegisterBeanType(epService);
    
            // wrong 1st parameter - string
            TryInvalid(epService, "select A.Before('x') from A as a",
                    "Error starting statement: Failed to validate select-clause expression 'a.Before('x')': Failed to resolve enumeration method, date-time method or mapped property 'a.Before('x')': For date-time method 'before' the first parameter expression returns 'class System.String', however requires a Date, Calendar, long-type return value or event (with timestamp) [select A.Before('x') from A as a]");
    
            // wrong 1st parameter - event not defined with timestamp expression
            TryInvalid(epService, "select A.Before(b) from A#lastevent as a, SupportBean#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Before(b)': For date-time method 'before' the first parameter is event type 'SupportBean', however no timestamp property has been defined for this event type [select A.Before(b) from A#lastevent as a, SupportBean#lastevent as b]");
    
            // wrong 1st parameter - boolean
            TryInvalid(epService, "select A.Before(true) from A#lastevent as a, SupportBean#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Before(true)': For date-time method 'before' the first parameter expression returns 'class java.lang.bool?', however requires a Date, Calendar, long-type return value or event (with timestamp) [select A.Before(true) from A#lastevent as a, SupportBean#lastevent as b]");
    
            // wrong zero parameters
            TryInvalid(epService, "select A.Before() from A#lastevent as a, SupportBean#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Before()': Parameters mismatch for date-time method 'before', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing interval start value, or an expression providing timestamp or timestamped-event and an expression providing interval start value and an expression providing interval finishes value, but receives no parameters [select A.Before() from A#lastevent as a, SupportBean#lastevent as b]");
    
            // wrong target
            TryInvalid(epService, "select TheString.Before(a) from A#lastevent as a, SupportBean#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'theString.Before(a)': Date-time enumeration method 'before' requires either a Calendar, Date, long, LocalDateTime or ZonedDateTime value as input or events of an event type that declares a timestamp property but received System.String [select TheString.Before(a) from A#lastevent as a, SupportBean#lastevent as b]");
            TryInvalid(epService, "select B.Before(a) from A#lastevent as a, SupportBean#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'b.Before(a)': Date-time enumeration method 'before' requires either a Calendar, Date, long, LocalDateTime or ZonedDateTime value as input or events of an event type that declares a timestamp property [select B.Before(a) from A#lastevent as a, SupportBean#lastevent as b]");
            TryInvalid(epService, "select A.Get('month').Before(a) from A#lastevent as a, SupportBean#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Get(\"month\").Before(a)': Invalid input for date-time method 'before' [select A.Get('month').Before(a) from A#lastevent as a, SupportBean#lastevent as b]");
    
            // test before/after
            TryInvalid(epService, "select A.Before(b, 'abc') from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Before(b,\"abc\")': Error validating date-time method 'before', expected a time-period expression or a numeric-type result for expression parameter 1 but received System.String [select A.Before(b, 'abc') from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select A.Before(b, 1, 'def') from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Before(b,1,\"def\")': Error validating date-time method 'before', expected a time-period expression or a numeric-type result for expression parameter 2 but received System.String [select A.Before(b, 1, 'def') from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select A.Before(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Before(b,1,2,3)': Parameters mismatch for date-time method 'before', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing interval start value, or an expression providing timestamp or timestamped-event and an expression providing interval start value and an expression providing interval finishes value, but receives 4 expressions [select A.Before(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b]");
    
            // test coincides
            TryInvalid(epService, "select A.Coincides(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Coincides(b,1,2,3)': Parameters mismatch for date-time method 'coincides', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing threshold for start and end value, or an expression providing timestamp or timestamped-event and an expression providing threshold for start value and an expression providing threshold for end value, but receives 4 expressions [select A.Coincides(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select A.Coincides(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Coincides(b,-1)': The coincides date-time method does not allow negative start and end values [select A.Coincides(b, -1) from A#lastevent as a, B#lastevent as b]");
    
            // test during+interval
            TryInvalid(epService, "select A.During(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.During(b,1,2,3)': Parameters mismatch for date-time method 'during', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance interval both start and end and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance start and an expression providing maximum distance start and an expression providing minimum distance end and an expression providing maximum distance end, but receives 4 expressions [select A.During(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b]");
    
            // test finishes+finished-by
            TryInvalid(epService, "select A.Finishes(b, 1, 2) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Finishes(b,1,2)': Parameters mismatch for date-time method 'finishes', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between end timestamps, but receives 3 expressions [select A.Finishes(b, 1, 2) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select A.Finishes(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Finishes(b,-1)': The finishes date-time method does not allow negative threshold value [select A.Finishes(b, -1) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select A.Finishedby(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Finishedby(b,-1)': The finishedby date-time method does not allow negative threshold value [select A.Finishedby(b, -1) from A#lastevent as a, B#lastevent as b]");
    
            // test meets+met-by
            TryInvalid(epService, "select A.Meets(b, 1, 2) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Meets(b,1,2)': Parameters mismatch for date-time method 'meets', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between start and end timestamps, but receives 3 expressions [select A.Meets(b, 1, 2) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select A.Meets(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Meets(b,-1)': The meets date-time method does not allow negative threshold value [select A.Meets(b, -1) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select A.MetBy(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.MetBy(b,-1)': The metBy date-time method does not allow negative threshold value [select A.MetBy(b, -1) from A#lastevent as a, B#lastevent as b]");
    
            // test overlaps+overlapped-by
            TryInvalid(epService, "select A.Overlaps(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Overlaps(b,1,2,3)': Parameters mismatch for date-time method 'overlaps', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance interval both start and end, or an expression providing timestamp or timestamped-event and an expression providing minimum distance interval both start and end and an expression providing maximum distance interval both start and end, but receives 4 expressions [select A.Overlaps(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b]");
    
            // test start/startedby
            TryInvalid(epService, "select A.Starts(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Starts(b,1,2,3)': Parameters mismatch for date-time method 'starts', the method has multiple footprints accepting an expression providing timestamp or timestamped-event, or an expression providing timestamp or timestamped-event and an expression providing maximum distance between start timestamps, but receives 4 expressions [select A.Starts(b, 1, 2, 3) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select A.Starts(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.Starts(b,-1)': The starts date-time method does not allow negative threshold value [select A.Starts(b, -1) from A#lastevent as a, B#lastevent as b]");
            TryInvalid(epService, "select A.StartedBy(b, -1) from A#lastevent as a, B#lastevent as b",
                    "Error starting statement: Failed to validate select-clause expression 'a.StartedBy(b,-1)': The startedBy date-time method does not allow negative threshold value [select A.StartedBy(b, -1) from A#lastevent as a, B#lastevent as b]");
        }
    
        private void RunAssertionBeforeInSelectClause(EPServiceProvider epService) {
    
            RegisterBeanType(epService);
    
            string[] fields = "c0,c1".Split(',');
            string epl =
                    "select " +
                            "a.longdateStart.Before(b.longdateStart) as c0," +
                            "a.Before(b) as c1 " +
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
    
            var expressions = new string[]{
                    "a.Before(b)",
                    "a.Before(b, 1 millisecond)",
                    "a.Before(b, 1 millisecond, 1000000000L)",
                    "a.longdateStart.Before(b)",
                    "a.utildateStart.Before(b)",
                    "a.caldateStart.Before(b)",
                    "a.Before(b.longdateStart)",
                    "a.Before(b.utildateStart)",
                    "a.Before(b.caldateStart)",
                    "a.longdateStart.Before(b.longdateStart)",
                    "a.longdateStart.Before(b.longdateStart)",
                    "a.utildateStart.Before(b.utildateStart)",
                    "a.caldateStart.Before(b.caldateStart)",
                    "a.utildateStart.Before(b.caldateStart)",
                    "a.utildateStart.Before(b.longdateStart)",
                    "a.caldateStart.Before(b.utildateStart)",
                    "a.caldateStart.Before(b.longdateStart)",
                    "a.ldtStart.Before(b.ldtStart)",
                    "a.zdtStart.Before(b.zdtStart)"
            };
            string seedTime = "2002-05-30T09:00:00.000";
            foreach (string expression in expressions) {
                AssertExpressionBean(epService, seedTime, 0, expression, expected, expectedValidator);
            }
        }
    
        private void RunAssertionBeforeWhereClause(EPServiceProvider epService) {
    
            string seedTime = "2002-05-30T09:00:00.000";
            var expectedValidator = new BeforeValidator(1L, Int64.MaxValue);
            var expected = new object[][]{
                    new object[] {"2002-05-30T08:59:59.000", 0, true},
                    new object[] {"2002-05-30T08:59:59.000", 999, true},
                    new object[] {"2002-05-30T08:59:59.000", 1000, false},
                    new object[] {"2002-05-30T08:59:59.000", 1001, false},
                    new object[] {"2002-05-30T08:59:59.999", 0, true},
                    new object[] {"2002-05-30T08:59:59.999", 1, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
            };
            AssertExpression(epService, seedTime, 0, "a.Before(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100000, "a.Before(b)", expected, expectedValidator);
    
            expected = new object[][]{
                    new object[] {"2002-05-30T08:59:59.000", 0, true},
                    new object[] {"2002-05-30T08:59:59.899", 0, true},
                    new object[] {"2002-05-30T08:59:59.900", 0, true},
                    new object[] {"2002-05-30T08:59:59.901", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
            };
            expectedValidator = new BeforeValidator(100L, Int64.MaxValue);
            AssertExpression(epService, seedTime, 0, "a.Before(b, 100 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100000, "a.Before(b, 100 milliseconds)", expected, expectedValidator);
    
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 0, "a.Before(b, 100 milliseconds, 500 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100000, "a.Before(b, 100 milliseconds, 500 milliseconds)", expected, expectedValidator);
    
            // test expression params
            SetVStartEndVariables(epService, 100, 500);
            AssertExpression(epService, seedTime, 0, "a.Before(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            SetVStartEndVariables(epService, 200, 800);
            expected = new object[][]{
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T08:59:59.199", 0, false},
                    new object[] {"2002-05-30T08:59:59.199", 1, true},
                    new object[] {"2002-05-30T08:59:59.200", 0, true},
                    new object[] {"2002-05-30T08:59:59.800", 0, true},
                    new object[] {"2002-05-30T08:59:59.801", 0, false},
            };
            expectedValidator = new BeforeValidator(200L, 800L);
            AssertExpression(epService, seedTime, 0, "a.Before(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            // test negative and reversed max and min
            expected = new object[][]{
                    new object[] {"2002-05-30T08:59:59.500", 0, false},
                    new object[] {"2002-05-30T09:00:00.990", 0, false},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.500", 0, true},
                    new object[] {"2002-05-30T09:00:00.501", 0, false},
            };
            expectedValidator = new BeforeValidator(-500L, -100L);
            AssertExpression(epService, seedTime, 0, "a.Before(b, -100 milliseconds, -500 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.Before(b, -500 milliseconds, -100 milliseconds)", expected, expectedValidator);
    
            // test month logic
            seedTime = "2002-03-01T09:00:00.000";
            expected = new object[][]{
                    new object[] {"2002-02-01T09:00:00.000", 0, true},
                    new object[] {"2002-02-01T09:00:00.001", 0, false}
            };
            expectedValidator = new BeforeValidator(GetMillisecForDays(28), Int64.MaxValue);
            AssertExpression(epService, seedTime, 100, "a.Before(b, 1 month)", expected, expectedValidator);
    
            expected = new object[][]{
                    new object[] {"2002-01-01T08:59:59.999", 0, false},
                    new object[] {"2002-01-01T09:00:00.000", 0, true},
                    new object[] {"2002-01-11T09:00:00.000", 0, true},
                    new object[] {"2002-02-01T09:00:00.000", 0, true},
                    new object[] {"2002-02-01T09:00:00.001", 0, false}
            };
            expectedValidator = new BeforeValidator(GetMillisecForDays(28), GetMillisecForDays(28 + 31));
            AssertExpression(epService, seedTime, 100, "a.Before(b, 1 month, 2 month)", expected, expectedValidator);
        }
    
        private void RunAssertionAfterWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new AfterValidator(1L, Int64.MaxValue);
            string seedTime = "2002-05-30T09:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, true},
            };
            AssertExpression(epService, seedTime, 0, "a.After(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.After(b, 1 millisecond)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.After(b, 1 millisecond, 1000000000L)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.After(b, 1000000000L, 1 millisecond)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.startTS.After(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.After(b.startTS)", expected, expectedValidator);
    
            expected = new object[][]{
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.002", 0, true},
            };
            AssertExpression(epService, seedTime, 1, "a.After(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 1, "a.After(b, 1 millisecond, 1000000000L)", expected, expectedValidator);
    
            expected = new object[][]{
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.099", 0, false},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.101", 0, true},
            };
            expectedValidator = new AfterValidator(100L, Int64.MaxValue);
            AssertExpression(epService, seedTime, 0, "a.After(b, 100 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.After(b, 100 milliseconds, 1000000000L)", expected, expectedValidator);
    
            expected = new object[][]{
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.099", 0, false},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.500", 0, true},
                    new object[] {"2002-05-30T09:00:00.501", 0, false},
            };
            expectedValidator = new AfterValidator(100L, 500L);
            AssertExpression(epService, seedTime, 0, "a.After(b, 100 milliseconds, 500 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.After(b, 100 milliseconds, 500 milliseconds)", expected, expectedValidator);
    
            // test expression params
            SetVStartEndVariables(epService, 100, 500);
            AssertExpression(epService, seedTime, 0, "a.After(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            SetVStartEndVariables(epService, 200, 800);
            expected = new object[][]{
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.199", 0, false},
                    new object[] {"2002-05-30T09:00:00.200", 0, true},
                    new object[] {"2002-05-30T09:00:00.800", 0, true},
                    new object[] {"2002-05-30T09:00:00.801", 0, false},
            };
            expectedValidator = new AfterValidator(200L, 800L);
            AssertExpression(epService, seedTime, 0, "a.After(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            // test negative distances
            expected = new object[][]{
                    new object[] {"2002-05-30T08:59:59.599", 0, false},
                    new object[] {"2002-05-30T08:59:59.600", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
            };
            expectedValidator = new AfterValidator(-500L, -100L);
            AssertExpression(epService, seedTime, 100, "a.After(b, -100 milliseconds, -500 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100, "a.After(b, -500 milliseconds, -100 milliseconds)", expected, expectedValidator);
    
            // test month logic
            seedTime = "2002-02-01T09:00:00.000";
            expected = new object[][]{
                    new object[] {"2002-03-01T09:00:00.099", 0, false},
                    new object[] {"2002-03-01T09:00:00.100", 0, true}
            };
            expectedValidator = new AfterValidator(GetMillisecForDays(28), Int64.MaxValue);
            AssertExpression(epService, seedTime, 100, "a.After(b, 1 month)", expected, expectedValidator);
    
            expected = new object[][]{
                    new object[] {"2002-03-01T09:00:00.099", 0, false},
                    new object[] {"2002-03-01T09:00:00.100", 0, true},
                    new object[] {"2002-04-01T09:00:00.100", 0, true},
                    new object[] {"2002-04-01T09:00:00.101", 0, false}
            };
            AssertExpression(epService, seedTime, 100, "a.After(b, 1 month, 2 month)", expected, null);
        }
    
        private void RunAssertionCoincidesWhereClause(EPServiceProvider epService) {
    
            var expectedValidator = new CoincidesValidator();
            string seedTime = "2002-05-30T09:00:00.000";
            object[][] expected = {
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
            };
            AssertExpression(epService, seedTime, 0, "a.Coincides(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.Coincides(b, 0 millisecond)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.Coincides(b, 0, 0)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.startTS.Coincides(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.Coincides(b.startTS)", expected, expectedValidator);
    
            expected = new object[][]{
                    new object[] {"2002-05-30T09:00:00.000", 1, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 1, false},
            };
            AssertExpression(epService, seedTime, 1, "a.Coincides(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 1, "a.Coincides(b, 0, 0)", expected, expectedValidator);
    
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 0, "a.Coincides(b, 100 milliseconds)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 0, "a.Coincides(b, 100 milliseconds, 0.1 sec)", expected, expectedValidator);
    
            expected = new object[][]{
                    new object[] {"2002-05-30T08:59:59.799", 0, false},
                    new object[] {"2002-05-30T08:59:59.800", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.099", 0, true},
                    new object[] {"2002-05-30T09:00:00.100", 0, true},
                    new object[] {"2002-05-30T09:00:00.200", 0, true},
                    new object[] {"2002-05-30T09:00:00.201", 0, false},
            };
            expectedValidator = new CoincidesValidator(200L, 500L);
            AssertExpression(epService, seedTime, 0, "a.Coincides(b, 200 milliseconds, 500 milliseconds)", expected, expectedValidator);
    
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 50, "a.Coincides(b, 200 milliseconds, 50 milliseconds)", expected, expectedValidator);
    
            // test expression params
            SetVStartEndVariables(epService, 200, 50);
            AssertExpression(epService, seedTime, 50, "a.Coincides(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            SetVStartEndVariables(epService, 200, 70);
            expected = new object[][]{
                    new object[] {"2002-05-30T08:59:59.800", 0, false},
                    new object[] {"2002-05-30T08:59:59.800", 179, false},
                    new object[] {"2002-05-30T08:59:59.800", 180, true},
                    new object[] {"2002-05-30T08:59:59.800", 200, true},
                    new object[] {"2002-05-30T08:59:59.800", 320, true},
                    new object[] {"2002-05-30T08:59:59.800", 321, false},
            };
            expectedValidator = new CoincidesValidator(200L, 70L);
            AssertExpression(epService, seedTime, 50, "a.Coincides(b, V_START milliseconds, V_END milliseconds)", expected, expectedValidator);
    
            // test month logic
            seedTime = "2002-02-01T09:00:00.000";    // lasts to "2002-04-01T09:00:00.000" (28+31 days)
            expected = new object[][]{
                    new object[] {"2002-02-15T09:00:00.099", GetMillisecForDays(28 + 14), true},
                    new object[] {"2002-01-01T08:00:00.000", GetMillisecForDays(28 + 30), false}
            };
            expectedValidator = new CoincidesValidator(GetMillisecForDays(28));
            AssertExpression(epService, seedTime, GetMillisecForDays(28 + 31), "a.Coincides(b, 1 month)", expected, expectedValidator);
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
            AssertExpression(epService, seedTime, 100, "a.During(b)", expected, expectedValidator);
    
            expected = new object[][]{
                    new object[] {"2002-05-30T08:59:59.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 0, false},
                    new object[] {"2002-05-30T09:00:00.001", 1, false},
            };
            AssertExpression(epService, seedTime, 0, "a.During(b)", expected, expectedValidator);
    
            expected = new object[][]{
                    new object[] {"2002-05-30T09:00:00.001", 0, true},
                    new object[] {"2002-05-30T09:00:00.001", 2000000, true},
            };
            AssertExpression(epService, seedTime, 100, "a.startTS.During(b)", expected, null);    // want to use null-validator here
    
            // test 1-parameter footprint
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.During(b, 15 milliseconds)", expected, expectedValidator);
    
            // test 2-parameter footprint
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.During(b, 5 milliseconds, 20 milliseconds)", expected, expectedValidator);
    
            // test 4-parameter footprint
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.During(b, 5 milliseconds, 20 milliseconds, 10 milliseconds, 30 milliseconds)", expected, expectedValidator);
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
            AssertExpression(epService, seedTime, 100, "a.Finishes(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100, "a.Finishes(b, 0)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100, "a.Finishes(b, 0 milliseconds)", expected, expectedValidator);
    
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.Finishes(b, 5 milliseconds)", expected, expectedValidator);
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
            AssertExpression(epService, seedTime, 100, "a.FinishedBy(b)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100, "a.FinishedBy(b, 0)", expected, expectedValidator);
            AssertExpression(epService, seedTime, 100, "a.FinishedBy(b, 0 milliseconds)", expected, expectedValidator);
    
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.FinishedBy(b, 5 milliseconds)", expected, expectedValidator);
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
            AssertExpression(epService, seedTime, 100, "a.Includes(b)", expected, expectedValidator);
    
            // test 1-parameter form
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.Includes(b, 5 milliseconds)", expected, expectedValidator);
    
            // test 2-parameter form
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.Includes(b, 5 milliseconds, 20 milliseconds)", expected, expectedValidator);
    
            // test 4-parameter form
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.Includes(b, 5 milliseconds, 20 milliseconds, 10 milliseconds, 30 milliseconds)", expected, expectedValidator);
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
            AssertExpression(epService, seedTime, 0, "a.Meets(b)", expected, expectedValidator);
    
            // test 1-parameter form
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 0, "a.Meets(b, 5 milliseconds)", expected, expectedValidator);
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
            AssertExpression(epService, seedTime, 100, "a.MetBy(b)", expected, expectedValidator);
    
            expected = new object[][]{
                    new object[] {"2002-05-30T08:59:59.999", 1, false},
                    new object[] {"2002-05-30T09:00:00.000", 0, true},
                    new object[] {"2002-05-30T09:00:00.000", 1, true},
            };
            AssertExpression(epService, seedTime, 0, "a.MetBy(b)", expected, expectedValidator);
    
            // test 1-parameter form
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 0, "a.MetBy(b, 5 milliseconds)", expected, expectedValidator);
    
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.MetBy(b, 5 milliseconds)", expected, expectedValidator);
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
            AssertExpression(epService, seedTime, 100, "a.Overlaps(b)", expected, expectedValidator);
    
            // test 1-parameter form (overlap by not more then X msec)
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.Overlaps(b, 5 milliseconds)", expected, expectedValidator);
    
            // test 2-parameter form (overlap by min X and not more then Y msec)
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.Overlaps(b, 5 milliseconds, 10 milliseconds)", expected, expectedValidator);
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
            AssertExpression(epService, seedTime, 100, "a.OverlappedBy(b)", expected, expectedValidator);
    
            // test 1-parameter form (overlap by not more then X msec)
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.OverlappedBy(b, 5 milliseconds)", expected, expectedValidator);
    
            // test 2-parameter form (overlap by min X and not more then Y msec)
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.OverlappedBy(b, 5 milliseconds, 10 milliseconds)", expected, expectedValidator);
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
            AssertExpression(epService, seedTime, 100, "a.Starts(b)", expected, expectedValidator);
    
            // test 1-parameter form (max distance between start times)
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.Starts(b, 5 milliseconds)", expected, expectedValidator);
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
            AssertExpression(epService, seedTime, 100, "a.StartedBy(b)", expected, expectedValidator);
    
            // test 1-parameter form (max distance between start times)
            expected = new object[][]{
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
            AssertExpression(epService, seedTime, 100, "a.StartedBy(b, 5 milliseconds)", expected, expectedValidator);
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
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportTimeStartEndA).FullName, configBean);
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportTimeStartEndB).FullName, configBean);
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
    
            epService.EPRuntime.SendEvent(new object[]{fieldType.MakeStart(seedTime), fieldType.MakeEnd(seedTime, seedDuration)}, "B_" + fieldType.GetName());
    
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
    
                epService.EPRuntime.SendEvent(new object[]{fieldType.MakeStart(testtime), fieldType.MakeEnd(testtime, testduration)}, "A_" + fieldType.GetName());
    
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
            private long start;
            private long end;

            public BeforeValidator(long start, long end) {
                this.start = start;
                this.end = end;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                long delta = rightStart - leftEnd;
                return start <= delta && delta <= end;
            }
        }

        public class AfterValidator : Validator {
            private long start;
            private long end;

            public AfterValidator(long start, long end) {
                this.start = start;
                this.end = end;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                long delta = leftStart - rightEnd;
                return start <= delta && delta <= end;
            }
        }

        public class CoincidesValidator : Validator {
            private readonly long startThreshold;
            private readonly long endThreshold;

            public CoincidesValidator() {
                startThreshold = 0L;
                endThreshold = 0L;
            }

            public CoincidesValidator(long startThreshold) {
                this.startThreshold = startThreshold;
                this.endThreshold = startThreshold;
            }

            public CoincidesValidator(long startThreshold, long endThreshold) {
                this.startThreshold = startThreshold;
                this.endThreshold = endThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                long startDelta = Math.Abs(leftStart - rightStart);
                long endDelta = Math.Abs(leftEnd - rightEnd);
                return startDelta <= startThreshold && endDelta <= endThreshold;
            }
        }

        public class DuringValidator : Validator {
    
            private int form;
            private long threshold;
            private long minThreshold;
            private long maxThreshold;
            private long minStartThreshold;
            private long maxStartThreshold;
            private long minEndThreshold;
            private long maxEndThreshold;

            public DuringValidator() {
                form = 1;
            }

            public DuringValidator(long threshold) {
                form = 2;
                this.threshold = threshold;
            }

            public DuringValidator(long minThreshold, long maxThreshold) {
                form = 3;
                this.minThreshold = minThreshold;
                this.maxThreshold = maxThreshold;
            }

            public DuringValidator(long minStartThreshold, long maxStartThreshold, long minEndThreshold, long maxEndThreshold) {
                form = 4;
                this.minStartThreshold = minStartThreshold;
                this.maxStartThreshold = maxStartThreshold;
                this.minEndThreshold = minEndThreshold;
                this.maxEndThreshold = maxEndThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                if (form == 1) {
                    return rightStart < leftStart &&
                            leftEnd < rightEnd;
                } else if (form == 2) {
                    long distanceStart = leftStart - rightStart;
                    if (distanceStart <= 0 || distanceStart > threshold) {
                        return false;
                    }
                    long distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd <= 0 || distanceEnd > threshold);
                } else if (form == 3) {
                    long distanceStart = leftStart - rightStart;
                    if (distanceStart < minThreshold || distanceStart > maxThreshold) {
                        return false;
                    }
                    long distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd < minThreshold || distanceEnd > maxThreshold);
                } else if (form == 4) {
                    long distanceStart = leftStart - rightStart;
                    if (distanceStart < minStartThreshold || distanceStart > maxStartThreshold) {
                        return false;
                    }
                    long distanceEnd = rightEnd - leftEnd;
                    return !(distanceEnd < minEndThreshold || distanceEnd > maxEndThreshold);
                }
                throw new IllegalStateException("Invalid form: " + form);
            }
        }

        public class FinishesValidator : Validator {
            private long threshold;

            public FinishesValidator() {
            }

            public FinishesValidator(long threshold) {
                this.threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                if (threshold == null) {
                    return rightStart < leftStart && leftEnd == rightEnd;
                } else {
                    if (rightStart >= leftStart) {
                        return false;
                    }
                    long delta = Math.Abs(leftEnd - rightEnd);
                    return delta <= threshold;
                }
            }
        }

        public class FinishedByValidator : Validator {
            private long threshold;

            public FinishedByValidator() {
            }

            public FinishedByValidator(long threshold) {
                this.threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                if (threshold == null) {
                    return leftStart < rightStart && leftEnd == rightEnd;
                } else {
                    if (leftStart >= rightStart) {
                        return false;
                    }
                    long delta = Math.Abs(leftEnd - rightEnd);
                    return delta <= threshold;
                }
            }
        }

        public class IncludesValidator : Validator {
    
            private int form;
            private long threshold;
            private long minThreshold;
            private long maxThreshold;
            private long minStartThreshold;
            private long maxStartThreshold;
            private long minEndThreshold;
            private long maxEndThreshold;
    
            public IncludesValidator() {
                form = 1;
            }

            public IncludesValidator(long threshold) {
                form = 2;
                this.threshold = threshold;
            }

            public IncludesValidator(long minThreshold, long maxThreshold) {
                form = 3;
                this.minThreshold = minThreshold;
                this.maxThreshold = maxThreshold;
            }

            public IncludesValidator(long minStartThreshold, long maxStartThreshold, long minEndThreshold, long maxEndThreshold) {
                form = 4;
                this.minStartThreshold = minStartThreshold;
                this.maxStartThreshold = maxStartThreshold;
                this.minEndThreshold = minEndThreshold;
                this.maxEndThreshold = maxEndThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                if (form == 1) {
                    return leftStart < rightStart &&
                            rightEnd < leftEnd;
                } else if (form == 2) {
                    long distanceStart = rightStart - leftStart;
                    if (distanceStart <= 0 || distanceStart > threshold) {
                        return false;
                    }
                    long distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd <= 0 || distanceEnd > threshold);
                } else if (form == 3) {
                    long distanceStart = rightStart - leftStart;
                    if (distanceStart < minThreshold || distanceStart > maxThreshold) {
                        return false;
                    }
                    long distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd < minThreshold || distanceEnd > maxThreshold);
                } else if (form == 4) {
                    long distanceStart = rightStart - leftStart;
                    if (distanceStart < minStartThreshold || distanceStart > maxStartThreshold) {
                        return false;
                    }
                    long distanceEnd = leftEnd - rightEnd;
                    return !(distanceEnd < minEndThreshold || distanceEnd > maxEndThreshold);
                }
                throw new IllegalStateException("Invalid form: " + form);
            }
        }

        public class MeetsValidator : Validator {
            private long threshold;

            public MeetsValidator() {
            }

            public MeetsValidator(long threshold) {
                this.threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                if (threshold == null) {
                    return rightStart == leftEnd;
                } else {
                    long delta = Math.Abs(rightStart - leftEnd);
                    return delta <= threshold;
                }
            }
        }

        public class MetByValidator : Validator {
            private long threshold;

            public MetByValidator() {
            }

            public MetByValidator(long threshold) {
                this.threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                if (threshold == null) {
                    return leftStart == rightEnd;
                } else {
                    long delta = Math.Abs(leftStart - rightEnd);
                    return delta <= threshold;
                }
            }
        }

        public class OverlapsValidator : Validator {
            private int form;
            private long threshold;
            private long minThreshold;
            private long maxThreshold;

            public OverlapsValidator() {
                form = 1;
            }

            public OverlapsValidator(long threshold) {
                form = 2;
                this.threshold = threshold;
            }

            public OverlapsValidator(long minThreshold, long maxThreshold) {
                form = 3;
                this.minThreshold = minThreshold;
                this.maxThreshold = maxThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                bool match = (leftStart < rightStart) &&
                        (rightStart < leftEnd) &&
                        (leftEnd < rightEnd);
    
                if (form == 1) {
                    return match;
                } else if (form == 2) {
                    if (!match) {
                        return false;
                    }
                    long delta = leftEnd - rightStart;
                    return 0 <= delta && delta <= threshold;
                } else if (form == 3) {
                    if (!match) {
                        return false;
                    }
                    long delta = leftEnd - rightStart;
                    return minThreshold <= delta && delta <= maxThreshold;
                }
                throw new ArgumentException("Invalid form " + form);
            }
        }

        public class OverlappedByValidator : Validator {
            private int form;
            private long threshold;
            private long minThreshold;
            private long maxThreshold;

            public OverlappedByValidator() {
                form = 1;
            }

            public OverlappedByValidator(long threshold) {
                form = 2;
                this.threshold = threshold;
            }

            public OverlappedByValidator(long minThreshold, long maxThreshold) {
                form = 3;
                this.minThreshold = minThreshold;
                this.maxThreshold = maxThreshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
    
                bool match = (rightStart < leftStart) &&
                        (leftStart < rightEnd) &&
                        (rightEnd < leftEnd);
    
                if (form == 1) {
                    return match;
                } else if (form == 2) {
                    if (!match) {
                        return false;
                    }
                    long delta = rightEnd - leftStart;
                    return 0 <= delta && delta <= threshold;
                } else if (form == 3) {
                    if (!match) {
                        return false;
                    }
                    long delta = rightEnd - leftStart;
                    return minThreshold <= delta && delta <= maxThreshold;
                }
                throw new ArgumentException("Invalid form " + form);
            }
        }

        public class StartsValidator : Validator {
            private long threshold;

            public StartsValidator() {
            }

            public StartsValidator(long threshold) {
                this.threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                if (threshold == null) {
                    return (leftStart == rightStart) && (leftEnd < rightEnd);
                } else {
                    long delta = Math.Abs(leftStart - rightStart);
                    return (delta <= threshold) && (leftEnd < rightEnd);
                }
            }
        }

        public class StartedByValidator : Validator {
            private long threshold;

            public StartedByValidator() {
            }

            public StartedByValidator(long threshold) {
                this.threshold = threshold;
            }
    
            public bool Validate(long leftStart, long leftEnd, long rightStart, long rightEnd) {
                if (threshold == null) {
                    return (leftStart == rightStart) && (leftEnd > rightEnd);
                } else {
                    long delta = Math.Abs(leftStart - rightStart);
                    return (delta <= threshold) && (leftEnd > rightEnd);
                }
            }
        }
    }
} // end of namespace
