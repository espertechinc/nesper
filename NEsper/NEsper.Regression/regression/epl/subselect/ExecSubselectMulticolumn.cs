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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    using Map = IDictionary<string, object>;

    public class ExecSubselectMulticolumn : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
            configuration.AddEventType("MarketData", typeof(SupportMarketDataBean));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInvalid(epService);
            RunAssertionColumnsUncorrelated(epService);
            RunAssertionCorrelatedAggregation(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
    
            string epl = "select (select TheString, sum(IntPrimitive) from SupportBean#lastevent as sb) from S0";
            TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subquery with multi-column select requires that either all or none of the selected columns are under aggregation, unless a group-by clause is also specified [select (select TheString, sum(IntPrimitive) from SupportBean#lastevent as sb) from S0]");
    
            epl = "select (select TheString, TheString from SupportBean#lastevent as sb) from S0";
            TryInvalid(epService, epl, "Error starting statement: Column 1 in subquery does not have a unique column name assigned [select (select TheString, TheString from SupportBean#lastevent as sb) from S0]");
    
            epl = "select * from S0(p00 = (select TheString, TheString from SupportBean#lastevent as sb))";
            TryInvalid(epService, epl, "Failed to validate subquery number 1 querying SupportBean: Subquery multi-column select is not allowed in this context. [select * from S0(p00 = (select TheString, TheString from SupportBean#lastevent as sb))]");
    
            epl = "select exists(select sb.* as v1, IntPrimitive*2 as v3 from SupportBean#lastevent as sb) as subrow from S0 as s0";
            TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subquery multi-column select does not allow wildcard or stream wildcard when selecting multiple columns. [select exists(select sb.* as v1, IntPrimitive*2 as v3 from SupportBean#lastevent as sb) as subrow from S0 as s0]");
    
            epl = "select (select sb.* as v1, IntPrimitive*2 as v3 from SupportBean#lastevent as sb) as subrow from S0 as s0";
            TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subquery multi-column select does not allow wildcard or stream wildcard when selecting multiple columns. [select (select sb.* as v1, IntPrimitive*2 as v3 from SupportBean#lastevent as sb) as subrow from S0 as s0]");
    
            epl = "select (select *, IntPrimitive from SupportBean#lastevent as sb) as subrow from S0 as s0";
            TryInvalid(epService, epl, "Error starting statement: Failed to plan subquery number 1 querying SupportBean: Subquery multi-column select does not allow wildcard or stream wildcard when selecting multiple columns. [select (select *, IntPrimitive from SupportBean#lastevent as sb) as subrow from S0 as s0]");
    
            epl = "select * from S0(p00 in (select TheString, TheString from SupportBean#lastevent as sb))";
            TryInvalid(epService, epl, "Failed to validate subquery number 1 querying SupportBean: Subquery multi-column select is not allowed in this context. [select * from S0(p00 in (select TheString, TheString from SupportBean#lastevent as sb))]");
        }
    
        private void RunAssertionColumnsUncorrelated(EPServiceProvider epService) {
            string stmtText = "select " +
                    "(select TheString as v1, IntPrimitive as v2 from SupportBean#lastevent) as subrow " +
                    "from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertion(epService, listener, stmt);
    
            stmt.Dispose();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
            Assert.AreEqual(stmtText, stmt.Text);
    
            TryAssertion(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void TryAssertion(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
    
            FragmentEventType fragmentType = stmt.EventType.GetFragmentType("subrow");
            Assert.IsFalse(fragmentType.IsIndexed);
            Assert.IsFalse(fragmentType.IsNative);
            var rows = new object[][]{
                    new object[] {"v1", typeof(string)},
                    new object[] {"v2", typeof(int)},
            };
            for (int i = 0; i < rows.Length; i++) {
                string message = "Failed assertion for " + rows[i][0];
                EventPropertyDescriptor prop = fragmentType.FragmentType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                Assert.AreEqual(rows[i][1], prop.PropertyType, message);
            }
    
            string[] fields = "subrow.v1,subrow.v2".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, fields, new object[]{null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 20});
        }
    
        private void RunAssertionCorrelatedAggregation(EPServiceProvider epService) {
            string stmtText = "select p00, " +
                    "(select " +
                    "  sum(IntPrimitive) as v1, " +
                    "  sum(IntPrimitive + 1) as v2, " +
                    "  window(IntPrimitive) as v3, " +
                    "  window(sb.*) as v4 " +
                    "  from SupportBean#keepall sb " +
                    "  where TheString = s0.p00) as subrow " +
                    "from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var rows = new object[][]{
                    new object[] {"p00", typeof(string), false},
                    new object[] {"subrow", typeof(Map), true}
            };
            for (int i = 0; i < rows.Length; i++) {
                string message = "Failed assertion for " + rows[i][0];
                EventPropertyDescriptor prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                Assert.AreEqual(rows[i][1], prop.PropertyType, message);
                Assert.AreEqual(rows[i][2], prop.IsFragment, message);
            }
    
            FragmentEventType fragmentType = stmt.EventType.GetFragmentType("subrow");
            Assert.IsFalse(fragmentType.IsIndexed);
            Assert.IsFalse(fragmentType.IsNative);
            rows = new object[][]{
                    new object[] {"v1", typeof(int)},
                    new object[] {"v2", typeof(int)},
                    new object[] {"v3", typeof(int[])},
                    new object[] {"v4", typeof(SupportBean[])},
            };
            for (int i = 0; i < rows.Length; i++) {
                string message = "Failed assertion for " + rows[i][0];
                EventPropertyDescriptor prop = fragmentType.FragmentType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                Assert.AreEqual(rows[i][1], prop.PropertyType, message);
            }
    
            string[] fields = "p00,subrow.v1,subrow.v2".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "T1"));
            EventBean row = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(row, fields, new object[]{"T1", null, null});
            Assert.IsNull(row.Get("subrow.v3"));
            Assert.IsNull(row.Get("subrow.v4"));
    
            var sb1 = new SupportBean("T1", 10);
            epService.EPRuntime.SendEvent(sb1);
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "T1"));
            row = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(row, fields, new object[]{"T1", 10, 11});
            EPAssertionUtil.AssertEqualsAnyOrder((int[]) row.Get("subrow.v3"), new int[]{10});
            EPAssertionUtil.AssertEqualsAnyOrder((object[]) row.Get("subrow.v4"), new object[]{sb1});
    
            var sb2 = new SupportBean("T1", 20);
            epService.EPRuntime.SendEvent(sb2);
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "T1"));
            row = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(row, fields, new object[]{"T1", 30, 32});
            EPAssertionUtil.AssertEqualsAnyOrder((int[]) row.Get("subrow.v3"), new int[]{10, 20});
            EPAssertionUtil.AssertEqualsAnyOrder((object[]) row.Get("subrow.v4"), new object[]{sb1, sb2});
    
            stmt.Dispose();
        }
    }
} // end of namespace
