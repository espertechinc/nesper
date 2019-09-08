///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTransposeDOMGetter : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@Name('s0') insert into MyNestedStream select nested1 from SimpleEventWSchema#lastevent",
                path);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, true)
                },
                env.Statement("s0").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").EventType);

            env.CompileDeploy(
                "@Name('s1') select nested1.attr1 as attr1, nested1.prop1 as prop1, nested1.prop2 as prop2, nested1.Nested2.prop3 as prop3, nested1.Nested2.prop3[0] as prop3_0, nested1.Nested2 as nested2 from MyNestedStream#lastevent",
                path);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("prop1", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor("prop2", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("attr1", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor(
                        "prop3",
                        typeof(int?[]),
                        typeof(int?),
                        false,
                        false,
                        true,
                        false,
                        false),
                    new EventPropertyDescriptor("prop3_0", typeof(int?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("nested2", typeof(XmlNode), null, false, false, false, false, true)
                },
                env.Statement("s1").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s1").EventType);

            env.CompileDeploy("@Name('sw') select * from MyNestedStream", path);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, true)
                },
                env.Statement("sw").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("sw").EventType);

            env.CompileDeploy(
                "@Name('iw') insert into MyNestedStreamTwo select nested1.* from SimpleEventWSchema#lastevent",
                path);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {
                    new EventPropertyDescriptor("prop1", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor("prop2", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("attr1", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor("nested2", typeof(XmlNode), null, false, false, false, false, true)
                },
                env.Statement("iw").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("iw").EventType);

            SupportXML.SendDefaultEvent(env.EventService, "test", "SimpleEventWSchema");
            var stmtInsertWildcardBean = env.GetEnumerator("iw").Advance();
            EPAssertionUtil.AssertProps(
                stmtInsertWildcardBean,
                new [] { "prop1","prop2","attr1" },
                new object[] {"SAMPLE_V1", true, "SAMPLE_ATTR1"});

            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("s0").Advance());
            var stmtInsertBean = env.GetEnumerator("s0").Advance();
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("iw").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("sw").Advance());

            var fragmentNested1 = (EventBean) stmtInsertBean.GetFragment("nested1");
            Assert.AreEqual(5, fragmentNested1.Get("nested2.prop3[2]"));
            Assert.AreEqual("SimpleEventWSchema.Nested1", fragmentNested1.EventType.Name);

            var fragmentNested2 = (EventBean) stmtInsertWildcardBean.GetFragment("nested2");
            Assert.AreEqual(4, fragmentNested2.Get("prop3[1]"));
            Assert.AreEqual("SimpleEventWSchema.Nested1.Nested2", fragmentNested2.EventType.Name);

            env.UndeployAll();
        }
    }
} // end of namespace