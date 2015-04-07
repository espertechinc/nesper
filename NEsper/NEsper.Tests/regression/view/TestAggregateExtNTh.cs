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

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestAggregateExtNTh 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestNth()
        {
            String epl = "select " +
                    "TheString, " +
                    "nth(IntPrimitive,0) as int1, " +  // current
                    "nth(IntPrimitive,1) as int2 " +   // one before
                    "from SupportBean.win:keepall() group by TheString output last every 3 events order by TheString";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            RunAssertion();
    
            stmt.Dispose();
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
            stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
            Assert.AreEqual(epl, model.ToEPL());
    
            RunAssertion();
    
            TryInvalid("select Nth() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'nth(*)': The nth aggregation function requires two parameters, an expression returning aggregation values and a numeric index constant [select Nth() from SupportBean]");
        }
    
        private void RunAssertion()
        {
            String[] fields = "TheString,int1,int2".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 11));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 12));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"G1", 12, 10}, new Object[] {"G2", 11, null}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 30));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 25));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"G2", 25, 20}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", -1));
            _epService.EPRuntime.SendEvent(new SupportBean("G1", -2));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 8));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"G1", -2, -1}, new Object[] {"G2", 8, 25}});
        }
    
        private void TryInvalid(String epl, String message) {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    }
}
