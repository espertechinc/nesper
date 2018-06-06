///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.timer;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprCast : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionCaseDates(epService);
            RunAssertionCastSimple(epService);
            RunAssertionCastAsParse(epService);
            RunAssertionCastDoubleAndNull_OM(epService);
            RunAssertionCastStringAndNull_Compile(epService);
            RunAssertionCastInterface(epService);
            RunAssertionCastBoolean(epService);
        }
    
        private void RunAssertionCaseDates(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create map schema MyType(yyyymmdd string, yyyymmddhhmmss string, hhmmss string, yyyymmddhhmmssvv string)");
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_StringAlphabetic));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionDatetimeBaseTypes(epService, true);
            RunAssertionDatetimeBaseTypes(epService, false);
            RunAssertionDatetimeJava8Types(epService);
    
            RunAssertionDatetimeRenderOutCol(epService);
    
            RunAssertionDynamicDateFormat(epService);
    
            RunAssertionConstantDate(epService);
    
            RunAssertionISO8601Date(epService);
    
            RunAssertionDateformatNonString(epService);
    
            RunAssertionDatetimeInvalid(epService);
        }

        private void RunAssertionDateformatNonString(EPServiceProvider epService) {
            var sdt = SupportDateTime.Make("2002-05-30T09:00:00.000");

#if false
            string sdfDate = SimpleDateFormat.Instance.Format(sdt.Utildate);
            string ldtDate = sdt.Localdate.Format(DateTimeFormatter.ISO_DATE_TIME);
    
            var epl = "select " +
                    "cast('" + sdfDate + "',date,dateformat:SimpleDateFormat.Instance) as c0," +
                    "cast('" + ldtDate + "',localdatetime,dateformat:java.time.format.DateTimeFormatter.ISO_DATE_TIME) as c1" +
                    " from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            var @event = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@event, "c0,c1".Split(','), new object[]
            {
                sdt.Utildate, sdt.Localdate
            });
    
            stmt.Dispose();
