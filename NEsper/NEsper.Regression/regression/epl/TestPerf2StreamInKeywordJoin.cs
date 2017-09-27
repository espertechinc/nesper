///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerf2StreamInKeywordJoin 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
    
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
        }
    
        [TearDown]
        public void TearDown() {
            _listener = null;
        }
    
        [Test]
        public void TestInKeywordSingleIndexLookup()
        {
            const string epl = "select IntPrimitive as val from SupportBean#keepall sb, SupportBean_S0 s0 unidirectional " +
                               "where sb.TheString in (s0.p00, s0.p01)";
            string[] fields = "val".Split(',');
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            for (int i = 0; i < 10000; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }

            long delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E645", "E8975"));
                        EPAssertionUtil.AssertPropsPerRow(
                            _listener.GetAndResetLastNewData(), fields, new object[][] { new object[] { 645 }, new object[] { 8975 } });
                    }
                });

            Assert.That(delta, Is.LessThan(500));
            Log.Info("delta=" + delta);
        }
    
        [Test]
        public void TestInKeywordMultiIndexLookup()
        {
            const string epl = "select id as val from SupportBean_S0#keepall s0, SupportBean sb unidirectional " +
                               "where sb.TheString in (s0.p00, s0.p01)";
            string[] fields = "val".Split(',');
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            for (int i = 0; i < 10000; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean_S0(i, "p00_" + i, "p01_" + i));
            }

            long delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        _epService.EPRuntime.SendEvent(new SupportBean("p01_645", 0));
                        EPAssertionUtil.AssertProps(
                            _listener.AssertOneGetNewAndReset(), fields, new object[]
                            {
                                645
                            });
                    }
                });

            Assert.That(delta, Is.LessThan(500));
            Log.Info("delta=" + delta);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
