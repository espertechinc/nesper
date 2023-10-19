///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;

using NEsper.Avro.Extensions;
using NEsper.Avro.Support;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreNewStruct
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithNewWRepresentation(execs);
            WithDefaultColumnsAndSODA(execs);
            WithNewWithCase(execs);
            WithInvalid(execs);
            WithWithBacktick(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWithBacktick(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreNewStructWithBacktick());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreNewStructInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithNewWithCase(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreNewStructNewWithCase());
            return execs;
        }

        public static IList<RegressionExecution> WithDefaultColumnsAndSODA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreNewStructDefaultColumnsAndSODA());
            return execs;
        }

        public static IList<RegressionExecution> WithNewWRepresentation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreNewStructNewWRepresentation());
            return execs;
        }

        private class ExprCoreNewStructWithBacktick : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpressions(fields, "new { `a` = theString, `b.c` = theString, `}` = theString }");

                builder.WithAssertion(new SupportBean("E1", 0))
                    .Verify(
                        "c0",
                        actual => {
                            var c0 = (IDictionary<string, object>)actual;
                            Assert.AreEqual("E1", c0.Get("a"));
                            Assert.AreEqual("E1", c0.Get("b.c"));
                            Assert.AreEqual("E1", c0.Get("}"));
                        });

                builder.Run(env);
                env.UndeployAll();
            }
        }

        private class ExprCoreNewStructNewWRepresentation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionNewWRepresentation(env, rep, milestone);
                }
            }
        }

        private class ExprCoreNewStructDefaultColumnsAndSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "case theString" +
                          " when \"A\" then new{theString=\"Q\",intPrimitive,col2=theString||\"A\"}" +
                          " when \"B\" then new{theString,intPrimitive=10,col2=theString||\"B\"} " +
                          "end as val0 from SupportBean as sb";

                env.CompileDeploy(epl).AddListener("s0");
                TryAssertionDefault(env);
                env.UndeployAll();

                env.EplToModelCompileDeploy(epl).AddListener("s0");
                TryAssertionDefault(env);
                env.UndeployAll();

                // test to-expression string
                epl = "@name('s0') select " +
                      "case theString" +
                      " when \"A\" then new{theString=\"Q\",intPrimitive,col2=theString||\"A\" }" +
                      " when \"B\" then new{theString,intPrimitive = 10,col2=theString||\"B\" } " +
                      "end from SupportBean as sb";
                env.CompileDeploy(epl).AddListener("s0");
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(
                        "case theString when \"A\" then new{theString=\"Q\",intPrimitive,col2=theString||\"A\"} when \"B\" then new{theString,intPrimitive=10,col2=theString||\"B\"} end",
                        statement.EventType.PropertyNames[0]));
                env.UndeployAll();
            }
        }

        private static void TryAssertionDefault(RegressionEnvironment env)
        {
            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual(typeof(IDictionary<string, object>), statement.EventType.GetPropertyType("val0"));
                    var fragType = statement.EventType.GetFragmentType("val0");
                    Assert.IsFalse(fragType.IsIndexed);
                    Assert.IsFalse(fragType.IsNative);
                    Assert.AreEqual(typeof(string), fragType.FragmentType.GetPropertyType("theString"));
                    Assert.AreEqual(typeof(int?), fragType.FragmentType.GetPropertyType("intPrimitive"));
                    Assert.AreEqual(typeof(string), fragType.FragmentType.GetPropertyType("col2"));
                });

            var fieldsInner = "theString,intPrimitive,col2".SplitCsv();
            env.SendEventBean(new SupportBean("E1", 1));
            AssertPropsMap(env, fieldsInner, new object[] { null, null, null });

            env.SendEventBean(new SupportBean("A", 2));
            AssertPropsMap(env, fieldsInner, new object[] { "Q", 2, "AA" });

            env.SendEventBean(new SupportBean("B", 3));
            AssertPropsMap(env, fieldsInner, new object[] { "B", 10, "BB" });
        }

        private class ExprCoreNewStructNewWithCase : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select " +
                          "case " +
                          "  when theString = 'A' then new { col1 = 'X', col2 = 10 } " +
                          "  when theString = 'B' then new { col1 = 'Y', col2 = 20 } " +
                          "  when theString = 'C' then new { col1 = null, col2 = null } " +
                          "  else new { col1 = 'Z', col2 = 30 } " +
                          "end as val0 from SupportBean sb";
                TryAssertion(env, epl, milestone);

                epl = "@name('s0') select " +
                      "case theString " +
                      "  when 'A' then new { col1 = 'X', col2 = 10 } " +
                      "  when 'B' then new { col1 = 'Y', col2 = 20 } " +
                      "  when 'C' then new { col1 = null, col2 = null } " +
                      "  else new{ col1 = 'Z', col2 = 30 } " +
                      "end as val0 from SupportBean sb";
                TryAssertion(env, epl, milestone);
            }
        }

        private class ExprCoreNewStructInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select case when true then new { col1 = 'a' } else 1 end from SupportBean";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'case when true then new{col1=\"a\"} e...(44 chars)': Case node 'when' expressions require that all results either return a single value or a Map-type (new-operator) value, check the else-condition [select case when true then new { col1 = 'a' } else 1 end from SupportBean]");

                epl = "select case when true then new { col1 = 'a' } when false then 1 end from SupportBean";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'case when true then new{col1=\"a\"} w...(55 chars)': Case node 'when' expressions require that all results either return a single value or a Map-type (new-operator) value, check when-condition number 1 [select case when true then new { col1 = 'a' } when false then 1 end from SupportBean]");

                epl = "select case when true then new { col1 = 'a' } else new { col1 = 1 } end from SupportBean";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'case when true then new{col1=\"a\"} e...(54 chars)': Incompatible case-when return types by new-operator in case-when number 1: Type by name 'Case-when number 1' in property 'col1' expected String but receives Integer [select case when true then new { col1 = 'a' } else new { col1 = 1 } end from SupportBean]");

                epl = "select case when true then new { col1 = 'a' } else new { col2 = 'a' } end from SupportBean";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'case when true then new{col1=\"a\"} e...(56 chars)': Incompatible case-when return types by new-operator in case-when number 1: Type by name 'Case-when number 1' in property 'col2' property name not found in target");

                epl = "select case when true then new { col1 = 'a', col1 = 'b' } end from SupportBean";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate select-clause expression 'case when true then new{col1=\"a\",co...(46 chars)': Failed to validate new-keyword property names, property 'col1' has already been declared [select case when true then new { col1 = 'a', col1 = 'b' } end from SupportBean]");
            }
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            string epl,
            AtomicLong milestone)
        {
            env.CompileDeploy(epl).AddListener("s0").Milestone(milestone.GetAndIncrement());

            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual(typeof(IDictionary<string, object>), statement.EventType.GetPropertyType("val0"));
                    var fragType = statement.EventType.GetFragmentType("val0");
                    Assert.IsFalse(fragType.IsIndexed);
                    Assert.IsFalse(fragType.IsNative);
                    Assert.AreEqual(typeof(string), fragType.FragmentType.GetPropertyType("col1"));
                    Assert.AreEqual(typeof(int?), fragType.FragmentType.GetPropertyType("col2"));
                });

            var fieldsInner = "col1,col2".SplitCsv();
            env.SendEventBean(new SupportBean("E1", 1));
            AssertPropsMap(env, fieldsInner, new object[] { "Z", 30 });

            env.SendEventBean(new SupportBean("A", 2));
            AssertPropsMap(env, fieldsInner, new object[] { "X", 10 });

            env.SendEventBean(new SupportBean("B", 3));
            AssertPropsMap(env, fieldsInner, new object[] { "Y", 20 });

            env.SendEventBean(new SupportBean("C", 4));
            AssertPropsMap(env, fieldsInner, new object[] { null, null });

            env.UndeployAll();
        }

        private static void TryAssertionNewWRepresentation(
            RegressionEnvironment env,
            EventRepresentationChoice rep,
            AtomicLong milestone)
        {
            var epl = rep.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvided)) +
                      "@name('s0') select new { theString = 'x' || theString || 'x', intPrimitive = intPrimitive + 2} as val0 from SupportBean as sb";
            env.CompileDeploy(epl).AddListener("s0").Milestone(milestone.GetAndIncrement());

            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual(
                        rep.IsAvroEvent() ? typeof(GenericRecord) : typeof(IDictionary<string, object>),
                        statement.EventType.GetPropertyType("val0"));
                    var fragType = statement.EventType.GetFragmentType("val0");
                    if (rep == EventRepresentationChoice.JSONCLASSPROVIDED) {
                        Assert.IsNull(fragType);
                    }
                    else {
                        Assert.IsFalse(fragType.IsIndexed);
                        Assert.IsFalse(fragType.IsNative);
                        Assert.AreEqual(typeof(string), fragType.FragmentType.GetPropertyType("theString"));
                        Assert.AreEqual(
                            typeof(int?),
                            Boxing.GetBoxedType(fragType.FragmentType.GetPropertyType("intPrimitive")));
                    }
                });

            var fieldsInner = "theString,intPrimitive".SplitCsv();
            env.SendEventBean(new SupportBean("E1", -5));
            env.AssertEventNew(
                "s0",
                @event => {
                    if (rep.IsAvroEvent()) {
                        SupportAvroUtil.AvroToJson(@event);
                        var inner = (GenericRecord)@event.Get("val0");
                        Assert.AreEqual("xE1x", inner.Get("theString"));
                        Assert.AreEqual(-3, inner.Get("intPrimitive"));
                    }
                    else {
                        EPAssertionUtil.AssertPropsMap(
                            (IDictionary<string, object>)@event.Get("val0"),
                            fieldsInner,
                            new object[] { "xE1x", -3 });
                    }
                });

            env.UndeployAll();
        }

        [Serializable]
        public class MyLocalJsonProvided
        {
            public IDictionary<string, object> val0;
        }

        private static void AssertPropsMap(
            RegressionEnvironment env,
            string[] fieldsInner,
            object[] expecteds)
        {
            env.AssertEventNew(
                "s0",
                @event => EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>)@event.Get("val0"),
                    fieldsInner,
                    expecteds));
        }
    }
} // end of namespace