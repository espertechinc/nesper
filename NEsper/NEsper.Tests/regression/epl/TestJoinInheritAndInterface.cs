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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestJoinInheritAndInterface 
    {
        private EPServiceProvider epService;
        private SupportUpdateListener testListener;
    
        [SetUp]
        public void SetUp()
        {
            epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
        }
        
        [TearDown]
        public void TearDown()
        {
            testListener = null;
        }
    
        [Test]
        public void TestInterfaceJoin()
        {
            String viewExpr = "select a, b from " +
                    typeof(ISupportA).FullName + ".win:length(10), " +
                    typeof(ISupportB).FullName + ".win:length(10)" +
                    " where a = b";
    
            EPStatement testView = epService.EPAdministrator.CreateEPL(viewExpr);
            testListener = new SupportUpdateListener();
            testView.Events += testListener.Update;
    
            epService.EPRuntime.SendEvent(new ISupportAImpl("1", "ab1"));
            epService.EPRuntime.SendEvent(new ISupportBImpl("2", "ab2"));
            Assert.IsFalse(testListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new ISupportBImpl("1", "ab3"));
            Assert.IsTrue(testListener.IsInvoked);
            EventBean theEvent = testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("1", theEvent.Get("a"));
            Assert.AreEqual("1", theEvent.Get("b"));
        }
    }
}
