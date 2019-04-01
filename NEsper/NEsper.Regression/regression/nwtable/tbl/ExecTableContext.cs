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
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableContext : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
    
            RunAssertionPartitioned(epService);
            RunAssertionNonOverlapping(epService);
            RunInvalidAssertion(epService);
        }
    
        private void RunInvalidAssertion(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context SimpleCtx start after 1 sec end after 1 sec");
            epService.EPAdministrator.CreateEPL("context SimpleCtx create table MyTable(pkey string primary key, thesum sum(int), col0 string)");
    
            SupportMessageAssertUtil.TryInvalid(epService, "select * from MyTable",
                    "Error starting statement: Table by name 'MyTable' has been declared for context 'SimpleCtx' and can only be used within the same context [");
            SupportMessageAssertUtil.TryInvalid(epService, "select (select * from MyTable) from SupportBean",
                    "Error starting statement: Failed to plan subquery number 1 querying MyTable: Mismatch in context specification, the context for the table 'MyTable' is 'SimpleCtx' and the query specifies no context  [select (select * from MyTable) from SupportBean]");
            SupportMessageAssertUtil.TryInvalid(epService, "insert into MyTable select TheString as pkey from SupportBean",
                    "Error starting statement: Table by name 'MyTable' has been declared for context 'SimpleCtx' and can only be used within the same context [");
        }
    
        private void RunAssertionNonOverlapping(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context CtxNowTillS0 start @now end SupportBean_S0");
            epService.EPAdministrator.CreateEPL("context CtxNowTillS0 create table MyTable(pkey string primary key, thesum sum(int), col0 string)");
            epService.EPAdministrator.CreateEPL("context CtxNowTillS0 into table MyTable select sum(IntPrimitive) as thesum from SupportBean group by TheString");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("context CtxNowTillS0 select pkey as c0, thesum as c1 from MyTable output snapshot when terminated").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 60));
            epService.EPRuntime.SendEvent(new SupportBean_S0(-1)); // terminated
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), "c0,c1".Split(','),
                    new object[][]{new object[] {"E1", 110}, new object[] {"E2", 20}});
    
            epService.EPAdministrator.CreateEPL("context CtxNowTillS0 create index MyIdx on MyTable(col0)");
            epService.EPAdministrator.CreateEPL("context CtxNowTillS0 select * from MyTable, SupportBean_S1 where col0 = p11");
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 90));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 30));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(-1)); // terminated
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), "c0,c1".Split(','),
                    new object[][]{new object[] {"E1", 30}, new object[] {"E3", 100}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPartitioned(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context CtxPerString " +
                    "partition by TheString from SupportBean, p00 from SupportBean_S0");
            epService.EPAdministrator.CreateEPL("context CtxPerString create table MyTable(thesum sum(int))");
            epService.EPAdministrator.CreateEPL("context CtxPerString into table MyTable select sum(IntPrimitive) as thesum from SupportBean");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("context CtxPerString select MyTable.thesum as c0 from SupportBean_S0").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 60));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            Assert.AreEqual(110, listener.AssertOneGetNewAndReset().Get("c0"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E2"));
            Assert.AreEqual(20, listener.AssertOneGetNewAndReset().Get("c0"));
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_MyTable__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_MyTable__public", false);
        }
    }
} // end of namespace
