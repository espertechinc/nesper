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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    [TestFixture]
    public class TestEnumCountOf
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("Bean", typeof(SupportBean_ST0_Container));
            config.AddEventType("SupportCollection", typeof(SupportCollection));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestCountOfEvents()
        {

            var fields = new[] { "val0", "val1" };
            const string eplFragment = "select " +
                                       "contained.countof(x=> x.P00 = 9) as val0, " +
                                       "contained.countof() as val1 " +
                                       " from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(int), typeof(int) });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E2,9"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] { 2, 3 });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] { null, null });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(new String[0]));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] { 0, 0 });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,9"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] { 1, 1 });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] { 0, 1 });
        }

        [Test]
        public void TestCountOfScalar()
        {
            var fields = new[] { "val0", "val1" };
            const string eplFragment = "select " +
                                       "Strvals.countof() as val0, " +
                                       "Strvals.countof(x => x = 'E1') as val1 " +
                                       " from SupportCollection";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(int), typeof(int) });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 2, 1 });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E1,E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 4, 2 });
        }
    }
}
