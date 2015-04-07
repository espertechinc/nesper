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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    [TestFixture]
    public class TestEnumTakeAndTakeLast
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
        public void TestTakeEvents() {
    
            String[] fields = "val0,val1,val2,val3,val4,val5".Split(',');
            String epl = "select " +
                    "contained.Take(2) as val0," +
                    "contained.Take(1) as val1," +
                    "contained.Take(0) as val2," +
                    "contained.Take(-1) as val3," +
                    "contained.TakeLast(2) as val4," +
                    "contained.TakeLast(1) as val5" +
                    " from Bean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]
                                                                        {
                                                                            typeof(ICollection<object>),
                                                                            typeof(ICollection<object>),
                                                                            typeof(ICollection<object>), 
                                                                            typeof(ICollection<object>), 
                                                                            typeof(ICollection<object>), 
                                                                            typeof(ICollection<object>)
                                                                        });
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,3"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val2", "");
            LambdaAssertionUtil.AssertST0Id(_listener, "val3", "");
            LambdaAssertionUtil.AssertST0Id(_listener, "val4", "E2,E3");
            LambdaAssertionUtil.AssertST0Id(_listener, "val5", "E3");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val2", "");
            LambdaAssertionUtil.AssertST0Id(_listener, "val3", "");
            LambdaAssertionUtil.AssertST0Id(_listener, "val4", "E1,E2");
            LambdaAssertionUtil.AssertST0Id(_listener, "val5", "E2");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val2", "");
            LambdaAssertionUtil.AssertST0Id(_listener, "val3", "");
            LambdaAssertionUtil.AssertST0Id(_listener, "val4", "E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val5", "E1");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            foreach (String field in fields) {
                LambdaAssertionUtil.AssertST0Id(_listener, field, "");
            }
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            foreach (String field in fields) {
                LambdaAssertionUtil.AssertST0Id(_listener, field, null);
            }
            _listener.Reset();
        }
    
        [Test]
        public void TestTakeScalar()
        {
            String[] fields = "val0,val1,val2,val3".Split(',');
            String epl = "select " +
                    "Strvals.Take(2) as val0," +
                    "Strvals.Take(1) as val1," +
                    "Strvals.TakeLast(2) as val2," +
                    "Strvals.TakeLast(1) as val3" +
                    " from SupportCollection";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, fields, new Type[]
                                                                        {
                                                                            typeof(ICollection<object>),
                                                                            typeof(ICollection<object>),
                                                                            typeof(ICollection<object>), 
                                                                            typeof(ICollection<object>)
                                                                        });
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E1","E2");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val1", "E1");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val2", "E2","E3");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val3", "E3");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E1","E2");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val1", "E1");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val2", "E1","E2");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val3", "E2");
            _listener.Reset();

            LambdaAssertionUtil.AssertSingleAndEmptySupportColl(_epService, _listener, fields);
        }
    }
}
