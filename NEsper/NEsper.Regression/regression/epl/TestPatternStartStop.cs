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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPatternStartStop 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _updateListener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _updateListener = null;
        }
    
        [Test]
        public void TestStartStop()
        {
            String stmtText = "select * from pattern [every(a=" + typeof(SupportBean).FullName +
                    " or b=" + typeof(SupportBeanComplexProps).FullName + ")]";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += _updateListener.Update;
    
            for (int i = 0; i < 100; i++)
            {
                SendAndAssert();
    
                statement.Stop();
    
                _epService.EPRuntime.SendEvent(new SupportBean());
                _epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
                Assert.IsFalse(_updateListener.IsInvoked);
    
                statement.Start();
            }
        }
    
        private void SendAndAssert()
        {
            for (int i = 0; i < 1000; i++)
            {
                Object theEvent = null;
                if (i % 3 == 0)
                {
                    theEvent = new SupportBean();
                }
                else
                {
                    theEvent = SupportBeanComplexProps.MakeDefaultBean();
                }
    
                _epService.EPRuntime.SendEvent(theEvent);
    
                EventBean eventBean = _updateListener.AssertOneGetNewAndReset();
                if (theEvent is SupportBean)
                {
                    Assert.AreSame(theEvent, eventBean.Get("a"));
                    Assert.IsNull(eventBean.Get("b"));
                }
                else
                {
                    Assert.AreSame(theEvent, eventBean.Get("b"));
                    Assert.IsNull(eventBean.Get("a"));
                }
            }
        }
    }
}