#endif
        }

        private void RunAssertionISO8601Date(EPServiceProvider epService) {
            var epl = "select " +
                      "cast('1997-07-16T19:20:30Z',dto,dateformat:'iso') as c0," +
                      "cast('1997-07-16T19:20:30+01:00',dto,dateformat:'iso') as c1," +
                      "cast('1997-07-16T19:20:30',dto,dateformat:'iso') as c2," +
                      "cast('1997-07-16T19:20:30.45Z',dto,dateformat:'iso') as c3," +
                      "cast('1997-07-16T19:20:30.45+01:00',dto,dateformat:'iso') as c4," +
                      "cast('1997-07-16T19:20:30.45',dto,dateformat:'iso') as c5," +
                      "cast('1997-07-16T19:20:30.45',long,dateformat:'iso') as c6," +
                      "cast('1997-07-16T19:20:30.45',dto,dateformat:'iso') as c7," +
                      "cast(TheString,dto,dateformat:'iso') as c8," +
                      "cast(TheString,long,dateformat:'iso') as c9," +
                      "cast(TheString,dto,dateformat:'iso') as c10" +
                      " from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean());
            var @event = listener.AssertOneGetNewAndReset();
            SupportDateTimeUtil.CompareDate(@event.Get("c0").AsDateTimeOffset(), 1997, 7, 16, 19, 20, 30, 0, "GMT+00:00");
            SupportDateTimeUtil.CompareDate(@event.Get("c1").AsDateTimeOffset(), 1997, 7, 16, 19, 20, 30, 0, "GMT+01:00");
            SupportDateTimeUtil.CompareDate(@event.Get("c2").AsDateTimeOffset(), 1997, 7, 16, 19, 20, 30, 0, TimeZoneInfo.Local.StandardName);
            SupportDateTimeUtil.CompareDate(@event.Get("c3").AsDateTimeOffset(), 1997, 7, 16, 19, 20, 30, 450, "GMT+00:00");
            SupportDateTimeUtil.CompareDate(@event.Get("c4").AsDateTimeOffset(), 1997, 7, 16, 19, 20, 30, 450, "GMT+01:00");
            SupportDateTimeUtil.CompareDate(@event.Get("c5").AsDateTimeOffset(), 1997, 7, 16, 19, 20, 30, 450, TimeZoneInfo.Local.StandardName);
            Assert.That(@event.Get("c6").GetType(), Is.EqualTo(typeof(long)));
            Assert.That(@event.Get("c7").GetType(), Is.EqualTo(typeof(DateTimeOffset)));
            foreach (var prop in "c8,c9,c10".Split(',')) {
                Assert.IsNull(@event.Get(prop));
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionConstantDate(EPServiceProvider epService) {
            var epl = "select cast('20030201',dto,dateformat:\"yyyyMMdd\") as c0 from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var expectedDate = DateTimeOffset.ParseExact("20030201", "yyyyMMdd", null, DateTimeStyles.None);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new object[] { expectedDate });

            stmt.Dispose();
        }
    
        private void RunAssertionDynamicDateFormat(EPServiceProvider epService) {

            // try legacy date types
            var epl = "select " +
                      "cast(a,date,dateformat:b) as c0," +
                      "cast(a,dto,dateformat:b) as c1," +
                      "cast(a,dtx,dateformat:b) as c2," +
                      "cast(a,long,dateformat:b) as c3" +
                      " from SupportBean_StringAlphabetic";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            RunAssertionDynamicDateFormat(epService, listener, "20100502", "yyyyMMdd");
            RunAssertionDynamicDateFormat(epService, listener, "20100502101112", "yyyyMMddhhmmss");
            RunAssertionDynamicDateFormat(epService, listener, null, "yyyyMMdd");
    
            // invalid date
            try {
                epService.EPRuntime.SendEvent(new SupportBean_StringAlphabetic("x", "yyyyMMddhhmmss"));
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessageContains(ex, "Exception parsing date 'x' format 'yyyyMMddhhmmss': String was not recognized as a valid DateTime.");
            }

#if NO_ERROR_IN_CLR
            // invalid format
            try {
                epService.EPRuntime.SendEvent(new SupportBean_StringAlphabetic("20100502", "UUHHYY"));
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessageContains(ex, "Illegal pattern character 'U'");
            }
#endif

            stmt.Dispose();
        }
    
        private void RunAssertionDynamicDateFormat(
            EPServiceProvider epService, 
            SupportUpdateListener listener, 
            string date, 
            string format)
        {
            epService.EPRuntime.SendEvent(new SupportBean_StringAlphabetic(date, format));

            DateTime? expectedDate = null;
            if (date != null)
            {
                expectedDate = DateTime.ParseExact(date, format, null, DateTimeStyles.None);
            }

            DateTimeOffset? expectedDto = null;
            DateTimeEx expectedDtx = null;
            long? theLong = null;

            if (expectedDate != null)
            {
                expectedDtx = DateTimeEx.GetInstance(TimeZoneInfo.Local, expectedDate.Value);
                expectedDto = expectedDtx.DateTime;
                theLong = expectedDtx.TimeInMillis;
            }

            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1,c2,c3".Split(','), new object[] {
                expectedDate,
                expectedDto,
                expectedDtx,
                theLong
            });
        }
    
        private void RunAssertionDatetimeInvalid(EPServiceProvider epService) {
            // not a valid named parameter
            SupportMessageAssertUtil.TryInvalid(epService, "select cast(TheString, date, x:1) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'cast(TheString,date,x:1)': Unexpected named parameter 'x', expecting any of the following: [dateformat]");

            // invalid date format
#if NO_ERROR_IN_CLR
            SupportMessageAssertUtil.TryInvalid(epService, "select cast(TheString, date, dateformat:'BBBBMMDD') from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'cast(TheString,date,dateformat:\"BBB...(42 chars)': Invalid date format 'BBBBMMDD' (as obtained from new SimpleDateFormat): Illegal pattern character 'B'");
#endif
            SupportMessageAssertUtil.TryInvalid(epService, "select cast(TheString, date, dateformat:1) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'cast(TheString,date,dateformat:1)': Failed to validate named parameter 'dateformat', expected a single expression returning a string-typed value");
    
            // invalid input
            SupportMessageAssertUtil.TryInvalid(epService, "select cast(IntPrimitive, date, dateformat:'yyyyMMdd') from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'cast(IntPrimitive,date,dateformat:\"...(45 chars)': Use of the 'dateformat' named parameter requires a string-type input");
    
            // invalid target
            SupportMessageAssertUtil.TryInvalid(epService, "select cast(TheString, int, dateformat:'yyyyMMdd') from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'cast(TheString,int,dateformat:\"yyyy...(41 chars)': Use of the 'dateformat' named parameter requires a target type of long or datetime");
        }
    
        private void RunAssertionDatetimeRenderOutCol(EPServiceProvider epService) {
            var epl = "select cast(yyyymmdd,date,dateformat:\"yyyyMMdd\") from MyType";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            Assert.AreEqual("cast(yyyymmdd,date,dateformat:\"yyyyMMdd\")", stmt.EventType.PropertyNames[0]);
            stmt.Dispose();
        }
    
        private void RunAssertionDatetimeJava8Types(EPServiceProvider epService) {
            var epl = "select " +
                      "cast(yyyymmdd,date,dateformat:\"yyyyMMdd\") as c0, " +
                      "cast(yyyymmdd,System.DateTime,dateformat:\"yyyyMMdd\") as c1, " +
                      "cast(yyyymmdd,dto,dateformat:\"yyyyMMdd\") as c2, " +
                      "cast(yyyymmdd,dtx,dateformat:\"yyyyMMdd\") as c3, " +
                      "cast(yyyymmdd,long,dateformat:\"yyyyMMdd\") as c4, " +
                      "cast(yyyymmdd,System.Int64,dateformat:\"yyyyMMdd\") as c5, " +
                      "cast(yyyymmdd,datetime,dateformat:\"yyyyMMdd\").get(\"month\") as c6, " +
                      "cast(yyyymmdd,long,dateformat:\"yyyyMMdd\").get(\"month\") as c8 " +
                      "from MyType";
            var stmt = SupportModelHelper.CreateByCompileOrParse(epService, false, epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var values = new Dictionary<string, object>();
            values.Put("yyyymmdd", "20100510");
            epService.EPRuntime.SendEvent(values, "MyType");

            var formatYYYYMMdd = "yyyyMMdd";
            var dateYYMMdd = DateTime.ParseExact("20100510", formatYYYYMMdd, null, DateTimeStyles.None);
            var dateYYMMddDto = new DateTimeOffset(dateYYMMdd, TimeZoneInfo.Local.GetUtcOffset(dateYYMMdd));
            var dateYYMMddDtx = DateTimeEx.GetInstance(TimeZoneInfo.Local, dateYYMMdd);

            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1,c2,c3,c4,c5,c6,c8".Split(','), new object[] {
                dateYYMMdd,
                dateYYMMdd,
                dateYYMMddDto,
                dateYYMMddDtx,
                dateYYMMddDto.TimeInMillis(),
                dateYYMMddDto.TimeInMillis(),
                5,
                5
            });

            stmt.Dispose();
        }

        private void RunAssertionDatetimeBaseTypes(EPServiceProvider epService, bool soda)
        {
            var epl = "select " +
                      "cast(yyyymmdd,date,dateformat:\"yyyyMMdd\") as c0, " +
                      "cast(yyyymmdd,System.DateTime,dateformat:\"yyyyMMdd\") as c1, " +
                      "cast(yyyymmdd,dto,dateformat:\"yyyyMMdd\") as c2, " +
                      "cast(yyyymmdd,dtx,dateformat:\"yyyyMMdd\") as c3, " +
                      "cast(yyyymmdd,long,dateformat:\"yyyyMMdd\") as c4, " +
                      "cast(yyyymmdd,System.Int64,dateformat:\"yyyyMMdd\") as c5, " +
                      "cast(yyyymmdd,datetime,dateformat:\"yyyyMMdd\").get(\"month\") as c6, " +
                      "cast(yyyymmdd,long,dateformat:\"yyyyMMdd\").get(\"month\") as c8 " +
                      "from MyType";

            var stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var values = new Dictionary<string, object>();
            values.Put("yyyymmdd", "20100510");
            epService.EPRuntime.SendEvent(values, "MyType");

            var formatYYYYMMdd = "yyyyMMdd";
            var dateYYMMdd = DateTime.ParseExact("20100510", formatYYYYMMdd, null, DateTimeStyles.None);
            var dateYYMMddDto = new DateTimeOffset(dateYYMMdd, TimeZoneInfo.Local.GetUtcOffset(dateYYMMdd));
            var dateYYMMddDtx = DateTimeEx.GetInstance(TimeZoneInfo.Local, dateYYMMdd);

            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "c0,c1,c2,c3,c4,c5,c6,c8".Split(','),
                new object[]
                {
                    dateYYMMdd,
                    dateYYMMdd,
                    dateYYMMddDto,
                    dateYYMMddDtx,
                    dateYYMMddDto.TimeInMillis(),
                    dateYYMMddDto.TimeInMillis(),
                    5,
                    5
                });

            stmt.Dispose();
        }
    
        private void RunAssertionCastSimple(EPServiceProvider epService) {
            var stmtText = "select cast(TheString as string) as t0, " +
                           " cast(IntBoxed, int) as t1, " +
                           " cast(FloatBoxed, " + typeof(float).FullName + ") as t2, " +
                           " cast(TheString, System.String) as t3, " +
                           " cast(IntPrimitive, " + typeof(int).FullName + ") as t4, " +
                           " cast(IntPrimitive, long) as t5, " +
                           " cast(IntPrimitive, System.Object) as t6, " +
                           " cast(FloatBoxed, long) as t7 " +
                           " from " + typeof(SupportBean).FullName;

            var selectTestCase = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            selectTestCase.Events += listener.Update;
    
            Assert.AreEqual(typeof(string), selectTestCase.EventType.GetPropertyType("t0"));
            Assert.AreEqual(typeof(int?), selectTestCase.EventType.GetPropertyType("t1"));
            Assert.AreEqual(typeof(float?), selectTestCase.EventType.GetPropertyType("t2"));
            Assert.AreEqual(typeof(string), selectTestCase.EventType.GetPropertyType("t3"));
            Assert.AreEqual(typeof(int?), selectTestCase.EventType.GetPropertyType("t4"));
            Assert.AreEqual(typeof(long?), selectTestCase.EventType.GetPropertyType("t5"));
            Assert.AreEqual(typeof(object), selectTestCase.EventType.GetPropertyType("t6"));
            Assert.AreEqual(typeof(long?), selectTestCase.EventType.GetPropertyType("t7"));
    
            var bean = new SupportBean("abc", 100);
            bean.FloatBoxed = 9.5f;
            bean.IntBoxed = 3;
            epService.EPRuntime.SendEvent(bean);
            var theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{"abc", 3, 9.5f, "abc", 100, 100L, 100, 9L});
    
            bean = new SupportBean(null, 100);
            bean.FloatBoxed = null;
            bean.IntBoxed = null;
            epService.EPRuntime.SendEvent(bean);
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{null, null, null, null, 100, 100L, 100, null});
            bean = new SupportBean(null, 100);
            bean.FloatBoxed = null;
            bean.IntBoxed = null;
            epService.EPRuntime.SendEvent(bean);
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{null, null, null, null, 100, 100L, 100, null});
    
            // test cast with chained and null
            selectTestCase.Dispose();
            stmtText = "select cast(one as " + typeof(SupportBean).FullName + ").get_TheString as t0," +
                    "cast(null, " + typeof(SupportBean).FullName + ") as t1" +
                    " from " + typeof(SupportBeanObject).FullName;
            selectTestCase = epService.EPAdministrator.CreateEPL(stmtText);
            selectTestCase.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanObject(new SupportBean("E1", 1)));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "t0,t1".Split(','), new object[]{"E1", null});
            Assert.AreEqual(typeof(SupportBean), selectTestCase.EventType.GetPropertyType("t1"));
    
            selectTestCase.Dispose();
        }
    
        private void RunAssertionCastAsParse(EPServiceProvider epService) {
            var stmtText = "select cast(TheString, int) as t0 from " + typeof(SupportBean).FullName;
            var selectTestCase = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            selectTestCase.Events += listener.Update;
    
            Assert.AreEqual(typeof(int?), selectTestCase.EventType.GetPropertyType("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBean("12", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "t0".Split(','), new object[]{12});
    
            selectTestCase.Dispose();
        }
    
        private void RunAssertionCastDoubleAndNull_OM(EPServiceProvider epService) {
            var stmtText = "select cast(item?,double) as t0 " +
                    "from " + typeof(SupportMarkerInterface).FullName;
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.Cast("item?", "double"), "t0");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarkerInterface).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            var selectTestCase = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            selectTestCase.Events += listener.Update;
    
            Assert.AreEqual(typeof(double?), selectTestCase.EventType.GetPropertyType("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(100));
            Assert.AreEqual(100d, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot((byte) 2));
            Assert.AreEqual(2d, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(77.7777));
            Assert.AreEqual(77.7777d, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(6L));
            Assert.AreEqual(6d, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(null));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot("abc"));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("t0"));
    
            selectTestCase.Dispose();
        }
    
        private void RunAssertionCastStringAndNull_Compile(EPServiceProvider epService) {
            var stmtText = "select cast(item?,System.String) as t0 " +
                    "from " + typeof(SupportMarkerInterface).FullName;
    
            var model = epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            var selectTestCase = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            Assert.AreEqual(stmtText, model.ToEPL());
            selectTestCase.Events += listener.Update;
    
            Assert.AreEqual(typeof(string), selectTestCase.EventType.GetPropertyType("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(100));
            Assert.AreEqual("100", listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot((byte) 2));
            Assert.AreEqual("2", listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(77.7777));
            Assert.AreEqual("77.7777", listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(6L));
            Assert.AreEqual("6", listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(null));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("t0"));
    
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot("abc"));
            Assert.AreEqual("abc", listener.AssertOneGetNewAndReset().Get("t0"));
    
            selectTestCase.Dispose();
        }
    
        private void RunAssertionCastInterface(EPServiceProvider epService) {
            var caseExpr = "select cast(item?, " + typeof(SupportMarkerInterface).FullName + ") as t0, " +
                    " cast(item?, " + typeof(ISupportA).FullName + ") as t1, " +
                    " cast(item?, " + typeof(ISupportBaseAB).FullName + ") as t2, " +
                    " cast(item?, " + typeof(ISupportBaseABImpl).FullName + ") as t3, " +
                    " cast(item?, " + typeof(ISupportC).FullName + ") as t4, " +
                    " cast(item?, " + typeof(ISupportD).FullName + ") as t5, " +
                    " cast(item?, " + typeof(ISupportAImplSuperG).FullName + ") as t6, " +
                    " cast(item?, " + typeof(ISupportAImplSuperGImplPlus).FullName + ") as t7 " +
                    " from " + typeof(SupportMarkerInterface).FullName;
    
            var selectTestCase = epService.EPAdministrator.CreateEPL(caseExpr);
            var listener = new SupportUpdateListener();
            selectTestCase.Events += listener.Update;
    
            Assert.AreEqual(typeof(SupportMarkerInterface), selectTestCase.EventType.GetPropertyType("t0"));
            Assert.AreEqual(typeof(ISupportA), selectTestCase.EventType.GetPropertyType("t1"));
            Assert.AreEqual(typeof(ISupportBaseAB), selectTestCase.EventType.GetPropertyType("t2"));
            Assert.AreEqual(typeof(ISupportBaseABImpl), selectTestCase.EventType.GetPropertyType("t3"));
            Assert.AreEqual(typeof(ISupportC), selectTestCase.EventType.GetPropertyType("t4"));
            Assert.AreEqual(typeof(ISupportD), selectTestCase.EventType.GetPropertyType("t5"));
            Assert.AreEqual(typeof(ISupportAImplSuperG), selectTestCase.EventType.GetPropertyType("t6"));
            Assert.AreEqual(typeof(ISupportAImplSuperGImplPlus), selectTestCase.EventType.GetPropertyType("t7"));

            object bean = new SupportBeanDynRoot("abc");
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(bean));
            var theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{bean, null, null, null, null, null, null, null});
    
            bean = new ISupportDImpl("", "", "");
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(bean));
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{null, null, null, null, null, bean, null, null});
    
            bean = new ISupportBCImpl("", "", "");
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(bean));
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{null, null, bean, null, bean, null, null, null});
    
            bean = new ISupportAImplSuperGImplPlus();
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(bean));
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{null, bean, bean, null, bean, null, bean, bean});
    
            bean = new ISupportBaseABImpl("");
            epService.EPRuntime.SendEvent(new SupportBeanDynRoot(bean));
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{null, null, bean, bean, null, null, null, null});
    
            selectTestCase.Dispose();
        }
    
        private void RunAssertionCastBoolean(EPServiceProvider epService)
        {
            var stmtText =
                "select " +
                "cast(BoolPrimitive, bool) as t0, " +
                " cast(BoolBoxed | BoolPrimitive, bool) as t1, " +
                " cast(BoolBoxed, string) as t2 " +
                " from " + typeof(SupportBean).FullName;
    
            var selectTestCase = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            selectTestCase.Events += listener.Update;
    
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("t0"));
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("t1"));
            Assert.AreEqual(typeof(string), selectTestCase.EventType.GetPropertyType("t2"));
    
            var bean = new SupportBean("abc", 100);
            bean.BoolPrimitive = true;
            bean.BoolBoxed = true;
            epService.EPRuntime.SendEvent(bean);
            var theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{true, true, "True"});
    
            bean = new SupportBean(null, 100);
            bean.BoolPrimitive = false;
            bean.BoolBoxed = false;
            epService.EPRuntime.SendEvent(bean);
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{false, false, "False"});
    
            bean = new SupportBean(null, 100);
            bean.BoolPrimitive = true;
            bean.BoolBoxed = null;
            epService.EPRuntime.SendEvent(bean);
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{true, null, null});
    
            selectTestCase.Dispose();
        }
    
        private void AssertResults(EventBean theEvent, object[] result) {
            for (var i = 0; i < result.Length; i++) {
                Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
            }
        }
    }
} // end of namespace
