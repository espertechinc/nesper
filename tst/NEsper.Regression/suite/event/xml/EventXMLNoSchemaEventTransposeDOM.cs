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

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLNoSchemaEventTransposeDOM
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithPreconfig(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLNoSchemaEventXMLCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfig(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
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
            string eventTypeName,
            RegressionPath path)
        {
            env.CompileDeploy("@name('insert') insert into MyNestedStream select nested1 from " + eventTypeName, path);
            env.AssertStatement(
                "insert",
                statement => {
                    SupportEventPropUtil.AssertPropsEquals(
                        statement.EventType.PropertyDescriptors.ToArray(),
                        new SupportEventPropDesc("nested1", typeof(string)));
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                });

            env.CompileDeploy("@name('s0') select * from " + eventTypeName, path);
            env.AssertStatement(
                "s0",
                statement => {
                    EPAssertionUtil.AssertEqualsAnyOrder(
                        Array.Empty<object>(),
                        statement.EventType.PropertyDescriptors.ToArray());
                    SupportEventTypeAssertionUtil.AssertConsistency(statement.EventType);
                });

            var doc = SupportXML.MakeDefaultEvent("test");
            env.SendEventXMLDOM(doc, eventTypeName);

            env.AssertIterator(
                "insert",
                iterator => {
                    var stmtInsertWildcardBean = iterator.Advance();
                    ClassicAssert.IsNotNull(stmtInsertWildcardBean.Get("nested1"));
                    SupportEventTypeAssertionUtil.AssertConsistency(stmtInsertWildcardBean);
                });
            env.AssertIterator(
                "s0",
                iterator => {
                    var stmtSelectWildcardBean = iterator.Advance();
                    SupportEventTypeAssertionUtil.AssertConsistency(stmtSelectWildcardBean);
                    ClassicAssert.AreEqual(0, stmtSelectWildcardBean.EventType.PropertyNames.Length);
                });

            env.UndeployAll();
        }
    }
} // end of namespace