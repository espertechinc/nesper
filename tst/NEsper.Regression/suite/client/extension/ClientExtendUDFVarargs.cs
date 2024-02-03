///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendUDFVarargs
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithCombinations(execs);
            WithCollOfEvent(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithCollOfEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendUDFVarargsCollOfEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithCombinations(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendUDFVarargsCombinations());
            return execs;
        }

        private class ClientExtendUDFVarargsCollOfEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          typeof(ClientExtendUDFVarargs).FullName +
                          ".MySupportVarArgsMethod(1, " +
                          "(select * from SupportBean#keepall), (select * from SupportBean#keepall).selectFrom(v => v)) as c0 from SupportBean_S0;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var data = (object[])@event.Get("c0");
                        ClassicAssert.AreEqual(1, data[0]);
                        ClassicAssert.IsTrue(data[1] is SupportBean);
                        ClassicAssert.IsTrue(((ICollection<SupportBean>)data[2]).First() is SupportBean);
                    });

                env.UndeployAll();
            }
        }

        private class ClientExtendUDFVarargsCombinations : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                RunVarargAssertion(
                    env,
                    milestone,
                    MakePair("varargsOnlyInt(1, 2, 3, 4)", "1,2,3,4"),
                    MakePair("varargsOnlyInt(1, 2, 3)", "1,2,3"),
                    MakePair("varargsOnlyInt(1, 2)", "1,2"),
                    MakePair("varargsOnlyInt(1)", "1"),
                    MakePair("varargsOnlyInt()", ""));

                RunVarargAssertion(
                    env,
                    milestone,
                    MakePair("varargsW1Param('abc', 1.0, 2.0)", "\"abc\",1.0d,2.0d"),
                    MakePair("varargsW1Param('abc', 1, 2)", "\"abc\",1.0d,2.0d"),
                    MakePair("varargsW1Param('abc', 1)", "\"abc\",1.0d"),
                    MakePair("varargsW1Param('abc')", "\"abc\"")
                );

                RunVarargAssertion(
                    env,
                    milestone,
                    MakePair("varargsW2Param(1, 2.0, 3L, 4L)", "1,2.0d,3L,4L"),
                    MakePair("varargsW2Param(1, 2.0, 3L)", "1,2.0d,3L"),
                    MakePair("varargsW2Param(1, 2.0)", "1,2.0d"),
                    MakePair("varargsW2Param(1, 2.0, 3, 4L)", "1,2.0d,3L,4L"),
                    MakePair("varargsW2Param(1, 2.0, 3L, 4L)", "1,2.0d,3L,4L"),
                    MakePair("varargsW2Param(1, 2.0, 3, 4)", "1,2.0d,3L,4L"),
                    MakePair("varargsW2Param(1, 2.0, 3L, 4)", "1,2.0d,3L,4L"));

                RunVarargAssertion(
                    env,
                    milestone,
                    MakePair("varargsOnlyWCtx(1, 2, 3)", "CTX+1,2,3"),
                    MakePair("varargsOnlyWCtx(1, 2)", "CTX+1,2"),
                    MakePair("varargsOnlyWCtx(1)", "CTX+1"),
                    MakePair("varargsOnlyWCtx()", "CTX+"));

                RunVarargAssertion(
                    env,
                    milestone,
                    MakePair("varargsW1ParamWCtx('a', 1, 2, 3)", "CTX+a,1,2,3"),
                    MakePair("varargsW1ParamWCtx('a', 1, 2)", "CTX+a,1,2"),
                    MakePair("varargsW1ParamWCtx('a', 1)", "CTX+a,1"),
                    MakePair("varargsW1ParamWCtx('a')", "CTX+a,"));

                RunVarargAssertion(
                    env,
                    milestone,
                    MakePair("varargsW2ParamWCtx('a', 'b', 1, 2, 3)", "CTX+a,b,1,2,3"),
                    MakePair("varargsW2ParamWCtx('a', 'b', 1, 2)", "CTX+a,b,1,2"),
                    MakePair("varargsW2ParamWCtx('a', 'b', 1)", "CTX+a,b,1"),
                    MakePair("varargsW2ParamWCtx('a', 'b')", "CTX+a,b,"),
                    MakePair(typeof(SupportSingleRowFunction).FullName + ".VarargsW2ParamWCtx('a', 'b')", "CTX+a,b,"));

                var bigInteger = typeof(BigInteger).FullName;
                
                RunVarargAssertion(
                    env,
                    milestone,
                    MakePair($"varargsOnlyObject('a', 1, {bigInteger}.Parse('2'))", "\"a\",1,2"),
                    MakePair($"varargsOnlyNumber(1f, 2L, 3, {bigInteger}.Parse('4'))", "1.0f,2L,3,4"));

                RunVarargAssertion(
                    env,
                    milestone,
                    MakePair($"varargsOnlyNumber(1f, 2L, 3, {bigInteger}.Parse('4'))", "1.0f,2L,3,4"));

                RunVarargAssertion(
                    env,
                    milestone,
                    MakePair(
                        "varargsOnlyISupportBaseAB(new " + typeof(ISupportBImpl).FullName + "('a', 'b'))",
                        "ISupportBImpl{ValueB='a', ValueBaseAB='b'}"));

                // tests for array-passthru
                RunVarargAssertion(
                    env,
                    milestone,
                    MakePair("varargsOnlyString({'a'})", "\"a\""),
                    MakePair("varargsOnlyString({'a', 'b'})", "\"a\",\"b\""),
                    MakePair("varargsOnlyObject({'a', 'b'})", "\"a\",\"b\""),
                    MakePair("varargsOnlyObject({})", ""),
                    MakePair("varargsObjectsWCtx({1, 'a'})", "CTX+1,\"a\""),
                    MakePair("varargsW1ParamObjectsWCtx(1, new object[] {'a', 1})", "CTX+,1,\"a\",1")
                );

                // try Arrays.asList
                TryAssertionArraysAsList(env, milestone);

                env.TryInvalidCompile(
                    "select varargsOnlyInt(1, null) from SupportBean",
                    "Failed to validate select-clause expression 'varargsOnlyInt(1,null)': Could not find static method");

                RunVarargAssertion(
                    env,
                    milestone,
                    MakePair("varargsOnlyBoxedFloat(cast(1, byte), cast(2, short), null, 3)", "1.0f,2.0f,null,3.0f"));
                RunVarargAssertion(env, milestone, MakePair("varargsOnlyBoxedShort(null, cast(1, byte))", "null,1"));
                RunVarargAssertion(env, milestone, MakePair("varargsOnlyBoxedByte(null, cast(1, byte))", "null,1"));

                // test excact match takes priority over varargs
                RunVarargAssertion(
                    env,
                    milestone,
                    MakePair("varargOverload()", "many"),
                    MakePair("varargOverload(1)", "P1"),
                    MakePair("varargOverload(1, 2)", "P2"),
                    MakePair("varargOverload(1, 2, 3)", "p3"),
                    MakePair("varargOverload(1, 2, 3, 4)", "many")
                );
            }
        }

        private static void RunVarargAssertion(
            RegressionEnvironment env,
            AtomicLong milestone,
            params UniformPair<string>[] pairs)
        {
            var buf = new StringWriter();
            buf.Write("@name('test') select ");
            var count = 0;
            foreach (var pair in pairs) {
                buf.Write(pair.First);
                buf.Write(" as c");
                buf.Write(Convert.ToString(count));
                count++;
                buf.Write(",");
            }

            buf.Write("IntPrimitive from SupportBean");

            env.CompileDeployAddListenerMile(buf.ToString(), "test", milestone.GetAndIncrement());

            env.SendEventBean(new SupportBean());
            env.AssertEventNew(
                "test",
                @event => {
                    var index = 0;
                    foreach (var pair in pairs) {
                        ClassicAssert.AreEqual(pair.Second, @event.Get("c" + index), "failed for '" + pair.First + "'");
                        index++;
                    }
                });

            env.UndeployAll();
        }

        private static UniformPair<string> MakePair(
            string expression,
            string expected)
        {
            return new UniformPair<string>(expression, expected);
        }

        private static void TryAssertionArraysAsList(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var epl = "@name('s0') select " +
                      "com.espertech.esper.compat.collections.Collections.List(\"a\") as c0, " +
                      "com.espertech.esper.compat.collections.Collections.List({\"a\"}) as c1, " +
                      "com.espertech.esper.compat.collections.Collections.List(\"a\", \"b\") as c2, " +
                      "com.espertech.esper.compat.collections.Collections.List({\"a\", \"b\"}) as c3 " +
                      "from SupportBean";
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            env.SendEventBean(new SupportBean());
            env.AssertEventNew(
                "s0",
                @event => {
                    AssertEqualsColl(@event, "c0", "a");
                    AssertEqualsColl(@event, "c1", "a");
                    AssertEqualsColl(@event, "c2", "a", "b");
                    AssertEqualsColl(@event, "c3", "a", "b");
                });

            env.UndeployAll();
        }

        private static void AssertEqualsColl(
            EventBean @event,
            string property,
            params string[] values)
        {
            var data = @event.Get(property).UnwrapIntoArray<object>();
            EPAssertionUtil.AssertEqualsExactOrder(values, data);
        }

        public static object[] MySupportVarArgsMethod(params object[] varargs)
        {
            return varargs;
        }
    }
} // end of namespace