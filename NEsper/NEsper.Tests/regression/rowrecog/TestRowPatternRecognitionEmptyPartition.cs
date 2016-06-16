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
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionEmptyPartition 
    {
        [Test]
        public void TestEmptyPartition()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
    
            String[] fields = "value".Split(',');
            String text = "select * from MyEvent.win:length(10) " +
                    "match_recognize (" +
                    "  partition by value" +
                    "  measures E1.value as value" +
                    "  pattern (E1 E2 | E2 E1 ) " +
                    "  define " +
                    "    E1 as E1.TheString = 'A', " +
                    "    E2 as E2.TheString = 'B' " +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("A", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {1});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("B", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("A", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {2});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("B", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("A", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("A", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {3});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("B", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {4});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("A", 6));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B", 7));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B", 8));
            epService.EPRuntime.SendEvent(new SupportRecogBean("A", 7));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {7});
    
            // Comment-in for testing partition removal.
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportRecogBean("A", i));
                //epService.EPRuntime.SendEvent(new SupportRecogBean("B", i));
                //EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {i});
            }
        }
    }
}
