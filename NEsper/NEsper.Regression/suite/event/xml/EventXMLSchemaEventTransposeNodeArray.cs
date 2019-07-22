///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTransposeNodeArray : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // try array property insert
            env.CompileDeploy("@Name('s0') select nested3.nested4 as narr from SimpleEventWSchema#lastevent");
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor(
                        "narr",
                        typeof(XmlNode[]),
                        typeof(XmlNode),
                        false,
                        false,
                        true,
                        false,
                        true)
                },
                env.Statement("s0").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").EventType);

            SupportXML.SendDefaultEvent(env.EventService, "test", "SimpleEventWSchema");

            var result = env.Statement("s0").First();
            SupportEventTypeAssertionUtil.AssertConsistency(result);
            var fragments = (EventBean[]) result.GetFragment("narr");
            Assert.AreEqual(3, fragments.Length);
            Assert.AreEqual("SAMPLE_V8", fragments[0].Get("prop5[1]"));
            Assert.AreEqual("SAMPLE_V11", fragments[2].Get("prop5[1]"));

            var fragmentItem = (EventBean) result.GetFragment("narr[2]");
            Assert.AreEqual("SimpleEventWSchema.nested3.nested4", fragmentItem.EventType.Name);
            Assert.AreEqual("SAMPLE_V10", fragmentItem.Get("prop5[0]"));

            // try array index property insert
            env.CompileDeploy("@Name('ii') select nested3.nested4[1] as narr from SimpleEventWSchema#lastevent");
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("narr", typeof(XmlNode), null, false, false, false, false, true)
                },
                env.Statement("ii").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("ii").EventType);

            SupportXML.SendDefaultEvent(env.EventService, "test", "SimpleEventWSchema");

            var resultItem = env.GetEnumerator("ii").Advance();
            Assert.AreEqual("b", resultItem.Get("narr.Id"));
            SupportEventTypeAssertionUtil.AssertConsistency(resultItem);
            var fragmentsInsertItem = (EventBean) resultItem.GetFragment("narr");
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentsInsertItem);
            Assert.AreEqual("b", fragmentsInsertItem.Get("Id"));
            Assert.AreEqual("SAMPLE_V9", fragmentsInsertItem.Get("prop5[0]"));

            env.UndeployAll();
        }
    }
} // end of namespace