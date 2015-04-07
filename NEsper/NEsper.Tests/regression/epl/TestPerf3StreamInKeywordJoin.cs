///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerf3StreamInKeywordJoin 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
    
            _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
            _epService.EPAdministrator.Configuration.AddEventType("S2", typeof(SupportBean_S2));
        }
    
        [TearDown]
        public void TearDown() {
            _listener = null;
        }
    
        [Test]
        public void TestInKeywordSingleIndexLookup()
        {
            string epl = "select s0.id as val from " +
                    "S0.win:keepall() s0, " +
                    "S1.win:keepall() s1, " +
                    "S2.win:keepall() s2 " +
                    "where p00 in (p10, p20)";
            string[] fields = "val".Split(',');
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            for (int i = 0; i < 10000; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean_S0(i, "P00_" + i));
            }
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "x"));
    
            long delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 1000; i++) {
                        _epService.EPRuntime.SendEvent(new SupportBean_S2(1, "P00_6541"));
                        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][]{new object[]{6541}});
                    }
                });

            Assert.That(delta, Is.LessThan(500));
            Log.Info("delta=" + delta);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
