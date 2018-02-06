///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextWDeclaredExpression
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType<SupportBean>();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestDeclaredExpression() {
            _epService.EPAdministrator.CreateEPL("create context MyCtx as " +
                    "group by IntPrimitive < 0 as n, " +
                    "group by IntPrimitive > 0 as p " +
                    "from SupportBean");
            _epService.EPAdministrator.CreateEPL("create expression getLabelOne { context.label }");
            _epService.EPAdministrator.CreateEPL("create expression getLabelTwo { 'x'||context.label||'x' }");

            _epService.EPAdministrator.CreateEPL("expression getLabelThree { context.label } " +
                    "context MyCtx " +
                    "select getLabelOne() as c0, getLabelTwo() as c1, getLabelThree() as c2 from SupportBean")
                    .AddListener(_listener);

            RunAssertionExpression();
        }

        [Test]
        public void TestAliasExpression() {
            _epService.EPAdministrator.CreateEPL("create context MyCtx as " +
                    "group by IntPrimitive < 0 as n, " +
                    "group by IntPrimitive > 0 as p " +
                    "from SupportBean");
            _epService.EPAdministrator.CreateEPL("create expression getLabelOne alias for { context.label }");
            _epService.EPAdministrator.CreateEPL("create expression getLabelTwo alias for { 'x'||context.label||'x' }");
            _epService.EPAdministrator.CreateEPL("expression getLabelThree alias for { context.label } " +
                    "context MyCtx " +
                    "select getLabelOne as c0, getLabelTwo as c1, getLabelThree as c2 from SupportBean")
                    .AddListener(_listener);

            RunAssertionExpression();
        }

        [Test]
        public void TestContextFilter()
        {
            var expr = "create expression THE_EXPRESSION alias for {theString='x'}";
            _epService.EPAdministrator.CreateEPL(expr);

            var context = "create context context2 initiated @now and pattern[every(SupportBean(THE_EXPRESSION))] terminated after 10 minutes";
            _epService.EPAdministrator.CreateEPL(context);

            var listener = new SupportUpdateListener();
            var statement = "context context2 select * from pattern[e1=SupportBean(THE_EXPRESSION) -> e2=SupportBean(theString='y')]";
            _epService.EPAdministrator.CreateEPL(statement).AddListener(listener);

            _epService.EPRuntime.SendEvent(new SupportBean("x", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("y", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "e1.intPrimitive,e2.intPrimitive".SplitCsv(), new object[] { 1, 2 });
        }

        private void RunAssertionExpression()
        {
            var fields = "c0,c1,c2".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"n", "xnx", "n"});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"p", "xpx", "p"});
        }
    }
}
