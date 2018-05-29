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
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableNonAccessDotSubqueryAndJoin : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionUse(epService, false);
            RunAssertionUse(epService, true);
        }
    
        private void RunAssertionUse(EPServiceProvider epService, bool soda) {
            string eplCreate = "create table MyTable (" +
                    "col0 string, " +
                    "col1 sum(int), " +
                    "col2 sorted(IntPrimitive) @Type('SupportBean'), " +
                    "col3 int[], " +
                    "col4 window(*) @Type('SupportBean')" +
                    ")";
            SupportModelHelper.CreateByCompileOrParse(epService, soda, eplCreate);
    
            string eplIntoTable = "into table MyTable select sum(IntPrimitive) as col1, sorted() as col2, " +
                    "window(*) as col4 from SupportBean#length(3)";
            EPStatement stmtIntoTable = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplIntoTable);
            var sentSB = new SupportBean[2];
            sentSB[0] = MakeSendSupportBean(epService, "E1", 20);
            sentSB[1] = MakeSendSupportBean(epService, "E2", 21);
            stmtIntoTable.Dispose();
    
            string eplMerge = "on SupportBean merge MyTable when matched then update set col3={1,2,4,2}, col0=\"x\"";
            EPStatement stmtMerge = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplMerge);
            MakeSendSupportBean(epService, null, -1);
            stmtMerge.Dispose();
    
            string eplSelect = "select " +
                    "col0 as c0_1, mt.col0 as c0_2, " +
                    "col1 as c1_1, mt.col1 as c1_2, " +
                    "col2 as c2_1, mt.col2 as c2_2, " +
                    "col2.minBy() as c2_3, mt.col2.MaxBy() as c2_4, " +
                    "col2.sorted().firstOf() as c2_5, mt.col2.sorted().firstOf() as c2_6, " +
                    "col3.mostFrequent() as c3_1, mt.col3.mostFrequent() as c3_2, " +
                    "col4 as c4_1 " +
                    "from SupportBean unidirectional, MyTable as mt";
            EPStatement stmtSelect = SupportModelHelper.CreateByCompileOrParse(epService, soda, eplSelect);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
    
            var expectedType = new object[][]{
                    new object[] {"c0_1", typeof(string)}, new object[] {"c0_2", typeof(string)},
                    new object[] {"c1_1", typeof(int)}, new object[] {"c1_2", typeof(int)},
                    new object[] {"c2_1", typeof(SupportBean[])}, new object[] {"c2_2", typeof(SupportBean[])},
                    new object[] {"c2_3", typeof(SupportBean)}, new object[] {"c2_4", typeof(SupportBean)},
                    new object[] {"c2_5", typeof(SupportBean)}, new object[] {"c2_6", typeof(SupportBean)},
                    new object[] {"c3_1", typeof(int?)}, new object[] {"c3_2", typeof(int?)},
                    new object[] {"c4_1", typeof(SupportBean[])}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType, stmtSelect.EventType, SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);
    
            MakeSendSupportBean(epService, null, -1);
            EventBean @event = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@event, "c0_1,c0_2,c1_1,c1_2".Split(','), new object[]{"x", "x", 41, 41});
            EPAssertionUtil.AssertProps(@event, "c2_1,c2_2".Split(','), new object[]{sentSB, sentSB});
            EPAssertionUtil.AssertProps(@event, "c2_3,c2_4".Split(','), new object[]{sentSB[0], sentSB[1]});
            EPAssertionUtil.AssertProps(@event, "c2_5,c2_6".Split(','), new object[]{sentSB[0], sentSB[0]});
            EPAssertionUtil.AssertProps(@event, "c3_1,c3_2".Split(','), new object[]{2, 2});
            EPAssertionUtil.AssertProps(@event, "c4_1".Split(','), new object[]{sentSB});
    
            // unnamed column
            string eplSelectUnnamed = "select col2.sorted().firstOf(), mt.col2.sorted().firstOf()" +
                    " from SupportBean unidirectional, MyTable mt";
            EPStatement stmtSelectUnnamed = epService.EPAdministrator.CreateEPL(eplSelectUnnamed);
            var expectedTypeUnnamed = new object[][]{new object[] {"col2.sorted().firstOf()", typeof(SupportBean)},
                    new object[] {"mt.col2.sorted().firstOf()", typeof(SupportBean)}};
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedTypeUnnamed, stmtSelectUnnamed.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
    
            // invalid: ambiguous resolution
            SupportMessageAssertUtil.TryInvalid(epService, "" +
                            "select col0 from SupportBean, MyTable, MyTable",
                    "Error starting statement: Failed to validate select-clause expression 'col0': Ambiguous table column 'col0' should be prefixed by a stream name [");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private SupportBean MakeSendSupportBean(EPServiceProvider epService, string theString, int intPrimitive) {
            var b = new SupportBean(theString, intPrimitive);
            epService.EPRuntime.SendEvent(b);
            return b;
        }
    }
} // end of namespace
