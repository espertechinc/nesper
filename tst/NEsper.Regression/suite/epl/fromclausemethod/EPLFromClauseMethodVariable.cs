///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.epl.fromclausemethod
{
    public class EPLFromClauseMethodVariable
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithConstantVariable(execs);
            WithNonConstantVariable(execs);
            WithContextVariable(execs);
            WithVariableMapAndOA(execs);
            WithVariableInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithVariableInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodVariableInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithVariableMapAndOA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodVariableMapAndOA());
            return execs;
        }

        public static IList<RegressionExecution> WithContextVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodContextVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithNonConstantVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodNonConstantVariable(true));
            execs.Add(new EPLFromClauseMethodNonConstantVariable(false));
            return execs;
        }

        public static IList<RegressionExecution> WithConstantVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodConstantVariable());
            return execs;
        }

        private class EPLFromClauseMethodVariableInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // invalid footprint
                env.TryInvalidCompile(
                    "select * from method:MyConstantServiceVariable.fetchABean() as h0",
                    "Method footprint does not match the number or type of expression parameters, expecting no parameters in method: Could not find enumeration method, date-time method, instance method or property named 'fetchABean' in class '" +
                    typeof(MyConstantServiceVariable).FullName +
                    "' taking no parameters (nearest match found was 'fetchABean' taking type(s) 'int') [");

                // null variable value and metadata is instance method
                env.TryInvalidCompile(
                    "select field1, field2 from method:MyNullMap.getMapData()",
                    "Failed to access variable method invocation metadata: The variable value is null and the metadata method is an instance method");

                // variable with context and metadata is instance method
                var path = new RegressionPath();
                env.CompileDeploy("@Public create context BetweenStartAndEnd start SupportBean end SupportBean", path);
                env.CompileDeploy(
                    "@Public context BetweenStartAndEnd create variable " +
                    typeof(MyMethodHandlerMap).FullName +
                    " themap",
                    path);
                env.TryInvalidCompile(
                    path,
                    "context BetweenStartAndEnd select field1, field2 from method:themap.getMapData()",
                    "Failed to access variable method invocation metadata: The variable value is null and the metadata method is an instance method");

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodVariableMapAndOA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var epl in new string[] {
                             "@name('s0') select field1, field2 from method:MyMethodHandlerMap.getMapData()",
                             "@name('s0') select field1, field2 from method:MyMethodHandlerOA.getOAData()"
                         }) {
                    env.CompileDeploy(epl);
                    env.AssertIterator(
                        "s0",
                        iterator => EPAssertionUtil.AssertProps(
                            iterator.Advance(),
                            "field1,field2".SplitCsv(),
                            new object[] { "a", "b" }));
                    env.UndeployAll();
                }
            }
        }

        private class EPLFromClauseMethodContextVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Public create context MyContext " +
                    "initiated by SupportBean_S0 as c_s0 " +
                    "terminated by SupportBean_S1(id=c_s0.id)",
                    path);
                env.CompileDeploy(
                    "@Public context MyContext " +
                    "create variable MyNonConstantServiceVariable var = MyNonConstantServiceVariableFactory.make()",
                    path);
                env.CompileDeploy(
                        "@name('s0') context MyContext " +
                        "select id as c0 from SupportBean(intPrimitive=context.c_s0.id) as sb, " +
                        "method:var.fetchABean(intPrimitive) as h0",
                        path)
                    .AddListener("s0");
                env.CompileDeploy(
                    "context MyContext on SupportBean_S2(id = context.c_s0.id) set var.postfix=p20",
                    path);

                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S0(2));

                SendEventAssert(env, "E1", 1, "_1_context_postfix");

                env.Milestone(0);

                SendEventAssert(env, "E2", 2, "_2_context_postfix");

                env.SendEventBean(new SupportBean_S2(1, "a"));
                env.SendEventBean(new SupportBean_S2(2, "b"));

                env.Milestone(1);

                SendEventAssert(env, "E1", 1, "_1_a");
                SendEventAssert(env, "E2", 2, "_2_b");

                // invalid context
                env.TryInvalidCompile(
                    path,
                    "select * from method:var.fetchABean(intPrimitive) as h0",
                    "Variable by name 'var' has been declared for context 'MyContext' and can only be used within the same context");
                env.CompileDeploy("@Public create context ABC start @now end after 1 minute", path);
                env.TryInvalidCompile(
                    path,
                    "context ABC select * from method:var.fetchABean(intPrimitive) as h0",
                    "Variable by name 'var' has been declared for context 'MyContext' and can only be used within the same context");

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodConstantVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select id as c0 from SupportBean as sb, " +
                          "method:MyConstantServiceVariable.fetchABean(intPrimitive) as h0";
                env.CompileDeploy(epl).AddListener("s0");

                SendEventAssert(env, "E1", 10, "_10_");

                env.Milestone(0);

                SendEventAssert(env, "E2", 20, "_20_");

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodNonConstantVariable : RegressionExecution
        {
            private readonly bool soda;

            public EPLFromClauseMethodNonConstantVariable(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var modifyEPL = "on SupportBean_S0 set MyNonConstantServiceVariable.postfix=p00";
                env.CompileDeploy(soda, modifyEPL);

                var epl = "@name('s0') select id as c0 from SupportBean as sb, " +
                          "method:MyNonConstantServiceVariable.fetchABean(intPrimitive) as h0";
                env.CompileDeploy(soda, epl).AddListener("s0");

                SendEventAssert(env, "E1", 10, "_10_postfix");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1, "newpostfix"));
                SendEventAssert(env, "E1", 20, "_20_newpostfix");

                env.Milestone(1);

                // return to original value
                env.SendEventBean(new SupportBean_S0(2, "postfix"));
                SendEventAssert(env, "E1", 30, "_30_postfix");

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       '}';
            }
        }

        private static void SendEventAssert(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            string expected)
        {
            var fields = "c0".SplitCsv();
            env.SendEventBean(new SupportBean(theString, intPrimitive));
            env.AssertPropsNew("s0", fields, new object[] { expected });
        }

        [Serializable]
        public class MyConstantServiceVariable
        {
            public SupportBean_A FetchABean(int intPrimitive)
            {
                return new SupportBean_A("_" + intPrimitive + "_");
            }
        }

        [Serializable]
        public class MyNonConstantServiceVariable
        {
            private string postfix;

            public MyNonConstantServiceVariable(string postfix)
            {
                this.postfix = postfix;
            }

            public void SetPostfix(string postfix)
            {
                this.postfix = postfix;
            }

            public string GetPostfix()
            {
                return postfix;
            }

            public SupportBean_A FetchABean(int intPrimitive)
            {
                return new SupportBean_A("_" + intPrimitive + "_" + postfix);
            }
        }

        public class MyStaticService
        {
            public static SupportBean_A FetchABean(int intPrimitive)
            {
                return new SupportBean_A("_" + intPrimitive + "_");
            }
        }

        public class MyNonConstantServiceVariableFactory
        {
            public static MyNonConstantServiceVariable Make()
            {
                return new MyNonConstantServiceVariable("context_postfix");
            }
        }

        [Serializable]
        public class MyMethodHandlerMap
        {
            private readonly string field1;
            private readonly string field2;

            public MyMethodHandlerMap(
                string field1,
                string field2)
            {
                this.field1 = field1;
                this.field2 = field2;
            }

            public IDictionary<string, object> GetMapDataMetadata()
            {
                IDictionary<string, object> fields = new Dictionary<string, object>();
                fields.Put("field1", typeof(string));
                fields.Put("field2", typeof(string));
                return fields;
            }

            public IDictionary<string, object>[] GetMapData()
            {
                var maps = new IDictionary<string, object>[1];
                var row = new Dictionary<string, object>();
                maps[0] = row;
                row.Put("field1", field1);
                row.Put("field2", field2);
                return maps;
            }
        }

        [Serializable]
        public class MyMethodHandlerOA
        {
            private readonly string field1;
            private readonly string field2;

            public MyMethodHandlerOA(
                string field1,
                string field2)
            {
                this.field1 = field1;
                this.field2 = field2;
            }

            public static LinkedHashMap<string, object> GetOADataMetadata()
            {
                var fields = new LinkedHashMap<string, object>();
                fields.Put("field1", typeof(string));
                fields.Put("field2", typeof(string));
                return fields;
            }

            public object[][] GetOAData()
            {
                return new object[][] { new object[] { field1, field2 } };
            }
        }
    }
} // end of namespace