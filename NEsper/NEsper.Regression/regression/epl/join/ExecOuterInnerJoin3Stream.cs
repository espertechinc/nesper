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

namespace com.espertech.esper.regression.epl.join
{
    public class ExecOuterInnerJoin3Stream : RegressionExecution {
        private static readonly string EVENT_S0 = typeof(SupportBean_S0).FullName;
        private static readonly string EVENT_S1 = typeof(SupportBean_S1).FullName;
        private static readonly string EVENT_S2 = typeof(SupportBean_S2).FullName;
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionFullJoinVariantThree(epService);
            RunAssertionFullJoinVariantTwo(epService);
            RunAssertionFullJoinVariantOne(epService);
            RunAssertionLeftJoinVariantThree(epService);
            RunAssertionLeftJoinVariantTwo(epService);
            RunAssertionRightJoinVariantOne(epService);
        }
    
        private void RunAssertionFullJoinVariantThree(EPServiceProvider epService) {
            string joinStatement = "select * from " +
                    EVENT_S1 + "#keepall as s1 inner join " +
                    EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20 " +
                    " full outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10";
    
            TryAssertionFull(epService, joinStatement);
        }
    
        private void RunAssertionFullJoinVariantTwo(EPServiceProvider epService) {
            string joinStatement = "select * from " +
                    EVENT_S2 + "#length(1000) as s2 " +
                    " inner join " + EVENT_S1 + "#keepall s1 on s1.p10 = s2.p20" +
                    " full outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10";
    
            TryAssertionFull(epService, joinStatement);
        }
    
        private void RunAssertionFullJoinVariantOne(EPServiceProvider epService) {
            string joinStatement = "select * from " +
                    EVENT_S0 + "#length(1000) as s0 " +
                    " full outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10" +
                    " inner join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20";
    
            TryAssertionFull(epService, joinStatement);
        }
    
        private void RunAssertionLeftJoinVariantThree(EPServiceProvider epService) {
            string joinStatement = "select * from " +
                    EVENT_S1 + "#keepall as s1 left outer join " +
                    EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
                    "inner join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20";
    
            TryAssertionFull(epService, joinStatement);
        }
    
        private void RunAssertionLeftJoinVariantTwo(EPServiceProvider epService) {
            string joinStatement = "select * from " +
                    EVENT_S2 + "#length(1000) as s2 " +
                    " inner join " + EVENT_S1 + "#keepall s1 on s1.p10 = s2.p20" +
                    " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10";
    
            TryAssertionFull(epService, joinStatement);
        }
    
        private void RunAssertionRightJoinVariantOne(EPServiceProvider epService) {
            string joinStatement = "select * from " +
                    EVENT_S0 + "#length(1000) as s0 " +
                    " right outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10" +
                    " inner join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20";
    
            TryAssertionFull(epService, joinStatement);
        }
    
        private void TryAssertionFull(EPServiceProvider epService, string expression) {
            string[] fields = "s0.id, s0.p00, s1.id, s1.p10, s2.id, s2.p20".Split(',');
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            joinView.Events += listener.Update;
    
            // s1, s2, s0
            epService.EPRuntime.SendEvent(new SupportBean_S1(100, "A_1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(200, "A_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, 100, "A_1", 200, "A_1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{0, "A_1", 100, "A_1", 200, "A_1"});
    
            // s1, s0, s2
            epService.EPRuntime.SendEvent(new SupportBean_S1(103, "D_1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(203, "D_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, 103, "D_1", 203, "D_1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "D_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, "D_1", 103, "D_1", 203, "D_1"});
    
            // s2, s1, s0
            epService.EPRuntime.SendEvent(new SupportBean_S2(201, "B_1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, 101, "B_1", 201, "B_1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "B_1", 101, "B_1", 201, "B_1"});
    
            // s2, s0, s1
            epService.EPRuntime.SendEvent(new SupportBean_S2(202, "C_1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C_1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "C_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "C_1", 102, "C_1", 202, "C_1"});
    
            // s0, s1, s2
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "E_1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(104, "E_1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(204, "E_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4, "E_1", 104, "E_1", 204, "E_1"});
    
            // s0, s2, s1
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "F_1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(205, "F_1"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(105, "F_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5, "F_1", 105, "F_1", 205, "F_1"});
        }
    }
} // end of namespace
