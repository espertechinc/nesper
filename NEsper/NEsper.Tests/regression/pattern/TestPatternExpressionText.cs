///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.pattern;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestPatternExpressionText
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>("SupportBean");
            config.AddEventType<SupportBean_A>("A");
            config.AddEventType<SupportBean_B>("B");
            config.AddEventType<SupportBean_C>("C");
            config.AddEventType<SupportBean_D>("D");
            config.AddEventType<SupportBean_E>("E");
            config.AddEventType<SupportBean_F>("F");
            config.AddEventType<SupportBean_G>("G");
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _epService.Dispose();
        }
    
        [Test]
        public void TestOp() 
        {
            RunAssertion("every a=SupportBean -> b=SupportBean@consume", null);
            RunAssertion("every a=SupportBean -> b=SupportBean@consume", null);
            RunAssertion("every a=SupportBean -> b=SupportBean@consume(2)", null);
            RunAssertion("a=A -> b=B", null);
            RunAssertion("b=B and every d=D", null);
            RunAssertion("every b=B and d=B", null);
            RunAssertion("b=B and d=D", null);
            RunAssertion("every (b=B and d=D)", null);
            RunAssertion("every (b=B and every d=D)", null);
            RunAssertion("every b=B and every d=D", null);
            RunAssertion("every (every b=B and d=D)", null);
            RunAssertion("every a=A and d=D and b=B", null);
            RunAssertion("every (every b=B and every d=D)", null);
            RunAssertion("a=A and d=D and b=B", null);
            RunAssertion("every a=A and every d=D and b=B", null);
            RunAssertion("b=B and b=B", null);
            RunAssertion("every a=A and every d=D and every b=B", null);
            RunAssertion("every (a=A and every d=D and b=B)", null);
            RunAssertion("every (b=B and b=B)", null);
            RunAssertion("every b=B", null);
            RunAssertion("b=B", null);
            RunAssertion("every (every (every b=B))", "every every every b=B");
            RunAssertion("every (every b=B())", "every every b=B");
            RunAssertion("b=B -> d=D or not d=D", null);
            RunAssertion("b=B -> (d=D or not d=D)", "b=B -> d=D or not d=D");
            RunAssertion("b=B -[1000]> d=D or not d=D", null);
            RunAssertion("b=B -> every d=D", null);
            RunAssertion("b=B -> d=D", null);
            RunAssertion("b=B -> not d=D", null);
            RunAssertion("b=B -[1000]> not d=D", null);
            RunAssertion("every b=B -> every d=D", null);
            RunAssertion("every b=B -> d=D", null);
            RunAssertion("every b=B -[10]> d=D", null);
            RunAssertion("every (b=B -> every d=D)", null);
            RunAssertion("every (a_1=A -> b=B -> a_2=A)", null);
            RunAssertion("c=C -> d=D -> a=A", null);
            RunAssertion("every (a_1=A -> b=B -> a_2=A)", null);
            RunAssertion("every (a_1=A -[10]> b=B -[10]> a_2=A)", null);
            RunAssertion("every (every a=A -> every b=B)", null);
            RunAssertion("every (a=A -> every b=B)", null);
            RunAssertion("a=A(id='A2') until D", "a=A(id=\"A2\") until D");
            RunAssertion("b=B until a=A", null);
            RunAssertion("b=B until D", null);
            RunAssertion("(a=A or b=B) until d=D", null);
            RunAssertion("(a=A or b=B) until (g=G or d=D)", null);
            RunAssertion("a=A until G", null);
            RunAssertion("[2] a=A", null);
            RunAssertion("[1:1] a=A", null);
            RunAssertion("[4] (a=A or b=B)", null);
            RunAssertion("[2] b=B until a=A", null);
            RunAssertion("[2:2] b=B until g=G", null);
            RunAssertion("[:4] b=B until g=G", null);
            RunAssertion("[1:] b=B until g=G", null);
            RunAssertion("[1:2] b=B until a=A", null);
            RunAssertion("c=C -> [2] b=B -> d=D", null);
            RunAssertion("d=D until timer:interval(7 sec)", "d=D until timer:interval(7 seconds)");
            RunAssertion("every (d=D until b=B)", null);
            RunAssertion("every d=D until b=B", null);
            RunAssertion("(every d=D) until b=B", "every d=D until b=B");
            RunAssertion("a=A until (every (timer:interval(6 sec) and not A))", "a=A until every (timer:interval(6 seconds) and not A)");
            RunAssertion("[2] (a=A or b=B)", null);
            RunAssertion("every [2] a=A", "every ([2] a=A)");
            RunAssertion("every [2] a=A until d=D", "every ([2] a=A) until d=D");  // every has precedence; ESPER-339
            RunAssertion("[3] (a=A or b=B)", null);
            RunAssertion("[4] (a=A or b=B)", null);
            RunAssertion("(a=A until b=B) until c=C", "a=A until b=B until c=C");
            RunAssertion("b=B and not d=D", null);
            RunAssertion("every b=B and not g=G", null);
            RunAssertion("every b=B and not g=G", null);
            RunAssertion("b=B and not a=A(id=\"A1\")", null);
            RunAssertion("every (b=B and not b3=B(id=\"B3\"))", null);
            RunAssertion("every (b=B or not D)", null);
            RunAssertion("every (every b=B and not B)", null);
            RunAssertion("every (b=B and not B)", null);
            RunAssertion("(b=B -> d=D) and G", null);
            RunAssertion("(b=B -> d=D) and (a=A -> e=E)", null);
            RunAssertion("b=B -> (d=D() or a=A)", "b=B -> d=D or a=A");
            RunAssertion("b=B -> ((d=D -> a=A) or (a=A -> e=E))", "b=B -> (d=D -> a=A) or (a=A -> e=E)");
            RunAssertion("(b=B -> d=D) or a=A", null);
            RunAssertion("(b=B and d=D) or a=A", "b=B and d=D or a=A");
            RunAssertion("a=A or a=A", null);
            RunAssertion("a=A or b=B or c=C", null);
            RunAssertion("every b=B or every d=D", null);
            RunAssertion("a=A or b=B", null);
            RunAssertion("a=A or every b=B", null);
            RunAssertion("every a=A or d=D", null);
            RunAssertion("every (every b=B or d=D)", null);
            RunAssertion("every (b=B or every d=D)", null);
            RunAssertion("every (every d=D or every b=B)", null);
            RunAssertion("timer:at(10,8,*,*,*)", null);
            RunAssertion("every timer:at(*/5,*,*,*,*,*)", null);
            RunAssertion("timer:at(10,9,*,*,*,10) or timer:at(30,9,*,*,*,*)", null);
            RunAssertion("b=B(id=\"B3\") -> timer:at(20,9,*,*,*,*)", null);
            RunAssertion("timer:at(59,8,*,*,*,59) -> d=D", null);
            RunAssertion("timer:at(22,8,*,*,*) -> b=B -> timer:at(55,*,*,*,*)", null);
            RunAssertion("timer:at(40,*,*,*,*,1) and b=B", null);
            RunAssertion("timer:at(40,9,*,*,*,1) or d=D", null);
            RunAssertion("timer:at(22,8,*,*,*) -> b=B -> timer:at(55,8,*,*,*)", null);
            RunAssertion("timer:at(22,8,*,*,*,1) where timer:within(30 minutes)", null);
            RunAssertion("timer:at(*,9,*,*,*) and timer:at(55,*,*,*,*)", null);
            RunAssertion("timer:at(40,8,*,*,*,1) and b=B", null);
            RunAssertion("timer:interval(2 seconds)", null);
            RunAssertion("timer:interval(2.001)", null);
            RunAssertion("timer:interval(2999 milliseconds)", null);
            RunAssertion("timer:interval(4 seconds) -> b=B", null);
            RunAssertion("b=B -> timer:interval(0)", null);
            RunAssertion("b=B -> timer:interval(6.0d) -> d=D", null);
            RunAssertion("every (b=B -> timer:interval(2.0d) -> d=D)", null);
            RunAssertion("b=B or timer:interval(2.001)", null);
            RunAssertion("b=B or timer:interval(8.5)", null);
            RunAssertion("timer:interval(8.5) or timer:interval(7.5)", null);
            RunAssertion("timer:interval(999999 milliseconds) or g=G", null);
            RunAssertion("b=B and timer:interval(4000 milliseconds)", null);
            RunAssertion("b=B(id=\"B1\") where timer:within(2 seconds)", null);
            RunAssertion("(every b=B) where timer:within(2.001)", null);
            RunAssertion("every (b=B) where timer:within(6.001)", "every b=B where timer:within(6.001)");
            RunAssertion("b=B -> d=D where timer:within(4001 milliseconds)", null);
            RunAssertion("b=B -> d=D where timer:within(4 seconds)", null);
            RunAssertion("every (b=B where timer:within(4.001) and d=D where timer:within(6.001))", null);
            RunAssertion("every b=B -> d=D where timer:within(4000 seconds)", null);
            RunAssertion("every b=B -> every d=D where timer:within(4000 seconds)", null);
            RunAssertion("b=B -> d=D where timer:within(3999 seconds)", null);
            RunAssertion("every b=B -> (every d=D) where timer:within(2001)", null);
            RunAssertion("every (b=B -> d=D) where timer:within(6001)", null);
            RunAssertion("b=B where timer:within(2000) or d=D where timer:within(6000)", null);
            RunAssertion("(b=B where timer:within(2000) or d=D where timer:within(6000)) where timer:within(1999)", null);
            RunAssertion("every (b=B where timer:within(2001) and d=D where timer:within(6001))", null);
            RunAssertion("b=B where timer:within(2001) or d=D where timer:within(6001)", null);
            RunAssertion("B where timer:within(2000) or d=D where timer:within(6001)", null);
            RunAssertion("every b=B where timer:within(2001) and every d=D where timer:within(6001)", null);
            RunAssertion("(every b=B) where timer:within(2000) and every d=D where timer:within(6001)", null);
            RunAssertion("b=B(id=\"B1\") where timer:withinmax(2 seconds,100)", null);
            RunAssertion("(every b=B) where timer:withinmax(4.001,2)", null);
            RunAssertion("every b=B where timer:withinmax(2.001,4)", null);
            RunAssertion("every (b=B where timer:withinmax(2001,0))", "every b=B where timer:withinmax(2001,0)");
            RunAssertion("(every b=B) where timer:withinmax(4.001,2)", null);
            RunAssertion("every b=B -> d=D where timer:withinmax(4000 milliseconds,1)", null);
            RunAssertion("every b=B -> every d=D where timer:withinmax(4000,1)", null);
            RunAssertion("every b=B -> (every d=D) where timer:withinmax(1 days,3)", null);
            RunAssertion("a=A -> (every b=B) while (b.id!=\"B3\")", null);
            RunAssertion("(every b=B) while (b.id!=\"B1\")", null);
            RunAssertion("every-distinct(a.IntPrimitive,1) a=SupportBean(TheString like \"A%\")", null);
            RunAssertion("every-distinct(a.IntPrimitive,1 seconds) a=SupportBean(TheString like \"A%\")", null);
            RunAssertion("every-distinct(IntPrimitive) a=SupportBean", null);
            RunAssertion("[2] every-distinct(a.IntPrimitive) a=SupportBean", null);
            RunAssertion("every-distinct(a[0].IntPrimitive) ([2] a=SupportBean)", null);
            RunAssertion("every-distinct(a[0].IntPrimitive,a[0].IntPrimitive,1 hours) ([2] a=SupportBean)", null);
            RunAssertion("(every-distinct(a.IntPrimitive) a=SupportBean) where timer:within(10 seconds)", null);
            RunAssertion("every-distinct(a.IntPrimitive) a=SupportBean where timer:within(10)", null);
            RunAssertion("every-distinct(a.IntPrimitive,1 hours) a=SupportBean where timer:within(10)", null);
            RunAssertion("every-distinct(a.IntPrimitive,b.IntPrimitive) (a=SupportBean(TheString like \"A%\") and b=SupportBean(TheString like \"B%\"))", null);
            RunAssertion("every-distinct(a.IntPrimitive) (a=SupportBean and not SupportBean)", null);
            RunAssertion("every-distinct(a.IntPrimitive,1 hours) (a=SupportBean and not SupportBean)", null);
            RunAssertion("every-distinct(a.IntPrimitive+b.IntPrimitive,1 hours) (a=SupportBean -> b=SupportBean)", null);
            RunAssertion("every-distinct(a.IntPrimitive) a=SupportBean -> b=SupportBean(IntPrimitive=a.IntPrimitive)", null);
            RunAssertion("every-distinct(a.IntPrimitive) a=SupportBean -> every-distinct(b.IntPrimitive) b=SupportBean(TheString like \"B%\")", null);
        }
    
        private void RunAssertion(string patternText, string expectedIfDifferent) {
            var epl = "@Name('A') select * from pattern [" + patternText + "]";
            RunAssertionEPL(epl, patternText, expectedIfDifferent);
    
            epl = "@Audit @Name('A') select * from pattern [" + patternText + "]";
            RunAssertionEPL(epl, patternText, expectedIfDifferent);
        }
    
        private void RunAssertionEPL(string epl, string patternText, string expectedIfDifferent) {
            var model = _epService.EPAdministrator.CompileEPL(epl);
            var spi = (EPStatementSPI) _epService.EPAdministrator.Create(model);
            var spec = ((EPServiceProviderSPI) (_epService)).StatementLifecycleSvc.GetStatementSpec(spi.StatementId);
            var pattern = (PatternStreamSpecCompiled) spec.StreamSpecs[0];
            var writer = new StringWriter();
            pattern.EvalFactoryNode.ToEPL(writer, PatternExpressionPrecedenceEnum.MINIMUM);
            if (expectedIfDifferent == null) {
                Assert.AreEqual(patternText, writer.ToString());
            }
            else {
                Assert.AreEqual(expectedIfDifferent, writer.ToString());
            }
            spi.Dispose();
        }
    }
}
