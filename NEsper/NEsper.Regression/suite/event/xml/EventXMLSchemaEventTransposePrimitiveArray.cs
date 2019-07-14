///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTransposePrimitiveArray : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // try array property in select
            env.CompileDeploy("@Name('s0') select * from TestNested2#lastevent").AddListener("s0");

            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("prop3", typeof(int?[]), null, false, false, true, false, false)
                },
                env.Statement("s0").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").EventType);

            var sender = env.EventService.GetEventSender("TestNested2");
            sender.SendEvent(
                SupportXML.GetDocument("<nested2><prop3>2</prop3><prop3></prop3><prop3>4</prop3></nested2>"));
            var theEvent = env.GetEnumerator("s0").Advance();
            EPAssertionUtil.AssertEqualsExactOrder(
                theEvent.Get("prop3").Unwrap<object>(),
                new object[] {2, null, 4});
            SupportEventTypeAssertionUtil.AssertConsistency(theEvent);
            env.UndeployModuleContaining("s0");

            // try array property nested
            env.CompileDeploy("@Name('s0') select nested3.* from ABCType#lastevent");
            SupportXML.SendDefaultEvent(env.EventService, "test", "ABCType");
            var stmtSelectResult = env.GetEnumerator("s0").Advance();
            SupportEventTypeAssertionUtil.AssertConsistency(stmtSelectResult);
            Assert.AreEqual(typeof(string[]), stmtSelectResult.EventType.GetPropertyType("nested4[2].prop5"));
            Assert.AreEqual("SAMPLE_V8", stmtSelectResult.Get("nested4[0].prop5[1]"));
            EPAssertionUtil.AssertEqualsExactOrder(
                (string[]) stmtSelectResult.Get("nested4[2].prop5"),
                new object[] {"SAMPLE_V10", "SAMPLE_V11"});

            var fragmentNested4 = (EventBean) stmtSelectResult.GetFragment("nested4[2]");
            EPAssertionUtil.AssertEqualsExactOrder(
                (string[]) fragmentNested4.Get("prop5"),
                new object[] {"SAMPLE_V10", "SAMPLE_V11"});
            Assert.AreEqual("SAMPLE_V11", fragmentNested4.Get("prop5[1]"));
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentNested4);

            env.UndeployAll();
        }
    }
} // end of namespace