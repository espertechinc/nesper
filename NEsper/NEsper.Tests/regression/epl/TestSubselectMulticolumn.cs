///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    using Map = IDictionary<string,object>;

    [TestFixture]
    public class TestSubselectMulticolumn
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddEventType("S0", typeof(SupportBean_S0));
            config.AddEventType("S1", typeof(SupportBean_S1));
            config.AddEventType("MarketData", typeof(SupportMarketDataBean));
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
        public void TestInvalid()
        {
            String epl = "select (select TheString, sum(IntPrimitive) from SupportBean.std:lastevent() as sb) from S0";
            TryInvalid(epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subquery with multi-column select requires that either all or none of the selected columns are under aggregation, unless a group-by clause is also specified [select (select TheString, sum(IntPrimitive) from SupportBean.std:lastevent() as sb) from S0]");

            epl = "select (select TheString, TheString from SupportBean.std:lastevent() as sb) from S0";
            TryInvalid(epl, "Error starting statement: Column 1 in subquery does not have a unique column name assigned [select (select TheString, TheString from SupportBean.std:lastevent() as sb) from S0]");

            epl = "select * from S0(p00 = (select TheString, TheString from SupportBean.std:lastevent() as sb))";
            TryInvalid(epl, "Failed to validate subquery number 1 querying SupportBean: Subquery multi-column select is not allowed in this context. [select * from S0(p00 = (select TheString, TheString from SupportBean.std:lastevent() as sb))]");
    
            epl = "select Exists(select sb.* as v1, IntPrimitive*2 as v3 from SupportBean.std:lastevent() as sb) as subrow from S0 as s0";
            TryInvalid(epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subquery multi-column select does not allow wildcard or stream wildcard when selecting multiple columns. [select Exists(select sb.* as v1, IntPrimitive*2 as v3 from SupportBean.std:lastevent() as sb) as subrow from S0 as s0]");
    
            epl = "select (select sb.* as v1, IntPrimitive*2 as v3 from SupportBean.std:lastevent() as sb) as subrow from S0 as s0";
            TryInvalid(epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subquery multi-column select does not allow wildcard or stream wildcard when selecting multiple columns. [select (select sb.* as v1, IntPrimitive*2 as v3 from SupportBean.std:lastevent() as sb) as subrow from S0 as s0]");
    
            epl = "select (select *, IntPrimitive from SupportBean.std:lastevent() as sb) as subrow from S0 as s0";
            TryInvalid(epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subquery multi-column select does not allow wildcard or stream wildcard when selecting multiple columns. [select (select *, IntPrimitive from SupportBean.std:lastevent() as sb) as subrow from S0 as s0]");
    
            epl = "select * from S0(p00 in (select TheString, string from SupportBean.std:lastevent() as sb))";
            TryInvalid(epl, "Failed to validate subquery number 1 querying SupportBean: Subquery multi-column select is not allowed in this context. [select * from S0(p00 in (select TheString, string from SupportBean.std:lastevent() as sb))]");
        }
    
        private void TryInvalid(String epl, String message)
        {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        [Test]
        public void TestColumnsUncorrelated() {
            String stmtText = "select " +
                    "(select TheString as v1, IntPrimitive as v2 from SupportBean.std:lastevent()) as subrow " +
                    "from S0 as s0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            RunAssertion(stmt);
    
            stmt.Dispose();
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
            Assert.AreEqual(stmtText, stmt.Text);
    
            RunAssertion(stmt);
        }
    
        private void RunAssertion(EPStatement stmt) {
    
            FragmentEventType fragmentType = stmt.EventType.GetFragmentType("subrow");
            Assert.IsFalse(fragmentType.IsIndexed);
            Assert.IsFalse(fragmentType.IsNative);
            Object[][] rows = new Object[][]{
                    new Object[] {"v1", typeof(string)},
                    new Object[] {"v2", typeof(int?)},
            };
            for (int i = 0; i < rows.Length; i++) {
                String message = "Failed assertion for " + rows[i][0];
                EventPropertyDescriptor prop = fragmentType.FragmentType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                Assert.AreEqual(rows[i][1], prop.PropertyType, message);
            }
    
            String[] fields = "subrow.v1,subrow.v2".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, fields, new Object[]{null, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 10});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 20});
        }
    
        [Test]
        public void TestCorrelatedAggregation() {
            String stmtText = "select p00, " +
                    "(select " +
                    "  sum(IntPrimitive) as v1, " +
                    "  sum(IntPrimitive + 1) as v2, " +
                    "  Window(IntPrimitive) as v3, " +
                    "  Window(sb.*) as v4 " +
                    "  from SupportBean.win:keepall() sb " +
                    "  where TheString = s0.P00) as subrow " +
                    "from S0 as s0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            Object[][] rows = new Object[][]{
                    new Object[] {"p00", typeof(string), false},
                    new Object[] {"subrow", typeof(Map), true}
            };
            for (int i = 0; i < rows.Length; i++) {
                String message = "Failed assertion for " + rows[i][0];
                EventPropertyDescriptor prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                Assert.AreEqual(rows[i][1], prop.PropertyType, message);
                Assert.AreEqual(rows[i][2], prop.IsFragment, message);
            }
    
            FragmentEventType fragmentType = stmt.EventType.GetFragmentType("subrow");
            Assert.IsFalse(fragmentType.IsIndexed);
            Assert.IsFalse(fragmentType.IsNative);
            rows = new Object[][]{
                    new Object[] {"v1", typeof(int?)},
                    new Object[] {"v2", typeof(int?)},
                    new Object[] {"v3", typeof(int?[])},
                    new Object[] {"v4", typeof(SupportBean[])},
            };
            for (int i = 0; i < rows.Length; i++) {
                String message = "Failed assertion for " + rows[i][0];
                EventPropertyDescriptor prop = fragmentType.FragmentType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                Assert.AreEqual(rows[i][1], prop.PropertyType, message);
            }
    
            String[] fields = "p00,subrow.v1,subrow.v2".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "T1"));
            EventBean row = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(row, fields, new Object[]{"T1", null, null});
            Assert.IsNull(row.Get("subrow.v3"));
            Assert.IsNull(row.Get("subrow.v4"));
    
            SupportBean sb1 = new SupportBean("T1", 10);
            _epService.EPRuntime.SendEvent(sb1);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "T1"));
            row = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(row, fields, new Object[]{"T1", 10, 11});
            EPAssertionUtil.AssertEqualsAnyOrder((int?[])row.Get("subrow.v3"), new int?[] { 10 });
            EPAssertionUtil.AssertEqualsAnyOrder((Object[]) row.Get("subrow.v4"), new Object[]{sb1});
    
            SupportBean sb2 = new SupportBean("T1", 20);
            _epService.EPRuntime.SendEvent(sb2);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "T1"));
            row = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(row, fields, new Object[]{"T1", 30, 32});
            EPAssertionUtil.AssertEqualsAnyOrder((int?[])row.Get("subrow.v3"), new int?[] { 10, 20 });
            EPAssertionUtil.AssertEqualsAnyOrder((Object[]) row.Get("subrow.v4"), new Object[]{sb1, sb2});
        }
    }
}
