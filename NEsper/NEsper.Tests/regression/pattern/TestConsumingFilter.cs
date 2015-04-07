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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestConsumingFilter : SupportBeanConstants
    {
        private EPServiceProvider _engine;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _engine = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _listener = new SupportUpdateListener();
            _engine.Initialize();
            _engine.EPAdministrator.Configuration.AddEventType<SupportBean>("SupportBean");
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestFollowedBy()
        {
            String[] fields = "a,b".Split(',');
            String pattern = "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean -> b=SupportBean@consume]";
            _engine.EPAdministrator.CreateEPL(pattern).Events += _listener.Update;
    
            _engine.EPRuntime.SendEvent(new SupportBean("E1", 0));
            _engine.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", "E2"});
    
            _engine.EPRuntime.SendEvent(new SupportBean("E3", 0));
            _engine.EPRuntime.SendEvent(new SupportBean("E4", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E3", "E4"});
    
            _engine.EPRuntime.SendEvent(new SupportBean("E5", 0));
            _engine.EPRuntime.SendEvent(new SupportBean("E6", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E5", "E6"});
        }
    
        [Test]
        public void TestAnd()
        {
            String[] fields = "a,b".Split(',');
            String pattern = "select a.TheString as a, b.TheString as b from pattern[every (a=SupportBean and b=SupportBean)]";
            _engine.EPAdministrator.CreateEPL(pattern).Events += _listener.Update;
    
            _engine.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", "E1"});
            _engine.EPAdministrator.DestroyAllStatements();
    
            pattern = "select a.TheString as a, b.TheString as b from pattern [every (a=SupportBean and b=SupportBean(IntPrimitive=10)@consume(2))]";
            _engine.EPAdministrator.CreateEPL(pattern).Events += _listener.Update;
    
            _engine.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _engine.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2", "E1"});
    
            _engine.EPRuntime.SendEvent(new SupportBean("E3", 1));
            _engine.EPRuntime.SendEvent(new SupportBean("E4", 1));
            _engine.EPRuntime.SendEvent(new SupportBean("E5", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E3", "E5"});
            _engine.EPAdministrator.DestroyAllStatements();
            
            // test SODA
            EPStatementObjectModel model = _engine.EPAdministrator.CompileEPL(pattern);
            Assert.AreEqual(pattern, model.ToEPL());
            EPStatement stmt = _engine.EPAdministrator.Create(model);
            Assert.AreEqual(pattern, stmt.Text);
            stmt.Events += _listener.Update;
    
            _engine.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _engine.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2", "E1"});
        }
    
        [Test]
        public void TestOr() {
            String[] fields = "a,b".Split(',');
            RunAssertion(fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean or b=SupportBean] order by a asc",
                    new Object[][] { new Object[] { null, "E1" }, new Object[] { "E1", null } });
    
            RunAssertion(fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean@consume(1) or every b=SupportBean@consume(1)] order by a asc",
                    new Object[][] { new Object[] { null, "E1" }, new Object[] { "E1", null } });
    
            RunAssertion(fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean@consume(2) or b=SupportBean@consume(1)] order by a asc",
                    new Object[]{"E1", null});
    
            RunAssertion(fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean@consume(1) or b=SupportBean@consume(2)] order by a asc",
                    new Object[]{null, "E1"});
    
            RunAssertion(fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean or b=SupportBean@consume(2)] order by a asc",
                    new Object[]{null, "E1"});
    
            RunAssertion(fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean@consume(1) or b=SupportBean] order by a asc",
                    new Object[]{"E1", null});
    
            RunAssertion(fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean(IntPrimitive=11)@consume(1) or b=SupportBean] order by a asc",
                    new Object[]{null, "E1"});
    
            RunAssertion(fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean(IntPrimitive=10)@consume(1) or b=SupportBean] order by a asc",
                    new Object[]{"E1", null});
    
            fields = "a,b,c".Split(',');
            RunAssertion(fields, "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(1) or b=SupportBean@consume(2) or c=SupportBean@consume(3)] order by a,b,c",
                    new Object[][] { new Object[] { null, null, "E1" } });
    
            RunAssertion(fields, "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(1) or every b=SupportBean@consume(2) or every c=SupportBean@consume(2)] order by a,b,c",
                    new Object[][] { new Object[] { null, null, "E1" }, new Object[] { null, "E1", null } });
    
            RunAssertion(fields, "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(2) or every b=SupportBean@consume(2) or every c=SupportBean@consume(2)] order by a,b,c",
                    new Object[][] { new Object[] { null, null, "E1" }, new Object[] { null, "E1", null }, new Object[] { "E1", null, null } });
    
            RunAssertion(fields, "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(2) or every b=SupportBean@consume(2) or every c=SupportBean@consume(1)] order by a,b,c",
                    new Object[][] { new Object[] { null, "E1", null }, new Object[] { "E1", null, null } });
    
            RunAssertion(fields, "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(2) or every b=SupportBean@consume(1) or every c=SupportBean@consume(2)] order by a,b,c",
                    new Object[][] { new Object[] { null, null, "E1" }, new Object[] { "E1", null, null } });
    
            RunAssertion(fields, "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(0) or every b=SupportBean or every c=SupportBean] order by a,b,c",
                    new Object[][] { new Object[] { null, null, "E1" }, new Object[] { null, "E1", null }, new Object[] { "E1", null, null } });
        }
    
        [Test]
        public void TestInvalid()
        {
            TryInvalid("select * from pattern[every a=SupportBean@consume()]",
                    "Incorrect syntax near ')' expecting any of the following tokens {IntegerLiteral, FloatingPointLiteral} but found a closing parenthesis ')' at line 1 column 50, please check the filter specification within the pattern expression within the from clause [select * from pattern[every a=SupportBean@consume()]]");
            TryInvalid("select * from pattern[every a=SupportBean@consume(-1)]",
                    "Incorrect syntax near '-' expecting any of the following tokens {IntegerLiteral, FloatingPointLiteral} but found a minus '-' at line 1 column 50, please check the filter specification within the pattern expression within the from clause [select * from pattern[every a=SupportBean@consume(-1)]]");
            TryInvalid("select * from pattern[every a=SupportBean@xx]",
                    "Error in expression: Unexpected pattern filter @ annotation, expecting 'consume' but received 'xx' [select * from pattern[every a=SupportBean@xx]]");
        }
    
        private void TryInvalid(String epl, String message) {
            try {
                _engine.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                // expected
                Assert.AreEqual(message, ex.Message);
            }
        }
        
        private void RunAssertion(String[] fields, String pattern, Object expected) {
            _engine.EPAdministrator.DestroyAllStatements();
            _engine.EPAdministrator.CreateEPL(pattern).Events += _listener.Update;
            _engine.EPRuntime.SendEvent(new SupportBean("E1", 10));
    
            if (expected is Object[][]) {
                EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, (Object[][]) expected);
            }
            else {
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, (Object[]) expected);
            }
        }
    }
    
    
}
