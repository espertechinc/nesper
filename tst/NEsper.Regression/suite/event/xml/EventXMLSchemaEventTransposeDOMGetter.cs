///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTransposeDOMGetter
    {
        public static IList<RegressionExecution> Executions()
        {
            List<RegressionExecution> execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTransposeDOMGetterCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTransposeDOMGetterPreconfig());
            return execs;
        }

        public class EventXMLSchemaEventTransposeDOMGetterPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "SimpleEventWSchema", new RegressionPath());
            }
        }

        public class EventXMLSchemaEventTransposeDOMGetterCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var resourceManager = env.Container.ResourceManager();
                var schemaUriSimpleSchema = resourceManager.ResolveResourceURL("regression/simpleSchema.xsd");
                string epl = "@public @buseventtype " +
                             "@XMLSchema(RootElementName='simpleEvent', SchemaResource='" +
                             schemaUriSimpleSchema +
                             "')" +
                             "create xml schema MyEventCreateSchema()";
                RegressionPath path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            String eventTypeName,
            RegressionPath path)
        {
            env.CompileDeploy("@Name('s0') insert into MyNestedStream select nested1 from " + eventTypeName + "#lastevent", path);

            SupportEventPropUtil.AssertPropsEquals(
                env.Statement("s0").EventType.PropertyDescriptors,
                new SupportEventPropDesc("nested1", typeof(XmlNode))
                    .WithFragment());
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").EventType);

            env.CompileDeploy(
                "@Name('s1') select nested1.attr1 as attr1, nested1.prop1 as prop1, nested1.prop2 as prop2, nested1.nested2.prop3 as prop3, nested1.nested2.prop3[0] as prop3_0, nested1.nested2 as nested2 from MyNestedStream#lastevent",
                path);
            SupportEventPropUtil.AssertPropsEquals(
                env.Statement("s1").EventType.PropertyDescriptors,
                new SupportEventPropDesc("prop1", typeof(string)).WithComponentType(typeof(char)),
                new SupportEventPropDesc("prop2", typeof(bool?)),
                new SupportEventPropDesc("attr1", typeof(string)).WithComponentType(typeof(char)),
                new SupportEventPropDesc("prop3", typeof(int?[])).WithComponentType(typeof(int?)).WithIndexed(),
                new SupportEventPropDesc("prop3_0", typeof(int?)),
                new SupportEventPropDesc("nested2", typeof(XmlNode)).WithFragment());
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s1").EventType);

            env.CompileDeploy("@Name('sw') select * from MyNestedStream", path);
            SupportEventPropUtil.AssertPropsEquals(
                env.Statement("sw").EventType.PropertyDescriptors,
                new SupportEventPropDesc("nested1", typeof(XmlNode)).WithFragment());
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("sw").EventType);

            env.CompileDeploy(
                "@Name('iw') insert into MyNestedStreamTwo select nested1.* from " + eventTypeName + "#lastevent",
                path);
            SupportEventPropUtil.AssertPropsEquals(
                env.Statement("iw").EventType.PropertyDescriptors,
                new SupportEventPropDesc("prop1", typeof(string)).WithComponentType(typeof(char)).WithIndexed(),
                new SupportEventPropDesc("prop2", typeof(bool?)),
                new SupportEventPropDesc("attr1", typeof(string)).WithComponentType(typeof(char)).WithIndexed(),
                new SupportEventPropDesc("nested2", typeof(XmlNode)).WithFragment());
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("iw").EventType);

            SupportXML.SendDefaultEvent(env.EventService, "test", eventTypeName);
            var stmtInsertWildcardBean = env.GetEnumerator("iw").Advance();
            EPAssertionUtil.AssertProps(
                stmtInsertWildcardBean,
                new[] {"prop1", "prop2", "attr1"},
                new object[] {"SAMPLE_V1", true, "SAMPLE_ATTR1"});

            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("s0").Advance());
            var stmtInsertBean = env.GetEnumerator("s0").Advance();
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("iw").Advance());
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("sw").Advance());

            var fragmentNested1 = (EventBean) stmtInsertBean.GetFragment("nested1");
            Assert.AreEqual(5, fragmentNested1.Get("nested2.prop3[2]"));
            Assert.AreEqual(eventTypeName + ".nested1", fragmentNested1.EventType.Name);

            var fragmentNested2 = (EventBean) stmtInsertWildcardBean.GetFragment("nested2");
            Assert.AreEqual(4, fragmentNested2.Get("prop3[1]"));
            Assert.AreEqual(eventTypeName + ".nested1.nested2", fragmentNested2.EventType.Name);

            env.UndeployAll();
        }
    }
} // end of namespace