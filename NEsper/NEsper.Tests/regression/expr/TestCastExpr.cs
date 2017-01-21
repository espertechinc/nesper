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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.timer;
using com.espertech.esper.support.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
    public class TestCastExpr 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }

        [Test]
        public void TestCaseDates() 
        {
            _epService.EPAdministrator.CreateEPL("create map schema MyType(yyyymmdd string)");
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_StringAlphabetic>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            RunAssertionDatetimeTypes(true);
            RunAssertionDatetimeTypes(false);

            RunAssertionDatetimeRenderOutCol();

            RunAssertionDatetimeInvalid();

            RunAssertionDynamicDateFormat();

            RunAssertionConstantDate();

            RunAssertionISO8601Date();
        }

        private void RunAssertionISO8601Date() {
            var epl = "select " +
                    "cast('1997-07-16T19:20:30Z',date,dateformat:'iso') as c0," +
                    "cast('1997-07-16T19:20:30+01:00',date,dateformat:'iso') as c1," +
                    "cast('1997-07-16T19:20:30',date,dateformat:'iso') as c2," +
                    "cast('1997-07-16T19:20:30.45Z',date,dateformat:'iso') as c3," +
                    "cast('1997-07-16T19:20:30.45+01:00',date,dateformat:'iso') as c4," +
                    "cast('1997-07-16T19:20:30.45',date,dateformat:'iso') as c5," +
                    "cast('1997-07-16T19:20:30.45',long,dateformat:'iso') as c6," +
                    "cast('1997-07-16T19:20:30.45',date,dateformat:'iso') as c7," +
                    "cast(theString,date,dateformat:'iso') as c8," +
                    "cast(theString,long,dateformat:'iso') as c9," +
                    "cast(theString,date,dateformat:'iso') as c10" +
                    " from SupportBean";
            var stmt = _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBean());
            var @event = _listener.AssertOneGetNewAndReset();
            SupportDateTimeUtil.CompareDate(@event.Get("c0").AsDateTimeOffset(), 1997, 7, 16, 19, 20, 30, 0, "GMT+00:00");
            SupportDateTimeUtil.CompareDate(@event.Get("c1").AsDateTimeOffset(), 1997, 7, 16, 19, 20, 30, 0, "GMT+01:00");
            SupportDateTimeUtil.CompareDate(@event.Get("c2").AsDateTimeOffset(), 1997, 7, 16, 19, 20, 30, 0, TimeZone.CurrentTimeZone.StandardName);
            SupportDateTimeUtil.CompareDate(@event.Get("c3").AsDateTimeOffset(), 1997, 7, 16, 19, 20, 30, 450, "GMT+00:00");
            SupportDateTimeUtil.CompareDate(@event.Get("c4").AsDateTimeOffset(), 1997, 7, 16, 19, 20, 30, 450, "GMT+01:00");
            SupportDateTimeUtil.CompareDate(@event.Get("c5").AsDateTimeOffset(), 1997, 7, 16, 19, 20, 30, 450, TimeZone.CurrentTimeZone.StandardName);
            Assert.That(@event.Get("c6").GetType(), Is.EqualTo(typeof (long)));
            Assert.That(@event.Get("c7").GetType(), Is.EqualTo(typeof (DateTimeOffset)));
            foreach (var prop in "c8,c9,c10".Split(',')) {
                Assert.IsNull(@event.Get(prop));
            }
        }

        private void RunAssertionConstantDate() 
        {
            var epl = "select cast('20030201',date,dateformat:\"yyyyMMdd\") as c0 from SupportBean";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(_listener);

            var expectedDate = DateTimeOffset.ParseExact("20030201", "yyyyMMdd", null, DateTimeStyles.None);
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0".Split(','), new Object[] {expectedDate});

            stmt.Dispose();
        }

        private void RunAssertionDynamicDateFormat() 
        {
            var epl = "select " +
                    "cast(a,date,dateformat:b) as c0," +
                    "cast(a,long,dateformat:b) as c1" +
                    " from SupportBean_StringAlphabetic";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(_listener);

            RunAssertionDynamicDateFormat("20100502", "yyyyMMdd");
            RunAssertionDynamicDateFormat("20100502101112", "yyyyMMddhhmmss");
            RunAssertionDynamicDateFormat(null, "yyyyMMdd");

            // invalid date
            try {
                _epService.EPRuntime.SendEvent(new SupportBean_StringAlphabetic("x", "yyyyMMddhhmmss"));
            }
            catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessageContains(ex, "Exception parsing date 'x' format 'yyyyMMddhhmmss': String was not recognized as a valid DateTime");
            }

