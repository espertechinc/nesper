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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraContext : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            TryAssertionContext(epService, true);
            TryAssertionContext(epService, false);
        }
    
        private void TryAssertionContext(EPServiceProvider epService, bool namedWindow) {
            epService.EPAdministrator.CreateEPL("create context ContextOne start SupportBean_S0 end SupportBean_S1");
    
            string eplCreate = namedWindow ?
                    "context ContextOne create window MyInfra#keepall as (pkey0 string, pkey1 int, c0 long)" :
                    "context ContextOne create table MyInfra as (pkey0 string primary key, pkey1 int primary key, c0 long)";
            epService.EPAdministrator.CreateEPL(eplCreate);
    
            epService.EPAdministrator.CreateEPL("context ContextOne insert into MyInfra select TheString as pkey0, IntPrimitive as pkey1, LongPrimitive as c0 from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));  // start
    
            MakeSendSupportBean(epService, "E1", 10, 100);
            MakeSendSupportBean(epService, "E2", 20, 200);
    
            SupportUpdateListener listenerUnAggUngr = Register(epService, "context ContextOne select * from MyInfra output snapshot when terminated");
            SupportUpdateListener listenerFullyAggUngr = Register(epService, "context ContextOne select count(*) as thecnt from MyInfra output snapshot when terminated");
            SupportUpdateListener listenerAggUngr = Register(epService, "context ContextOne select pkey0, count(*) as thecnt from MyInfra output snapshot when terminated");
            SupportUpdateListener listenerFullyAggGroup = Register(epService, "context ContextOne select pkey0, count(*) as thecnt from MyInfra group by pkey0 output snapshot when terminated");
            SupportUpdateListener listenerAggGroup = Register(epService, "context ContextOne select pkey0, pkey1, count(*) as thecnt from MyInfra group by pkey0 output snapshot when terminated");
            SupportUpdateListener listenerAggGroupRollup = Register(epService, "context ContextOne select pkey0, pkey1, count(*) as thecnt from MyInfra group by rollup (pkey0, pkey1) output snapshot when terminated");
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(0));  // end
    
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerUnAggUngr.GetAndResetLastNewData(), "pkey0,pkey1,c0".Split(','), new object[][]{new object[] {"E1", 10, 100L}, new object[] {"E2", 20, 200L}});
            EPAssertionUtil.AssertProps(listenerFullyAggUngr.AssertOneGetNewAndReset(), "thecnt".Split(','), new object[]{2L});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerAggUngr.GetAndResetLastNewData(), "pkey0,thecnt".Split(','), new object[][]{new object[] {"E1", 2L}, new object[] {"E2", 2L}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerFullyAggGroup.GetAndResetLastNewData(), "pkey0,thecnt".Split(','), new object[][]{new object[] {"E1", 1L}, new object[] {"E2", 1L}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerAggGroup.GetAndResetLastNewData(), "pkey0,pkey1,thecnt".Split(','), new object[][]{new object[] {"E1", 10, 1L}, new object[] {"E2", 20, 1L}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerAggGroupRollup.GetAndResetLastNewData(), "pkey0,pkey1,thecnt".Split(','), new object[][]{
                    new object[] {"E1", 10, 1L}, new object[]{"E2", 20, 1L}, new object[]{"E1", null, 1L}, new object[]{"E2", null, 1L}, new object[]{null, null, 2L}});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private SupportUpdateListener Register(EPServiceProvider epService, string epl) {
            var listener = new SupportUpdateListener();
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
            return listener;
        }
    
        private void MakeSendSupportBean(EPServiceProvider epService, string theString, int intPrimitive, long longPrimitive) {
            var b = new SupportBean(theString, intPrimitive);
            b.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(b);
        }
    
    }
} // end of namespace
