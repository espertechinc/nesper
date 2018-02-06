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
    public class TestEnumAggregate  {
    
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
        public void TestAggregateEvents() {
    
            String[] fields = new String[] {"val0", "val1", "val2"};
            String eplFragment = "select " +
                    "contained.Aggregate(0, (result, item) => result + item.P00) as val0, " +
                    "contained.Aggregate('', (result, item) => result || ', ' || item.id) as val1, " +
                    "contained.Aggregate('', (result, item) => result || (case when result='' then '' else ',' end) || item.id) as val2 " +
                    " from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(int?), typeof(string), typeof(string)});
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] {25, ", E1, E2, E2", "E1,E2,E2"});
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] {null, null, null});
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(new String[0]));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] {0, "", ""});
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,12"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{12, ", E1", "E1"});
        }
    
        [Test]
        public void TestAggregateScalar() {
    
            String[] fields = "val0".Split(',');
            String eplFragment = "select " +
                    "Strvals.Aggregate('', (result, item) => result || '+' || item) as val0 " +
                    "from SupportCollection";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(string)});
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"+E1+E2+E3"});
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"+E1"});
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {""});
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null});
            stmtFragment.Dispose();
        }
    }
}
