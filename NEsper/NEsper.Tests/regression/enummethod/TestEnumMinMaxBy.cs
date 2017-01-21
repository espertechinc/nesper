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
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    [TestFixture]
    public class TestEnumMinMaxBy
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
        public void TestMinMaxBy()
        {
            String[] fields = "val0,val1,val2,val3".SplitCsv();
            String eplFragment = "select " +
                    "contained.MinBy(x => p00) as val0," +
                    "contained.MaxBy(x => p00) as val1," +
                    "contained.MinBy(x => p00).id as val2," +
                    "contained.MaxBy(x => p00).P00 as val3 " +
                    "from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(SupportBean_ST0), typeof(SupportBean_ST0), typeof(string), typeof(int?) });

            SupportBean_ST0_Container bean = SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2");
            _epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] { bean.Contained[2], bean.Contained[0], "E2", 12 });

            bean = SupportBean_ST0_Container.Make2Value("E1,12");
            _epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] { bean.Contained[0], bean.Contained[0], "E1", 12 });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] { null, null, null, null });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] { null, null, null, null });

            stmtFragment.Dispose();

            // test scalar-coll with lambda
            String[] fieldsLambda = "val0,val1".Split(',');
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(TestEnumMinMax.MyService).FullName, "ExtractNum");
            String eplLambda = "select " +
                    "Strvals.minBy(v => extractNum(v)) as val0, " +
                    "Strvals.maxBy(v => extractNum(v)) as val1 " +
                    "from SupportCollection";
            EPStatement stmtLambda = _epService.EPAdministrator.CreateEPL(eplLambda);
            stmtLambda.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtLambda.EventType, fieldsLambda, new []{ typeof(string), typeof(string) });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLambda, new Object[] {"E1", "E5"});

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLambda, new Object[] {"E1", "E1"});
            _listener.Reset();

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLambda, new Object[] {null, null});
            _listener.Reset();

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsLambda, new Object[] {null, null});
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
