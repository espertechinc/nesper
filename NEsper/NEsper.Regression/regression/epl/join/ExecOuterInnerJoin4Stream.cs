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
    public class ExecOuterInnerJoin4Stream : RegressionExecution {
        private static readonly string[] FIELDS = "s0.id, s0.p00, s1.id, s1.p10, s2.id, s2.p20, s3.id, s3.p30".Split(',');
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
            configuration.AddEventType("S2", typeof(SupportBean_S2));
            configuration.AddEventType("S3", typeof(SupportBean_S3));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionFullMiddleJoinVariantTwo(epService);
            RunAssertionFullMiddleJoinVariantOne(epService);
            RunAssertionFullSidedJoinVariantTwo(epService);
            RunAssertionFullSidedJoinVariantOne(epService);
            RunAssertionStarJoinVariantTwo(epService);
            RunAssertionStarJoinVariantOne(epService);
        }
    
        private void RunAssertionFullMiddleJoinVariantTwo(EPServiceProvider epService) {
            string joinStatement = "select * from S3#keepall s3 " +
                    " inner join S2#keepall s2 on s3.p30 = s2.p20 " +
                    " full outer join S1#keepall s1 on s2.p20 = s1.p10 " +
                    " inner join S0#keepall s0 on s1.p10 = s0.p00";
    
            TryAssertionMiddle(epService, joinStatement);
        }
    
        private void RunAssertionFullMiddleJoinVariantOne(EPServiceProvider epService) {
            string joinStatement = "select * from S0#keepall s0 " +
                    " inner join S1#keepall s1 on s0.p00 = s1.p10 " +
                    " full outer join S2#keepall s2 on s1.p10 = s2.p20 " +
                    " inner join S3#keepall s3 on s2.p20 = s3.p30";
    
            TryAssertionMiddle(epService, joinStatement);
        }
    
        private void RunAssertionFullSidedJoinVariantTwo(EPServiceProvider epService) {
            string joinStatement = "select * from S3#keepall s3 " +
                    " full outer join S2#keepall s2 on s3.p30 = s2.p20 " +
                    " full outer join S1#keepall s1 on s2.p20 = s1.p10 " +
                    " inner join S0#keepall s0 on s1.p10 = s0.p00";
    
            TryAssertionSided(epService, joinStatement);
        }
    
        private void RunAssertionFullSidedJoinVariantOne(EPServiceProvider epService) {
            string joinStatement = "select * from S0#keepall s0 " +
                    " inner join S1#keepall s1 on s0.p00 = s1.p10 " +
                    " full outer join S2#keepall s2 on s1.p10 = s2.p20 " +
                    " full outer join S3#keepall s3 on s2.p20 = s3.p30";
    
            TryAssertionSided(epService, joinStatement);
        }
    
        private void RunAssertionStarJoinVariantTwo(EPServiceProvider epService) {
            string joinStatement = "select * from S0#keepall s0 " +
                    " left outer join S1#keepall s1 on s0.p00 = s1.p10 " +
                    " full outer join S2#keepall s2 on s0.p00 = s2.p20 " +
                    " inner join S3#keepall s3 on s0.p00 = s3.p30";
    
            TryAssertionStar(epService, joinStatement);
        }
    
        private void RunAssertionStarJoinVariantOne(EPServiceProvider epService) {
            string joinStatement = "select * from S3#keepall s3 " +
                    " inner join S0#keepall s0 on s0.p00 = s3.p30 " +
                    " full outer join S2#keepall s2 on s0.p00 = s2.p20 " +
                    " left outer join S1#keepall s1 on s1.p10 = s0.p00";
    
            TryAssertionStar(epService, joinStatement);
        }
    
        private void TryAssertionMiddle(EPServiceProvider epService, string expression) {
            string[] fields = "s0.id, s0.p00, s1.id, s1.p10, s2.id, s2.p20, s3.id, s3.p30".Split(',');
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // s0, s1, s2, s3
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(100, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(200, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(300, "A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{0, "A", 100, "A", 200, "A", 300, "A"});
    
            // s0, s2, s3, s1
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(201, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(301, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "B"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "B", 101, "B", 201, "B", 301, "B"});
    
            // s2, s3, s1, s0
            epService.EPRuntime.SendEvent(new SupportBean_S2(202, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(302, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "C", 102, "C", 202, "C", 302, "C"});
    
            // s1, s2, s0, s3
            epService.EPRuntime.SendEvent(new SupportBean_S1(103, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(203, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(303, "D"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, "D", 103, "D", 203, "D", 303, "D"});
    
            stmt.Dispose();
        }
    
        private void TryAssertionSided(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // s0, s1, s2, s3
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(100, "A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{0, "A", 100, "A", null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(200, "A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{0, "A", 100, "A", 200, "A", null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(300, "A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{0, "A", 100, "A", 200, "A", 300, "A"});
    
            // s0, s2, s3, s1
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(201, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(301, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "B"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{1, "B", 101, "B", 201, "B", 301, "B"});
    
            // s2, s3, s1, s0
            epService.EPRuntime.SendEvent(new SupportBean_S2(202, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(302, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{2, "C", 102, "C", 202, "C", 302, "C"});
    
            // s1, s2, s0, s3
            epService.EPRuntime.SendEvent(new SupportBean_S1(103, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(203, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "D"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{3, "D", 103, "D", 203, "D", null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(303, "D"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{3, "D", 103, "D", 203, "D", 303, "D"});
    
            stmt.Dispose();
        }
    
        private void TryAssertionStar(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // s0, s1, s2, s3
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(100, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(200, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(300, "A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{0, "A", 100, "A", 200, "A", 300, "A"});
    
            // s0, s2, s3, s1
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(201, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(301, "B"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{1, "B", null, null, 201, "B", 301, "B"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "B"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{1, "B", 101, "B", 201, "B", 301, "B"});
    
            // s2, s3, s1, s0
            epService.EPRuntime.SendEvent(new SupportBean_S2(202, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(302, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{2, "C", 102, "C", 202, "C", 302, "C"});
    
            // s1, s2, s0, s3
            epService.EPRuntime.SendEvent(new SupportBean_S1(103, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(203, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(303, "D"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{3, "D", 103, "D", 203, "D", 303, "D"});
    
            // s3, s0, s1, s2
            epService.EPRuntime.SendEvent(new SupportBean_S3(304, "E"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "E"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{4, "E", null, null, null, null, 304, "E"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(104, "E"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{4, "E", 104, "E", null, null, 304, "E"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(204, "E"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), FIELDS, new object[]{4, "E", 104, "E", 204, "E", 304, "E"});
    
            stmt.Dispose();
        }
    }
} // end of namespace
