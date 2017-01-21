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


namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewPropertyAccess 
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
        public void TestWhereAndSelect()
        {
            String viewExpr = "select Mapped('keyOne') as a," +
                                     "Indexed[1] as b, Nested.NestedNested.NestedNestedValue as c, MapProperty, " +
                                     "ArrayProperty[0] " +
                    "  from " + typeof(SupportBeanComplexProps).FullName + ".win:length(3) " +
                    " where Mapped('keyOne') = 'valueOne' and " +
                          " Indexed[1] = 2 and " +
                          " Nested.NestedNested.NestedNestedValue = 'NestedNestedValue'";
    
            EPStatement testView = epService.EPAdministrator.CreateEPL(viewExpr);
            testListener = new SupportUpdateListener();
            testView.Events += testListener.Update;
    
            SupportBeanComplexProps eventObject = SupportBeanComplexProps.MakeDefaultBean();
            epService.EPRuntime.SendEvent(eventObject);
            EventBean theEvent = testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual(eventObject.GetMapped("keyOne"), theEvent.Get("a"));
            Assert.AreEqual(eventObject.GetIndexed(1), theEvent.Get("b"));
            Assert.AreEqual(eventObject.Nested.NestedNested.NestedNestedValue, theEvent.Get("c"));
            Assert.AreEqual(eventObject.MapProperty, theEvent.Get("MapProperty"));
            Assert.AreEqual(eventObject.ArrayProperty[0], theEvent.Get("ArrayProperty[0]"));
    
            eventObject.SetIndexed(1, int.MinValue);
            Assert.IsFalse(testListener.IsInvoked);
            epService.EPRuntime.SendEvent(eventObject);
            Assert.IsFalse(testListener.IsInvoked);
    
            eventObject.SetIndexed(1, 2);
            epService.EPRuntime.SendEvent(eventObject);
            Assert.IsTrue(testListener.IsInvoked);
        }
    }
}
