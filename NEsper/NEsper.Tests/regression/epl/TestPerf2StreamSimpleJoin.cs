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
using com.espertech.esper.compat;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerf2StreamSimpleJoin
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            _listener = null;
            _epService.Initialize();
        }

        [Test]
        public void TestPerformanceCoercionForward()
        {
            String stmt = "select A.LongBoxed as value from " +
                          typeof(SupportBean).FullName + "(TheString='A').win:length(1000000) as A," +
                          typeof(SupportBean).FullName + "(TheString='B').win:length(1000000) as B" +
                          " where A.LongBoxed=B.IntPrimitive";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmt);
            statement.Events += _listener.Update;

            // preload
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(MakeSupportEvent("A", 0, i));
            }

            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 5000; i++)
            {
                int index = 5000 + i % 1000;
                _epService.EPRuntime.SendEvent(MakeSupportEvent("B", index, 0));
                Assert.AreEqual((long)index, _listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;

            statement.Dispose();
            Assert.Less(delta, 1500, "Failed perf test, delta=" + delta);
        }

        [Test]
        public void TestPerformanceCoercionBack()
        {
            String stmt = "select A.IntPrimitive as value from " +
                    typeof(SupportBean).FullName + "(TheString='A').win:length(1000000) as A," +
                    typeof(SupportBean).FullName + "(TheString='B').win:length(1000000) as B" +
                " where A.IntPrimitive=B.LongBoxed";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmt);
            statement.Events += _listener.Update;

            // preload
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(MakeSupportEvent("A", i, 0));
            }

            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 5000; i++)
            {
                int index = 5000 + i % 1000;
                _epService.EPRuntime.SendEvent(MakeSupportEvent("B", 0, index));
                Assert.AreEqual(index, _listener.AssertOneGetNewAndReset().Get("value"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;

            statement.Dispose();
            Assert.Less(delta, 1500, "Failed perf test, delta=" + delta);
        }

        private static Object MakeSupportEvent(String theString, int intPrimitive, long longBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.LongBoxed = longBoxed;
            return bean;
        }
    }
}
