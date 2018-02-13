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
    public class ExecNWTableInfraOnUpdate : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
    
            RunAssertionUpdateOrderOfFields(epService, true);
            RunAssertionUpdateOrderOfFields(epService, false);
    
            RunAssertionSubquerySelf(epService, true);
            RunAssertionSubquerySelf(epService, false);
        }
    
        private void RunAssertionUpdateOrderOfFields(EPServiceProvider epService, bool namedWindow) {
    
            string eplCreate = namedWindow ?
                    "create window MyInfra#keepall as SupportBean" :
                    "create table MyInfra(theString string primary key, intPrimitive int, intBoxed int, doublePrimitive double)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfra select theString, intPrimitive, intBoxed, doublePrimitive from SupportBean");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("on SupportBean_S0 as sb " +
                    "update MyInfra as mywin" +
                    " set intPrimitive=id, intBoxed=mywin.intPrimitive, doublePrimitive=initial.intPrimitive" +
                    " where mywin.theString = sb.p00");
            var listenerWindow = new SupportUpdateListener();
            stmt.AddListener(listenerWindow);
            string[] fields = "intPrimitive,intBoxed,doublePrimitive".Split(',');
    
            epService.EPRuntime.SendEvent(MakeSupportBean("E1", 1, 2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "E1"));
            EPAssertionUtil.AssertProps(listenerWindow.GetAndResetLastNewData()[0], fields, new Object[]{5, 5, 1.0});
    
            epService.EPRuntime.SendEvent(MakeSupportBean("E2", 10, 20));
            epService.EPRuntime.SendEvent(new SupportBean_S0(6, "E2"));
            EPAssertionUtil.AssertProps(listenerWindow.GetAndResetLastNewData()[0], fields, new Object[]{6, 6, 10.0});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(7, "E1"));
            EPAssertionUtil.AssertProps(listenerWindow.GetAndResetLastNewData()[0], fields, new Object[]{7, 7, 5.0});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionSubquerySelf(EPServiceProvider epService, bool namedWindow) {
            // ESPER-507
    
            string eplCreate = namedWindow ?
                    "create window MyInfraSS#keepall as SupportBean" :
                    "create table MyInfraSS(theString string primary key, intPrimitive int)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfraSS select theString, intPrimitive from SupportBean");
    
            // This is better done with "set intPrimitive = intPrimitive + 1"
            string epl = "@Name(\"Self Update\")\n" +
                    "on SupportBean_A c\n" +
                    "update MyInfraSS s\n" +
                    "set intPrimitive = (select intPrimitive from MyInfraSS t where t.theString = c.id) + 1\n" +
                    "where s.theString = c.id";
            epService.EPAdministrator.CreateEPL(epl);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 6));
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
    
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "theString,intPrimitive".Split(','), new Object[][]{new object[] {"E1", 3}, new object[] {"E2", 7}});
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraSS", false);
        }
    
        private SupportBean MakeSupportBean(string theString, int intPrimitive, double doublePrimitive) {
            var sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }
    }
} // end of namespace
