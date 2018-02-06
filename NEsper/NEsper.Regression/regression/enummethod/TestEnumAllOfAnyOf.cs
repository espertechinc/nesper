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
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    [TestFixture]
    public class TestEnumAllOfAnyOf
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
        public void TestAllOfAnyOfEvents()
        {

            String[] fields = "val0,val1".Split(',');
            String eplFragment = "select " +
                    "contained.allof(x => p00 = 12) as val0," +
                    "contained.anyof(x => p00 = 12) as val1 " +
                    "from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(bool), typeof(bool) });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,12", "E2,2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false, true });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,12", "E2,12", "E2,12"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, true });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,0", "E2,0", "E2,0"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false, false });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
        }

        [Test]
        public void TestAllOfAnyOfScalar()
        {

            String[] fields = "val0,val1".Split(',');
            String eplFragment = "select " +
                    "Strvals.allof(x => x='E2') as val0," +
                    "Strvals.anyof(x => x='E2') as val1 " +
                    "from SupportCollection";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(bool), typeof(bool) });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false, true });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, true });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false, false });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true, false });
        }
    }
}
