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
    public class TestEnumMostLeastFrequent
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
        public void TestMostLeastEvents()
        {

            String[] fields = "val0,val1".Split(',');
            String eplFragment = "select " +
                    "contained.MostFrequent(x => p00) as val0," +
                    "contained.LeastFrequent(x => p00) as val1 " +
                    "from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(int?), typeof(int?) });

            SupportBean_ST0_Container bean = SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2", "E3,12");
            _epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 12, 11 });

            bean = SupportBean_ST0_Container.Make2Value("E1,12");
            _epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 12, 12 });

            bean = SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2", "E3,12", "E1,12", "E2,11", "E3,11");
            _epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 12, 2 });

            bean = SupportBean_ST0_Container.Make2Value("E2,11", "E1,12", "E2,15", "E3,12", "E1,12", "E2,11", "E3,11");
            _epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 11, 15 });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null });
        }

        [Test]
        public void TestScalar()
        {

            String[] fields = "val0,val1".Split(',');
            String eplFragment = "select " +
                    "Strvals.MostFrequent() as val0, " +
                    "Strvals.LeastFrequent() as val1 " +
                    "from SupportCollection";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(string), typeof(string) });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E2,E1,E3,E3,E4,E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E3", "E4" });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", "E1" });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, null });

            stmtFragment.Dispose();

            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(TestEnumMinMax.MyService).FullName, "ExtractNum");
            String eplLambda = "select " +
                    "Strvals.mostFrequent(v => extractNum(v)) as val0, " +
                    "Strvals.leastFrequent(v => extractNum(v)) as val1 " +
                    "from SupportCollection";
            EPStatement stmtLambda = _epService.EPAdministrator.CreateEPL(eplLambda);
            stmtLambda.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtLambda.EventType, fields, new []{typeof(int?), typeof(int?)});

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E2,E1,E3,E3,E4,E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{3, 4});

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{1, 1});

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null, null});

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null, null});
        }
    }
}
