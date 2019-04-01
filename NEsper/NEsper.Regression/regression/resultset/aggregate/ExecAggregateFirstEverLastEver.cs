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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

namespace com.espertech.esper.regression.resultset.aggregate
{
    public class ExecAggregateFirstEverLastEver : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
        }
    
        public override void Run(EPServiceProvider epService) {
    
            RunAssertionFirstLastEver(epService, true);
            RunAssertionFirstLastEver(epService, false);
            RunAssertionOnDelete(epService);
    
            SupportMessageAssertUtil.TryInvalid(epService, "select countever(distinct IntPrimitive) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'countever(distinct IntPrimitive)': Aggregation function 'countever' does now allow distinct [");
        }
    
        private void RunAssertionOnDelete(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as select * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL("on SupportBean_A delete from MyWindow where TheString = id");
    
            string[] fields = "firsteverstring,lasteverstring,counteverall".Split(',');
            string epl = "select firstever(TheString) as firsteverstring, " +
                    "lastever(TheString) as lasteverstring," +
                    "countever(*) as counteverall from MyWindow";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E2", 2L});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E3", 3L});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E3", 3L});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E3", 3L});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E3", 3L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFirstLastEver(EPServiceProvider epService, bool soda) {
            string[] fields = "firsteverstring,firsteverint,lasteverstring,lasteverint,counteverstar,counteverexpr,counteverexprfilter".Split(',');
    
            string epl = "select " +
                    "firstever(TheString) as firsteverstring, " +
                    "lastever(TheString) as lasteverstring, " +
                    "firstever(IntPrimitive) as firsteverint, " +
                    "lastever(IntPrimitive) as lasteverint, " +
                    "countever(*) as counteverstar, " +
                    "countever(IntBoxed) as counteverexpr, " +
                    "countever(IntBoxed,BoolPrimitive) as counteverexprfilter " +
                    "from SupportBean#length(2)";
            EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            MakeSendBean(epService, "E1", 10, 100, true);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10, "E1", 10, 1L, 1L, 1L});
    
            MakeSendBean(epService, "E2", 11, null, true);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10, "E2", 11, 2L, 1L, 1L});
    
            MakeSendBean(epService, "E3", 12, 120, false);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10, "E3", 12, 3L, 2L, 1L});
    
            stmt.Dispose();
        }
    
        private void MakeSendBean(EPServiceProvider epService, string theString, int intPrimitive, int? intBoxed, bool boolPrimitive) {
            var sb = new SupportBean(theString, intPrimitive);
            sb.IntBoxed = intBoxed;
            sb.BoolPrimitive = boolPrimitive;
            epService.EPRuntime.SendEvent(sb);
        }
    }
} // end of namespace
