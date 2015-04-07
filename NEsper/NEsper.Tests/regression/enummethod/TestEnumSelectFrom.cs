///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.enummethod
{
    using DataMap = IDictionary<string, object>;

    [TestFixture]
    public class TestEnumSelectFrom  {
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
    
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
        public void TestNew() {
    
            String eplFragment = "select " +
                    "contained.selectFrom(x => new {c0 = id||'x', c1 = key0||'y'}) as val0 " +
                    "from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val0".Split(','), new Type[] { typeof(ICollection<object>) });
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("E1,12,0", "E2,11,0", "E3,2,0"));
            EPAssertionUtil.AssertPropsPerRow(
                ToMapArray(_listener.AssertOneGetNewAndReset().Get("val0")), "c0,c1".Split(','),
                new Object[][] { new Object[] { "E1x", "12y" }, new Object[] { "E2x", "11y" }, new Object[] { "E3x", "2y" } });
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value("E4,0,1"));
            EPAssertionUtil.AssertPropsPerRow(
                ToMapArray(_listener.AssertOneGetNewAndReset().Get("val0")), "c0,c1".Split(','),
                new Object[][] { new Object[] {"E4x","0y"}});
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value(null));
            EPAssertionUtil.AssertPropsPerRow(
                ToMapArray(_listener.AssertOneGetNewAndReset().Get("val0")), "c0,c1".Split(','), null);
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make3Value());
            EPAssertionUtil.AssertPropsPerRow(
                ToMapArray(_listener.AssertOneGetNewAndReset().Get("val0")), "c0,c1".Split(','),
                new Object[0][]);
        }
    
        private DataMap[] ToMapArray(Object result)
        {
            return result.UnwrapIntoArray<DataMap>();
        }
    
        [Test]
        public void TestSelect() {
    
            String eplFragment = "select " +
                    "contained.selectFrom(x => id) as val0 " +
                    "from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val0".Split(','), new Type[]{typeof(ICollection<object>)});
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E3,2"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E1", "E2", "E3");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", null);
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", new String[0]);
            _listener.Reset();
            stmtFragment.Dispose();

            // test scalar-coll with lambda
            String[] fields = "val0".SplitCsv();
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(TestEnumMinMax.MyService).FullName, "ExtractNum");
            String eplLambda = "select " +
                    "Strvals.selectFrom(v => extractNum(v)) as val0 " +
                    "from SupportCollection";
            EPStatement stmtLambda = _epService.EPAdministrator.CreateEPL(eplLambda);
            stmtLambda.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtLambda.EventType, fields, new [] { typeof(ICollection<object>), typeof(ICollection<object>) });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E5,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", 2, 1, 5, 4);
            _listener.Reset();

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", 1);
            _listener.Reset();

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", null);
            _listener.Reset();

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0");
        }    

        private void TryInvalid(String epl, String message) {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    }
}
