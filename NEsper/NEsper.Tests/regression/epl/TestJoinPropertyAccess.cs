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
    public class TestJoinPropertyAccess 
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
        public void TestRegularJoin()
        {
            SupportBeanCombinedProps combined = SupportBeanCombinedProps.MakeDefaultBean();
            SupportBeanComplexProps complex = SupportBeanComplexProps.MakeDefaultBean();
            Assert.AreEqual("0ma0", combined.GetIndexed(0).GetMapped("0ma").Value);
    
            String viewExpr = "select nested.nested, s1.Indexed[0], nested.Indexed[1] from " +
                    typeof(SupportBeanComplexProps).FullName + ".win:length(3) nested, " +
                    typeof(SupportBeanCombinedProps).FullName + ".win:length(3) s1" +
                    " where Mapped('keyOne') = Indexed[2].Mapped('2ma').value and" +
                    " Indexed[0].Mapped('0ma').value = '0ma0'";
    
            EPStatement testView = epService.EPAdministrator.CreateEPL(viewExpr);
            testListener = new SupportUpdateListener();
            testView.Events += testListener.Update;
    
            epService.EPRuntime.SendEvent(combined);
            epService.EPRuntime.SendEvent(complex);
    
            EventBean theEvent = testListener.GetAndResetLastNewData()[0];
            Assert.AreSame(complex.Nested, theEvent.Get("nested.nested"));
            Assert.AreSame(combined.GetIndexed(0), theEvent.Get("s1.Indexed[0]"));
            Assert.AreEqual(complex.GetIndexed(1), theEvent.Get("nested.Indexed[1]"));
        }
    
        [Test]
        public void TestOuterJoin()
        {
            String viewExpr = "select * from " +
                    typeof(SupportBeanComplexProps).FullName + ".win:length(3) s0" +
                    " left outer join " +
                    typeof(SupportBeanCombinedProps).FullName + ".win:length(3) s1" +
                    " on Mapped('keyOne') = Indexed[2].Mapped('2ma').value";
    
            EPStatement testView = epService.EPAdministrator.CreateEPL(viewExpr);
            testListener = new SupportUpdateListener();
            testView.Events += testListener.Update;
    
            SupportBeanCombinedProps combined = SupportBeanCombinedProps.MakeDefaultBean();
            epService.EPRuntime.SendEvent(combined);
            SupportBeanComplexProps complex = SupportBeanComplexProps.MakeDefaultBean();
            epService.EPRuntime.SendEvent(complex);
    
            // double check that outer join criteria match
            Assert.AreEqual(complex.GetMapped("keyOne"), combined.GetIndexed(2).GetMapped("2ma").Value);
    
            EventBean theEvent = testListener.GetAndResetLastNewData()[0];
            Assert.AreEqual("Simple", theEvent.Get("s0.SimpleProperty"));
            Assert.AreSame(complex, theEvent.Get("s0"));
            Assert.AreSame(combined, theEvent.Get("s1"));
        }
    }
}
