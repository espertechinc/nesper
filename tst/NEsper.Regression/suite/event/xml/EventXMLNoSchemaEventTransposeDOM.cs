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

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaEventTransposeDOM
    {
        public static List<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaEventXMLCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaEventXMLPreconfig());
            return execs;
        }

        public class EventXMLNoSchemaEventXMLPreconfig : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "TestXMLJustRootElementType", new RegressionPath());
            }
        }

        public class EventXMLNoSchemaEventXMLCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='simpleEvent')" +
                          "create xml schema MyEventCreateSchema()";
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                RunAssertion(env, "MyEventCreateSchema", path);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            String eventTypeName,
            RegressionPath path)
        {
            env.CompileDeploy("@Name('insert') insert into MyNestedStream select nested1 from " + eventTypeName, path);

            SupportEventPropUtil.AssertPropsEquals(
                env.Statement("insert").EventType.PropertyDescriptors.ToArray(),
                new SupportEventPropDesc("nested1", typeof(string))
                    .WithIndexed()
                    .WithComponentType(typeof(char))
                );
                
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("insert").EventType);

            env.CompileDeploy("@Name('s0') select * from " + eventTypeName, path);
            CollectionAssert.AreEquivalent(new EventPropertyDescriptor[0], env.Statement("s0").EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(env.Statement("s0").EventType);

            SupportXML.SendDefaultEvent(env.EventService, "test", eventTypeName);
            var stmtInsertWildcardBean = env.GetEnumerator("insert").Advance();
            var stmtSelectWildcardBean = env.GetEnumerator("s0").Advance();
            Assert.IsNotNull(stmtInsertWildcardBean.Get("nested1"));
            SupportEventTypeAssertionUtil.AssertConsistency(stmtSelectWildcardBean);
            SupportEventTypeAssertionUtil.AssertConsistency(env.GetEnumerator("insert").Advance());

            Assert.AreEqual(0, stmtSelectWildcardBean.EventType.PropertyNames.Length);

            env.UndeployAll();
        }
    }
} // end of namespace