///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.option;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileSubstitutionParams
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSubstParamNamedParameter(execs);
            WithSubstParamMethodInvocation(execs);
            WithSubstParamUnnamedParameterWType(execs);
            WithSubstParamPattern(execs);
            WithSubstParamSimpleOneParameterWCast(execs);
            WithSubstParamWInheritance(execs);
            WithSubstParamSimpleTwoParameterFilter(execs);
            WithSubstParamSimpleTwoParameterWhere(execs);
            WithSubstParamSimpleNoParameter(execs);
            WithSubstParamPrimitiveVsBoxed(execs);
            WithSubstParamSubselect(execs);
            WithSubstParamInvalidUse(execs);
            WithSubstParamInvalidNoCallback(execs);
            WithSubstParamInvalidInsufficientValues(execs);
            WithSubstParamInvalidParametersUntyped(execs);
            WithSubstParamInvalidParametersTyped(execs);
            WithSubstParamResolverContext(execs);
            WithSubstParamMultiStmt(execs);
            WithSubstParamArray(execs);
            WithSODAInvalidConstantUseSubsParamsInstead(execs);
            WithSubstParamGenericType(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamGenericType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamGenericType(false));
            execs.Add(new ClientCompileSubstParamGenericType(true));
            return execs;
        }

        public static IList<RegressionExecution> WithSODAInvalidConstantUseSubsParamsInstead(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSODAInvalidConstantUseSubsParamsInstead());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamArray(false));
            execs.Add(new ClientCompileSubstParamArray(true));
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamMultiStmt(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamMultiStmt());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamResolverContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamResolverContext());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamInvalidParametersTyped(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamInvalidParametersTyped());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamInvalidParametersUntyped(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamInvalidParametersUntyped());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamInvalidInsufficientValues(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamInvalidInsufficientValues());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamInvalidNoCallback(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamInvalidNoCallback());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamInvalidUse(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamInvalidUse());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamSubselect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamPrimitiveVsBoxed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamPrimitiveVsBoxed());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamSimpleNoParameter(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamSimpleNoParameter());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamSimpleTwoParameterWhere(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamSimpleTwoParameterWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamSimpleTwoParameterFilter(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamSimpleTwoParameterFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamWInheritance(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamWInheritance());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamSimpleOneParameterWCast(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamSimpleOneParameterWCast());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamUnnamedParameterWType(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamUnnamedParameterWType(false));
            execs.Add(new ClientCompileSubstParamUnnamedParameterWType(true));
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamMethodInvocation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamMethodInvocation());
            return execs;
        }

        public static IList<RegressionExecution> WithSubstParamNamedParameter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileSubstParamNamedParameter(false));
            execs.Add(new ClientCompileSubstParamNamedParameter(true));
            return execs;
        }

        private class ClientCompileSubstParamGenericType : RegressionExecution
        {
            private readonly bool soda;

            public ClientCompileSubstParamGenericType(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "?:a0:System.Collections.Generic.IList<String> as c0, " +
                          "?:a1:System.Collections.Generic.IDictionary<String,Integer> as c1 " +
                          "from SupportBean";
                var compiled = env.Compile(soda, epl, new CompilerArguments(env.Configuration));

                var param = new SupportPortableDeploySubstitutionParams();
                param.Add("a0", new List<string>(Arrays.AsList("a"))).Add("a1", Collections.SingletonMap("k1", 10));
                var options =
                    new DeploymentOptions().WithStatementSubstitutionParameter(param.SetStatementParameters);
                env.Deploy(compiled, options).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        Assert.AreEqual(typeof(IList<string>), eventType.GetPropertyType("c0"));
                        Assert.AreEqual(typeof(IDictionary<string, int>), eventType.GetPropertyType("c1"));
                    });

                env.SendEventBean(new SupportBean());

                env.AssertEventNew(
                    "s0",
                    @event => {
                        EPAssertionUtil.AssertEqualsExactOrder(
                            new object[] { "a" },
                            @event.Get("c0").UnwrapIntoArray<string>());
                        EPAssertionUtil.AssertPropsMap(@event.Get("c1").UnwrapStringDictionary(), "k1".Split(","), 10);
                    });

                env.UndeployAll();
            }

            public string Name()
            {
                return GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       '}';
            }
        }

        private class ClientCompileSODAInvalidConstantUseSubsParamsInstead : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Expression expression = Expressions.Eq(
                    Expressions.Property("object"),
                    Expressions.Constant(new object())
                );

                var model = new EPStatementObjectModel()
                    .WithSelectClause(SelectClause.CreateWildcard())
                    .WithFromClause(FromClause.Create(FilterStream.Create("SupportObjectCtor", expression)));
                try {
                    var module = new Module();
                    module.Items.Add(new ModuleItem(model));
                    env.Compiler.Compile(module, new CompilerArguments(env.Configuration));
                    Assert.Fail();
                }
                catch (EPCompileException ex) {
                    SupportMessageAssertUtil.AssertMessage(
                        ex,
                        "Exception processing statement: Invalid constant of type 'System.Object' encountered as the class has no compiler representation, please use substitution parameters instead");
                }
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class ClientCompileSubstParamArray : RegressionExecution
        {
            private readonly bool soda;

            public ClientCompileSubstParamArray(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "?:a0:int[] as c0, " +
                          "?:a1:int[primitive] as c1, " +
                          "?:a2:System.Object[] as c2, " +
                          "?:a3:string[][] as c3, " +
                          "?:a4:System.Object[][] as c4 " +
                          "from SupportBean";

                EPCompiled compiled;
                if (soda) {
                    var copy = env.EplToModel(epl);
                    Assert.AreEqual(epl.Trim(), copy.ToEPL());
                    compiled = env.Compile(copy, new CompilerArguments(env.Configuration));
                }
                else {
                    compiled = env.Compile(epl);
                }

                var @params = new SupportPortableDeploySubstitutionParams();
                @params.Add("a0", new int?[] { 1, 2 })
                    .Add("a1", new int[] { 3, 4 })
                    .Add("a2", new object[] { "a", "b" })
                    .Add("a3", new string[][] { new string[] { "A" } })
                    .Add("a4", new object[][] { new object[] { 5, 6 } });
                var options = new DeploymentOptions()
                    .WithStatementSubstitutionParameter(@params.SetStatementParameters);
                env.Deploy(compiled, options).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        Assert.AreEqual(typeof(int?[]), eventType.GetPropertyType("c0"));
                        Assert.AreEqual(typeof(int[]), eventType.GetPropertyType("c1"));
                        Assert.AreEqual(typeof(object[]), eventType.GetPropertyType("c2"));
                        Assert.AreEqual(typeof(string[][]), eventType.GetPropertyType("c3"));
                        Assert.AreEqual(typeof(object[][]), eventType.GetPropertyType("c4"));
                    });

                env.SendEventBean(new SupportBean());

                env.AssertEventNew(
                    "s0",
                    @event => {
                        EPAssertionUtil.AssertEqualsExactOrder(new int?[] { 1, 2 }, (int?[])@event.Get("c0"));
                        EPAssertionUtil.AssertEqualsExactOrder(new int[] { 3, 4 }, (int[])@event.Get("c1"));
                        EPAssertionUtil.AssertEqualsExactOrder(new object[] { "a", "b" }, (object[])@event.Get("c2"));
                        EPAssertionUtil.AssertEqualsExactOrder(
                            new string[][] { new string[] { "A" } },
                            (string[][])@event.Get("c3"));
                        EPAssertionUtil.AssertEqualsExactOrder(
                            new object[][] { new object[] { 5, 6 } },
                            (object[][])@event.Get("c4"));
                    });

                env.UndeployAll();
            }

            public string Name()
            {
                return GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       '}';
            }
        }

        private class ClientCompileSubstParamMultiStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean_S0(Id=?:subs_1:int);\n" +
                               "@name('s1') select * from SupportBean_S1(P10=?:subs_2:string);\n";
                var compiled = env.Compile(epl);

                var options = new DeploymentOptions().WithStatementSubstitutionParameter(
                    _ => {
                        if (_.StatementName.Equals("s1")) {
                            _.SetObject("subs_2", "abc");
                        }
                        else {
                            _.SetObject("subs_1", 100);
                        }
                    });
                try {
                    env.Deployment.Deploy(compiled, options);
                }
                catch (EPDeployException e) {
                    throw new EPRuntimeException(e);
                }

                env.AddListener("s0").AddListener("s1");

                env.SendEventBean(new SupportBean_S1(-1, "abc"));
                env.AssertListenerInvoked("s1");

                env.SendEventBean(new SupportBean_S0(100));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        private class ClientCompileSubstParamResolverContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                MySubstitutionOption.Contexts.Clear();
                var compiled = env.Compile("@name('s0') select ?:p0:int as c0 from SupportBean");
                var options = new DeploymentOptions()
                    .WithStatementSubstitutionParameter(new MySubstitutionOption().SetStatementParameters);
                options.DeploymentId = "abc";
                try {
                    env.Deployment.Deploy(compiled, options);
                    Assert.Fail();
                }
                catch (EPDeployException) {
                    // expected
                }

                Assert.AreEqual(1, MySubstitutionOption.Contexts.Count);
                var ctx = MySubstitutionOption.Contexts[0];
                Assert.IsNotNull(ctx.Annotations);
                Assert.AreEqual("abc", ctx.DeploymentId);
                Assert.IsNotNull(ctx.Epl);
                Assert.IsTrue(ctx.StatementId > 0);
                Assert.AreEqual("s0", ctx.StatementName);
                Assert.AreEqual(typeof(int?), ctx.SubstitutionParameterTypes[0]);
                Assert.AreEqual((int?)1, ctx.SubstitutionParameterNames.Get("p0"));
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        private class ClientCompileSubstParamPrimitiveVsBoxed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile("select ?:p0:int as c0, ?:p1:Integer as c1 from SupportBean");
                DeployWithResolver(
                    env,
                    compiled,
                    "s0",
                    new SupportPortableDeploySubstitutionParams().Add("p0", 10).Add("p1", 11));
                env.AddListener("s0");

                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", "c0,c1".Split(","), new object[] { 10, 11 });

                env.UndeployAll();
            }
        }

        private class ClientCompileSubstParamInvalidUse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // invalid mix or named and unnamed
                env.TryInvalidCompile(
                    "select ? as c0,?:a as c1 from SupportBean",
                    "Inconsistent use of substitution parameters, expecting all substitutions to either all provide a name or provide no name");

                // keyword used for name
                env.TryInvalidCompile(
                    "select ?:select from SupportBean",
                    "Incorrect syntax near 'select' (a reserved keyword) at line 1 column 9");

                // invalid type incompatible
                env.TryInvalidCompile(
                    "select ?:p0:int as c0, ?:p0:long from SupportBean",
                    "Substitution parameter 'p0' incompatible type assignment between types 'System.Nullable<System.Int32>' and 'System.Nullable<System.Int64>'");
            }
        }

        private class ClientCompileSubstParamNamedParameter : RegressionExecution
        {
            private readonly bool soda;

            public ClientCompileSubstParamNamedParameter(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select ?:pint:int as c0 from SupportBean(TheString=?:pstring:string and IntPrimitive=?:pint:int and LongPrimitive=?:plong:long)";
                var compiled = env.Compile(soda, epl, new CompilerArguments(new Configuration()));
                DeployWithResolver(
                    env,
                    compiled,
                    null,
                    new SupportPortableDeploySubstitutionParams()
                        .Add("pstring", "E1")
                        .Add("pint", 10)
                        .Add("plong", 100L));
                env.AddListener("s0");

                var @event = new SupportBean("E1", 10);
                @event.LongPrimitive = 100;
                env.SendEventBean(@event);
                env.AssertPropsNew("s0", "c0".Split(","), new object[] { 10 });

                env.Milestone(0);

                env.SendEventBean(@event);
                env.AssertPropsNew("s0", "c0".Split(","), new object[] { 10 });

                env.UndeployAll();
            }

            public string Name()
            {
                return GetType().Name + "soda_" + soda;
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }

        private class ClientCompileSubstParamUnnamedParameterWType : RegressionExecution
        {
            private readonly bool soda;

            public ClientCompileSubstParamUnnamedParameterWType(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile(
                    soda,
                    "@name('s0') select * from SupportBean(TheString=(?::SupportBean.GetTheString()))",
                    new CompilerArguments(new Configuration()));
                DeployWithResolver(
                    env,
                    compiled,
                    null,
                    new SupportPortableDeploySubstitutionParams().Add(1, new SupportBean("E1", 0)));
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertListenerInvoked("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }

            public string Name()
            {
                return GetType().Name +
                       "{" +
                       "soda=" +
                       soda +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }

        private class ClientCompileSubstParamMethodInvocation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile(
                    "@name('s0') select * from SupportBean(TheString = ?:psb:SupportBean.GetTheString())");
                DeployWithResolver(
                    env,
                    compiled,
                    null,
                    new SupportPortableDeploySubstitutionParams("psb", new SupportBean("E1", 0)));
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ClientCompileSubstParamPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "select * from pattern[SupportBean(TheString=?::string)]";
                var compiled = env.Compile(epl);

                DeployWithResolver(env, compiled, "s0", new SupportPortableDeploySubstitutionParams(1, "e1"));
                env.AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(epl, statement.GetProperty(StatementProperty.EPL)));

                DeployWithResolver(env, compiled, "s1", new SupportPortableDeploySubstitutionParams(1, "e2"));
                env.AddListener("s1");
                env.AssertStatement(
                    "s1",
                    statement => Assert.AreEqual(epl, statement.GetProperty(StatementProperty.EPL)));

                env.SendEventBean(new SupportBean("e2", 10));
                env.AssertListenerNotInvoked("s0");
                env.AssertListenerInvoked("s1");

                env.SendEventBean(new SupportBean("e1", 10));
                env.AssertListenerNotInvoked("s1");
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ClientCompileSubstParamSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "select (select Symbol from SupportMarketDataBean(Symbol=?::string)#lastevent) as mysymbol from SupportBean";
                var compiled = env.Compile(stmtText);

                DeployWithResolver(env, compiled, "s0", new SupportPortableDeploySubstitutionParams(1, "S1"));
                env.AddListener("s0");

                DeployWithResolver(env, compiled, "s1", new SupportPortableDeploySubstitutionParams(1, "S2"));
                env.AddListener("s1");

                // test no event, should return null
                env.SendEventBean(new SupportBean("e1", -1));
                env.AssertEqualsNew("s0", "mysymbol", null);
                env.AssertEqualsNew("s1", "mysymbol", null);

                // test one non-matching event
                env.SendEventBean(new SupportMarketDataBean("XX", 0, 0L, ""));
                env.SendEventBean(new SupportBean("e1", -1));
                env.AssertEqualsNew("s0", "mysymbol", null);
                env.AssertEqualsNew("s1", "mysymbol", null);

                // test S2 matching event
                env.SendEventBean(new SupportMarketDataBean("S2", 0, 0L, ""));
                env.SendEventBean(new SupportBean("e1", -1));
                env.AssertEqualsNew("s0", "mysymbol", null);
                env.AssertEqualsNew("s1", "mysymbol", "S2");

                // test S1 matching event
                env.SendEventBean(new SupportMarketDataBean("S1", 0, 0L, ""));
                env.SendEventBean(new SupportBean("e1", -1));
                env.AssertEqualsNew("s0", "mysymbol", "S1");
                env.AssertEqualsNew("s1", "mysymbol", "S2");

                env.UndeployAll();
            }
        }

        private class ClientCompileSubstParamSimpleOneParameterWCast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = "select * from SupportBean(TheString=cast(?, string))";
                var compiled = env.Compile(stmt);

                DeployWithResolver(env, compiled, "s0", new SupportPortableDeploySubstitutionParams(1, "e1"));
                env.AddListener("s0");

                DeployWithResolver(env, compiled, "s1", new SupportPortableDeploySubstitutionParams(1, "e2"));
                env.AddListener("s1");

                env.SendEventBean(new SupportBean("e2", 10));
                env.AssertListenerNotInvoked("s0");
                env.AssertListenerInvoked("s1");

                env.SendEventBean(new SupportBean("e1", 10));
                env.AssertListenerNotInvoked("s1");
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ClientCompileSubstParamWInheritance : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test substitution parameter and inheritance in key matching
                var path = new RegressionPath();
                var types =
                    "@public @buseventtype create schema MyEventOne as " +
                    typeof(MyEventOne).MaskTypeName() +
                    ";\n" +
                    "@public @buseventtype create schema MyEventTwo as " +
                    typeof(MyEventTwo).MaskTypeName() +
                    ";\n";
                env.CompileDeploy(types, path);

                var epl = "select * from MyEventOne(Key = ?::IKey)";
                var compiled = env.Compile(epl, path);
                var lKey = new MyObjectKeyInterface();
                DeployWithResolver(env, compiled, "s0", new SupportPortableDeploySubstitutionParams(1, lKey));
                env.AddListener("s0");

                env.SendEventBean(new MyEventOne(lKey));
                env.AssertListenerInvoked("s0");

                // Test substitution parameter and concrete subclass in key matching
                epl = "select * from MyEventTwo where Key = ?::MyObjectKeyConcrete";
                compiled = env.Compile(epl, path);
                var cKey = new MyObjectKeyConcrete();
                DeployWithResolver(env, compiled, "s1", new SupportPortableDeploySubstitutionParams(1, cKey));
                env.AddListener("s1");

                env.SendEventBean(new MyEventTwo(cKey));
                env.AssertListenerInvoked("s1");

                env.UndeployAll();
            }
        }

        private class ClientCompileSubstParamSimpleTwoParameterFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = "select * from SupportBean(TheString=?::string,IntPrimitive=?::int)";
                RunSimpleTwoParameter(env, stmt, "A", true);
            }
        }

        private class ClientCompileSubstParamSimpleTwoParameterWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmt = "select * from SupportBean where TheString=?::string and IntPrimitive=?::int";
                RunSimpleTwoParameter(env, stmt, "B", false);
            }
        }

        private class ClientCompileSubstParamSimpleNoParameter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile("select * from SupportBean(TheString=\"e1\")");
                DeployWithResolver(env, compiled, "s0", new SupportPortableDeploySubstitutionParams());
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("e2", 10));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("e1", 10));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ClientCompileSubstParamInvalidNoCallback : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidDeployNoCallbackProvided(env, "@name('s0') select * from SupportBean(TheString=?::string)");
                TryInvalidDeployNoCallbackProvided(
                    env,
                    "@name('s0') select * from SupportBean(TheString=cast(?,string))");
                TryInvalidDeployNoCallbackProvided(
                    env,
                    "@name('s0') select * from SupportBean(TheString=?:myname:string)");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class ClientCompileSubstParamInvalidInsufficientValues : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                EPCompiled compiled;

                compiled = env.Compile(
                    "@name('s0') select * from SupportBean(TheString=?::string, IntPrimitive=?::int)");
                TryInvalidResolver(
                    env,
                    compiled,
                    "Substitution parameters have not been provided: Missing value for substitution parameter 1 for statement 's0'",
                    prepared => { });
                TryInvalidResolver(
                    env,
                    compiled,
                    "Substitution parameters have not been provided: Missing value for substitution parameter 2 for statement 's0'",
                    prepared => prepared.SetObject(1, "abc"));

                compiled = env.Compile(
                    "@name('s0') select * from SupportBean(TheString=?:p0:string, IntPrimitive=?:p1:int)");
                TryInvalidResolver(
                    env,
                    compiled,
                    "Substitution parameters have not been provided: Missing value for substitution parameter 'p0' for statement 's0'",
                    prepared => { });
                TryInvalidResolver(
                    env,
                    compiled,
                    "Substitution parameters have not been provided: Missing value for substitution parameter 'p1' for statement 's0'",
                    prepared => prepared.SetObject("p0", "x"));
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class ClientCompileSubstParamInvalidParametersUntyped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                EPCompiled compiled;
                DeploymentOptions options;

                compiled = env.Compile("select * from SupportBean(TheString='ABC')");
                options = new DeploymentOptions().WithStatementSubstitutionParameter(
                    prepared => {
                        TryInvalidSetObject(
                            prepared,
                            stmt => stmt.SetObject("x", 10),
                            "The statement has no substitution parameters");
                        TryInvalidSetObject(
                            prepared,
                            stmt => stmt.SetObject(1, 10),
                            "The statement has no substitution parameters");
                    });
                DeployWithOptionsWUndeploy(env, compiled, options);

                // numbered, untyped, casted at eventService
                compiled = env.Compile("select * from SupportBean(TheString=cast(?, String))");
                options = new DeploymentOptions().WithStatementSubstitutionParameter(
                    prepared => {
                        TryInvalidSetObject(
                            prepared,
                            stmt => stmt.SetObject("x", 10),
                            "Substitution parameter names have not been provided for this statement");
                        TryInvalidSetObject(
                            prepared,
                            stmt => stmt.SetObject(0, "a"),
                            "Invalid substitution parameter index, expected an index between 1 and 1");
                        TryInvalidSetObject(
                            prepared,
                            stmt => stmt.SetObject(2, "a"),
                            "Invalid substitution parameter index, expected an index between 1 and 1");
                        prepared.SetObject(1, "xxx");
                    });
                DeployWithOptionsWUndeploy(env, compiled, options);

                // named, untyped, casted at eventService
                compiled = env.Compile("select * from SupportBean(TheString=cast(?:p0, String))");
                options = new DeploymentOptions().WithStatementSubstitutionParameter(
                    prepared => {
                        TryInvalidSetObject(
                            prepared,
                            stmt => stmt.SetObject("x", 10),
                            "Failed to find substitution parameter named 'x', available parameters are [\"p0\"]");
                        TryInvalidSetObject(
                            prepared,
                            stmt => stmt.SetObject(0, "a"),
                            "Substitution parameter names have been provided for this statement, please set the value by name");
                        prepared.SetObject("p0", "xxx");
                    });
                DeployWithOptionsWUndeploy(env, compiled, options);
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class ClientCompileSubstParamInvalidParametersTyped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                EPCompiled compiled;
                DeploymentOptions options;

                // numbered, typed
                compiled = env.Compile("select * from SupportBean(TheString=?::string)");
                options = new DeploymentOptions().WithStatementSubstitutionParameter(
                    prepared => {
                        TryInvalidSetObject(
                            prepared,
                            stmt => stmt.SetObject(1, 10),
                            "Failed to set substitution parameter 1, expected a value of type 'System.String': ");
                        prepared.SetObject(1, "abc");
                    });
                DeployWithOptionsWUndeploy(env, compiled, options);

                // name, typed
                compiled = env.Compile("select * from SupportBean(TheString=?:p0:string)");
                options = new DeploymentOptions().WithStatementSubstitutionParameter(
                    prepared => {
                        TryInvalidSetObject(
                            prepared,
                            stmt => stmt.SetObject("p0", 10),
                            "Failed to set substitution parameter 'p0', expected a value of type 'System.String': ");
                        prepared.SetObject("p0", "abc");
                    });
                DeployWithOptionsWUndeploy(env, compiled, options);

                // name, primitive
                compiled = env.Compile("select * from SupportBean(IntPrimitive=?:p0:int)");
                options = new DeploymentOptions().WithStatementSubstitutionParameter(
                    prepared => {
                        // There is only boxed type consistent with all other column/variable/schema typing:
                        // tryInvalidSetObject(prepared, stmt =>  stmt.setObject("p0", null), "Failed to set substitution parameter 'p0', expected a value of type 'int': Received a null-value for a primitive type");
                        prepared.SetObject("p0", 10);
                    });
                DeployWithOptionsWUndeploy(env, compiled, options);
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private static void RunSimpleTwoParameter(
            RegressionEnvironment env,
            string stmtText,
            string statementName,
            bool compareText)
        {
            var compiled = env.Compile(stmtText);

            DeployWithResolver(
                env,
                compiled,
                statementName,
                new SupportPortableDeploySubstitutionParams(1, "e1", 2, 1));
            env.AddListener(statementName);
            if (compareText) {
                env.AssertStatement(
                    statementName,
                    statement => Assert.AreEqual(
                        "select * from SupportBean(TheString=?::string,IntPrimitive=?::int)",
                        statement.GetProperty(StatementProperty.EPL)));
            }

            DeployWithResolver(
                env,
                compiled,
                statementName + "__1",
                new SupportPortableDeploySubstitutionParams().Add(1, "e2").Add(2, 2));
            env.AddListener(statementName + "__1");
            if (compareText) {
                env.AssertStatement(
                    statementName + "__1",
                    statement => Assert.AreEqual(
                        "select * from SupportBean(TheString=?::string,IntPrimitive=?::int)",
                        statement.GetProperty(StatementProperty.EPL)));
            }

            env.SendEventBean(new SupportBean("e2", 2));
            env.AssertListenerNotInvoked(statementName);
            env.AssertListenerInvoked(statementName + "__1");

            env.SendEventBean(new SupportBean("e1", 1));
            env.AssertListenerInvoked(statementName);
            env.AssertListenerNotInvoked(statementName + "__1");

            env.SendEventBean(new SupportBean("e1", 2));
            env.AssertListenerNotInvoked(statementName);
            env.AssertListenerNotInvoked(statementName + "__1");

            env.UndeployAll();
        }

        private static void DeployWithResolver(
            RegressionEnvironment env,
            EPCompiled compiled,
            string statementName,
            SupportPortableDeploySubstitutionParams resolver)
        {
            var options = new DeploymentOptions().WithStatementSubstitutionParameter(resolver.SetStatementParameters);
            options.WithStatementNameRuntime(new SupportPortableDeployStatementName(statementName).GetStatementName);
            env.Deploy(compiled, options);
        }

        private static void TryInvalidDeployNoCallbackProvided(
            RegressionEnvironment env,
            string stmt)
        {
            var compiled = env.Compile(stmt);
            try {
                env.Deployment.Deploy(compiled);
                Assert.Fail();
            }
            catch (EPDeploySubstitutionParameterException ex) {
                Assert.AreEqual(-1, ex.RolloutItemNumber);
                Assert.AreEqual(
                    "Substitution parameters have not been provided: Statement 's0' has 1 substitution parameters",
                    ex.Message);
            }
            catch (EPDeployException ex) {
                throw new EPRuntimeException(ex);
            }
        }

        private static void TryInvalidSetObject(
            StatementSubstitutionParameterContext prepared,
            Consumer<StatementSubstitutionParameterContext> consumer,
            string message)
        {
            try {
                consumer.Invoke(prepared);
                Assert.Fail();
            }
            catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, message);
            }
        }

        private static void TryInvalidResolver(
            RegressionEnvironment env,
            EPCompiled compiled,
            string expected,
            StatementSubstitutionParameterOption resolver)
        {
            var options = new DeploymentOptions().WithStatementSubstitutionParameter(resolver);
            try {
                env.Deployment.Deploy(compiled, options);
                Assert.Fail();
            }
            catch (EPDeploySubstitutionParameterException e) {
                SupportMessageAssertUtil.AssertMessage(e.Message, expected);
            }
            catch (EPDeployException e) {
                throw new EPRuntimeException(e);
            }
        }

        private static void DeployWithOptionsWUndeploy(
            RegressionEnvironment env,
            EPCompiled compiled,
            DeploymentOptions options)
        {
            env.Deploy(compiled, options).UndeployAll();
        }

        public class MySubstitutionOption
        {
            public static IList<StatementSubstitutionParameterContext> Contexts { get; } = new List<StatementSubstitutionParameterContext>();

            public void SetStatementParameters(StatementSubstitutionParameterContext env)
            {
                Contexts.Add(env);
            }
        }

        public interface IKey
        {
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire
        /// when running remote tests and serialization is just convenient.
        /// Serialization generally not used for HA and HA testing.
        /// </summary>
        public class MyObjectKeyInterface : IKey
        {
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire
        /// when running remote tests and serialization is just convenient.
        /// Serialization generally not used for HA and HA testing.
        /// </summary>
        public class MyEventOne
        {
            private readonly IKey _key;

            public MyEventOne(IKey key)
            {
                _key = key;
            }

            public IKey GetKey()
            {
                return _key;
            }
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire
        /// when running remote tests and serialization is just convenient.
        /// Serialization generally not used for HA and HA testing.
        /// </summary>
        public class MyObjectKeyConcrete
        {
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        public class MyEventTwo
        {
            private MyObjectKeyConcrete _key;

            public MyEventTwo(MyObjectKeyConcrete key)
            {
                _key = key;
            }

            public MyObjectKeyConcrete GetKey()
            {
                return _key;
            }
        }
    }
} // end of namespace