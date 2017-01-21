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
    public class TestEnumWhere  {
    
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
        public void TestWhereEvents() {
    
            String epl = "select " +
                    "contained.Where(x => p00 = 9) as val0," +
                    "contained.Where((x, i) => x.P00 = 9 and i >= 1) as val1 from Bean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmt.EventType, "val0,val1".Split(','), new Type[] { typeof(ICollection<object>), typeof(ICollection<object>) });
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E3,1"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E2");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E2");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,9", "E2,1", "E3,1"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E1");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E3,9"));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "E3");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "E3");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", null);
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", null);
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            LambdaAssertionUtil.AssertST0Id(_listener, "val0", "");
            LambdaAssertionUtil.AssertST0Id(_listener, "val1", "");
            _listener.Reset();
        }
    
        [Test]
        public void TestWhereString() {
    
            String[] fields = "val0,val1".Split(',');
            String eplFragment = "select " +
                    "Strvals.Where(x => x not like '%1%') as val0, " +
                    "Strvals.Where((x, i) => x not like '%1%' and i > 1) as val1 " +
                    "from SupportCollection";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(ICollection<object>), typeof(ICollection<object>) });
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E2", "E3");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val1", "E3");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E4,E2,E1"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", "E4", "E2");
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val1", new String[0]);
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", new String[0]);
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val1", new String[0]);
            _listener.Reset();
    
            stmtFragment.Dispose();
    
            // test boolean
            eplFragment = "select " +
                    "Boolvals.Where(x => x) as val0 " +
                    "from SupportCollection";
            stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val0".Split(','), new Type[] { typeof(ICollection<object>) });
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeBoolean("true,true,false"));
            LambdaAssertionUtil.AssertValuesArrayScalar(_listener, "val0", true, true);
            _listener.Reset();
        }
    }
}
