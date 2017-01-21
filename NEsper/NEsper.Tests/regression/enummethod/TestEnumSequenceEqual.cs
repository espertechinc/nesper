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
    public class TestEnumSequenceEqual
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {

            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean_ST0_Container", typeof(SupportBean_ST0_Container));
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
        public void TestSelectFrom()
        {
            String[] fields = "val0".Split(',');
            String eplFragment = "select contained.SelectFrom(x => key0).sequenceEqual(contained.SelectFrom(y => id)) as val0 " +
                    "from SupportBean_ST0_Container";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val0".Split(','), new Type[] { typeof(bool) });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("I1,E1,0", "I2,E2,0"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("I3,I3,0", "X4,X4,0"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("I3,I3,0", "X4,Y4,0"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("I3,I3,0", "Y4,X4,0"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false });
        }

        [Test]
        public void TestTwoProperties()
        {

            String[] fields = "val0".Split(',');
            String eplFragment = "select " +
                    "Strvals.sequenceEqual(strvalstwo) as val0 " +
                    "from SupportCollection";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val0".Split(','), new Type[] { typeof(bool) });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3", "E1,E2,E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E3", "E1,E2,E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E3", "E1,E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3", "E1,E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,null,E3", "E1,E2,null,E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3", "E1,E2,null"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,null", "E1,E2,E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1", ""));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("", "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1", "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("", ""));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { true });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null, ""));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("", null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { false });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null, null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null });
        }

        [Test]
        public void TestInvalid()
        {
            String epl;

            epl = "select window(*).sequenceEqual(Strvals) from SupportCollection.std:lastevent()";
            TryInvalid(epl, "Error starting statement: Failed to validate select-clause expression 'window(*).sequenceEqual(Strvals)': Invalid input for built-in enumeration method 'sequenceEqual' and 1-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type 'SupportCollection' [select window(*).sequenceEqual(Strvals) from SupportCollection.std:lastevent()]");
        }

        private void TryInvalid(String epl, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

    }
}
