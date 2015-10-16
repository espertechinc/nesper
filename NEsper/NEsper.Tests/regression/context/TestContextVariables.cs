///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextVariables
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("SupportBean", typeof(SupportBean));
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            configuration.AddEventType("SupportBean_S2", typeof(SupportBean_S2));
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
    
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestSegmentedByKey() {
            string[] fields = "mycontextvar".Split(',');
            _epService.EPAdministrator.CreateEPL("create context MyCtx as " +
                    "partition by TheString from SupportBean, p00 from SupportBean_S0");
            _epService.EPAdministrator.CreateEPL("context MyCtx create variable int mycontextvar = 0");
            _epService.EPAdministrator.CreateEPL("context MyCtx on SupportBean(IntPrimitive > 0) set mycontextvar = IntPrimitive");
            _epService.EPAdministrator.CreateEPL("context MyCtx select mycontextvar from SupportBean_S0").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("P1", 0));   // allocate partition P1
            _epService.EPRuntime.SendEvent(new SupportBean("P1", 10));   // set variable
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "P1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {10});
    
            _epService.EPRuntime.SendEvent(new SupportBean("P2", 11));   // allocate and set variable partition E2
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "P2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {11});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "P1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {10});
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "P2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {11});
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5, "P3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {0});
    
            _epService.EPRuntime.SendEvent(new SupportBean("P3", 12));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(6, "P3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{12});
    
            foreach (string statement in _epService.EPAdministrator.StatementNames) {
                _epService.EPAdministrator.GetStatement(statement).Stop();
            }
        }
    
        [Test]
        public void TestOverlapping() {
            string[] fields = "mycontextvar".Split(',');
            _epService.EPAdministrator.CreateEPL("create context MyCtx as " +
                    "initiated by SupportBean_S0 s0 terminated by SupportBean_S1(p10 = s0.p00)");
            _epService.EPAdministrator.CreateEPL("context MyCtx create variable int mycontextvar = 5");
            _epService.EPAdministrator.CreateEPL("context MyCtx on SupportBean(TheString = context.s0.p00) set mycontextvar = IntPrimitive");
            _epService.EPAdministrator.CreateEPL("context MyCtx on SupportBean(IntPrimitive < 0) set mycontextvar = IntPrimitive");
            _epService.EPAdministrator.CreateEPL("context MyCtx select mycontextvar from SupportBean_S2(p20 = context.s0.p00)").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "P1"));    // allocate partition P1
            _epService.EPRuntime.SendEvent(new SupportBean_S2(1, "P1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{5});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "P2"));    // allocate partition P2
            _epService.EPRuntime.SendEvent(new SupportBean("P2", 10));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(2, "P2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {10});
    
            // set all to -1
            _epService.EPRuntime.SendEvent(new SupportBean("P2", -1));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(2, "P2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{-1});
            _epService.EPRuntime.SendEvent(new SupportBean_S2(2, "P1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{-1});
    
            _epService.EPRuntime.SendEvent(new SupportBean("P2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("P1", 21));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(2, "P2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{20});
            _epService.EPRuntime.SendEvent(new SupportBean_S2(2, "P1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {21});
    
            // terminate context partitions
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "P1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "P2"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "P1"));    // allocate partition P1
            _epService.EPRuntime.SendEvent(new SupportBean_S2(1, "P1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {5});
    
            _epService.EPAdministrator.DestroyAllStatements();


            // test module deployment and undeployment
            String epl = "@Name(\"context\")\n" +
                    "create context MyContext\n" +
                    "initiated by distinct(theString) SupportBean as input\n" +
                    "terminated by SupportBean(theString = input.theString);\n" +
                    "\n" +
                    "@Name(\"ctx variable counter\")\n" +
                    "context MyContext create variable integer counter = 0;\n";
            DeploymentResult result = _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            _epService.EPAdministrator.DeploymentAdmin.Undeploy(result.DeploymentId);
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
        }
    
        [Test]
        public void TestIterateAndListen() {
            _epService.EPAdministrator.CreateEPL("@Name('ctx') create context MyCtx as initiated by SupportBean_S0 s0 terminated after 24 hours");
    
            string[] fields = "mycontextvar".Split(',');
            SupportUpdateListener listenerCreateVariable = new SupportUpdateListener();
            EPStatement stmtVar = _epService.EPAdministrator.CreateEPL("@Name('var') context MyCtx create variable int mycontextvar = 5");
            stmtVar.Events += listenerCreateVariable.Update;
    
            SupportUpdateListener listenerUpdate = new SupportUpdateListener();
            EPStatement stmtUpd = _epService.EPAdministrator.CreateEPL("@Name('upd') context MyCtx on SupportBean(TheString = context.s0.p00) set mycontextvar = IntPrimitive");
            stmtUpd.Events += listenerUpdate.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "P1"));    // allocate partition P1
            _epService.EPRuntime.SendEvent(new SupportBean("P1", 100));    // update
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetNewAndReset(), "mycontextvar".Split(','), new object[] { 100 });
            EPAssertionUtil.AssertPropsPerRow(EPAssertionUtil.EnumeratorToArray(stmtUpd.GetEnumerator()), fields, new object[][]{ new object[] {100}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "P2"));    // allocate partition P1
            _epService.EPRuntime.SendEvent(new SupportBean("P2", 101));    // update
            EPAssertionUtil.AssertProps(listenerUpdate.AssertOneGetNewAndReset(), "mycontextvar".Split(','), new object[]{101});
            EPAssertionUtil.AssertPropsPerRow(EPAssertionUtil.EnumeratorToArray(stmtUpd.GetEnumerator()), fields, new object[][] { new object[] { 100 }, new object[] { 101 } });

            Assert.IsFalse(stmtVar.HasFirst());
            Assert.IsFalse(listenerCreateVariable.GetAndClearIsInvoked());
        }
    
        [Test]
        public void TestGetSetAPI() {
            _epService.EPAdministrator.CreateEPL("create context MyCtx as initiated by SupportBean_S0 s0 terminated after 24 hours");
            _epService.EPAdministrator.CreateEPL("context MyCtx create variable int mycontextvar = 5");
            _epService.EPAdministrator.CreateEPL("context MyCtx on SupportBean(TheString = context.s0.p00) set mycontextvar = IntPrimitive");
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "P1"));    // allocate partition P1
            AssertVariableValues(0, 5);

            _epService.EPRuntime.SetVariableValue(Collections.SingletonDataMap("mycontextvar", 10), 0);
            AssertVariableValues(0, 10);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "P2"));    // allocate partition P2
            AssertVariableValues(1, 5);
    
            _epService.EPRuntime.SetVariableValue(Collections.SingletonDataMap("mycontextvar", 11), 1);
            AssertVariableValues(1, 11);
    
            // global variable - trying to set via context partition selection
            _epService.EPAdministrator.CreateEPL("create variable int myglobarvar = 0");
            try {
                _epService.EPRuntime.SetVariableValue(Collections.SingletonDataMap("myglobarvar", 11), 0);
                Assert.Fail();
            }
            catch (VariableNotFoundException ex) {
                Assert.AreEqual("Variable by name 'myglobarvar' is a global variable and not context-partitioned", ex.Message);
            }
    
            // global variable - trying to get via context partition selection
            try {
                _epService.EPRuntime.GetVariableValue(Collections.SingletonSet("myglobarvar"), new SupportSelectorById(1));
                Assert.Fail();
            }
            catch (VariableNotFoundException ex) {
                Assert.AreEqual("Variable by name 'myglobarvar' is a global variable and not context-partitioned", ex.Message);
            }
        }
    
        [Test]
        public void TestInvalid() {
            _epService.EPAdministrator.CreateEPL("create context MyCtxOne as partition by TheString from SupportBean");
            _epService.EPAdministrator.CreateEPL("create context MyCtxTwo as partition by p00 from SupportBean_S0");
            _epService.EPAdministrator.CreateEPL("context MyCtxOne create variable int myctxone_int = 0");
    
            // undefined context
            TryInvalid("context MyCtx create variable int mycontext_invalid1 = 0",
                    "Error starting statement: Context by name 'MyCtx' has not been declared [context MyCtx create variable int mycontext_invalid1 = 0]");
    
            // wrong context uses variable
            TryInvalid("context MyCtxTwo select myctxone_int from SupportBean_S0",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' is not available for use with context 'MyCtxTwo' [context MyCtxTwo select myctxone_int from SupportBean_S0]");
    
            // variable use outside of context
            TryInvalid("select myctxone_int from SupportBean_S0",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context [select myctxone_int from SupportBean_S0]");
            TryInvalid("select * from SupportBean_S0.win:expr(myctxone_int > 5)",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context [select * from SupportBean_S0.win:expr(myctxone_int > 5)]");
            TryInvalid("select * from SupportBean_S0.win:keepall() limit myctxone_int",
                    "Error starting statement: Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context [select * from SupportBean_S0.win:keepall() limit myctxone_int]");
            TryInvalid("select * from SupportBean_S0.win:keepall() limit 10 offset myctxone_int",
                    "Error starting statement: Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context [select * from SupportBean_S0.win:keepall() limit 10 offset myctxone_int]");
            TryInvalid("select * from SupportBean_S0.win:keepall() output every myctxone_int events",
                    "Error starting statement: Error in the output rate limiting clause: Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context [select * from SupportBean_S0.win:keepall() output every myctxone_int events]");
            TryInvalid("@Hint('reclaim_group_aged=myctxone_int') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Error starting statement: Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context [@Hint('reclaim_group_aged=myctxone_int') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
        }
    
        public void TryInvalid(string epl, string message) {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void AssertVariableValues(int agentInstanceId, int expected) {
            IDictionary<String, IList<ContextPartitionVariableState>> states = _epService.EPRuntime.GetVariableValue(Collections.SingletonSet("mycontextvar"), new SupportSelectorById(agentInstanceId));
            Assert.AreEqual(1, states.Count);
            IList<ContextPartitionVariableState> list = states.Get("mycontextvar");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(expected, list[0].State);
        }
    }
}
