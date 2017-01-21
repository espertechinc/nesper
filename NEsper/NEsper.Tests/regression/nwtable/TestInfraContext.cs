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
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraContext 
    {
        private EPServiceProviderSPI _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            _epService = (EPServiceProviderSPI) EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
        
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestContext()
        {
            foreach (var clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)})
            {
                _epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionContext(true);
            RunAssertionContext(false);
        }
    
        private void RunAssertionContext(bool namedWindow)
        {
            _epService.EPAdministrator.CreateEPL("create context ContextOne start SupportBean_S0 end SupportBean_S1");
    
            var eplCreate = namedWindow ?
                    "context ContextOne create window MyInfra.win:keepall() as (pkey0 string, pkey1 int, c0 long)" :
                    "context ContextOne create table MyInfra as (pkey0 string primary key, pkey1 int primary key, c0 long)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
    
            _epService.EPAdministrator.CreateEPL("context ContextOne insert into MyInfra select TheString as pkey0, IntPrimitive as pkey1, LongPrimitive as c0 from SupportBean");
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));  // start
    
            MakeSendSupportBean("E1", 10, 100);
            MakeSendSupportBean("E2", 20, 200);
    
            var listenerUnAggUngr = Register("context ContextOne select * from MyInfra output snapshot when terminated");
            var listenerFullyAggUngr = Register("context ContextOne select count(*) as thecnt from MyInfra output snapshot when terminated");
            var listenerAggUngr = Register("context ContextOne select pkey0, count(*) as thecnt from MyInfra output snapshot when terminated");
            var listenerFullyAggGroup = Register("context ContextOne select pkey0, count(*) as thecnt from MyInfra group by pkey0 output snapshot when terminated");
            var listenerAggGroup = Register("context ContextOne select pkey0, pkey1, count(*) as thecnt from MyInfra group by pkey0 output snapshot when terminated");
            var listenerAggGroupRollup = Register("context ContextOne select pkey0, pkey1, count(*) as thecnt from MyInfra group by rollup (pkey0, pkey1) output snapshot when terminated");
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0));  // end

            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerUnAggUngr.GetAndResetLastNewData(), "pkey0,pkey1,c0".Split(','), new object[][] { new object[] { "E1", 10, 100L }, new object[] { "E2", 20, 200L } });
            EPAssertionUtil.AssertProps(listenerFullyAggUngr.AssertOneGetNewAndReset(), "thecnt".Split(','), new object[]{2L});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerAggUngr.GetAndResetLastNewData(), "pkey0,thecnt".Split(','), new object[][] { new object[] { "E1", 2L }, new object[] { "E2", 2L } });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerFullyAggGroup.GetAndResetLastNewData(), "pkey0,thecnt".Split(','), new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 1L } });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerAggGroup.GetAndResetLastNewData(), "pkey0,pkey1,thecnt".Split(','), new object[][] { new object[] { "E1", 10, 1L }, new object[] { "E2", 20, 1L } });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerAggGroupRollup.GetAndResetLastNewData(), "pkey0,pkey1,thecnt".Split(','), new object[][]{
                    new object[]{"E1", 10, 1L}, new object[]{"E2", 20, 1L}, new object[]{"E1", null, 1L}, new object[]{"E2", null, 1L}, new object[]{null, null, 2L}});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private SupportUpdateListener Register(string epl) {
            var listener = new SupportUpdateListener();
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(listener);
            return listener;
        }
    
        private void MakeSendSupportBean(string theString, int intPrimitive, long longPrimitive) {
            var b = new SupportBean(theString, intPrimitive);
            b.LongPrimitive = longPrimitive;
            _epService.EPRuntime.SendEvent(b);
        }
    
    }
}
