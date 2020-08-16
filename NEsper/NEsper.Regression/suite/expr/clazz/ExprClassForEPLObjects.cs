///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.clazz
{
	public class ExprClassForEPLObjects {

	    public static ICollection<RegressionExecution> Executions() {
	        var executions = new List<RegressionExecution>();
	        executions.Add(new ExprClassResolutionFromClauseMethod());
	        executions.Add(new ExprClassResolutionOutputColType());
	        executions.Add(new ExprClassResolutionInvalid());
	        executions.Add(new ExprClassResolutionScript());
	        return executions;
	    }

	    private class ExprClassResolutionOutputColType : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl =
	                "inlined_class \"\"\"\n" +
	                    "  public class MyBean {\n" +
	                    "    private final int id;" +
	                    "    public MyBean(int id) {this.id = id;}\n" +
	                    "    public int getId() {return id;}\n" +
	                    "    public static MyBean getBean(int id) {return new MyBean(id);}\n" +
	                    "  }\n" +
	                    "\"\"\" \n" +
	                    "@name('s0') select MyBean.getBean(intPrimitive) as c0 from SupportBean";
	            env.CompileDeploy(epl).AddListener("s0");
	            var eventType = env.Statement("s0").EventType;
	            Assert.AreEqual("MyBean", eventType.GetPropertyType("c0").Name);

	            env.SendEventBean(new SupportBean("E1", 10));
	            var result = env.Listener("s0").AssertOneGetNewAndReset().Get("c0");
	            try {
		            Assert.That(result.GetType().GetProperty("Id").GetValue(result), Is.EqualTo(10));
	            } catch (Exception ex) {
	                Assert.Fail(ex.Message);
	            }

	            env.UndeployAll();
	        }
	    }

	    private class ExprClassResolutionInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            // test Annotation
	            var eplAnnotation = EscapeClass("public @interface MyAnnotation{}") +
	                                "@MyAnnotation @name('s0') select * from SupportBean\n";
	            TryInvalidCompile(env, eplAnnotation, "Failed to process statement annotations: Failed to resolve @-annotation class: Could not load annotation class by name 'MyAnnotation', please check imports");

	            // test Create-Schema for bean type
	            var eplBeanEventType = EscapeClass("public class MyEventBean {}") +
	                                   "create schema MyEvent as MyEventBean\n";
	            TryInvalidCompile(env, eplBeanEventType, "Could not load class by name 'MyEventBean', please check imports");

	            // test Create-Schema for property type
	            var eplPropertyType = EscapeClass("public class MyEventBean {}") +
	                                  "create schema MyEvent as (field1 MyEventBean)\n";
	            TryInvalidCompile(env, eplPropertyType, "Nestable type configuration encountered an unexpected property type name");

	            var eplNamedWindow = EscapeClass("public class MyType {}") +
	                                 "create window MyWindow(myfield MyType)\n";
	            TryInvalidCompile(env, eplNamedWindow, "Nestable type configuration encountered an unexpected property type name");

	            var eplTable = EscapeClass("public class MyType {}") +
	                           "create table MyTable(myfield MyType)\n";
	            TryInvalidCompile(env, eplTable, "skip");
	        }
	    }

	    private class ExprClassResolutionScript : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var eplScript = EscapeClass("public class MyScriptResult {}") +
	                            "expression Object[] js:myItemProducerScript() [\n" +
	                            "myItemProducerScript();" +
	                            "function myItemProducerScript() {" +
	                            "  var arrayType = Java.type(\"MyScriptResult\");\n" +
	                            "  var rows = new arrayType(2);\n" +
	                            "  return rows;\n" +
	                            "}]" +
	                            "@name('s0') select myItemProducerScript() from SupportBean";
	            env.CompileDeploy(eplScript).AddListener("s0");

	            try {
	                env.SendEventBean(new SupportBean("E1", 1));
	                Assert.Fail();
	            } catch (EPException ex) {
	                AssertMessage(ex, "EPRuntimeException: Unexpected exception in statement 's0'");
	            }

	            env.UndeployAll();
	        }
	    }

	    private class ExprClassResolutionFromClauseMethod : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var eplCreateClass =
	                "create inlined_class \"\"\"\n" +
	                    "  public class MyFromClauseMethod {\n" +
	                    "    public static MyBean[] Beans() {\n" +
	                    "       get => new MyBean[] {new MyBean(1), new MyBean(2)};\n" +
	                    "    }\n" +
	                    "    public class MyBean {\n" +
	                    "      public MyBean(int id) {this.Id = Id;}\n" +
	                    "      public int Id { get; set; };\n" +
	                    "    }\n" +
	                    "  }\n" +
	                    "\"\"\" \n";
	            env.CompileDeploy(eplCreateClass, path);

	            var epl = "@name('s0')" +
	                      "@name('s0') select s.id as c0 from SupportBean as e,\n" +
	                      "method:MyFromClauseMethod.getBeans() as s";
	            var compiled = env.Compile(epl, path);
	            var assembly = compiled.Assembly;
	            var assemblyTypes = assembly.GetExportedTypes();
	            foreach (var assemblyType in assemblyTypes) {
	                if (assemblyType.Name.Contains("MyFromClauseMethod")) {
	                    Assert.Fail("EPCompiled should not contain create-class class");
	                }
	            }
	            env.Deploy(compiled).AddListener("s0");

	            env.SendEventBean(new SupportBean("E1", 10));
	            EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), "c0".SplitCsv(),
		            new object[][] {
			            new object[]{1},
			            new object[]{2}
		            });

	            env.UndeployAll();
	        }
	    }

	    private static string EscapeClass(string text) {
	        return "inlined_class \"\"\"\n" + text + "\"\"\" \n";
	    }
	}
} // end of namespace
