///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaWithRestriction
    {
        public static List<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaWithRestrictionPreconfig());
            execs.Add(new EventXMLSchemaWithRestrictionCreateSchema());
            return execs;
        }

        public class EventXMLSchemaWithRestrictionPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "OrderEvent", new RegressionPath());
            }
        }

        public class EventXMLSchemaWithRestrictionCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var resourceManager = env.Container.ResourceManager();
                var schemaStream = resourceManager.GetResourceAsStream("regression/simpleSchemaWithRestriction.xsd");
                Assert.IsNotNull(schemaStream);
                var schemaTextSimpleSchemaWithRestriction = schemaStream.ConsumeStream();
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='order', SchemaText='" + schemaTextSimpleSchemaWithRestriction + "')" +
                          "create xml schema MyEventCreateSchema()";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            string eventTypeName,
            RegressionPath path)
        {
            var text = "@Name('s0') select order_amount from " + eventTypeName;
            env.CompileDeploy(text, path).AddListener("s0");

            SupportXML.SendXMLEvent(
                env,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<order>\n" +
                "<order_amount>202.1</order_amount>" +
                "</order>",
                "OrderEvent");
            var theEvent = env.Listener("s0").LastNewData[0];
            Assert.AreEqual(typeof(double), theEvent.Get("order_amount").GetType());
            Assert.AreEqual(202.1d, theEvent.Get("order_amount"));
            env.Listener("s0").Reset();

            env.UndeployAll();
        }
    }
} // end of namespace