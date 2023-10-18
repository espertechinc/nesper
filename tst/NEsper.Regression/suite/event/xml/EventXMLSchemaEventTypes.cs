///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.xml
{
    public class EventXMLSchemaEventTypes
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithPreconfigured(execs);
            WithCreateSchema(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchema(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTypesCreateSchema());
            return execs;
        }

        public static IList<RegressionExecution> WithPreconfigured(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventXMLSchemaEventTypesPreconfigured());
            return execs;
        }

        private class EventXMLSchemaEventTypesPreconfigured : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, "TestTypesEvent", new RegressionPath());
            }
        }

        public class EventXMLSchemaEventTypesCreateSchema : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var resourceManager = env.Container.ResourceManager();
                var schemaUriTypeTestSchema = resourceManager.ResolveResourceURL("regression/typeTestSchema.xsd");
                var epl = "@public @buseventtype " +
                          "@XMLSchema(RootElementName='typesEvent', SchemaResource='" +
                          schemaUriTypeTestSchema +
                          "')" +
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
            var stmtSelectWild = "@name('s0') select * from " + eventTypeName;
            env.CompileDeploy(stmtSelectWild, path).AddListener("s0");

            env.AssertStatement(
                "s0",
                statement => {
                    var type = statement.EventType;
                    SupportEventTypeAssertionUtil.AssertConsistency(type);

                    object[][] types = {
                        new object[] { "attrNonPositiveInteger", typeof(int?) },
                        new object[] { "attrNonNegativeInteger", typeof(int?) },
                        new object[] { "attrNegativeInteger", typeof(int?) },
                        new object[] { "attrPositiveInteger", typeof(int?) },
                        new object[] { "attrLong", typeof(long?) },
                        new object[] { "attrUnsignedLong", typeof(ulong?) },
                        new object[] { "attrInt", typeof(int?) },
                        new object[] { "attrUnsignedInt", typeof(uint?) },
                        new object[] { "attrDecimal", typeof(double?) },
                        new object[] { "attrInteger", typeof(int?) },
                        new object[] { "attrFloat", typeof(float?) },
                        new object[] { "attrDouble", typeof(double?) },
                        new object[] { "attrString", typeof(string) },
                        new object[] { "attrShort", typeof(short?) },
                        new object[] { "attrUnsignedShort", typeof(ushort?) },
                        new object[] { "attrByte", typeof(byte?) },
                        new object[] { "attrUnsignedByte", typeof(byte?) },
                        new object[] { "attrBoolean", typeof(bool?) },
                        new object[] { "attrDateTime", typeof(string) },
                        new object[] { "attrDate", typeof(string) },
                        new object[] { "attrTime", typeof(string) }
                    };

                    for (var i = 0; i < types.Length; i++) {
                        var name = types[i][0].ToString();
                        var desc = type.GetPropertyDescriptor(name);
                        var expected = (Type)types[i][1];
                        Assert.AreEqual(expected, desc.PropertyType, "Failed for " + name);
                    }
                });
            
            env.UndeployAll();
        }
    }
} // end of namespace