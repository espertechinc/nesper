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

namespace com.espertech.esper.regression.expr
{
    [TestFixture]
    public class TestMathExpr 
    {
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestIntDivisionIntResultZeroDevision()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Expression.IsIntegerDivision = true;
            config.EngineDefaults.Expression.IsDivisionByZeroReturnsNull = true;
            config.AddEventType<SupportBean>();
    
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            String viewExpr = "select IntPrimitive/IntBoxed as result from SupportBean";
            EPStatement selectTestView = epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;
            Assert.AreEqual(typeof(int?), selectTestView.EventType.GetPropertyType("result"));
    
            SendEvent(epService, 100, 3);
            Assert.AreEqual(33, _listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEvent(epService, 100, null);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEvent(epService, 100, 0);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("result"));
        }
    
        [Test]
        public void TestIntDivisionDoubleResultZeroDevision()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
    
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            String viewExpr = "select IntPrimitive/IntBoxed as result from SupportBean";
            EPStatement selectTestView = epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("result"));
    
            SendEvent(epService, 100, 3);
            Assert.AreEqual(100/3d, _listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEvent(epService, 100, null);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEvent(epService, 100, 0);
            Assert.AreEqual(Double.PositiveInfinity, _listener.AssertOneGetNewAndReset().Get("result"));
    
            SendEvent(epService, -5, 0);
            Assert.AreEqual(Double.NegativeInfinity, _listener.AssertOneGetNewAndReset().Get("result"));
        }
    
        private void SendEvent(EPServiceProvider epService, int intPrimitive, int? intBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
}
