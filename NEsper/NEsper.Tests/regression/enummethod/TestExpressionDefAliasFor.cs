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
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.enummethod
{
    [TestFixture]
    public class TestExpressionDefAliasFor  {
    
        private EPServiceProvider epService;
        private SupportUpdateListener listener;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
            listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            listener = null;
        }
    
        [Test]
        public void TestContextPartition() 
        {
            String epl =
                    "create expression the_expr alias for {theString='a' and intPrimitive=1};\n" +
                    "create context the_context start @now end after 10 minutes;\n" +
                    "@Name('s0') context the_context select * from SupportBean(the_expr)\n";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);

            epService.EPAdministrator.GetStatement("s0").AddListener(listener);

            epService.EPRuntime.SendEvent(new SupportBean("a", 1));
            Assert.IsTrue(listener.IsInvokedAndReset());

            epService.EPRuntime.SendEvent(new SupportBean("b", 1));
            Assert.False(listener.IsInvokedAndReset());
        }

        [Test]
        public void TestDocSamples() {
            epService.EPAdministrator.CreateEPL("create schema SampleEvent()");
            epService.EPAdministrator.CreateEPL("expression twoPI alias for {Math.PI * 2}\n" +
                    "select twoPI from SampleEvent");
    
            epService.EPAdministrator.CreateEPL("create schema EnterRoomEvent()");
            epService.EPAdministrator.CreateEPL("expression countPeople alias for {count(*)} \n" +
                    "select countPeople from EnterRoomEvent.win:time(10 seconds) having countPeople > 10");
        }
    
        [Test]
        public void TestNestedAlias() {
            string[] fields = "c0".Split(',');
            epService.EPAdministrator.CreateEPL("create expression F1 alias for {10}");
            epService.EPAdministrator.CreateEPL("create expression F2 alias for {20}");
            epService.EPAdministrator.CreateEPL("create expression F3 alias for {F1+F2}");
            epService.EPAdministrator.CreateEPL("select F3 as c0 from SupportBean").AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {30});
        }
    
        [Test]
        public void TestAliasAggregation() {
            string epl = "@Audit expression total alias for {sum(IntPrimitive)} " +
                    "select total, total+1 from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(listener);
    
            string[] fields = "total,total+1".Split(',');
            foreach (string field in fields) {
                Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(field));
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {10, 11});
        }
    
        [Test]
        public void TestGlobalAliasAndSODA() {
            string eplDeclare = "create expression myaliastwo alias for {2}";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(eplDeclare);
            Assert.AreEqual(eplDeclare, model.ToEPL());
            EPStatement stmtDeclare = epService.EPAdministrator.Create(model);
            Assert.AreEqual(eplDeclare, stmtDeclare.Text);
    
            epService.EPAdministrator.CreateEPL("create expression myalias alias for {1}");
            epService.EPAdministrator.CreateEPL("select myaliastwo from SupportBean(IntPrimitive = myalias)").AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("myaliastwo"));
        }
    
        [Test]
        public void TestInvalid() {
            TryInvalid("expression total alias for {sum(xxx)} select total+1 from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'total+1': Error validating expression alias 'total': Failed to validate alias expression body expression 'sum(xxx)': Property named 'xxx' is not valid in any stream [expression total alias for {sum(xxx)} select total+1 from SupportBean]");
            TryInvalid("expression total xxx for {1} select total+1 from SupportBean",
                    "For expression alias 'total' expecting 'alias' keyword but received 'xxx' [expression total xxx for {1} select total+1 from SupportBean]");
            TryInvalid("expression total(a) alias for {1} select total+1 from SupportBean",
                    "For expression alias 'total' expecting no parameters but received 'a' [expression total(a) alias for {1} select total+1 from SupportBean]");
            TryInvalid("expression total alias for {a -> 1} select total+1 from SupportBean",
                    "For expression alias 'total' expecting an expression without parameters but received 'a ->' [expression total alias for {a -> 1} select total+1 from SupportBean]");
            TryInvalid("expression total alias for ['some text'] select total+1 from SupportBean",
                    "For expression alias 'total' expecting an expression but received a script [expression total alias for ['some text'] select total+1 from SupportBean]");
        }
    
        private void TryInvalid(string epl, string message) {
            try {
                epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    }
}
