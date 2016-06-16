///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraOnUpdate 
    {
        private EPServiceProvider epService;
        private SupportUpdateListener listenerWindow;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
            listenerWindow = new SupportUpdateListener();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            listenerWindow = null;
        }
    
        [Test]
        public void TestUpdateOrderOfFields() {
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
            RunAssertionUpdateOrderOfFields(true);
            RunAssertionUpdateOrderOfFields(false);
        }
    
        [Test]
        public void TestSubquerySelf() {
            RunAssertionSubquerySelf(true);
            RunAssertionSubquerySelf(false);
        }
    
        private void RunAssertionUpdateOrderOfFields(bool namedWindow) {
    
            string eplCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as SupportBean" :
                    "create table MyInfra(TheString string primary key, IntPrimitive int, IntBoxed int, DoublePrimitive double)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfra select TheString, IntPrimitive, IntBoxed, DoublePrimitive from SupportBean");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("on SupportBean_S0 as sb " +
                    "update MyInfra as mywin" +
                    " set IntPrimitive=id, IntBoxed=mywin.IntPrimitive, DoublePrimitive=initial.IntPrimitive" +
                    " where mywin.TheString = sb.p00");
            stmt.AddListener(listenerWindow);
            string[] fields = "IntPrimitive,IntBoxed,DoublePrimitive".Split(',');
    
            epService.EPRuntime.SendEvent(MakeSupportBean("E1", 1, 2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "E1"));
            EPAssertionUtil.AssertProps(listenerWindow.GetAndResetLastNewData()[0], fields, new object[]{5, 5, 1.0});
    
            epService.EPRuntime.SendEvent(MakeSupportBean("E2", 10, 20));
            epService.EPRuntime.SendEvent(new SupportBean_S0(6, "E2"));
            EPAssertionUtil.AssertProps(listenerWindow.GetAndResetLastNewData()[0], fields, new object[]{6, 6, 10.0});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(7, "E1"));
            EPAssertionUtil.AssertProps(listenerWindow.GetAndResetLastNewData()[0], fields, new object[]{7, 7, 5.0});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionSubquerySelf(bool namedWindow) {
            // ESPER-507
    
            string eplCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as SupportBean" :
                    "create table MyInfra(TheString string primary key, IntPrimitive int)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfra select TheString, IntPrimitive from SupportBean");
    
            // This is better done with "set IntPrimitive = IntPrimitive + 1"
            string epl = "@Name(\"Self Update\")\n" +
                    "on SupportBean_A c\n" +
                    "update MyInfra s\n" +
                    "set IntPrimitive = (select IntPrimitive from MyInfra t where t.TheString = c.id) + 1\n" +
                    "where s.TheString = c.id";
            epService.EPAdministrator.CreateEPL(epl);
            
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 6));
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));

            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "TheString,IntPrimitive".Split(','), new object[][] { new object[] { "E1", 3 }, new object[] { "E2", 7 } });
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private SupportBean MakeSupportBean(string theString, int intPrimitive, double doublePrimitive) {
            SupportBean sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }
    }
}
