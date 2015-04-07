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

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestDotExpressionDuckTyping 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
    	public void TestDuckTyping()
    	{
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ExpressionConfig.IsDuckTyping = true;
    
    	    _epService = EPServiceProviderManager.GetDefaultProvider(config);
    	    _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _listener = new SupportUpdateListener();
    
            _epService.EPAdministrator.Configuration.AddEventType<SupportBeanDuckType>("SupportBeanDuckType");
    
            const string epl = "select " +
                               "(dt).MakeString() as strval, " +
                               "(dt).MakeInteger() as intval, " +
                               "(dt).MakeCommon().MakeString() as commonstrval, " +
                               "(dt).MakeCommon().MakeInteger() as commonintval, " +
                               "(dt).ReturnDouble() as commondoubleval " +
                               "from SupportBeanDuckType dt ";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            var rows = new Object[][] {
                    new Object[] {"strval", typeof(Object)},
                    new Object[] {"intval", typeof(Object)},
                    new Object[] {"commonstrval", typeof(Object)},
                    new Object[] {"commonintval", typeof(Object)},
                    new Object[] {"commondoubleval", typeof(double)}   // this one is strongly typed
                    };
            for (int i = 0; i < rows.Length; i++) {
                EventPropertyDescriptor prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName);
                Assert.AreEqual(rows[i][1], prop.PropertyType);
            }
    
            String[] fields = "strval,intval,commonstrval,commonintval,commondoubleval".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBeanDuckTypeOne("x"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"x", null, null, -1, 12.9876d});
    
            _epService.EPRuntime.SendEvent(new SupportBeanDuckTypeTwo(-10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, -10, "mytext", null, 11.1234d});
        }
    }
}
