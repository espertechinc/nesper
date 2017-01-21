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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class Test4StreamOuterInnerJoin 
    {
        private static String[] fields = "s0.id, s0.P00, s1.id, s1.p10, s2.id, s2.p20, s3.id, s3.p30".Split(',');
        
        private EPServiceProvider epService;
        private SupportUpdateListener listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("S0", typeof(SupportBean_S0));
            config.AddEventType("S1", typeof(SupportBean_S1));
            config.AddEventType("S2", typeof(SupportBean_S2));
            config.AddEventType("S3", typeof(SupportBean_S3));
    
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            listener = null;
        }
    
        [Test]
        public void TestFullMiddleJoinVariantTwo()
        {
            String joinStatement =  "select * from S3.win:keepall() s3 " +
                                    " inner join S2.win:keepall() s2 on s3.p30 = s2.p20 " +
                                    " full outer join S1.win:keepall() s1 on s2.p20 = s1.p10 " +
                                    " inner join S0.win:keepall() s0 on s1.p10 = s0.P00";
    
            RunAssertionMiddle(joinStatement);
        }
    
        [Test]
        public void TestFullMiddleJoinVariantOne()
        {
            String joinStatement =  "select * from S0.win:keepall() s0 " +
                                    " inner join S1.win:keepall() s1 on s0.P00 = s1.p10 " +
                                    " full outer join S2.win:keepall() s2 on s1.p10 = s2.p20 " +
                                    " inner join S3.win:keepall() s3 on s2.p20 = s3.p30";
    
            RunAssertionMiddle(joinStatement);
        }
    
        [Test]
        public void TestFullSidedJoinVariantTwo()
        {
            String joinStatement =  "select * from S3.win:keepall() s3 " +
                                    " full outer join S2.win:keepall() s2 on s3.p30 = s2.p20 " +
                                    " full outer join S1.win:keepall() s1 on s2.p20 = s1.p10 " +
                                    " inner join S0.win:keepall() s0 on s1.p10 = s0.P00";
    
            RunAssertionSided(joinStatement);
        }
    
        [Test]
        public void TestFullSidedJoinVariantOne()
        {
            String joinStatement =  "select * from S0.win:keepall() s0 " +
                                    " inner join S1.win:keepall() s1 on s0.P00 = s1.p10 " +
                                    " full outer join S2.win:keepall() s2 on s1.p10 = s2.p20 " +
                                    " full outer join S3.win:keepall() s3 on s2.p20 = s3.p30";
    
            RunAssertionSided(joinStatement);
        }
    
        [Test]
        public void TestStarJoinVariantTwo()
        {
            String joinStatement =  "select * from S0.win:keepall() s0 " +
                                    " left outer join S1.win:keepall() s1 on s0.P00 = s1.p10 " +
                                    " full outer join S2.win:keepall() s2 on s0.P00 = s2.p20 " +
                                    " inner join S3.win:keepall() s3 on s0.P00 = s3.p30";
    
            RunAssertionStar(joinStatement);
        }
    
        [Test]
        public void TestStarJoinVariantOne()
        {
            String joinStatement =  "select * from S3.win:keepall() s3 " +
                                    " inner join S0.win:keepall() s0 on s0.P00 = s3.p30 " +
                                    " full outer join S2.win:keepall() s2 on s0.P00 = s2.p20 " +
                                    " left outer join S1.win:keepall() s1 on s1.p10 = s0.P00";
    
            RunAssertionStar(joinStatement);
        }
    
        public void RunAssertionMiddle(String expression)
        {
            String[] fields = "s0.id, s0.P00, s1.id, s1.p10, s2.id, s2.p20, s3.id, s3.p30".Split(',');
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(expression);
            joinView.Events += listener.Update;
    
            // s0, s1, s2, s3
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(100, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(200, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(300, "A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {0, "A", 100, "A", 200, "A", 300, "A"});
    
            // s0, s2, s3, s1
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(201, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(301, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "B"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {1, "B", 101, "B", 201, "B", 301, "B"});
    
            // s2, s3, s1, s0
            epService.EPRuntime.SendEvent(new SupportBean_S2(202, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(302, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {2, "C", 102, "C", 202, "C", 302, "C"});
    
            // s1, s2, s0, s3
            epService.EPRuntime.SendEvent(new SupportBean_S1(103, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(203, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(303, "D"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {3, "D", 103, "D", 203, "D", 303, "D"});
        }
    
        public void RunAssertionSided(String expression)
        {
            EPStatement joinView = epService.EPAdministrator.CreateEPL(expression);
            joinView.Events += listener.Update;
    
            // s0, s1, s2, s3
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(100, "A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {0, "A", 100, "A", null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(200, "A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {0, "A", 100, "A", 200, "A", null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(300, "A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {0, "A", 100, "A", 200, "A", 300, "A"});
    
            // s0, s2, s3, s1
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(201, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(301, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "B"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {1, "B", 101, "B", 201, "B", 301, "B"});
    
            // s2, s3, s1, s0
            epService.EPRuntime.SendEvent(new SupportBean_S2(202, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(302, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {2, "C", 102, "C", 202, "C", 302, "C"});
    
            // s1, s2, s0, s3
            epService.EPRuntime.SendEvent(new SupportBean_S1(103, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(203, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "D"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {3, "D", 103, "D", 203, "D", null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(303, "D"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {3, "D", 103, "D", 203, "D", 303, "D"});
        }
    
        public void RunAssertionStar(String expression)
        {
            EPStatement joinView = epService.EPAdministrator.CreateEPL(expression);
            joinView.Events += listener.Update;
    
            // s0, s1, s2, s3
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(100, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(200, "A"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(300, "A"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {0, "A", 100, "A", 200, "A", 300, "A"});
    
            // s0, s2, s3, s1
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(201, "B"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(301, "B"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {1, "B", null, null, 201, "B", 301, "B"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "B"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {1, "B", 101, "B", 201, "B", 301, "B"});
    
            // s2, s3, s1, s0
            epService.EPRuntime.SendEvent(new SupportBean_S2(202, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(302, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "C"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {2, "C", 102, "C", 202, "C", 302, "C"});
    
            // s1, s2, s0, s3
            epService.EPRuntime.SendEvent(new SupportBean_S1(103, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(203, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "D"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(303, "D"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {3, "D", 103, "D", 203, "D", 303, "D"});
    
            // s3, s0, s1, s2
            epService.EPRuntime.SendEvent(new SupportBean_S3(304, "E"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "E"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {4, "E", null, null, null, null, 304, "E"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(104, "E"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {4, "E", 104, "E", null, null, 304, "E"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(204, "E"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {4, "E", 104, "E", 204, "E", 304, "E"});
        }
    }
}
