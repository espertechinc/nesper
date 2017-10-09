///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraOnSelectWDelete : IndexBackingTableInfo
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Logging.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestWindowAgg()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
    
            RunAssertionWindowAgg(true);
            RunAssertionWindowAgg(false);
        }
    
        private void RunAssertionWindowAgg(bool namedWindow)
        {
            string[] fieldsWin = "TheString,IntPrimitive".Split(',');
            string[] fieldsSelect = "c0".Split(',');
            
            string eplCreate = namedWindow ?
                    "create window MyInfra#keepall as SupportBean" :
                    "create table MyInfra (TheString string primary key, IntPrimitive int primary key)";
            EPStatement stmtWin = _epService.EPAdministrator.CreateEPL(eplCreate);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra select TheString, IntPrimitive from SupportBean");
            string eplSelectDelete = "on S0 as s0 " +
                    "select and delete window(win.*).aggregate(0,(result,value) => result+value.IntPrimitive) as c0 " +
                    "from MyInfra as win where s0.p00=win.TheString";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(eplSelectDelete);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fieldsWin, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
            }
            else {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtWin.GetEnumerator(), fieldsWin, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
            }
    
            // select and delete bean E1
            _epService.EPRuntime.SendEvent(new SupportBean_S0(100, "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{1});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtWin.GetEnumerator(), fieldsWin, new object[][] { new object[] { "E2", 2 } });
    
            // add some E2 events
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 3));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtWin.GetEnumerator(), fieldsWin, new object[][] { new object[] { "E2", 2 }, new object[] { "E2", 3 }, new object[] { "E2", 4 } });
    
            // select and delete beans E2
            _epService.EPRuntime.SendEvent(new SupportBean_S0(101, "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{2 + 3 + 4});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtWin.GetEnumerator(), fieldsWin, new object[0][]);
    
            // test SODA
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(eplSelectDelete);
            Assert.AreEqual(eplSelectDelete, model.ToEPL());
            EPStatement stmtSD = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(eplSelectDelete, stmtSD.Text);
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    }
}
