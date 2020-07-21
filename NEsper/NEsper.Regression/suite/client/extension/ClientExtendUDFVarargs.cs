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
                MakePair("VarargsOnlyInt(1, 2, 3, 4)", "1,2,3,4"),
                MakePair("VarargsOnlyInt(1, 2, 3)", "1,2,3"),
                MakePair("VarargsOnlyInt(1, 2)", "1,2"),
                MakePair("VarargsOnlyInt(1)", "1"),
                MakePair("VarargsOnlyInt()", ""));

            RunVarargAssertion(
                env,
                milestone,
                MakePair("VarargsW1Param('abc', 1.0, 2.0)", "\"abc\",1.0d,2.0d"),
                MakePair("VarargsW1Param('abc', 1, 2)", "\"abc\",1.0d,2.0d"),
                MakePair("VarargsW1Param('abc', 1)", "\"abc\",1.0d"),
                MakePair("VarargsW1Param('abc')", "\"abc\"")
            );

            RunVarargAssertion(
                env,
                milestone,
                MakePair("VarargsW2Param(1, 2.0, 3L, 4L)", "1,2.0d,3L,4L"),
                MakePair("VarargsW2Param(1, 2.0, 3L)", "1,2.0d,3L"),
                MakePair("VarargsW2Param(1, 2.0)", "1,2.0d"),
                MakePair("VarargsW2Param(1, 2.0, 3, 4L)", "1,2.0d,3L,4L"),
                MakePair("VarargsW2Param(1, 2.0, 3L, 4L)", "1,2.0d,3L,4L"),
                MakePair("VarargsW2Param(1, 2.0, 3, 4)", "1,2.0d,3L,4L"),
                MakePair("VarargsW2Param(1, 2.0, 3L, 4)", "1,2.0d,3L,4L"));

            RunVarargAssertion(
                env,
                milestone,
                MakePair("VarargsOnlyWCtx(1, 2, 3)", "CTX+1,2,3"),
                MakePair("VarargsOnlyWCtx(1, 2)", "CTX+1,2"),
                MakePair("VarargsOnlyWCtx(1)", "CTX+1"),
                MakePair("VarargsOnlyWCtx()", "CTX+"));

            RunVarargAssertion(
                env,
                milestone,
                MakePair("VarargsW1ParamWCtx('a', 1, 2, 3)", "CTX+a,1,2,3"),
                MakePair("VarargsW1ParamWCtx('a', 1, 2)", "CTX+a,1,2"),
                MakePair("VarargsW1ParamWCtx('a', 1)", "CTX+a,1"),
                MakePair("VarargsW1ParamWCtx('a')", "CTX+a,"));

            RunVarargAssertion(
                env,
                milestone,
                MakePair("VarargsW2ParamWCtx('a', 'b', 1, 2, 3)", "CTX+a,b,1,2,3"),
                MakePair("VarargsW2ParamWCtx('a', 'b', 1, 2)", "CTX+a,b,1,2"),
                MakePair("VarargsW2ParamWCtx('a', 'b', 1)", "CTX+a,b,1"),
                MakePair("VarargsW2ParamWCtx('a', 'b')", "CTX+a,b,"),
                MakePair(typeof(SupportSingleRowFunction).FullName + ".VarargsW2ParamWCtx('a', 'b')", "CTX+a,b,"));

            RunVarargAssertion(
                env,
                milestone,
                MakePair("VarargsOnlyObject('a', 1, new BigInteger(2))", "\"a\",1,2"),
                MakePair("VarargsOnlyNumber(1f, 2L, 3, new BigInteger(4))", "1.0f,2L,3,4"));

            RunVarargAssertion(
                env,
                milestone,
                MakePair("VarargsOnlyNumber(1f, 2L, 3, new BigInteger(4))", "1.0f,2L,3,4"));

            RunVarargAssertion(
                env,
                milestone,
                MakePair(
                    "VarargsOnlyISupportBaseAB(new " + typeof(ISupportBImpl).FullName + "('a', 'b'))",
                    "ISupportBImpl{ValueB='a', ValueBaseAB='b'}"));

            // tests for array-passthru
            RunVarargAssertion(
                env,
                milestone,
                MakePair("VarargsOnlyString({'a'})", "\"a\""),
                MakePair("VarargsOnlyString({'a', 'b'})", "\"a\",\"b\""),
                MakePair("VarargsOnlyObject({'a', 'b'})", "\"a\",\"b\""),
                MakePair("VarargsOnlyObject({})", ""),
                MakePair("VarargsObjectsWCtx({1, 'a'})", "CTX+1,\"a\""),
                MakePair("VarargsW1ParamObjectsWCtx(1, {'a', 1})", "CTX+,1,\"a\",1")
            );

            // try Arrays.asList
            TryAssertionArraysAsList(env, milestone);

            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                "select VarargsOnlyInt(1, null) from SupportBean",
                "Failed to validate select-clause expression 'VarargsOnlyInt(1,null)': Could not find static method");

            RunVarargAssertion(
                env,
                milestone,
                MakePair("VarargsOnlyBoxedFloat(cast(1, byte), cast(2, short), null, 3)", "1.0f,2.0f,null,3.0f"));
            RunVarargAssertion(
                env,
                milestone,
                MakePair("VarargsOnlyBoxedShort(null, cast(1, byte))", "null,1"));
            RunVarargAssertion(
                env,
                milestone,
                MakePair("VarargsOnlyBoxedByte(null, cast(1, byte))", "null,1"));

            // test exact match takes priority over varargs
            RunVarargAssertion(
                env,
                milestone,
                MakePair("VarargOverload()", "many"),
                MakePair("VarargOverload(1)", "P1"),
                MakePair("VarargOverload(1, 2)", "P2"),
                MakePair("VarargOverload(1, 2, 3)", "p3"),
                MakePair("VarargOverload(1, 2, 3, 4)", "many")
            );
        }

        private void RunVarargAssertion(
            RegressionEnvironment env,
            AtomicLong milestone,
            params UniformPair<string>[] pairs)
        {
            var buf = new StringBuilder();
            buf.Append("@name('test') select ");
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
            var epl = "@name('s0') select " +
                      "com.espertech.esper.compat.collections.Collections.List(\"a\") as c0, " +
                      "com.espertech.esper.compat.collections.Collections.List({\"a\"}) as c1, " +
                      "com.espertech.esper.compat.collections.Collections.List(\"a\", \"b\") as c2, " +
                      "com.espertech.esper.compat.collections.Collections.List({\"a\", \"b\"}) as c3 " +
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