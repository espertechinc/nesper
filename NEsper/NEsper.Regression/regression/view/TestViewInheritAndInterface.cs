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


namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewInheritAndInterface 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }
    
        [Test]
        public void TestOverridingSubclass()
        {
            String viewExpr = "select val as value from " +
                    typeof(SupportOverrideOne).FullName + "#length(10)";
    
            EPStatement testView = _epService.EPAdministrator.CreateEPL(viewExpr);
            _testListener = new SupportUpdateListener();
            testView.Events += _testListener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportOverrideOneA("valA", "valOne", "valBase"));
            EventBean theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("valA", theEvent.Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportOverrideBase("x"));
            Assert.IsFalse(_testListener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportOverrideOneB("valB", "valTwo", "valBase2"));
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("valB", theEvent.Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportOverrideOne("valThree", "valBase3"));
            theEvent = _testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("valThree", theEvent.Get("value"));
        }
    
        [Test]
        public void TestImplementationClass()
        {
            String[] viewExpr = {
                "select baseAB from " + typeof(ISupportBaseAB).FullName + "#length(10)",
                "select baseAB, a from " + typeof(ISupportA).FullName + "#length(10)",
                "select baseAB, b from " + typeof(ISupportB).FullName + "#length(10)",
                "select c from " + typeof(ISupportC).FullName + "#length(10)",
                "select baseAB, a, g from " + typeof(ISupportAImplSuperG).FullName + "#length(10)",
                "select baseAB, a, b, g, c from " + typeof(ISupportAImplSuperGImplPlus).FullName + "#length(10)",
            };
    
            String[][] expected = {
                                 new [] {"baseAB"},
                                 new [] {"baseAB", "a"},
                                 new [] {"baseAB", "b"},
                                 new [] {"c"},
                                 new [] {"baseAB", "a", "g"},
                                 new [] {"baseAB", "a", "b", "g", "c"}
                                };
    
            EPStatement[] testViews = new EPStatement[viewExpr.Length];
            SupportUpdateListener[] listeners = new SupportUpdateListener[viewExpr.Length];
            for (int i = 0; i < viewExpr.Length; i++)
            {
                testViews[i] = _epService.EPAdministrator.CreateEPL(viewExpr[i]);
                listeners[i] = new SupportUpdateListener();
                testViews[i].Events += (listeners[i]).Update;
            }
    
            _epService.EPRuntime.SendEvent(new ISupportAImplSuperGImplPlus("g", "a", "baseAB", "b", "c"));
            for (int i = 0; i < listeners.Length; i++)
            {
                Assert.IsTrue(listeners[i].IsInvoked);
                EventBean theEvent = listeners[i].GetAndResetLastNewData()[0];
    
                for (int j = 0; j < expected[i].Length; j++)
                {
                    Assert.IsTrue(theEvent.EventType.IsProperty(expected[i][j]),
                                  "failed property valid check for stmt=" + viewExpr[i]);
                    Assert.AreEqual(expected[i][j], theEvent.Get(expected[i][j]),
                                    "failed property check for stmt=" + viewExpr[i]);
                }
            }
        }
    }
}
