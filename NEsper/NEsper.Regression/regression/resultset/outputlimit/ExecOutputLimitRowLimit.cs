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
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.outputlimit
{
    public class ExecOutputLimitRowLimit : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBeanNumeric", typeof(SupportBeanNumeric));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionLimitOneWithOrderOptimization(epService);
            RunAssertionBatchNoOffsetNoOrder(epService);
            RunAssertionLengthOffsetVariable(epService);
            RunAssertionOrderBy(epService);
            RunAssertionBatchOffsetNoOrderOM(epService);
            RunAssertionFullyGroupedOrdered(epService);
            RunAssertionEventPerRowUnGrouped(epService);
            RunAssertionGroupedSnapshot(epService);
            RunAssertionGroupedSnapshotNegativeRowcount(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionLimitOneWithOrderOptimization(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
    
            // batch-window assertions
            string eplWithBatchSingleKey = "select TheString from SupportBean#length_batch(10) order by TheString limit 1";
            TryAssertionLimitOneSingleKeySortBatch(epService, eplWithBatchSingleKey);
    
            string eplWithBatchMultiKey = "select TheString, IntPrimitive from SupportBean#length_batch(5) order by TheString asc, IntPrimitive desc limit 1";
            TryAssertionLimitOneMultiKeySortBatch(epService, eplWithBatchMultiKey);
    
            // context output-when-terminated assertions
            epService.EPAdministrator.CreateEPL("create context StartS0EndS1 as start SupportBean_S0 end SupportBean_S1");
    
            string eplContextSingleKey = "context StartS0EndS1 " +
                    "select TheString from SupportBean#keepall " +
                    "output snapshot when terminated " +
                    "order by TheString limit 1";
            TryAssertionLimitOneSingleKeySortBatch(epService, eplContextSingleKey);
    
            string eplContextMultiKey = "context StartS0EndS1 " +
                    "select TheString, IntPrimitive from SupportBean#keepall " +
                    "output snapshot when terminated " +
                    "order by TheString asc, IntPrimitive desc limit 1";
            TryAssertionLimitOneMultiKeySortBatch(epService, eplContextMultiKey);
        }
    
        private void TryAssertionLimitOneMultiKeySortBatch(EPServiceProvider epService, string epl) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSBSequenceAndAssert(epService, listener, "F", 10, new object[][]{new object[] {"F", 10}, new object[] {"X", 8}, new object[] {"F", 8}, new object[] {"G", 10}, new object[] {"X", 1}});
            SendSBSequenceAndAssert(epService, listener, "G", 12, new object[][]{new object[] {"X", 10}, new object[] {"G", 12}, new object[] {"H", 100}, new object[] {"G", 10}, new object[] {"X", 1}});
            SendSBSequenceAndAssert(epService, listener, "G", 11, new object[][]{new object[] {"G", 10}, new object[] {"G", 8}, new object[] {"G", 8}, new object[] {"G", 10}, new object[] {"G", 11}});
    
            stmt.Dispose();
        }
    
        private void TryAssertionLimitOneSingleKeySortBatch(EPServiceProvider epService, string epl) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSBSequenceAndAssert(epService, listener, "A", new string[]{"F", "Q", "R", "T", "M", "T", "A", "I", "P", "B"});
            SendSBSequenceAndAssert(epService, listener, "B", new string[]{"P", "Q", "P", "T", "P", "T", "P", "P", "P", "B"});
            SendSBSequenceAndAssert(epService, listener, "C", new string[]{"C", "P", "Q", "P", "T", "P", "T", "P", "P", "P", "X"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionBatchNoOffsetNoOrder(EPServiceProvider epService) {
            string statementString = "select irstream * from SupportBean#length_batch(3) limit 1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementString);
            var listener = new SupportUpdateListener();
            TryAssertion(epService, stmt, listener);
            stmt.Dispose();
        }
    
        private void RunAssertionLengthOffsetVariable(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create variable int myrows = 2");
            epService.EPAdministrator.CreateEPL("create variable int myoffset = 1");
            epService.EPAdministrator.CreateEPL("on SupportBeanNumeric set myrows = intOne, myoffset = intTwo");
    
            string statementString = "select * from SupportBean#length(5) output every 5 events limit myoffset, myrows";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementString);
            var listener = new SupportUpdateListener();
            TryAssertionVariable(epService, stmt, listener);
            stmt.Dispose();
            listener.Reset();
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 1));
    
            statementString = "select * from SupportBean#length(5) output every 5 events limit myrows offset myoffset";
            stmt = epService.EPAdministrator.CreateEPL(statementString);
            TryAssertionVariable(epService, stmt, listener);
            stmt.Dispose();
            listener.Reset();
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 1));
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(statementString);
            Assert.AreEqual(statementString, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            TryAssertionVariable(epService, stmt, listener);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOrderBy(EPServiceProvider epService) {
            string statementString = "select * from SupportBean#length(5) output every 5 events order by IntPrimitive limit 2 offset 2";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementString);
            var listener = new SupportUpdateListener();
    
            string[] fields = "TheString".Split(',');
            stmt.Events += listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendEvent(epService, "E1", 90);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendEvent(epService, "E2", 5);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendEvent(epService, "E3", 60);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}});
    
            SendEvent(epService, "E4", 99);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}, new object[] {"E4"}});
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E5", 6);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E3"}, new object[] {"E1"}});
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"E3"}, new object[] {"E1"}});
    
            stmt.Dispose();
        }
    
        private void TryAssertionVariable(EPServiceProvider epService, EPStatement stmt, SupportUpdateListener listener) {
            string[] fields = "TheString".Split(',');
            stmt.Events += listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendEvent(epService, "E1", 1);
            SendEvent(epService, "E2", 2);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E2"}});
    
            SendEvent(epService, "E3", 3);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E2"}, new object[] {"E3"}});
    
            SendEvent(epService, "E4", 4);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E2"}, new object[] {"E3"}});
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "E5", 5);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E2"}, new object[] {"E3"}});
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"E2"}, new object[] {"E3"}});
    
            SendEvent(epService, "E6", 6);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E3"}, new object[] {"E4"}});
            Assert.IsFalse(listener.IsInvoked);
    
            // change variable values
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 3));
            SendEvent(epService, "E7", 7);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E6"}, new object[] {"E7"}});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(-1, 0));
            SendEvent(epService, "E8", 8);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}, new object[] {"E8"}});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(10, 0));
            SendEvent(epService, "E9", 9);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}, new object[] {"E8"}, new object[] {"E9"}});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(6, 3));
            SendEvent(epService, "E10", 10);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E9"}, new object[] {"E10"}});
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"E9"}, new object[] {"E10"}});
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(1, 1));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E7"}});
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 1));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E7"}, new object[] {"E8"}});
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(1, 2));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E8"}});
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(6, 6));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(1, 4));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E10"}});
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric((int?) null, null));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E6"}, new object[] {"E7"}, new object[] {"E8"}, new object[] {"E9"}, new object[] {"E10"}});
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(null, 2));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E8"}, new object[] {"E9"}, new object[] {"E10"}});
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, null));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E6"}, new object[] {"E7"}});
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(-1, 4));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E10"}});
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(-1, 0));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E6"}, new object[] {"E7"}, new object[] {"E8"}, new object[] {"E9"}, new object[] {"E10"}});
    
            epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
        }
    
        private void RunAssertionBatchOffsetNoOrderOM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            model.SelectClause.StreamSelector = StreamSelector.RSTREAM_ISTREAM_BOTH;
            model.FromClause = FromClause.Create(FilterStream.Create("SupportBean").AddView("length_batch", Expressions.Constant(3)));
            model.RowLimitClause = RowLimitClause.Create(1);
    
            string statementString = "select irstream * from SupportBean#length_batch(3) limit 1";
            Assert.AreEqual(statementString, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            TryAssertion(epService, stmt, listener);
            stmt.Dispose();
            listener.Reset();
    
            model = epService.EPAdministrator.CompileEPL(statementString);
            Assert.AreEqual(statementString, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            TryAssertion(epService, stmt, listener);
            stmt.Dispose();
        }
    
        private void RunAssertionFullyGroupedOrdered(EPServiceProvider epService) {
            string statementString = "select TheString, sum(IntPrimitive) as mysum from SupportBean#length(5) group by TheString order by sum(IntPrimitive) limit 2";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementString);
            var listener = new SupportUpdateListener();
    
            string[] fields = "TheString,mysum".Split(',');
            stmt.Events += listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendEvent(epService, "E1", 90);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 90}});
    
            SendEvent(epService, "E2", 5);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E2", 5}, new object[] {"E1", 90}});
    
            SendEvent(epService, "E3", 60);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E2", 5}, new object[] {"E3", 60}});
    
            SendEvent(epService, "E3", 40);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E2", 5}, new object[] {"E1", 90}});
    
            SendEvent(epService, "E2", 1000);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1", 90}, new object[] {"E3", 100}});
        }
    
        private void RunAssertionEventPerRowUnGrouped(EPServiceProvider epService) {
            SendTimer(epService, 1000);
            string statementString = "select TheString, sum(IntPrimitive) as mysum from SupportBean#length(5) output every 10 seconds order by TheString desc limit 2";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementString);
            var listener = new SupportUpdateListener();
    
            string[] fields = "TheString,mysum".Split(',');
            stmt.Events += listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendEvent(epService, "E1", 10);
            SendEvent(epService, "E2", 5);
            SendEvent(epService, "E3", 20);
            SendEvent(epService, "E4", 30);
    
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E4", 65}, new object[] {"E3", 35}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupedSnapshot(EPServiceProvider epService) {
            SendTimer(epService, 1000);
            string statementString = "select TheString, sum(IntPrimitive) as mysum from SupportBean#length(5) group by TheString output snapshot every 10 seconds order by sum(IntPrimitive) desc limit 2";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementString);
            var listener = new SupportUpdateListener();
    
            string[] fields = "TheString,mysum".Split(',');
            stmt.Events += listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendEvent(epService, "E1", 10);
            SendEvent(epService, "E2", 5);
            SendEvent(epService, "E3", 20);
            SendEvent(epService, "E1", 30);
    
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E1", 40}, new object[] {"E3", 20}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupedSnapshotNegativeRowcount(EPServiceProvider epService) {
            SendTimer(epService, 1000);
            string statementString = "select TheString, sum(IntPrimitive) as mysum from SupportBean#length(5) group by TheString output snapshot every 10 seconds order by sum(IntPrimitive) desc limit -1 offset 1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementString);
            var listener = new SupportUpdateListener();
    
            string[] fields = "TheString,mysum".Split(',');
            stmt.Events += listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendEvent(epService, "E1", 10);
            SendEvent(epService, "E2", 5);
            SendEvent(epService, "E3", 20);
            SendEvent(epService, "E1", 30);
    
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E3", 20}, new object[] {"E2", 5}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create variable string myrows = 'abc'");
            TryInvalid(epService, "select * from SupportBean limit myrows",
                    "Error starting statement: Limit clause requires a variable of numeric type [select * from SupportBean limit myrows]");
            TryInvalid(epService, "select * from SupportBean limit 1, myrows",
                    "Error starting statement: Limit clause requires a variable of numeric type [select * from SupportBean limit 1, myrows]");
            TryInvalid(epService, "select * from SupportBean limit dummy",
                    "Error starting statement: Limit clause variable by name 'dummy' has not been declared [select * from SupportBean limit dummy]");
            TryInvalid(epService, "select * from SupportBean limit 1,dummy",
                    "Error starting statement: Limit clause variable by name 'dummy' has not been declared [select * from SupportBean limit 1,dummy]");
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void TryAssertion(EPServiceProvider epService, EPStatement stmt, SupportUpdateListener listener) {
            string[] fields = "TheString".Split(',');
            stmt.Events += listener.Update;
            SendEvent(epService, "E1", 1);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}});
    
            SendEvent(epService, "E2", 2);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E1"}});
    
            SendEvent(epService, "E3", 3);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"E1"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendEvent(epService, "E4", 4);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E4"}});
    
            SendEvent(epService, "E5", 5);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][]{new object[] {"E4"}});
    
            SendEvent(epService, "E6", 6);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"E4"}});
            EPAssertionUtil.AssertPropsPerRow(listener.LastOldData, fields, new object[][]{new object[] {"E1"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
        }
    
        private void SendEvent(EPServiceProvider epService, string theString, int intPrimitive) {
            epService.EPRuntime.SendEvent(new SupportBean(theString, intPrimitive));
        }
    
        private void SendSBSequenceAndAssert(EPServiceProvider epService, SupportUpdateListener listener, string expected, string[] theStrings) {
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            foreach (string theString in theStrings) {
                SendEvent(epService, theString, 0);
            }
            epService.EPRuntime.SendEvent(new SupportBean_S1(0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[]{expected});
        }
    
        private void SendSBSequenceAndAssert(EPServiceProvider epService, SupportUpdateListener listener, string expectedString, int expectedInt, object[][] rows) {
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            foreach (object[] row in rows) {
                SendEvent(epService, row[0].ToString(), (int) row[1]);
            }
            epService.EPRuntime.SendEvent(new SupportBean_S1(0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "TheString,IntPrimitive".Split(','), new object[]{expectedString, expectedInt});
        }
    }
} // end of namespace
