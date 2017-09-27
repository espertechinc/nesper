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
    public class TestJoinDerivedValueViews 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestJoinDerivedValue() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            _epService.EPAdministrator.CreateEPL("select\n" +
                    "Math.Sign(stream1.slope) as s1,\n" +
                    "Math.Sign(stream2.slope) as s2\n" +
                    "from\n" +
                    "SupportBean#length_batch(3)#linest(IntPrimitive, LongPrimitive) as stream1,\n" +
                    "SupportBean#length_batch(2)#linest(IntPrimitive, LongPrimitive) as stream2").Events += _listener.Update;
            _epService.EPRuntime.SendEvent(MakeEvent("E3", 1, 100));
            _epService.EPRuntime.SendEvent(MakeEvent("E4", 1, 100));
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        private SupportBean MakeEvent(String id, int intPrimitive, long longPrimitive) {
            SupportBean bean = new SupportBean(id, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    }
}
