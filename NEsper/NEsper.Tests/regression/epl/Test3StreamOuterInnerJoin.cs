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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class Test3StreamOuterInnerJoin 
    {
        private EPServiceProvider epService;
        private SupportUpdateListener updateListener;
    
        private readonly static String EVENT_S0 = typeof(SupportBean_S0).FullName;
        private readonly static String EVENT_S1 = typeof(SupportBean_S1).FullName;
        private readonly static String EVENT_S2 = typeof(SupportBean_S2).FullName;
    
        [SetUp]
        public void SetUp()
        {
            epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            updateListener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            updateListener = null;
        }
    
        [Test]
        public void TestFullJoinVariantThree()
        {
            String joinStatement = "select * from " +
                    EVENT_S1 + ".win:keepall() as s1 inner join " +
                    EVENT_S2 + ".win:length(1000) as s2 on s1.p10 = s2.p20 " +
                    " full outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10";
    
            RunAssertionFull(joinStatement);
        }
    
        [Test]
        public void TestFullJoinVariantTwo()
        {
            String joinStatement = "select * from " +
                    EVENT_S2 + ".win:length(1000) as s2 " +
                   " inner join " + EVENT_S1 + ".win:keepall() s1 on s1.p10 = s2.p20" +
                   " full outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10";
    
            RunAssertionFull(joinStatement);
        }
    
        [Test]
        public void TestFullJoinVariantOne()
        {
            String joinStatement = "select * from " +
                    EVENT_S0 + ".win:length(1000) as s0 " +
                " full outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10" +
                " inner join " + EVENT_S2 + ".win:length(1000) as s2 on s1.p10 = s2.p20";
    
            RunAssertionFull(joinStatement);
        }
    
        [Test]
        public void TestLeftJoinVariantThree()
        {
            String joinStatement = "select * from " +
                    EVENT_S1 + ".win:keepall() as s1 left outer join " +
                    EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10 " +
                    "inner join " + EVENT_S2 + ".win:length(1000) as s2 on s1.p10 = s2.p20";
    
            RunAssertionFull(joinStatement);
        }
    
        [Test]
        public void TestLeftJoinVariantTwo()
        {
            String joinStatement = "select * from " +
                    EVENT_S2 + ".win:length(1000) as s2 " +
                   " inner join " + EVENT_S1 + ".win:keepall() s1 on s1.p10 = s2.p20" +
                   " left outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10";
    
            RunAssertionFull(joinStatement);
        }
    
        [Test]
        public void TestRightJoinVariantOne()
        {
            String joinStatement = "select * from " +
                    EVENT_S0 + ".win:length(1000) as s0 " +
                " right outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10" +
                " inner join " + EVENT_S2 + ".win:length(1000) as s2 on s1.p10 = s2.p20";
    
            RunAssertionFull(joinStatement);
        }
    
        public void RunAssertionFull(String expression)
        {
            String[] fields = "s0.id, s0.P00, s1.id, s1.p10, s2.id, s2.p20".Split(',');
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(expression);
            joinView.Events += updateListener.Update;
    
            // s1, s2, s0
            epService.EPRuntime.SendEvent(new SupportBean_S1(100, "A_1"));
            Assert.IsFalse(updateListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(200, "A_1"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, 100, "A_1", 200, "A_1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A_1"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[] {0, "A_1", 100, "A_1", 200, "A_1"});
    
            // s1, s0, s2
            epService.EPRuntime.SendEvent(new SupportBean_S1(103, "D_1"));
            Assert.IsFalse(updateListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(203, "D_1"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, 103, "D_1", 203, "D_1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "D_1"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[] {3, "D_1", 103, "D_1", 203, "D_1"});
    
            // s2, s1, s0
            epService.EPRuntime.SendEvent(new SupportBean_S2(201, "B_1"));
            Assert.IsFalse(updateListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(101, "B_1"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, 101, "B_1", 201, "B_1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B_1"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[] {1, "B_1", 101, "B_1", 201, "B_1"});
    
            // s2, s0, s1
            epService.EPRuntime.SendEvent(new SupportBean_S2(202, "C_1"));
            Assert.IsFalse(updateListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C_1"));
            Assert.IsFalse(updateListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(102, "C_1"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[] {2, "C_1", 102, "C_1", 202, "C_1"});
    
            // s0, s1, s2
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "E_1"));
            Assert.IsFalse(updateListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(104, "E_1"));
            Assert.IsFalse(updateListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(204, "E_1"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[] {4, "E_1", 104, "E_1", 204, "E_1"});
    
            // s0, s2, s1
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "F_1"));
            Assert.IsFalse(updateListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(205, "F_1"));
            Assert.IsFalse(updateListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(105, "F_1"));
            EPAssertionUtil.AssertProps(updateListener.AssertOneGetNewAndReset(), fields, new Object[] {5, "F_1", 105, "F_1", 205, "F_1"});
        }
    }
}
