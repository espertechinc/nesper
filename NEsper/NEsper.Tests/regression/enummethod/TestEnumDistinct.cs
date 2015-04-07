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
    public class TestEnumDistinct
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
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().Name);}
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestDistinctEvents()
        {
            String[] fields = "val0".Split(',');
            String eplFragment = "select " +
                    "contained.DistinctOf(x => p00) as val0 " +
                    " from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(ICollection<object>)});
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,1"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E1,E2");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E3,1", "E2,2", "E4,1", "E1,2"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E3,E2");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            foreach (String field in fields) {
                LambdaAssertionUtil.AssertST0Id(_listener, field, null);
            }
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            foreach (String field in fields) {
                LambdaAssertionUtil.AssertST0Id(_listener, field, "");
            }
            _listener.Reset();
        }
    
        [Test]
        public void TestDistinctScalar() {
    
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("extractNum", typeof(TestEnumMinMax.MyService).FullName, "ExtractNum");
    
            String[] fields = "val0,val1".Split(',');
            String eplFragment = "select " +
                    "Strvals.distinctOf() as val0, " +
                    "Strvals.distinctOf(v => extractNum(v)) as val1 " +
                    "from SupportCollection";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(ICollection<object>), typeof(ICollection<object>)});
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E1,E2,E2"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E2", "E1");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val1", "E2", "E1");
            _listener.Reset();
    
            LambdaAssertionUtil.AssertSingleAndEmptySupportColl(_epService, _listener, fields);
            stmtFragment.Dispose();
        }
    }
}
