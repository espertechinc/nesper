///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreNewStruct
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ExprCoreNewStructNewWRepresentation());
            execs.Add(new ExprCoreNewStructDefaultColumnsAndSODA());
            execs.Add(new ExprCoreNewStructNewWithCase());
            execs.Add(new ExprCoreNewStructInvalid());
            return execs;
        }

        private static void TryAssertionDefault(RegressionEnvironment env)
        {
            Assert.AreEqual(typeof(IDictionary<string, object>), env.Statement("s0").EventType.GetPropertyType("val0"));
            var fragType = env.Statement("s0").EventType.GetFragmentType("val0");
            Assert.IsFalse(fragType.IsIndexed);
            Assert.IsFalse(fragType.IsNative);
            Assert.AreEqual(typeof(string), fragType.FragmentType.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(int?), fragType.FragmentType.GetPropertyType("IntPrimitive"));
            Assert.AreEqual(typeof(string), fragType.FragmentType.GetPropertyType("col2"));

            var fieldsInner = "theString,IntPrimitive,col2".SplitCsv();
            env.SendEventBean(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                fieldsInner,
                null,
                null,
                null);

            env.SendEventBean(new SupportBean("A", 2));
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                fieldsInner,
                "Q",
                2,
                "AA");

            env.SendEventBean(new SupportBean("B", 3));
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                fieldsInner,
                "B",
                10,
                "BB");
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            string epl,
            AtomicLong milestone)
        {
            env.CompileDeploy(epl).AddListener("s0").Milestone(milestone.GetAndIncrement());

            Assert.AreEqual(typeof(IDictionary<string, object>), env.Statement("s0").EventType.GetPropertyType("val0"));
            var fragType = env.Statement("s0").EventType.GetFragmentType("val0");
            Assert.IsFalse(fragType.IsIndexed);
            Assert.IsFalse(fragType.IsNative);
            Assert.AreEqual(typeof(string), fragType.FragmentType.GetPropertyType("col1"));
            Assert.AreEqual(typeof(int?), fragType.FragmentType.GetPropertyType("col2"));

            var fieldsInner = "col1,col2".SplitCsv();
            env.SendEventBean(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                fieldsInner,
                "Z",
                30);

            env.SendEventBean(new SupportBean("A", 2));
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                fieldsInner,
                "X",
                10);

            env.SendEventBean(new SupportBean("B", 3));
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                fieldsInner,
                "Y",
                20);

            env.SendEventBean(new SupportBean("C", 4));
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                fieldsInner,
                null,
                null);

            env.UndeployAll();
        }

        private static void TryAssertionNewWRepresentation(
            RegressionEnvironment env,
            EventRepresentationChoice rep,
            AtomicLong milestone)
        {
            var epl = rep.GetAnnotationText() +
                      "@Name('s0') select new { TheString = 'x' || TheString || 'x', IntPrimitive = IntPrimitive + 2} as val0 from SupportBean as sb";
            env.CompileDeploy(epl).AddListener("s0").Milestone(milestone.GetAndIncrement());

            Assert.AreEqual(
                rep.IsAvroEvent() ? typeof(GenericRecord) : typeof(IDictionary<string, object>),
                env.Statement("s0").EventType.GetPropertyType("val0"));
            var fragType = env.Statement("s0").EventType.GetFragmentType("val0");
            Assert.IsFalse(fragType.IsIndexed);
            Assert.IsFalse(fragType.IsNative);
            Assert.AreEqual(typeof(string), fragType.FragmentType.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(int?), fragType.FragmentType.GetPropertyType("IntPrimitive").GetBoxedType());

            var fieldsInner = "theString,IntPrimitive".SplitCsv();
            env.SendEventBean(new SupportBean("E1", -5));
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            if (rep.IsAvroEvent()) {
                SupportAvroUtil.AvroToJson(@event);
                var inner = (GenericRecord) @event.Get("val0");
                Assert.AreEqual("xE1x", inner.Get("TheString"));
                Assert.AreEqual(-3, inner.Get("IntPrimitive"));
            }
            else {
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) @event.Get("val0"),
                    fieldsInner,
                    "xE1x",
                    -3);
            }

            env.UndeployAll();
        }

        internal class ExprCoreNewStructNewWRepresentation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionNewWRepresentation(env, rep, milestone);
                }
            }
        }

        internal class ExprCoreNewStructDefaultColumnsAndSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "case TheString" +
                          " when \"A\" then new{TheString=\"Q\",IntPrimitive,col2=TheString||\"A\"}" +
                          " when \"B\" then new{TheString,IntPrimitive=10,col2=TheString||\"B\"} " +
                          "end as val0 from SupportBean as sb";

                env.CompileDeploy(epl).AddListener("s0");
                TryAssertionDefault(env);
                env.UndeployAll();

                env.EplToModelCompileDeploy(epl).AddListener("s0");
                TryAssertionDefault(env);
                env.UndeployAll();

                // test to-expression string
                epl = "@Name('s0') select " +
                      "case TheString" +
                      " when \"A\" then new{TheString=\"Q\",IntPrimitive,col2=TheString||\"A\" }" +
                      " when \"B\" then new{TheString,IntPrimitive = 10,col2=TheString||\"B\" } " +
                      "end from SupportBean as sb";
                env.CompileDeploy(epl).AddListener("s0");
                Assert.AreEqual(
                    "case TheString when \"A\" then new{TheString=\"Q\",IntPrimitive,col2=TheString||\"A\"} when \"B\" then new{TheString,IntPrimitive=10,col2=TheString||\"B\"} end",
                    env.Statement("s0").EventType.PropertyNames[0]);
                env.UndeployAll();
            }
        }

        internal class ExprCoreNewStructNewWithCase : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@Name('s0') select " +
                          "case " +
                          "  when TheString = 'A' then new { col1 = 'X', col2 = 10 } " +
                          "  when TheString = 'B' then new { col1 = 'Y', col2 = 20 } " +
                          "  when TheString = 'C' then new { col1 = null, col2 = null } " +
                          "  else new { col1 = 'Z', col2 = 30 } " +
                          "end as val0 from SupportBean sb";
                TryAssertion(env, epl, milestone);

                epl = "@Name('s0') select " +
                      "case TheString " +
                      "  when 'A' then new { col1 = 'X', col2 = 10 } " +
                      "  when 'B' then new { col1 = 'Y', col2 = 20 } " +
                      "  when 'C' then new { col1 = null, col2 = null } " +
                      "  else new{ col1 = 'Z', col2 = 30 } " +
                      "end as val0 from SupportBean sb";
                TryAssertion(env, epl, milestone);
            }
        }

        internal class ExprCoreNewStructInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select case when true then new { col1 = 'a' } else 1 end from SupportBean";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'case when true then new{col1=\"a\"} e...(44 chars)': Case node 'when' expressions require that all results either return a single value or a Map-type (new-operator) value, check the else-condition [select case when true then new { col1 = 'a' } else 1 end from SupportBean]");

                epl = "select case when true then new { col1 = 'a' } when false then 1 end from SupportBean";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'case when true then new{col1=\"a\"} w...(55 chars)': Case node 'when' expressions require that all results either return a single value or a Map-type (new-operator) value, check when-condition number 1 [select case when true then new { col1 = 'a' } when false then 1 end from SupportBean]");

                epl = "select case when true then new { col1 = 'a' } else new { col1 = 1 } end from SupportBean";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'case when true then new{col1=\"a\"} e...(54 chars)': Incompatible case-when return types by new-operator in case-when number 1: Type by name 'Case-when number 1' in property 'col1' expected class System.String but receives class System.Integer [select case when true then new { col1 = 'a' } else new { col1 = 1 } end from SupportBean]");

                epl = "select case when true then new { col1 = 'a' } else new { col2 = 'a' } end from SupportBean";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'case when true then new{col1=\"a\"} e...(56 chars)': Incompatible case-when return types by new-operator in case-when number 1: The property 'col1' is not provIded but required [select case when true then new { col1 = 'a' } else new { col2 = 'a' } end from SupportBean]");

                epl = "select case when true then new { col1 = 'a', col1 = 'b' } end from SupportBean";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'case when true then new{col1=\"a\",co...(46 chars)': Failed to validate new-keyword property names, property 'col1' has already been declared [select case when true then new { col1 = 'a', col1 = 'b' } end from SupportBean]");
            }
        }
    }
} // end of namespace