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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    using AnyMap = IDictionary<object, object>;

    [TestFixture]
    public class TestEnumToMap
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
        public void TestToMap()
        {
            // - duplicate value allowed, latest value wins
            // - null key & value allowed

            String eplFragment = "select contained.toMap(c => id, c=> p00) as val from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val".Split(','), new Type[] { typeof(AnyMap) });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E3,12", "E2,5"));
            ArrayAssertionUtil.AssertPropsMap(ToDataMap(_listener.AssertOneGetNewAndReset().Get("val")), "E1,E2,E3".Split(','), new Object[] { 1, 5, 12 });

            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E3,12", "E2,12", "E1,2"));
            ArrayAssertionUtil.AssertPropsMap(ToDataMap(_listener.AssertOneGetNewAndReset().Get("val")), "E1,E2,E3".Split(','), new Object[] { 2, 12, 12 });

            _epService.EPRuntime.SendEvent(new SupportBean_ST0_Container(new SupportBean_ST0[] { new SupportBean_ST0(null, null) }));
            ArrayAssertionUtil.AssertPropsMap(ToDataMap(_listener.AssertOneGetNewAndReset().Get("val")), "E1,E2,E3".Split(','), new Object[] { null, null, null });

            stmtFragment.Dispose();

            // test scalar-coll with lambda
            String[] fields = "val0".SplitCsv();
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(TestEnumMinMax.MyService).FullName, "ExtractNum");
            String eplLambda = "select Strvals.toMap(c => c, c => extractNum(c)) as val0 from SupportCollection";
            EPStatement stmtLambda = _epService.EPAdministrator.CreateEPL(eplLambda);
            stmtLambda.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtLambda.EventType, fields, new []{ typeof(AnyMap) });

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E3"));
            EPAssertionUtil.AssertPropsMap((AnyMap) _listener.AssertOneGetNewAndReset().Get("val0"), "E1,E2,E3".SplitCsv(), new Object[]{1, 2, 3});

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertPropsMap((AnyMap) _listener.AssertOneGetNewAndReset().Get("val0"), "E1".SplitCsv(), new Object[]{1});

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("val0"));

            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            Assert.AreEqual(0, ((AnyMap) _listener.AssertOneGetNewAndReset().Get("val0")).Count);
        }

        public static IDictionary<string, object> ToDataMap(object value)
        {
            IDictionary<object, object> rawMap = (IDictionary<object, object>)value;
            IDictionary<string, object> outMap = new HashMap<string, object>();
            foreach (var entry in rawMap)
            {
                outMap.Put((string)entry.Key, entry.Value);
            }

            return outMap;
        }
    }
}
