///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Text;

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

namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendUDFVarargs : RegressionExecution
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
                MakePair("varargsW1Param('abc', 1.0, 2.0)", "abc,1.0,2.0"),
                MakePair("varargsW1Param('abc', 1, 2)", "abc,1.0,2.0"),
                MakePair("varargsW1Param('abc', 1)", "abc,1.0"),
                MakePair("varargsW1Param('abc')", "abc")
            );

            RunVarargAssertion(
                env,
                milestone,
                MakePair("varargsW2Param(1, 2.0, 3L, 4L)", "1,2.0,3,4"),
                MakePair("varargsW2Param(1, 2.0, 3L)", "1,2.0,3"),
                MakePair("varargsW2Param(1, 2.0)", "1,2.0"),
                MakePair("varargsW2Param(1, 2.0, 3, 4L)", "1,2.0,3,4"),
                MakePair("varargsW2Param(1, 2.0, 3L, 4L)", "1,2.0,3,4"),
                MakePair("varargsW2Param(1, 2.0, 3, 4)", "1,2.0,3,4"),
                MakePair("varargsW2Param(1, 2.0, 3L, 4)", "1,2.0,3,4"));

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
                MakePair(typeof(SupportSingleRowFunction).FullName + ".varargsW2ParamWCtx('a', 'b')", "CTX+a,b,"));

            RunVarargAssertion(
                env,
                milestone,
                MakePair("varargsOnlyObject('a', 1, new BigInteger('2'))", "a,1,2"),
                MakePair("varargsOnlyNumber(1f, 2L, 3, new BigInteger('4'))", "1.0,2,3,4"));

            RunVarargAssertion(
                env,
                milestone,
                MakePair("varargsOnlyNumber(1f, 2L, 3, new BigInteger('4'))", "1.0,2,3,4"));

            RunVarargAssertion(
                env,
                milestone,
                MakePair(
                    "varargsOnlyISupportBaseAB(new " + typeof(ISupportBImpl).FullName + "('a', 'b'))",
                    "ISupportBImpl{valueB='a', valueBaseAB='b'}"));

            // tests for array-passthru
            RunVarargAssertion(
                env,
                milestone,
                MakePair("varargsOnlyString({'a'})", "a"),
                MakePair("varargsOnlyString({'a', 'b'})", "a,b"),
                MakePair("varargsOnlyObject({'a', 'b'})", "a,b"),
                MakePair("varargsOnlyObject({})", ""),
                MakePair("varargsObjectsWCtx({1, 'a'})", "CTX+1,a"),
                MakePair("varargsW1ParamObjectsWCtx(1, {'a', 1})", "CTX+,1,a,1")
            );

            // try Arrays.asList
            TryAssertionArraysAsList(env, milestone);

            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                "select varargsOnlyInt(1, null) from SupportBean",
                "Failed to valIdate select-clause expression 'varargsOnlyInt(1,null)': Could not find static method");

            RunVarargAssertion(
                env,
                milestone,
                MakePair("varargsOnlyBoxedFloat(cast(1, byte), cast(2, short), null, 3)", "1.0,2.0,null,3.0"));
            RunVarargAssertion(env, milestone, MakePair("varargsOnlyBoxedShort(null, cast(1, byte))", "null,1"));
            RunVarargAssertion(env, milestone, MakePair("varargsOnlyBoxedByte(null, cast(1, byte))", "null,1"));

            // test excact match takes priority over varargs
            RunVarargAssertion(
                env,
                milestone,
                MakePair("varargOverload()", "many"),
                MakePair("varargOverload(1)", "p1"),
                MakePair("varargOverload(1, 2)", "p2"),
                MakePair("varargOverload(1, 2, 3)", "p3"),
                MakePair("varargOverload(1, 2, 3, 4)", "many")
            );
        }

        private void RunVarargAssertion(
            RegressionEnvironment env,
            AtomicLong milestone,
            params UniformPair<string>[] pairs)
        {
            var buf = new StringBuilder();
            buf.Append("@Name('test') select ");
            var count = 0;
            foreach (var pair in pairs) {
                buf.Append(pair.First);
                buf.Append(" as c");
                buf.Append(Convert.ToString(count));
                count++;
                buf.Append(",");
            }

            buf.Append("IntPrimitive from SupportBean");

            env.CompileDeployAddListenerMile(buf.ToString(), "test", milestone.GetAndIncrement());

            env.SendEventBean(new SupportBean());
            var @out = env.Listener("test").AssertOneGetNewAndReset();

            count = 0;
            foreach (var pair in pairs) {
                Assert.AreEqual(pair.Second, @out.Get("c" + count), "failed for '" + pair.First + "'");
                count++;
            }

            env.UndeployAll();
        }

        private UniformPair<string> MakePair(
            string expression,
            string expected)
        {
            return new UniformPair<string>(expression, expected);
        }

        private void TryAssertionArraysAsList(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var epl = "@Name('s0') select " +
                      "java.util.Arrays.asList('a') as c0, " +
                      "java.util.Arrays.asList({'a'}) as c1, " +
                      "java.util.Arrays.asList('a', 'b') as c2, " +
                      "java.util.Arrays.asList({'a', 'b'}) as c3 " +
                      "from SupportBean";
            env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

            env.SendEventBean(new SupportBean());
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            AssertEqualsColl(@event, "c0", "a");
            AssertEqualsColl(@event, "c1", "a");
            AssertEqualsColl(@event, "c2", "a", "b");
            AssertEqualsColl(@event, "c3", "a", "b");

            env.UndeployAll();
        }

        private void AssertEqualsColl(
            EventBean @event,
            string property,
            params string[] values)
        {
            var data = @event.Get(property).Unwrap<object>();
            EPAssertionUtil.AssertEqualsExactOrder(values, data.ToArray());
        }
    }
} // end of namespace