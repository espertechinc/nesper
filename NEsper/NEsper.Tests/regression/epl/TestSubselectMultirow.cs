///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestSubselectMultirow
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));
            config.AddEventType("S0", typeof(SupportBean_S0));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestMultirowSingleColumn()
        {
            // test named window as well as stream
            _epService.EPAdministrator.CreateEPL("create window SupportWindow.win:length(3) as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into SupportWindow select * from SupportBean");

            String stmtText = "select p00, (select Window(IntPrimitive) from SupportBean.win:keepall() sb) as val from S0 as s0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
            String[] fields = "p00,val".Split(',');

            Object[][] rows = new Object[][]{
                    new Object[] {"p00", typeof(string)},
                    new Object[] {"val", typeof(int?[])}
            };
            for (int i = 0; i < rows.Length; i++)
            {
                String message = "Failed assertion for " + rows[i][0];
                EventPropertyDescriptor prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                Assert.AreEqual(rows[i][1], prop.PropertyType, message);
            }

            _epService.EPRuntime.SendEvent(new SupportBean("T1", 5));
            _epService.EPRuntime.SendEvent(new SupportBean("T2", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("T3", 15));
            _epService.EPRuntime.SendEvent(new SupportBean("T1", 6));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, new int[] { 5, 10, 15, 6 } });

            // test named window and late start
            stmt.Dispose();

            stmtText = "select p00, (select Window(IntPrimitive) from SupportWindow) as val from S0 as s0";
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, new int[] { 10, 15, 6 } });  // length window 3

            _epService.EPRuntime.SendEvent(new SupportBean("T1", 5));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, new int[] { 15, 6, 5 } });  // length window 3
        }

        [Test]
        public void TestMultirowUnderlyingCorrelated()
        {
            String stmtText = "select p00, " +
                    "(select Window(sb.*) from SupportBean.win:keepall() sb where TheString = s0.P00) as val " +
                    "from S0 as s0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            Object[][] rows = new Object[][]{
                    new Object[] {"p00", typeof(string)},
                    new Object[] {"val", typeof(SupportBean[])}
            };
            for (int i = 0; i < rows.Length; i++)
            {
                String message = "Failed assertion for " + rows[i][0];
                EventPropertyDescriptor prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                Assert.AreEqual(rows[i][1], prop.PropertyType, message);
            }

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "T1"));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("val"));

            SupportBean sb1 = new SupportBean("T1", 10);
            _epService.EPRuntime.SendEvent(sb1);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "T1"));

            EventBean received = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(typeof(SupportBean[]), received.Get("val").GetType());
            EPAssertionUtil.AssertEqualsAnyOrder((Object[])received.Get("val"), new Object[] { sb1 });

            SupportBean sb2 = new SupportBean("T2", 20);
            _epService.EPRuntime.SendEvent(sb2);
            SupportBean sb3 = new SupportBean("T2", 30);
            _epService.EPRuntime.SendEvent(sb3);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "T2"));

            received = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertEqualsAnyOrder((Object[])received.Get("val"), new Object[] { sb2, sb3 });
        }
    }
}
