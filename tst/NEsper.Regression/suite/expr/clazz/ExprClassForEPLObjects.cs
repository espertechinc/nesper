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
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil; // assertMessage
using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.expr.clazz
{
    public class ExprClassForEPLObjects
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithFromClauseMethod(execs);
            WithOutputColType(execs);
            WithInvalid(execs);
            WithScript(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithScript(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprClassResolutionScript());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprClassResolutionInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithOutputColType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprClassResolutionOutputColType());
            return execs;
        }

        public static IList<RegressionExecution> WithFromClauseMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprClassResolutionFromClauseMethod());
            return execs;
        }

        private class ExprClassResolutionOutputColType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "inlined_class \"\"\"\n" +
                    "  public class MyBean {\n" +
                    "    public MyBean(int id) {this.Id = id;}\n" +
                    "    public int Id { get; set; }\n" +
                    "    public static MyBean GetBean(int id) {return new MyBean(id);}\n" +
                    "  }\n" +
                    "\"\"\" \n" +
                    "@name('s0') select MyBean.GetBean(IntPrimitive) as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement =>
                        Assert.AreEqual("MyBean", statement.EventType.GetPropertyType("c0").Name));

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var result = @event.Get("c0");
                        try {
                            Assert.AreEqual(10, result.GetType().GetProperty("Id")?.GetValue(result));
                        }
                        catch (Exception t) {
                            Assert.Fail(t.Message);
                        }
                    });

                env.UndeployAll();
            }
        }

        private class ExprClassResolutionInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test Annotation
                var eplAnnotation =
                    EscapeClass("public class MyAnnotationAttribute : System.Attribute {}") +
                    "@MyAnnotation @Name('s0') select * from SupportBean\n";

                env.TryInvalidCompile(
                    eplAnnotation,
                    "Failed to process statement annotations: Failed to resolve @-annotation class: Could not load annotation class by name 'MyAnnotation', please check imports");

                // test Create-Schema for bean type
                var eplBeanEventType = EscapeClass("public class MyEventBean {}") +
                                       "create schema MyEvent as MyEventBean\n";
                env.TryInvalidCompile(
                    eplBeanEventType,
                    "Could not load class by name 'MyEventBean', please check imports");

                // test Create-Schema for property type
                var eplPropertyType = EscapeClass("public class MyEventBean {}") +
                                      "create schema MyEvent as (field1 MyEventBean)\n";
                env.TryInvalidCompile(
                    eplPropertyType,
                    "Nestable type configuration encountered an unexpected property type name");

                var eplNamedWindow = EscapeClass("public class MyType {}") +
                                     "create window MyWindow(myfield MyType)\n";
                env.TryInvalidCompile(
                    eplNamedWindow,
                    "Nestable type configuration encountered an unexpected property type name");

                var eplTable = EscapeClass("public class MyType {}") +
                               "create table MyTable(myfield MyType)\n";
                env.TryInvalidCompile(eplTable, "skip");
            }
        }

        private class ExprClassResolutionScript : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplScript = EscapeClass(
                                    "public class MyScriptResult {}") +
                                "expression Object[] js:myItemProducerScript() [\n" +
                                "function myItemProducerScript() {" +
                                "  var arrayType = host.resolveType(\"MyScriptResult\");\n" +
                                "  var rows = host.newArr(arrayType, 2);\n" +
                                "  return rows;\n" +
                                "};" +
                                "return myItemProducerScript();" +
                                "]" +
                                "@name('s0') select myItemProducerScript() from SupportBean";
                env.CompileDeploy(eplScript).AddListener("s0");

                env.AssertThat(
                    () => {
                        try {
                            env.SendEventBean(new SupportBean("E1", 1));
                            Assert.Fail();
                        }
                        catch (EPException ex) {
                            AssertMessage(ex, "java.lang.RuntimeException: Unexpected exception in statement 's0'");
                        }
                    });

                env.UndeployAll();
            }
        }

        private class ExprClassResolutionFromClauseMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCreateClass =
                    "@public create inlined_class \"\"\"\n" +
                    "  public class MyFromClauseMethod {\n" +
                    "    public static MyBean[] GetBeans() {\n" +
                    "       return new MyBean[] {\n" +
                    "           new MyBean(1),\n" +
                    "           new MyBean(2)\n" +
                    "       };\n" +
                    "    }\n" +
                    "    public class MyBean {\n" +
                    "      public MyBean(int id) {Id = id;}\n" +
                    "      public int Id { get; set; }\n" +
                    "    }\n" +
                    "  }\n" +
                    "\"\"\" \n";
                env.CompileDeploy(eplCreateClass, path);

                var epl =
                    "@name('s0')" +
                    "@name('s0') select s.Id as c0 from SupportBean as e,\n" +
                    "method:MyFromClauseMethod.GetBeans() as s";

                var compiled = env.Compile(epl, path);
                var assemblies = compiled.Assemblies;
                var assemblyTypes = assemblies.SelectMany(_ => _.GetExportedTypes());
                foreach (var assemblyType in assemblyTypes) {
                    if (assemblyType.Name.Contains("MyFromClauseMethod")) {
                        Assert.Fail("EPCompiled should not contain create-class class");
                    }
                }

                env.Deploy(compiled).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    "c0".SplitCsv(),
                    new object[][] { new object[] { 1 }, new object[] { 2 } });

                env.UndeployAll();
            }
        }

        private static string EscapeClass(string text)
        {
            return "inlined_class \"\"\"\n" + text + "\"\"\" \n";
        }
    }
} // end of namespace