#if NO_ERROR_IN_CLR
            // invalid format
            try {
                _epService.EPRuntime.SendEvent(new SupportBean_StringAlphabetic("20100502", "UUHHYY"));
            }
            catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessageContains(ex, "Illegal pattern character 'U'");
            }
#endif

            stmt.Dispose();
        }

        private void RunAssertionDynamicDateFormat(String date, String format)
        {
            _epService.EPRuntime.SendEvent(new SupportBean_StringAlphabetic(date, format));

            DateTimeOffset? expectedDate = null;
            if (date != null) {
                expectedDate = DateTimeOffset.ParseExact(date, format, null, DateTimeStyles.None);
            }

            long? theLong = null;
            if (expectedDate != null)
            {
                theLong = expectedDate.Value.TimeInMillis();
            }
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new Object[] {expectedDate, theLong});
        }

        private void RunAssertionDatetimeInvalid() {
            // not a valid named parameter
            SupportMessageAssertUtil.TryInvalid(_epService, "select cast(theString, date, x:1) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'cast(theString,date,x:1)': Unexpected named parameter 'x', expecting any of the following: [dateformat]");

            // invalid date format
#if NO_ERROR_IN_CLR
            SupportMessageAssertUtil.TryInvalid(_epService, "select cast(theString, date, dateformat:'BBBBMMDD') from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'cast(theString,date,dateformat:\"BBB...(42 chars)': Invalid date format 'YYYYMMDD': Illegal pattern character 'Y'");
#endif
            SupportMessageAssertUtil.TryInvalid(_epService, "select cast(theString, date, dateformat:1) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'cast(theString,date,dateformat:1)': Failed to validate named parameter 'dateformat', expected a single expression returning a string-typed value");

            // invalid input
            SupportMessageAssertUtil.TryInvalid(_epService, "select cast(intPrimitive, date, dateformat:'yyyyMMdd') from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'cast(intPrimitive,date,dateformat:\"...(45 chars)': Use of the 'dateformat' named parameter requires a string-type input");

            // invalid target
            SupportMessageAssertUtil.TryInvalid(_epService, "select cast(theString, int, dateformat:'yyyyMMdd') from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'cast(theString,int,dateformat:\"yyyy...(41 chars)': Use of the 'dateformat' named parameter requires a target type of datetime or long [select cast(theString, int, dateformat:'yyyyMMdd') from SupportBean]");
        }

        private void RunAssertionDatetimeRenderOutCol()
        {
            var epl = "select cast(yyyymmdd,date,dateformat:\"yyyyMMdd\") from MyType";
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            Assert.AreEqual("cast(yyyymmdd,date,dateformat:\"yyyyMMdd\")", stmt.EventType.PropertyNames[0]);
            stmt.Dispose();
        }

        private void RunAssertionDatetimeTypes(bool soda)
        {
            var epl = "select " +
                    "cast(yyyymmdd,date,dateformat:\"yyyyMMdd\") as c0, " +
                    "cast(yyyymmdd,System.DateTime,dateformat:\"yyyyMMdd\") as c1, " +
                    "cast(yyyymmdd,long,dateformat:\"yyyyMMdd\") as c2, " +
                    "cast(yyyymmdd,System.Int64,dateformat:\"yyyyMMdd\") as c3, " +
                    "cast(yyyymmdd,date,dateformat:\"yyyyMMdd\").get(\"month\") as c6, " +
                    "cast(yyyymmdd,long,dateformat:\"yyyyMMdd\").get(\"month\") as c8 " +
                    "from MyType";
            var stmt = SupportModelHelper.CreateByCompileOrParse(_epService, soda, epl);
            stmt.AddListener(_listener);

            var values = new Dictionary<string, object>();
            values.Put("yyyymmdd", "20100510");
            _epService.EPRuntime.SendEvent(values, "MyType");

            var formatYYYYMMdd = "yyyyMMdd";
            var dateYYMMddDate = DateTimeOffset.ParseExact("20100510", formatYYYYMMdd, null, DateTimeStyles.None);
            
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0,c1,c2,c3,c6,c8".Split(','), new Object[] {
                    dateYYMMddDate, 
                    dateYYMMddDate, 
                    dateYYMMddDate.TimeInMillis(),
                    dateYYMMddDate.TimeInMillis(),
                    5,
                    5
            });

            stmt.Dispose();
        }
    
        [Test]
        public void TestCastSimple()
        {
            var stmtText = "select cast(TheString as string) as t0, " +
                              " cast(IntBoxed, int) as t1, " +
                              " cast(FloatBoxed, " + typeof(float).FullName + ") as t2, " +
                              " cast(TheString, System.String) as t3, " +
                              " cast(IntPrimitive, " + typeof(int).FullName + ") as t4, " +
                              " cast(IntPrimitive, long) as t5, " +
                              " cast(IntPrimitive, System.Object) as t6, " +
                              " cast(FloatBoxed, long) as t7 " +
                              " from " + typeof(SupportBean).FullName;
    
            var selectTestCase = _epService.EPAdministrator.CreateEPL(stmtText);
            selectTestCase.Events += _listener.Update;
    
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
            _epService.EPRuntime.SendEvent(bean);
            var theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {"abc", 3, 9.5f, "abc", 100, 100L, 100, 9l});
    
            bean = new SupportBean(null, 100);
            bean.FloatBoxed = null;
            bean.IntBoxed = null;
            _epService.EPRuntime.SendEvent(bean);
            theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {null, null, null, null, 100, 100L, 100, null});
            bean = new SupportBean(null, 100);
            bean.FloatBoxed = null;
            bean.IntBoxed = null;
            _epService.EPRuntime.SendEvent(bean);
            theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {null, null, null, null, 100, 100L, 100, null});
    
            // test cast with chained
            selectTestCase.Dispose();
            stmtText = "select cast(one as " + typeof(SupportBean).FullName + ").get_TheString() as t0" +
                              " from " + typeof(SupportBeanObject).FullName;
            selectTestCase = _epService.EPAdministrator.CreateEPL(stmtText);
            selectTestCase.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBeanObject(new SupportBean("E1", 1)));
            Assert.AreEqual("E1", _listener.AssertOneGetNewAndReset().Get("t0"));
        }
    
        [Test]
        public void TestCastAsParse()
        {
            var stmtText = "select cast(TheString, int) as t0 from " + typeof(SupportBean).FullName;
            var selectTestCase = _epService.EPAdministrator.CreateEPL(stmtText);
            selectTestCase.Events += _listener.Update;
    
            Assert.AreEqual(typeof(int?), selectTestCase.EventType.GetPropertyType("t0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("12", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "t0".Split(','), new Object[] {12});
        }
    
        [Test]
        public void TestCastDoubleAndNull_OM()
        {
            var stmtText = "select cast(item?,double) as t0 " +
                              "from " + typeof(SupportMarkerInterface).FullName;
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.Cast("item?", "double"), "t0");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarkerInterface).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            var selectTestCase = _epService.EPAdministrator.Create(model);
            selectTestCase.Events += _listener.Update;
    
            Assert.AreEqual(typeof(double?), selectTestCase.EventType.GetPropertyType("t0"));
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(100));
            Assert.AreEqual(100d, _listener.AssertOneGetNewAndReset().Get("t0"));
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot((byte)2));
            Assert.AreEqual(2d, _listener.AssertOneGetNewAndReset().Get("t0"));
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(77.7777));
            Assert.AreEqual(77.7777d, _listener.AssertOneGetNewAndReset().Get("t0"));
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(6L));
            Assert.AreEqual(6d, _listener.AssertOneGetNewAndReset().Get("t0"));
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(null));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("t0"));
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot("abc"));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("t0"));
        }
    
        [Test]
        public void TestCastStringAndNull_Compile()
        {
            var stmtText = "select cast(item?,System.String) as t0 " +
                              "from " + typeof(SupportMarkerInterface).FullName;
    
            var model = _epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            var selectTestCase = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtText, model.ToEPL());
            selectTestCase.Events += _listener.Update;
    
            Assert.AreEqual(typeof(string), selectTestCase.EventType.GetPropertyType("t0"));
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(100));
            Assert.AreEqual("100", _listener.AssertOneGetNewAndReset().Get("t0"));
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot((byte)2));
            Assert.AreEqual("2", _listener.AssertOneGetNewAndReset().Get("t0"));
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(77.7777));
            Assert.AreEqual("77.7777", _listener.AssertOneGetNewAndReset().Get("t0"));
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(6L));
            Assert.AreEqual("6", _listener.AssertOneGetNewAndReset().Get("t0"));
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(null));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("t0"));
    
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot("abc"));
            Assert.AreEqual("abc", _listener.AssertOneGetNewAndReset().Get("t0"));
        }
    
        [Test]
        public void TestCastInterface()
        {
            var caseExpr = "select cast(item?, " + typeof(SupportMarkerInterface).FullName + ") as t0, " +
                              " cast(item?, " + typeof(ISupportA).FullName + ") as t1, " +
                              " cast(item?, " + typeof(ISupportBaseAB).FullName + ") as t2, " +
                              " cast(item?, " + typeof(ISupportBaseABImpl).FullName + ") as t3, " +
                              " cast(item?, " + typeof(ISupportC).FullName + ") as t4, " +
                              " cast(item?, " + typeof(ISupportD).FullName + ") as t5, " +
                              " cast(item?, " + typeof(ISupportAImplSuperG).FullName + ") as t6, " +
                              " cast(item?, " + typeof(ISupportAImplSuperGImplPlus).FullName + ") as t7 " +
                              " from " + typeof(SupportMarkerInterface).FullName;
    
            var selectTestCase = _epService.EPAdministrator.CreateEPL(caseExpr);
            selectTestCase.Events += _listener.Update;
    
            Assert.AreEqual(typeof(SupportMarkerInterface), selectTestCase.EventType.GetPropertyType("t0"));
            Assert.AreEqual(typeof(ISupportA), selectTestCase.EventType.GetPropertyType("t1"));
            Assert.AreEqual(typeof(ISupportBaseAB), selectTestCase.EventType.GetPropertyType("t2"));
            Assert.AreEqual(typeof(ISupportBaseABImpl), selectTestCase.EventType.GetPropertyType("t3"));
            Assert.AreEqual(typeof(ISupportC), selectTestCase.EventType.GetPropertyType("t4"));
            Assert.AreEqual(typeof(ISupportD), selectTestCase.EventType.GetPropertyType("t5"));
            Assert.AreEqual(typeof(ISupportAImplSuperG), selectTestCase.EventType.GetPropertyType("t6"));
            Assert.AreEqual(typeof(ISupportAImplSuperGImplPlus), selectTestCase.EventType.GetPropertyType("t7"));
    
            Object bean = new SupportBeanDynRoot("abc");
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(bean));
            var theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {bean, null, null, null, null, null, null, null});
    
            bean = new ISupportDImpl("", "", "");
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(bean));
            theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {null, null, null, null, null, bean, null, null});
    
            bean = new ISupportBCImpl("", "", "");
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(bean));
            theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {null, null, bean, null, bean, null, null, null});
    
            bean = new ISupportAImplSuperGImplPlus();
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(bean));
            theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {null, bean, bean, null, bean, null, bean, bean});
    
            bean = new ISupportBaseABImpl("");
            _epService.EPRuntime.SendEvent(new SupportBeanDynRoot(bean));
            theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {null, null, bean, bean, null, null, null, null});
        }
    
        [Test]
        public void TestCastBoolean()
        {
            var stmtText = "select cast(BoolPrimitive as " + typeof(bool).FullName + ") as t0, " +
                              " cast(BoolBoxed | BoolPrimitive, boolean) as t1, " +
                              " cast(BoolBoxed, string) as t2 " +
                              " from " + typeof(SupportBean).FullName;
    
            var selectTestCase = _epService.EPAdministrator.CreateEPL(stmtText);
            selectTestCase.Events += _listener.Update;
    
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("t0"));
            Assert.AreEqual(typeof(bool?), selectTestCase.EventType.GetPropertyType("t1"));
            Assert.AreEqual(typeof(string), selectTestCase.EventType.GetPropertyType("t2"));
    
            var bean = new SupportBean("abc", 100);
            bean.BoolPrimitive = true;
            bean.BoolBoxed = true;
            _epService.EPRuntime.SendEvent(bean);
            var theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {true, true, "True"});
    
            bean = new SupportBean(null, 100);
            bean.BoolPrimitive = false;
            bean.BoolBoxed = false;
            _epService.EPRuntime.SendEvent(bean);
            theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {false, false, "False"});
    
            bean = new SupportBean(null, 100);
            bean.BoolPrimitive = true;
            bean.BoolBoxed = null;
            _epService.EPRuntime.SendEvent(bean);
            theEvent = _listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new Object[] {true, null, null});
        }
    
        [Test]
        public void TestCastMapStringInt()
        {
            var map = new Dictionary<string,object>();
            map.Put("anInt",typeof(string));
            map.Put("anDouble",typeof(string));
            map.Put("anLong",typeof(string));
            map.Put("anFloat",typeof(string));
            map.Put("anByte",typeof(string));
            map.Put("anShort",typeof(string));
            map.Put("IntPrimitive",typeof(int));
            map.Put("IntBoxed",typeof(int));
    
            var config = new Configuration();
            config.AddEventType("TestEvent", map);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
    
            var stmt = "select cast(anInt, int) as intVal, " +
                                "cast(anDouble, double) as doubleVal, " +
                                "cast(anLong, long) as longVal, " +
                                "cast(anFloat, float) as floatVal, " +
                                "cast(anByte, byte) as byteVal, " +
                                "cast(anShort, short) as shortVal, " +
                                "cast(IntPrimitive, int) as intOne, " +
                                "cast(IntBoxed, int) as intTwo, " +
                                "cast(IntPrimitive, " + typeof(long).FullName + ") as longOne, " +
                                "cast(IntBoxed, long) as longTwo " +
                        "from TestEvent";
            
            var statement = _epService.EPAdministrator.CreateEPL(stmt);
            statement.Events += _listener.Update;
            
            map = new Dictionary<string,object>();
            map.Put("anInt","100");
            map.Put("anDouble","1.4E-1");
            map.Put("anLong","-10");
            map.Put("anFloat","1.001");
            map.Put("anByte","0x0A");
            map.Put("anShort","223");
            map.Put("IntPrimitive",10);
            map.Put("IntBoxed",11);
    
            _epService.EPRuntime.SendEvent(map, "TestEvent");
            var row = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(100, row.Get("intVal"));
            Assert.AreEqual(0.14d, row.Get("doubleVal"));
            Assert.AreEqual(-10L, row.Get("longVal"));
            Assert.AreEqual(1.001f, row.Get("floatVal"));
            Assert.AreEqual((byte)10, row.Get("byteVal"));
            Assert.AreEqual((short)223, row.Get("shortVal"));
            Assert.AreEqual(10, row.Get("intOne"));
            Assert.AreEqual(11, row.Get("intTwo"));
            Assert.AreEqual(10L, row.Get("longOne"));
            Assert.AreEqual(11L, row.Get("longTwo"));
        }

        [Test]
        public void TestDoubleCastWithProperties()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBeanRendererOne>();

            var stmtText = "select cast(cast(StringObjectMap('value'), string), int) as t0 " +
                           " from " + typeof(SupportBeanRendererOne).FullName;

            using (var stmt = _epService.EPAdministrator.CreateEPL(stmtText))
            {
                stmt.Events += _listener.Update;

                Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("t0"));

                var bean = new SupportBeanRendererOne();
                bean.StringObjectMap = new NullableDictionary<string, object>();
                bean.StringObjectMap["value"] = "10";
                _epService.EPRuntime.SendEvent(bean);

                var theEvent = _listener.AssertOneGetNewAndReset();
                AssertResults(theEvent, new Object[] { 10 });
            }
        }

        private void AssertResults(EventBean theEvent, Object[] result)
        {
            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
            }
        }
    }
}
