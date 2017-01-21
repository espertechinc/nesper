///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

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
    public class TestEnumReverse
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
            _listener = null;
        }

        [Test]
        public void TestReverseEvents()
        {
            String epl = "select contained.reverse() as val from Bean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val".Split(','), new Type[] { typeof(ICollection<object>) });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E3,1"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val", "E3,E2,E1");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E2,9", "E1,1"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val", "E1,E2");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val", "E1");
            _listener.Reset();

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            LambdaAssertionUtil.AssertST0Id(_listener, "val", null);
            _listener.Reset();

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            LambdaAssertionUtil.AssertST0Id(_listener, "val", "");
            _listener.Reset();
        }

        [Test]
        public void TestReverseScalar()
        {

            String[] fields = "val0".Split(',');
            String eplFragment = "select " +
                    "Strvals.reverse() as val0 " +
                    "from SupportCollection";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(ICollection<object>) });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E4", "E5", "E1", "E2");
            _listener.Reset();

            LambdaAssertionUtil.AssertSingleAndEmptySupportColl(_epService, _listener, fields);
        }
    }
}
