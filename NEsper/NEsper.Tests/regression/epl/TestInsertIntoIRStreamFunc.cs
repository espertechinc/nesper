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
using com.espertech.esper.client.soda;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestInsertIntoIRStreamFunc 
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestInsertIRStream()
        {
            SupportUpdateListener listenerInsert = new SupportUpdateListener();
            SupportUpdateListener listenerSelect = new SupportUpdateListener();
    
            String[] fields = "c0,c1".Split(',');
            String stmtTextOne = "insert irstream into MyStream " +
                    "select irstream TheString as c0, Istream() as c1 " +
                    "from SupportBean.std:lastevent()";
            _epService.EPAdministrator.CreateEPL(stmtTextOne).Events += listenerInsert.Update;
    
            String stmtTextTwo = "select * from MyStream";
            _epService.EPAdministrator.CreateEPL(stmtTextTwo).Events += listenerSelect.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listenerInsert.AssertOneGetNewAndReset(), fields, new Object[]{"E1", true});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new Object[]{"E1", true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listenerInsert.AssertPairGetIRAndReset(), fields, new Object[]{"E2", true}, new Object[]{"E1", false});
            EPAssertionUtil.AssertPropsPerRow(listenerSelect.GetAndResetDataListsFlattened(), fields, new Object[][]{new Object[] {"E2", true}, new Object[] {"E1", false}}, new Object[0][]);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            EPAssertionUtil.AssertProps(listenerInsert.AssertPairGetIRAndReset(), fields, new Object[]{"E3", true}, new Object[]{"E2", false});
            EPAssertionUtil.AssertPropsPerRow(listenerSelect.GetAndResetDataListsFlattened(), fields, new Object[][]{new Object[] {"E3", true}, new Object[] {"E2", false}}, new Object[0][]);
    
            // test SODA
            String eplModel = "select istream() from SupportBean";
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(eplModel);
            Assert.AreEqual(eplModel, model.ToEPL());
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(eplModel, stmt.Text);
            Assert.AreEqual(typeof(bool), stmt.EventType.GetPropertyType("istream()"));
    
            // test join
            _epService.EPAdministrator.DestroyAllStatements();
            fields = "c0,c1,c2".Split(',');
            String stmtTextJoin = "select irstream TheString as c0, id as c1, Istream() as c2 " +
                    "from SupportBean.std:lastevent(), SupportBean_S0.std:lastevent()";
            _epService.EPAdministrator.CreateEPL(stmtTextJoin).Events += listenerSelect.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 10, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listenerSelect.LastOldData[0], fields, new Object[]{"E1", 10, false});
            EPAssertionUtil.AssertProps(listenerSelect.LastNewData[0], fields, new Object[]{"E2", 10, true});
        }
    }
}
