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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.lambda;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestCreateExpression 
    {
        private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddEventType(typeof(SupportBean_S0));
            config.AddEventType(typeof(SupportCollection));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestInvalid() {
            _epService.EPAdministrator.CreateEPL("create expression E1 {''}");
            TryInvalid("create expression E1 {''}",
                        "Error starting statement: Expression 'E1' has already been declared [create expression E1 {''}]");

            _epService.EPAdministrator.CreateEPL("create expression int js:abc(p1, p2) [p1*p2]");
            TryInvalid("create expression int js:abc(a, a) [p1*p2]",
                    "Error starting statement: Script 'abc' that takes the same number of parameters has already been declared [create expression int js:abc(a, a) [p1*p2]]");
        }
    
        [Test]
        public void TestParseSpecialAndMixedExprAndScript()
        {
            SupportUpdateListener listener = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("create expression string js:myscript(p1) [\"--\"+p1+\"--\"]");
            _epService.EPAdministrator.CreateEPL("create expression myexpr {sb => '--'||TheString||'--'}");
    
            // test mapped property syntax
            String eplMapped = "select myscript('x') as c0, myexpr(sb) as c1 from SupportBean as sb";
            EPStatement stmtMapped = _epService.EPAdministrator.CreateEPL(eplMapped);
            stmtMapped.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new Object[]{"--x--", "--E1--"});
            stmtMapped.Dispose();
    
            // test expression chained syntax
            String eplExpr = "" +
                    "create expression scalarfilter {s => " +
                    "   Strvals.where(y => y != 'E1') " +
                    "}";
            _epService.EPAdministrator.CreateEPL(eplExpr);
            String eplSelect = "select scalarfilter(t).where(x => x != 'E2') as val1 from SupportCollection as t";
            _epService.EPAdministrator.CreateEPL(eplSelect).Events += listener.Update;
            _epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", "E3", "E4");
            _epService.EPAdministrator.DestroyAllStatements();
            listener.Reset();
    
            // test script chained syntax
            String eplScript = "create expression " + typeof(SupportBean).FullName + " js:callIt() [ clr.New('" + typeof(SupportBean).FullName + "',new Array('E1', 10)); ]";
            _epService.EPAdministrator.CreateEPL(eplScript);
            _epService.EPAdministrator.CreateEPL("select callIt() as val0, callIt().get_TheString() as val1 from SupportBean as sb").Events += listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0.TheString,val0.IntPrimitive,val1".Split(','), new Object[]{"E1", 10, "E1"});
        }
    
        [Test]
        public void TestScriptUse()
        {
            _epService.EPAdministrator.CreateEPL("create expression int js:abc(p1, p2) [p1*p2*10]");
            _epService.EPAdministrator.CreateEPL("create expression int js:abc(p1) [p1*10]");
    
            String epl = "select abc(IntPrimitive, DoublePrimitive) as c0, abc(IntPrimitive) as c1 from SupportBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(MakeBean("E1", 10, 3.5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new Object[]{350, 100});
    
            stmt.Dispose();
    
            // test SODA
            String eplExpr = "create expression somescript(i1) ['a']";
            EPStatementObjectModel modelExpr = _epService.EPAdministrator.CompileEPL(eplExpr);
            Assert.AreEqual(eplExpr, modelExpr.ToEPL());
            EPStatement stmtSODAExpr = _epService.EPAdministrator.Create(modelExpr);
            Assert.AreEqual(eplExpr, stmtSODAExpr.Text);
    
            String eplSelect = "select somescript(1) from SupportBean";
            EPStatementObjectModel modelSelect = _epService.EPAdministrator.CompileEPL(eplSelect);
            Assert.AreEqual(eplSelect, modelSelect.ToEPL());
            EPStatement stmtSODASelect = _epService.EPAdministrator.Create(modelSelect);
            Assert.AreEqual(eplSelect, stmtSODASelect.Text);
        }
    
        [Test]
        public void TestExpressionUse()
        {
            SupportUpdateListener listener = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("create expression TwoPi {Math.PI * 2}");
            _epService.EPAdministrator.CreateEPL("create expression factorPi {sb => Math.PI * IntPrimitive}");
    
            String[] fields = "c0,c1,c2".Split(',');
            String epl = "select " +
                    "TwoPi() as c0," +
                    "(select TwoPi() from SupportBean_S0.std:lastevent()) as c1," +
                    "factorPi(sb) as c2 " +
                    "from SupportBean sb";
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(epl);
            stmtSelect.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));   // factor is 3
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[] {Math.PI*2, Math.PI*2, Math.PI*3});
    
            stmtSelect.Dispose();
    
            // test local expression override
            EPStatement stmtOverride = _epService.EPAdministrator.CreateEPL("expression TwoPi {Math.PI * 10} select TwoPi() as c0 from SupportBean");
            stmtOverride.Events += listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new Object[] {Math.PI*10});
    
            // test SODA
            String eplExpr = "create expression JoinMultiplication {(s1,s2) => s1.IntPrimitive*s2.id}";
            EPStatementObjectModel modelExpr = _epService.EPAdministrator.CompileEPL(eplExpr);
            Assert.AreEqual(eplExpr, modelExpr.ToEPL());
            EPStatement stmtSODAExpr = _epService.EPAdministrator.Create(modelExpr);
            Assert.AreEqual(eplExpr, stmtSODAExpr.Text);
    
            // test SODA and join and 2-stream parameter
            String eplJoin = "select JoinMultiplication(sb,s0) from SupportBean.std:lastevent() as sb, SupportBean_S0.std:lastevent() as s0";
            EPStatementObjectModel modelJoin = _epService.EPAdministrator.CompileEPL(eplJoin);
            Assert.AreEqual(eplJoin, modelJoin.ToEPL());
            EPStatement stmtSODAJoin = _epService.EPAdministrator.Create(modelJoin);
            Assert.AreEqual(eplJoin, stmtSODAJoin.Text);
            _epService.EPAdministrator.DestroyAllStatements();
    
            // test subquery against named window and table defined in declared expression
            RunAssertionTestExpressionUse(true);
            RunAssertionTestExpressionUse(false);
        }

        private void RunAssertionTestExpressionUse(bool namedWindow)
        {
            SupportUpdateListener listener = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("create expression myexpr {(select IntPrimitive from MyInfra)}");
            String eplCreate = namedWindow ?
                    "create window MyInfra.win:keepall() as SupportBean" :
                    "create table MyInfra(TheString string, IntPrimitive int)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra select TheString, IntPrimitive from SupportBean");
            _epService.EPAdministrator.CreateEPL("select myexpr() as c0 from SupportBean_S0").AddListener(listener);
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new Object[] {100});
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }

        [Test]
        public void TestExprAndScriptLifecycleAndFilter() {
            // expression assertion
            RunAssertionLifecycleAndFilter("create expression MyFilter {sb => IntPrimitive = 1}",
                    "select * from SupportBean(MyFilter(sb)) as sb",
                    "create expression MyFilter {sb => IntPrimitive = 2}");
    
            // script assertion
            RunAssertionLifecycleAndFilter("create expression bool js:MyFilter(IntPrimitive) [IntPrimitive==1]",
                    "select * from SupportBean(MyFilter(IntPrimitive)) as sb",
                    "create expression bool js:MyFilter(IntPrimitive) [IntPrimitive==2]");
        }
    
        private void RunAssertionLifecycleAndFilter(String expressionBefore,
                                                    String selector,
                                                    String expressionAfter) {
            SupportUpdateListener l1 = new SupportUpdateListener();
            SupportUpdateListener l2 = new SupportUpdateListener();
    
            EPStatement stmtExpression = _epService.EPAdministrator.CreateEPL(expressionBefore);
    
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL(selector);
            stmtSelectOne.Events += l1.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsFalse(l1.GetAndClearIsInvoked());
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsTrue(l1.GetAndClearIsInvoked());
    
            stmtExpression.Dispose();
            _epService.EPAdministrator.CreateEPL(expressionAfter);
    
            EPStatement stmtSelectTwo = _epService.EPAdministrator.CreateEPL(selector);
            stmtSelectTwo.Events += l2.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            Assert.IsFalse(l1.GetAndClearIsInvoked() || l2.GetAndClearIsInvoked());
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            Assert.IsTrue(l1.GetAndClearIsInvoked());
            Assert.IsFalse(l2.GetAndClearIsInvoked());
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
            Assert.IsFalse(l1.GetAndClearIsInvoked());
            Assert.IsTrue(l2.GetAndClearIsInvoked());
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryInvalid(String epl, String message) {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private SupportBean MakeBean(String theString, int intPrimitive, double doublePrimitive) {
            SupportBean sb = new SupportBean();
            sb.IntPrimitive = intPrimitive;
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }
    }
}
