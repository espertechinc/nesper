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
using com.espertech.esper.client.soda;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestSubselectAllAnySomeExpr 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
    
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("ArrayBean", typeof(SupportBeanArrayCollMap));
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestRelationalOpAll()
        {
            String[] fields = "g,ge,l,le".Split(',');
            String stmtText = "select " +
                "IntPrimitive > all (select IntPrimitive from SupportBean(TheString like \"S%\")#keepall) as g, " +
                "IntPrimitive >= all (select IntPrimitive from SupportBean(TheString like \"S%\")#keepall) as ge, " +
                "IntPrimitive < all (select IntPrimitive from SupportBean(TheString like \"S%\")#keepall) as l, " +
                "IntPrimitive <= all (select IntPrimitive from SupportBean(TheString like \"S%\")#keepall) as le " +
                "from SupportBean(TheString like \"E%\")";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("S1", 1));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true, false, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("S2", 2));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, false, false, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, false, true, true});
    
            try
            {
                _epService.EPAdministrator.CreateEPL("select intArr > all (select IntPrimitive from SupportBean#keepall) from ArrayBean");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression subquery number 1 querying SupportBean: Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords [select intArr > all (select IntPrimitive from SupportBean#keepall) from ArrayBean]", ex.Message);
            }
            
            // test OM
            stmt.Dispose();
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true, true, true});
        }
    
        [Test]
        public void TestRelationalOpNullOrNoRows()
        {
            String[] fields = "vall,vany".Split(',');
            String stmtText = "select " +
                "IntBoxed >= all (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as vall, " +
                "IntBoxed >= any (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as vany " +
                " from SupportBean(TheString like 'E%')";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // subs is empty
            // select  null >= all (select val from subs), null >= any (select val from subs)
            SendEvent("E1", null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, false});
    
            // select  1 >= all (select val from subs), 1 >= any (select val from subs)
            SendEvent("E2", 1, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, false});
    
            // subs is {null}
            SendEvent("S1", null, null);
    
            SendEvent("E3", null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null});
            SendEvent("E4", 1, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null});
    
            // subs is {null, 1}
            SendEvent("S2", null, 1d);
    
            SendEvent("E5", null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null});
            SendEvent("E6", 1, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, true});
    
            SendEvent("E7", 0, null);
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(theEvent, fields, new Object[] {false, false});
        }
    
        [Test]
        public void TestRelationalOpSome()
        {
            String[] fields = "g,ge,l,le".Split(',');
            String stmtText = "select " +
                "IntPrimitive > any (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as g, " +
                "IntPrimitive >= any (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as ge, " +
                "IntPrimitive < any (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as l, " +
                "IntPrimitive <= any (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as le " +
                " from SupportBean(TheString like 'E%')";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, false, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("S1", 1));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true, false, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2a", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, false, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("S2", 2));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true, false, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, false, true, true});
        }
    
        [Test]
        public void TestEqualsNotEqualsAll()
        {
            String[] fields = "eq,neq,sqlneq,nneq".Split(',');
            String stmtText = "select " +
                              "IntPrimitive = all (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as eq, " +
                              "IntPrimitive != all (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as neq, " +
                              "IntPrimitive <> all (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as sqlneq, " +
                              "not IntPrimitive = all (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as nneq " +
                              " from SupportBean(TheString like 'E%')";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true, true, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("S1", 11));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, false, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("S1", 12));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, false, false, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 14));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true, true, true});
        }
    
        // Test "value = SOME (subselect)" which is the same as "value IN (subselect)"
        [Test]
        public void TestEqualsAnyOrSome()
        {
            String[] fields = "r1,r2,r3,r4".Split(',');
            String stmtText = "select " +
                        "IntPrimitive = SOME (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as r1, " +
                        "IntPrimitive = ANY (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as r2, " +
                        "IntPrimitive != SOME (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as r3, " +
                        "IntPrimitive <> ANY (select IntPrimitive from SupportBean(TheString like 'S%')#keepall) as r4 " +
                        "from SupportBean(TheString like 'E%')";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, false, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("S1", 11));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true, false, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 12));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, false, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("S2", 12));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 12));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 13));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, false, true, true});
        }
    
        [Test]
        public void TestEqualsInNullOrNoRows()
        {
            String[] fields = "eall,eany,neall,neany,isin".Split(',');
            String stmtText = "select " +
                "IntBoxed = all (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as eall, " +
                "IntBoxed = any (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as eany, " +
                "IntBoxed != all (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as neall, " +
                "IntBoxed != any (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as neany, " +
                "IntBoxed in (select DoubleBoxed from SupportBean(TheString like 'S%')#keepall) as isin " +
                " from SupportBean(TheString like 'E%')";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // subs is empty
            // select  null = all (select val from subs), null = any (select val from subs), null != all (select val from subs), null != any (select val from subs), null in (select val from subs) 
            SendEvent("E1", null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, false, true, false, false});
    
            // select  1 = all (select val from subs), 1 = any (select val from subs), 1 != all (select val from subs), 1 != any (select val from subs), 1 in (select val from subs)
            SendEvent("E2", 1, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, false, true, false, false});
    
            // subs is {null}
            SendEvent("S1", null, null);
    
            SendEvent("E3", null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, null, null, null});
            SendEvent("E4", 1, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, null, null, null});
    
            // subs is {null, 1}
            SendEvent("S2", null, 1d);
    
            SendEvent("E5", null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, null, null, null});
            SendEvent("E6", 1, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, true, false, null, true});
            SendEvent("E7", 0, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, null,  null, true, null});
        }
    
        [Test]
        public void TestInvalid()
        {
            try
            {
                String stmtText = "select intArr = all (select IntPrimitive from SupportBean#keepall) as r1 from ArrayBean";
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression subquery number 1 querying SupportBean: Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords [select intArr = all (select IntPrimitive from SupportBean#keepall) as r1 from ArrayBean]", ex.Message);
            }
        }
    
        private void SendEvent(string stringValue, int? intBoxed, double? doubleBoxed)
        {
            SupportBean bean = new SupportBean(stringValue, -1);
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
