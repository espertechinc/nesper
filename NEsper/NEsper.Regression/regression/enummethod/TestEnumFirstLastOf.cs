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
    public class TestEnumFirstLastOf
    {
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
        public void TestFirstLastScalar() {
    
            String[] fields = "val0,val1,val2,val3".Split(',');
            String eplFragment = "select " +
                    "Strvals.firstOf() as val0, " +
                    "Strvals.lastOf() as val1, " +
                    "Strvals.firstOf(x => x like '%1%') as val2, " +
                    "Strvals.lastOf(x => x like '%1%') as val3 " +
                    " from SupportCollection";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[]{typeof(string), typeof(string), typeof(string), typeof(string)});
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", "E3", "E1", "E1"});
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", "E1", "E1", "E1"});
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E2,E3,E4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2", "E4", null, null});
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, null, null});
    
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, null, null});
        }
    
        [Test]
        public void TestFirstLastProperty() {
    
            String[] fields = "val0,val1".Split(',');
            String eplFragment = "select " +
                    "Contained.firstOf().P00 as val0, " +
                    "Contained.lastOf().P00 as val1 " +
                    " from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, fields, new Type[] { typeof(int?), typeof(int?) });
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E3,3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{1, 3});
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{1, 1});
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null, null});
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null, null});
        }
    
        [Test]
        public void TestFirstLastNoPred() {
    
            String eplFragment = "select " +
                    "Contained.firstOf() as val0, " +
                    "Contained.lastOf() as val1 " +
                    " from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val0,val1".Split(','), new Type[]{typeof(SupportBean_ST0), typeof(SupportBean_ST0)});
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E3,9", "E2,9"));
            AssertId(_listener, "val0", "E1");
            AssertId(_listener, "val1", "E2");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E2,2"));
            AssertId(_listener, "val0", "E2");
            AssertId(_listener, "val1", "E2");
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            Assert.IsNull(_listener.AssertOneGetNew().Get("val0"));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("val1"));
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            Assert.IsNull(_listener.AssertOneGetNew().Get("val0"));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("val1"));
        }
    
        [Test]
        public void TestFirstLastPredicate() {
    
            String eplFragment = "select contained.firstOf(x => p00 = 9) as val from Bean";
            EPStatement stmtFragment = _epService.EPAdministrator.CreateEPL(eplFragment);
            stmtFragment.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypes(stmtFragment.EventType, "val".Split(','), new Type[]{typeof(SupportBean_ST0)});
    
            SupportBean_ST0_Container bean = SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E2,9");
            _epService.EPRuntime.SendEvent(bean);
            SupportBean_ST0 result = (SupportBean_ST0) _listener.AssertOneGetNewAndReset().Get("val");
            Assert.AreSame(result, bean.Contained[1]);
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value(null));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("val"));
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value());
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("val"));
    
            _epService.EPRuntime.SendEvent(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E2,1"));
            Assert.IsNull(_listener.AssertOneGetNewAndReset().Get("val"));
        }
    
        private void AssertId(SupportUpdateListener listener, String property, String id) {
            SupportBean_ST0 result = (SupportBean_ST0) listener.AssertOneGetNew().Get(property);
            Assert.AreEqual(id, result.Id);
        }
    }
}
