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

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client.scopetest;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherUpdateIStream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithBean(execs);
            WithFieldUpdateOrder(execs);
            WithInvalid(execs);
            WithInsertIntoWBeanWhere(execs);
            WithInsertIntoWMapNoWhere(execs);
            WithFieldsWithPriority(execs);
            WithNamedWindow(execs);
            WithTypeWidener(execs);
            WithInsertDirectBeanTypeInheritance(execs);
            WithSODA(execs);
            WithXMLEvent(execs);
            WithWrappedObject(execs);
            WithSendRouteSenderPreprocess(execs);
            WithCopyMethod(execs);
            WithSubquery(execs);
            WithUnprioritizedOrder(execs);
            WithListenerDeliveryMultiupdate(execs);
            WithListenerDeliveryMultiupdateMixed(execs);
            WithSubqueryMultikeyWArray(execs);
            WithMapIndexProps(execs);
            WithArrayElement(execs);
            WithArrayElementBoxed(execs);
            WithArrayElementInvalid(execs);
            WithExpression(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithExpression(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateExpression());
            return execs;
        }

        public static IList<RegressionExecution> WithArrayElementInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateArrayElementInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithArrayElementBoxed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateArrayElementBoxed());
            return execs;
        }

        public static IList<RegressionExecution> WithArrayElement(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateArrayElement());
            return execs;
        }

        public static IList<RegressionExecution> WithMapIndexProps(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateMapIndexProps());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateSubqueryMultikeyWArray());
            return execs;
        }

        public static IList<RegressionExecution> WithListenerDeliveryMultiupdateMixed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateListenerDeliveryMultiupdateMixed());
            return execs;
        }

        public static IList<RegressionExecution> WithListenerDeliveryMultiupdate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateListenerDeliveryMultiupdate());
            return execs;
        }

        public static IList<RegressionExecution> WithUnprioritizedOrder(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateUnprioritizedOrder());
            return execs;
        }

        public static IList<RegressionExecution> WithSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithCopyMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateCopyMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithSendRouteSenderPreprocess(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateSendRouteSenderPreprocess());
            return execs;
        }

        public static IList<RegressionExecution> WithWrappedObject(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateWrappedObject());
            return execs;
        }

        public static IList<RegressionExecution> WithXMLEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateXMLEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithSODA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateSODA());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertDirectBeanTypeInheritance(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateInsertDirectBeanTypeInheritance());
            return execs;
        }

        public static IList<RegressionExecution> WithTypeWidener(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateTypeWidener());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithFieldsWithPriority(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateFieldsWithPriority());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertIntoWMapNoWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateInsertIntoWMapNoWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertIntoWBeanWhere(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateInsertIntoWBeanWhere());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithFieldUpdateOrder(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateFieldUpdateOrder());
            return execs;
        }

        public static IList<RegressionExecution> WithBean(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateBean());
            return execs;
        }

        internal class EPLOtherUpdateArrayElementInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplSchema = "@Name('create') create schema MySchema(doublearray double[primitive], intarray int[primitive], notAnArray int)";
                env.Compile(eplSchema, path);
                // invalid property
                TryInvalidCompile(
                    env,
                    path,
                    "update istream MySchema set c1[0]=1",
                    "Failed to validate assignment expression 'c1[0]=1': Property 'c1[0]' is not available for write access");
                // index expression is not Integer
                TryInvalidCompile(
                    env,
                    path,
                    "update istream MySchema set doublearray[null]=1",
                    "Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'null' returns 'null (any type)' for expression 'doublearray'");
                // type incompatible cannot assign
                TryInvalidCompile(
                    env,
                    path,
                    "update istream MySchema set intarray[notAnArray]='x'",
                    "Failed to validate assignment expression 'intarray[notAnArray]=\"x\"': Invalid assignment of column '\"x\"' of type 'System.String' to event property 'intarray' typed as 'System.Int32', column and parameter types mismatch");
                // not-an-array
                TryInvalidCompile(
                    env,
                    path,
                    "update istream MySchema set notAnArray[notAnArray]=1",
                    "Failed to validate assignment expression 'notAnArray[notAnArray]=1': Property 'notAnArray' type is not array");
                // not found
                TryInvalidCompile(
                    env,
                    path,
                    "update istream MySchema set dummy[IntPrimitive]=1",
                    "Failed to validate update assignment expression 'IntPrimitive': Property named 'IntPrimitive' is not valid in any stream");
                path.Clear();
                // runtime-behavior for index-overflow and null-array and null-index and
                var epl = "@Name('create') @public @buseventtype create schema MySchema(doublearray double[primitive], indexvalue int, rhsvalue int);\n" +
                          "update istream MySchema set doublearray[indexvalue]=rhsvalue;\n";
                env.CompileDeploy(epl);
                // index returned is too large
                try {
                    env.SendEventMap(CollectionUtil.BuildMap("doublearray", new double[3], "indexvalue", 10, "rhsvalue", 1), "MySchema");
                    Assert.Fail();
                }
                catch (Exception ex) {
                    Assert.IsTrue(ex.Message.Contains("Array length 3 less than index 10 for property 'doublearray'"));
                }

                // index returned null
                env.SendEventMap(CollectionUtil.BuildMap("doublearray", new double[3], "indexvalue", null, "rhsvalue", 1), "MySchema");
                // rhs returned null for array-of-primitive
                env.SendEventMap(CollectionUtil.BuildMap("doublearray", new double[3], "indexvalue", 1, "rhsvalue", null), "MySchema");
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create map schema MyEvent(a int, b int);\n" +
                    "inlined_class \"\"\"\n" +
                    "  using com.espertech.esper.compat.collections;\n" +
                    "  public class Helper {\n" +
                    "    public static void Swap(System.Collections.Generic.IDictionary<string, object> @event) {\n" +
                    "      object temp = @event.Get(\"a\");\n" +
                    "      @event.Put(\"a\", @event.Get(\"b\"));\n" +
                    "      @event.Put(\"b\", temp);\n" +
                    "    }\n" +
                    "  }\n" +
                    "\"\"\"\n" +
                    "update istream MyEvent as me set Helper.Swap(me);\n" +
                    "@Name('s0') select * from MyEvent;\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventMap(CollectionUtil.BuildMap("a", 1, "b", 10), "MyEvent");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "a,b".SplitCsv(), new object[] {10, 1});
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateArrayElementBoxed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema MyEvent(dbls double[]);\n" +
                    "update istream MyEvent set dbls[3-2] = 1;\n" +
                    "@Name('s0') select dbls as c0 from MyEvent;\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventMap(Collections.SingletonDataMap("dbls", new double?[3]), "MyEvent");
                CollectionAssert.AreEquivalent(
                    new double?[] {null, 1d, null},
                    env.Listener("s0").AssertOneGetNewAndReset().Get("c0").UnwrapIntoArray<double?>());
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateArrayElement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema Arriving(position int, intarray int[], objectarray System.Object[]);\n" +
                          "update istream Arriving set intarray[position] = 1, objectarray[position] = 1;\n" +
                          "@Name('s0') select * from Arriving;\n";
                env.CompileDeploy(epl).AddListener("s0");
                AssertUpdate(env, 1, new[] {0, 1, 0}, new object[] {null, 1, null});
                AssertUpdate(env, 0, new[] {1, 0, 0}, new object[] {1, null, null});
                AssertUpdate(env, 2, new[] {0, 0, 1}, new object[] {null, null, 1});
                env.UndeployAll();
            }

            private void AssertUpdate(
                RegressionEnvironment env,
                int position,
                int[] expectedInt,
                object[] expectedObj)
            {
                env.SendEventMap(CollectionUtil.BuildMap("position", position, "intarray", new int[3], "objectarray", new object[3]), "Arriving");
                var @out = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(@out, "position,intarray,objectarray".SplitCsv(), new object[] {position, expectedInt, expectedObj});
            }
        }

        internal class EPLOtherUpdateSubqueryMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema Arriving(Value int);\n" +
                    "update istream Arriving set Value = (select sum(Value) as c0 from SupportEventWithIntArray#keepall group by Array);\n" +
                    "@Name('s0') select * from Arriving;\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportEventWithIntArray("E1", new[] {1, 2}, 10));
                env.SendEventBean(new SupportEventWithIntArray("E2", new[] {1, 2}, 11));
                env.Milestone(0);
                AssertUpdate(env, 21);
                env.SendEventBean(new SupportEventWithIntArray("E3", new[] {1, 2}, 12));
                AssertUpdate(env, 33);
                env.Milestone(1);
                env.SendEventBean(new SupportEventWithIntArray("E4", new[] {1}, 13));
                AssertUpdate(env, null);
                env.UndeployAll();
            }

            private void AssertUpdate(
                RegressionEnvironment env,
                int? expected)
            {
                env.SendEventMap(new Dictionary<string, object>(), "Arriving");
                Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("Value"));
            }
        }

        public class EPLOtherUpdateBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var text = "@Name('Insert') insert into MyStream select * from SupportBean";
                env.CompileDeploy(text, path).AddListener("Insert");
                text = "@Name('Update') update istream MyStream set IntPrimitive=10, TheString='O_' || TheString where IntPrimitive=1";
                env.CompileDeploy(text, path).AddListener("Update");
                text = "@Name('Select') select * from MyStream";
                env.CompileDeploy(text, path).AddListener("Select");
                var fields = "TheString,IntPrimitive".SplitCsv();
                env.SendEventBean(new SupportBean("E1", 9));
                EPAssertionUtil.AssertProps(env.Listener("Select").AssertOneGetNewAndReset(), fields, new object[] {"E1", 9});
                EPAssertionUtil.AssertProps(env.Listener("Insert").AssertOneGetNewAndReset(), fields, new object[] {"E1", 9});
                Assert.IsFalse(env.Listener("Update").IsInvoked);
                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertProps(env.Listener("Select").AssertOneGetNewAndReset(), fields, new object[] {"O_E2", 10});
                EPAssertionUtil.AssertProps(env.Listener("Insert").AssertOneGetNewAndReset(), fields, new object[] {"E2", 1});
                EPAssertionUtil.AssertProps(env.Listener("Update").LastOldData[0], fields, new object[] {"E2", 1});
                EPAssertionUtil.AssertProps(env.Listener("Update").LastNewData[0], fields, new object[] {"O_E2", 10});
                env.Listener("Update").Reset();
                env.Milestone(0);
                env.SendEventBean(new SupportBean("E3", 1));
                EPAssertionUtil.AssertProps(env.Listener("Select").AssertOneGetNewAndReset(), fields, new object[] {"O_E3", 10});
                EPAssertionUtil.AssertProps(env.Listener("Insert").AssertOneGetNewAndReset(), fields, new object[] {"E3", 1});
                EPAssertionUtil.AssertProps(env.Listener("Update").LastOldData[0], fields, new object[] {"E3", 1});
                EPAssertionUtil.AssertProps(env.Listener("Update").LastNewData[0], fields, new object[] {"O_E3", 10});
                env.Listener("Update").Reset();
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateFieldUpdateOrder : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('update') update istream SupportBean " + "set IntPrimitive=myvar, IntBoxed=IntPrimitive");
                Assert.AreEqual(StatementType.UPDATE, env.Statement("update").GetProperty(StatementProperty.STATEMENTTYPE));
                env.CompileDeploy("@Name('s0') select * from SupportBean").AddListener("s0");
                var fields = "IntPrimitive,IntBoxed".SplitCsv();
                env.SendEventBean(MakeSupportBean("E1", 1, 2));
                EPAssertionUtil.AssertProps(env.Listener("s0").GetAndResetLastNewData()[0], fields, new object[] {10, 1});
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("insert into SupportBeanStream select * from SupportBean", path);
                env.CompileDeploy("insert into SupportBeanStreamTwo select * from pattern[a=SupportBean -> b=SupportBean]", path);
                env.CompileDeploy("insert into SupportBeanStreamRO select * from SupportBeanReadOnly", path);
                TryInvalidCompile(
                    env,
                    path,
                    "update istream SupportBeanStream set IntPrimitive=LongPrimitive",
                    "Failed to validate assignment expression 'IntPrimitive=LongPrimitive': Invalid assignment of column 'LongPrimitive' of type 'System.Nullable<System.Int64>' to event property 'IntPrimitive' typed as 'System.Int32', column and parameter types mismatch [update istream SupportBeanStream set IntPrimitive=LongPrimitive]");
                TryInvalidCompile(
                    env,
                    path,
                    "update istream SupportBeanStream set xxx='abc'",
                    "Failed to validate assignment expression 'xxx=\"abc\"': Property 'xxx' is not available for write access [update istream SupportBeanStream set xxx='abc']");
                TryInvalidCompile(
                    env,
                    path,
                    "update istream SupportBeanStream set IntPrimitive=null",
                    "Failed to validate assignment expression 'IntPrimitive=null': Invalid assignment of column 'null' of null type to event property 'IntPrimitive' typed as 'System.Int32', nullable type mismatch [update istream SupportBeanStream set IntPrimitive=null]");
                TryInvalidCompile(
                    env,
                    path,
                    "update istream SupportBeanStreamTwo set a.IntPrimitive=10",
                    "Failed to validate assignment expression 'a.IntPrimitive=10': Property 'a.IntPrimitive' is not available for write access [update istream SupportBeanStreamTwo set a.IntPrimitive=10]");
                TryInvalidCompile(
                    env,
                    path,
                    "update istream SupportBeanStreamRO set side='a'",
                    "Failed to validate assignment expression 'side=\"a\"': Property 'side' is not available for write access [update istream SupportBeanStreamRO set side='a']");
                TryInvalidCompile(
                    env,
                    path,
                    "update istream SupportBean set LongPrimitive=sum(IntPrimitive)",
                    "Aggregation functions may not be used within update-set [update istream SupportBean set LongPrimitive=sum(IntPrimitive)]");
                TryInvalidCompile(
                    env,
                    path,
                    "update istream SupportBean set LongPrimitive=LongPrimitive where sum(IntPrimitive) = 1",
                    "Aggregation functions may not be used within an update-clause [update istream SupportBean set LongPrimitive=LongPrimitive where sum(IntPrimitive) = 1]");
                TryInvalidCompile(
                    env,
                    path,
                    "update istream SupportBean set LongPrimitive=prev(1, LongPrimitive)",
                    "Failed to validate update assignment expression 'prev(1,LongPrimitive)': Previous function cannot be used in this context [update istream SupportBean set LongPrimitive=prev(1, LongPrimitive)]");
                TryInvalidCompile(
                    env,
                    path,
                    "update istream MyXmlEvent set abc=1",
                    "Failed to validate assignment expression 'abc=1': Property 'abc' is not available for write access [update istream MyXmlEvent set abc=1]");
                TryInvalidCompile(
                    env,
                    path,
                    "update istream SupportBeanErrorTestingOne set Value='1'",
                    "The update-clause requires the underlying event representation to support copy (via Serializable by default) [update istream SupportBeanErrorTestingOne set Value='1']");
                TryInvalidCompile(
                    env,
                    path,
                    "update istream SupportBean set LongPrimitive=(select P0 from MyMapTypeInv#lastevent where TheString=P3)",
                    "Failed to plan subquery number 1 querying MyMapTypeInv: Failed to validate filter expression 'TheString=P3': Property named 'TheString' must be prefixed by a stream name, use the stream name itself or use the as-clause to name the stream with the property in the format \"stream.property\" [update istream SupportBean set LongPrimitive=(select P0 from MyMapTypeInv#lastevent where TheString=P3)]");
                TryInvalidCompile(
                    env,
                    path,
                    "update istream XYZ.GYH set a=1",
                    "Failed to resolve event type, named window or table by name 'XYZ.GYH' [update istream XYZ.GYH set a=1]");
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateInsertIntoWBeanWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('insert') insert into MyStreamBW select * from SupportBean", path);
                env.AddListener("insert");
                env.CompileDeploy("@Name('update_1') update istream MyStreamBW set IntPrimitive=10, TheString='O_' || TheString where IntPrimitive=1", path);
                env.AddListener("update_1");
                env.CompileDeploy("@Name('s0') select * from MyStreamBW", path);
                env.AddListener("s0");
                var fields = "TheString,IntPrimitive".SplitCsv();
                env.SendEventBean(new SupportBean("E1", 9));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E1", 9});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E1", 9});
                Assert.IsFalse(env.Listener("update_1").IsInvoked);
                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"O_E2", 10});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E2", 1});
                EPAssertionUtil.AssertProps(env.Listener("update_1").AssertOneGetOld(), fields, new object[] {"E2", 1});
                EPAssertionUtil.AssertProps(env.Listener("update_1").AssertOneGetNew(), fields, new object[] {"O_E2", 10});
                env.Listener("update_1").Reset();
                env.SendEventBean(new SupportBean("E3", 2));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E3", 2});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E3", 2});
                Assert.IsFalse(env.Listener("update_1").IsInvoked);
                env.SendEventBean(new SupportBean("E4", 1));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"O_E4", 10});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E4", 1});
                EPAssertionUtil.AssertProps(env.Listener("update_1").AssertOneGetOld(), fields, new object[] {"E4", 1});
                EPAssertionUtil.AssertProps(env.Listener("update_1").AssertOneGetNew(), fields, new object[] {"O_E4", 10});
                env.CompileDeploy("@Name('update_2') update istream MyStreamBW as xyz set IntPrimitive=xyz.IntPrimitive + 1000 where IntPrimitive=2", path);
                env.AddListener("update_2");
                env.SendEventBean(new SupportBean("E5", 2));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E5", 1002});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E5", 2});
                EPAssertionUtil.AssertProps(env.Listener("update_2").AssertOneGetOld(), fields, new object[] {"E5", 2});
                EPAssertionUtil.AssertProps(env.Listener("update_2").AssertOneGetNew(), fields, new object[] {"E5", 1002});
                env.Listener("update_2").Reset();
                env.UndeployModuleContaining("update_1");
                env.SendEventBean(new SupportBean("E6", 1));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E6", 1});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E6", 1});
                Assert.IsFalse(env.Listener("update_2").IsInvoked);
                env.SendEventBean(new SupportBean("E7", 2));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E7", 1002});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E7", 2});
                EPAssertionUtil.AssertProps(env.Listener("update_2").AssertOneGetOld(), fields, new object[] {"E7", 2});
                EPAssertionUtil.AssertProps(env.Listener("update_2").AssertOneGetNew(), fields, new object[] {"E7", 1002});
                env.Listener("update_2").Reset();
                Assert.IsFalse(env.GetEnumerator("update_2").MoveNext());
                var listenerUpdate2 = env.Listener("update_2");
                env.Statement("update_2").RemoveAllListeners();
                env.SendEventBean(new SupportBean("E8", 2));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E8", 1002});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E8", 2});
                Assert.IsFalse(listenerUpdate2.IsInvoked);
                var subscriber = new SupportSubscriber();
                env.Statement("update_2").Subscriber = subscriber;
                env.SendEventBean(new SupportBean("E9", 2));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E9", 1002});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E9", 2});
                SupportBean.Compare(subscriber.OldDataListFlattened[0], "E9", 2);
                SupportBean.Compare(subscriber.NewDataListFlattened[0], "E9", 1002);
                subscriber.Reset();
                env.UndeployModuleContaining("update_2");
                env.SendEventBean(new SupportBean("E10", 2));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E10", 2});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E10", 2});
                env.CompileDeploy("@Name('update_3') update istream MyStreamBW set IntPrimitive=IntBoxed", path);
                env.AddListener("update_3");
                env.SendEventBean(new SupportBean("E11", 2));
                EPAssertionUtil.AssertProps(env.Listener("update_3").AssertOneGetNew(), fields, new object[] {"E11", 2});
                env.Listener("update_3").Reset();
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateInsertIntoWMapNoWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('insert') insert into MyStreamII select * from MyMapTypeII", path).AddListener("insert");
                var update = env.Compile("@Name('update') update istream MyStreamII set P0=P1, P1=P0", path);
                env.Deploy(update);
                env.CompileDeploy("@Name('s0') select * from MyStreamII", path).AddListener("s0");
                var fields = "P0,P1,P2".SplitCsv();
                env.SendEventMap(MakeMap("P0", 10L, "P1", 1L, "P2", 100L), "MyMapTypeII");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {1L, 10L, 100L});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {10L, 1L, 100L});
                env.UndeployModuleContaining("update");
                env.Deploy(update).AddListener("update");
                env.SendEventMap(MakeMap("P0", 5L, "P1", 4L, "P2", 101L), "MyMapTypeII");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {4L, 5L, 101L});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {5L, 4L, 101L});
                env.UndeployModuleContaining("update");
                env.SendEventMap(MakeMap("P0", 20L, "P1", 0L, "P2", 102L), "MyMapTypeII");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {20L, 0L, 102L});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {20L, 0L, 102L});
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateFieldsWithPriority : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionFieldsWithPriority(env, rep);
                }
            }
        }

        internal class EPLOtherUpdateInsertDirectBeanTypeInheritance : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl =
                    "create schema BaseInterface as " + typeof(BaseInterface).MaskTypeName() + ";\n" +
                    "create schema BaseOne as " + typeof(BaseOne).MaskTypeName() + ";\n" +
                    "create schema BaseOneA as " + typeof(BaseOneA).MaskTypeName() + ";\n" +
                    "create schema BaseOneB as " + typeof(BaseOneB).MaskTypeName() + ";\n" +
                    "create schema BaseTwo as " + typeof(BaseTwo).MaskTypeName() + ";\n";

                env.CompileDeploy(epl, path);

                // test update applies to child types via interface
                env.CompileDeploy("@Name('insert') insert into BaseOne select P0 as I, P1 as P from MyMapTypeIDB", path);
                env.CompileDeploy("@Name('a') update istream BaseInterface set I='XYZ' where I like 'E%'", path);
                env.CompileDeploy("@Name('s0') select * from BaseOne", path).AddListener("s0");
                var fields = "I,P".SplitCsv();

                env.SendEventMap(MakeMap("P0", "E1", "P1", "E1"), "MyMapTypeIDB");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"XYZ", "E1"});
                env.SendEventMap(MakeMap("P0", "F1", "P1", "E2"), "MyMapTypeIDB");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"F1", "E2"});
                env.CompileDeploy("@Priority(2) @Name('b') update istream BaseOne set I='BLANK'", path);
                env.SendEventMap(MakeMap("P0", "somevalue", "P1", "E3"), "MyMapTypeIDB");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"BLANK", "E3"});
                env.CompileDeploy("@Priority(3) @Name('c') update istream BaseOneA set I='FINAL'", path);
                env.SendEventMap(MakeMap("P0", "somevalue", "P1", "E4"), "MyMapTypeIDB");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"BLANK", "E4"});
                env.UndeployModuleContaining("insert");
                
                env.CompileDeploy("@Name('insert') insert into BaseOneA select P0 as I, P1 as P, 'a' as pa from MyMapTypeIDB", path);
                env.SendEventMap(MakeMap("P0", "somevalue", "P1", "E5"), "MyMapTypeIDB");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"FINAL", "E5"});
                env.UndeployModuleContaining("insert");
                
                env.CompileDeploy("@Name('insert') insert into BaseOneB select P0 as I, P1 as P, 'b' as pb from MyMapTypeIDB", path);
                env.SendEventMap(MakeMap("P0", "somevalue", "P1", "E6"), "MyMapTypeIDB");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"BLANK", "E6"});
                env.UndeployModuleContaining("insert");
                
                env.CompileDeploy("@Name('insert') insert into BaseTwo select P0 as I, P1 as P from MyMapTypeIDB", path);
                env.UndeployModuleContaining("s0");
                
                env.CompileDeploy("@Name('s0') select * from BaseInterface", path).AddListener("s0");
                env.SendEventMap(MakeMap("P0", "E2", "P1", "E7"), "MyMapTypeIDB");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), new[] {"I"}, new object[] {"XYZ"});
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateNamedWindow: RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "P0,P1".SplitCsv();
                var path = new RegressionPath();
                env.CompileDeploy("@Name('window') create window AWindow#keepall select * from MyMapTypeNW", path).AddListener("window");
                env.CompileDeploy("@Name('insert') insert into AWindow select * from MyMapTypeNW", path).AddListener("insert");
                env.CompileDeploy("@Name('select') select * from AWindow", path).AddListener("select");
                env.CompileDeploy("update istream AWindow set P1='newvalue'", path);
                env.Milestone(0);
                env.SendEventMap(MakeMap("P0", "E1", "P1", "oldvalue"), "MyMapTypeNW");
                EPAssertionUtil.AssertProps(env.Listener("window").AssertOneGetNewAndReset(), fields, new object[] {"E1", "newvalue"});
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E1", "oldvalue"});
                EPAssertionUtil.AssertProps(env.Listener("select").AssertOneGetNewAndReset(), fields, new object[] {"E1", "newvalue"});
                env.CompileDeploy("@Name('onselect') on SupportBean(TheString='A') select win.* from AWindow as win", path).AddListener("onselect");
                env.SendEventBean(new SupportBean("A", 0));
                EPAssertionUtil.AssertProps(env.Listener("onselect").AssertOneGetNewAndReset(), fields, new object[] {"E1", "newvalue"});
                env.Milestone(1);
                env.CompileDeploy("@Name('oninsert') on SupportBean(TheString='B') insert into MyOtherStream select win.* from AWindow as win", path)
                    .AddListener("oninsert");
                env.SendEventBean(new SupportBean("B", 1));
                EPAssertionUtil.AssertProps(env.Listener("oninsert").AssertOneGetNewAndReset(), fields, new object[] {"E1", "newvalue"});
                env.Milestone(2);
                env.CompileDeploy("update istream MyOtherStream set P0='a', P1='b'", path);
                env.CompileDeploy("@Name('s0') select * from MyOtherStream", path).AddListener("s0");
                env.SendEventBean(new SupportBean("B", 1));
                EPAssertionUtil.AssertProps(env.Listener("oninsert").AssertOneGetNewAndReset(), fields, new object[] {"E1", "newvalue"});
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"a", "b"});
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateTypeWidener : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fields = "TheString,LongBoxed,IntBoxed".SplitCsv();
                env.CompileDeploy("insert into AStream select * from SupportBean", path);
                env.CompileDeploy("update istream AStream set LongBoxed=IntBoxed, IntBoxed=null", path);
                env.CompileDeploy("@Name('s0') select * from AStream", path).AddListener("s0");
                var bean = new SupportBean("E1", 0);
                bean.LongBoxed = 888L;
                bean.IntBoxed = 999;
                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E1", 999L, null});
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateSendRouteSenderPreprocess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test map
                env.CompileDeploy("@Name('s0') select * from MyMapTypeSR").AddListener("s0");
                env.CompileDeploy("update istream MyMapTypeSR set P0='a'");
                var fields = "P0,P1".SplitCsv();
                env.SendEventMap(MakeMap("P0", "E1", "P1", "E1"), "MyMapTypeSR");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"a", "E1"});
                env.SendEventMap(MakeMap("P0", "E2", "P1", "E2"), "MyMapTypeSR");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"a", "E2"});
                env.CompileDeploy("@Name('trigger') select * from SupportBean");
                env.Statement("trigger").Events += (
                    sender,
                    updateEventArgs) => env.EventService.RouteEventMap(MakeMap("P0", "E3", "P1", "E3"), "MyMapTypeSR");
                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"a", "E3"});
                env.CompileDeploy("@Drop @name('drop') update istream MyMapTypeSR set P0='a'");
                env.SendEventMap(MakeMap("P0", "E4", "P1", "E4"), "MyMapTypeSR");
                env.SendEventMap(MakeMap("P0", "E5", "P1", "E5"), "MyMapTypeSR");
                env.SendEventBean(new SupportBean());
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.UndeployModuleContaining("drop");
                env.UndeployModuleContaining("s0");
                env.UndeployModuleContaining("trigger");
                // test bean
                env.CompileDeploy("@Name('s0') select * from SupportBean").AddListener("s0");
                env.CompileDeploy("update istream SupportBean set IntPrimitive=999");
                fields = "TheString,IntPrimitive".SplitCsv();
                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E1", 999});
                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E2", 999});
                env.CompileDeploy("@Name('trigger') select * from MyMapTypeSR");
                env.Statement("trigger").Events += (
                    sender,
                    updateEventArgs) => env.EventService.RouteEventBean(new SupportBean("E3", 0), "SupportBean");
                env.SendEventMap(MakeMap("P0", "", "P1", ""), "MyMapTypeSR");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E3", 999});
                env.CompileDeploy("@Drop update istream SupportBean set IntPrimitive=1");
                env.SendEventBean(new SupportBean("E4", 0));
                env.SendEventBean(new SupportBean("E4", 0));
                env.SendEventMap(MakeMap("P0", "", "P1", ""), "MyMapTypeSR");
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.UpdateClause = UpdateClause.Create("MyMapTypeSODA", Expressions.Eq(Expressions.Property("P1"), Expressions.Constant("newvalue")));
                model.UpdateClause.OptionalAsClauseStreamName = "mytype";
                model.UpdateClause.OptionalWhereClause = Expressions.Eq("P0", "E1");
                Assert.AreEqual("update istream MyMapTypeSODA as mytype set P1=\"newvalue\" where P0=\"E1\"", model.ToEPL());
                // test map
                env.CompileDeploy("@Name('s0') select * from MyMapTypeSODA").AddListener("s0");
                env.CompileDeploy(model);
                var fields = "P0,P1".SplitCsv();
                env.SendEventMap(MakeMap("P0", "E1", "P1", "E1"), "MyMapTypeSODA");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E1", "newvalue"});
                // test unmap
                var text = "update istream MyMapTypeSODA as mytype set P1=\"newvalue\" where P0=\"E1\"";
                env.EplToModelCompileDeploy(text);
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateXMLEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var xml = "<simpleEvent><prop1>SAMPLE_V1</prop1></simpleEvent>";
                var simpleDoc = SupportXML.GetDocument(xml);
                var path = new RegressionPath();
                env.CompileDeploy("insert into ABCStreamXML select 1 as ValOne, 2 as ValTwo, * from MyXMLEvent", path);
                env.CompileDeploy("update istream ABCStreamXML set ValOne = 987, ValTwo=123 where prop1='SAMPLE_V1'", path);
                env.CompileDeploy("@Name('s0') select * from ABCStreamXML", path).AddListener("s0");
                env.SendEventXMLDOM(simpleDoc, "MyXMLEvent");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "ValOne,ValTwo,prop1".SplitCsv(),
                    new object[] {987, 123, "SAMPLE_V1"});
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateWrappedObject : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("insert into ABCStreamWO select 1 as ValOne, 2 as ValTwo, * from SupportBean", path);
                env.CompileDeploy("@Name('update') update istream ABCStreamWO set ValOne = 987, ValTwo=123", path);
                env.CompileDeploy("@Name('s0') select * from ABCStreamWO", path).AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "ValOne,ValTwo,TheString".SplitCsv(), new object[] {987, 123, "E1"});
                env.UndeployModuleContaining("update");
                env.CompileDeploy("@Name('update') update istream ABCStreamWO set TheString = 'A'", path);
                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "ValOne,ValTwo,TheString".SplitCsv(), new object[] {1, 2, "A"});
                env.UndeployModuleContaining("update");
                env.CompileDeploy("update istream ABCStreamWO set TheString = 'B', ValOne = 555", path);
                env.SendEventBean(new SupportBean("E3", 0));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "ValOne,ValTwo,TheString".SplitCsv(), new object[] {555, 2, "B"});
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateCopyMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("insert into ABCStreamCM select * from SupportBeanCopyMethod", path);
                env.CompileDeploy("update istream ABCStreamCM set ValOne = 'x', ValTwo='y'", path);
                env.CompileDeploy("@Name('s0') select * from ABCStreamCM", path).AddListener("s0");
                env.SendEventBean(new SupportBeanCopyMethod("1", "2"));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "ValOne,ValTwo".SplitCsv(), new object[] {"x", "y"});
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fields = "TheString,IntPrimitive".SplitCsv();
                env.CompileDeploy("insert into ABCStreamSQ select * from SupportBean", path);
                env.CompileDeploy(
                    "@Name('update') update istream ABCStreamSQ set TheString = (select s0 from MyMapTypeSelect#lastevent) where IntPrimitive in (select w0 from MyMapTypeWhere#keepall)",
                    path);
                env.CompileDeploy("@Name('s0') select * from ABCStreamSQ", path).AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E1", 0});
                env.SendEventMap(MakeMap("w0", 1), "MyMapTypeWhere");
                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {null, 1});
                env.SendEventBean(new SupportBean("E3", 2));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E3", 2});
                env.SendEventMap(MakeMap("s0", "newvalue"), "MyMapTypeSelect");
                env.SendEventBean(new SupportBean("E4", 1));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"newvalue", 1});
                env.SendEventMap(MakeMap("s0", "othervalue"), "MyMapTypeSelect");
                env.SendEventBean(new SupportBean("E5", 1));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"othervalue", 1});
                // test correlated subquery
                env.UndeployModuleContaining("update");
                env.CompileDeploy(
                    "@Name('update') update istream ABCStreamSQ set IntPrimitive = (select s1 from MyMapTypeSelect#keepall where s0 = ABCStreamSQ.TheString)",
                    path);
                // note that this will log an error (int primitive set to null), which is good, and leave the value unchanged
                env.SendEventBean(new SupportBean("E6", 8));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E6", 8});
                env.SendEventMap(MakeMap("s0", "E7", "s1", 91), "MyMapTypeSelect");
                env.SendEventBean(new SupportBean("E7", 0));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E7", 91});
                // test correlated with as-clause
                env.UndeployModuleContaining("update");
                env.CompileDeploy(
                    "@Name('update') update istream ABCStreamSQ as mystream set IntPrimitive = (select s1 from MyMapTypeSelect#keepall where s0 = mystream.TheString)",
                    path);
                // note that this will log an error (int primitive set to null), which is good, and leave the value unchanged
                env.SendEventBean(new SupportBean("E8", 111));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E8", 111});
                env.SendEventMap(MakeMap("s0", "E9", "s1", -1), "MyMapTypeSelect");
                env.SendEventBean(new SupportBean("E9", 0));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E9", -1});
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateUnprioritizedOrder : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "s0,s1".SplitCsv();
                var path = new RegressionPath();
                env.CompileDeploy("insert into ABCStreamUO select * from MyMapTypeUO", path);
                env.CompileDeploy("@Name('A') update istream ABCStreamUO set s0='A'", path);
                env.CompileDeploy("@Name('B') update istream ABCStreamUO set s0='B'", path);
                env.CompileDeploy("@Name('C') update istream ABCStreamUO set s0='C'", path);
                env.CompileDeploy("@Name('D') update istream ABCStreamUO set s0='D'", path);
                env.CompileDeploy("@Name('s0') select * from ABCStreamUO", path).AddListener("s0");
                env.SendEventMap(MakeMap("s0", "", "s1", 1), "MyMapTypeUO");
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"D", 1});
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateListenerDeliveryMultiupdate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var listenerInsert = new SupportUpdateListener();
                var listeners = new SupportUpdateListener[5];
                for (var i = 0; i < listeners.Length; i++) {
                    listeners[i] = new SupportUpdateListener();
                }

                var path = new RegressionPath();
                var fields = "TheString,IntPrimitive,value1".SplitCsv();
                env.CompileDeploy("@Name('insert') insert into ABCStreamLD select *, 'orig' as value1 from SupportBean", path)
                    .Statement("insert")
                    .AddListener(listenerInsert);
                env.CompileDeploy("@Name('A') update istream ABCStreamLD set TheString='A', value1='a' where IntPrimitive in (1,2)", path)
                    .Statement("A")
                    .AddListener(listeners[0]);
                env.CompileDeploy("@Name('B') update istream ABCStreamLD set TheString='B', value1='b' where IntPrimitive in (1,3)", path)
                    .Statement("B")
                    .AddListener(listeners[1]);
                env.CompileDeploy("@Name('C') update istream ABCStreamLD set TheString='C', value1='c' where IntPrimitive in (2,3)", path)
                    .Statement("C")
                    .AddListener(listeners[2]);
                env.CompileDeploy("@Name('s0') select * from ABCStreamLD", path).AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E1", 1, "orig"});
                EPAssertionUtil.AssertProps(listeners[0].AssertOneGetOld(), fields, new object[] {"E1", 1, "orig"});
                EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNew(), fields, new object[] {"A", 1, "a"});
                EPAssertionUtil.AssertProps(listeners[1].AssertOneGetOld(), fields, new object[] {"A", 1, "a"});
                EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNew(), fields, new object[] {"B", 1, "b"});
                Assert.IsFalse(listeners[2].IsInvoked);
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"B", 1, "b"});
                Reset(listeners);
                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E2", 2, "orig"});
                EPAssertionUtil.AssertProps(listeners[0].AssertOneGetOld(), fields, new object[] {"E2", 2, "orig"});
                EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNew(), fields, new object[] {"A", 2, "a"});
                Assert.IsFalse(listeners[1].IsInvoked);
                EPAssertionUtil.AssertProps(listeners[2].AssertOneGetOld(), fields, new object[] {"A", 2, "a"});
                EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNew(), fields, new object[] {"C", 2, "c"});
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"C", 2, "c"});
                Reset(listeners);
                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E3", 3, "orig"});
                Assert.IsFalse(listeners[0].IsInvoked);
                EPAssertionUtil.AssertProps(listeners[1].AssertOneGetOld(), fields, new object[] {"E3", 3, "orig"});
                EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNew(), fields, new object[] {"B", 3, "b"});
                EPAssertionUtil.AssertProps(listeners[2].AssertOneGetOld(), fields, new object[] {"B", 3, "b"});
                EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNew(), fields, new object[] {"C", 3, "c"});
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"C", 3, "c"});
                Reset(listeners);
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateListenerDeliveryMultiupdateMixed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var listenerInsert = new SupportUpdateListener();
                var listeners = new SupportUpdateListener[5];
                for (var i = 0; i < listeners.Length; i++) {
                    listeners[i] = new SupportUpdateListener();
                }

                var path = new RegressionPath();
                var fields = "TheString,IntPrimitive,value1".SplitCsv();
                env.CompileDeploy("@Name('insert') insert into ABCStreamLDM select *, 'orig' as value1 from SupportBean", path)
                    .Statement("insert")
                    .AddListener(listenerInsert);
                env.CompileDeploy("@Name('s0') select * from ABCStreamLDM", path).AddListener("s0");
                env.CompileDeploy("@Name('A') update istream ABCStreamLDM set TheString='A', value1='a'", path);
                env.CompileDeploy("@Name('B') update istream ABCStreamLDM set TheString='B', value1='b'", path).Statement("B").AddListener(listeners[1]);
                env.CompileDeploy("@Name('C') update istream ABCStreamLDM set TheString='C', value1='c'", path);
                env.CompileDeploy("@Name('D') update istream ABCStreamLDM set TheString='D', value1='d'", path).Statement("D").AddListener(listeners[3]);
                env.CompileDeploy("@Name('E') update istream ABCStreamLDM set TheString='E', value1='e'", path);
                env.SendEventBean(new SupportBean("E4", 4));
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E4", 4, "orig"});
                Assert.IsFalse(listeners[0].IsInvoked);
                EPAssertionUtil.AssertProps(listeners[1].AssertOneGetOld(), fields, new object[] {"A", 4, "a"});
                EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNew(), fields, new object[] {"B", 4, "b"});
                Assert.IsFalse(listeners[2].IsInvoked);
                EPAssertionUtil.AssertProps(listeners[3].AssertOneGetOld(), fields, new object[] {"C", 4, "c"});
                EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNew(), fields, new object[] {"D", 4, "d"});
                Assert.IsFalse(listeners[4].IsInvoked);
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E", 4, "e"});
                Reset(listeners);
                env.Statement("B").RemoveAllListeners();
                env.Statement("D").RemoveAllListeners();
                env.Statement("A").AddListener(listeners[0]);
                env.Statement("E").AddListener(listeners[4]);
                env.SendEventBean(new SupportBean("E5", 5));
                EPAssertionUtil.AssertProps(env.Listener("insert").AssertOneGetNewAndReset(), fields, new object[] {"E5", 5, "orig"});
                EPAssertionUtil.AssertProps(listeners[0].AssertOneGetOld(), fields, new object[] {"E5", 5, "orig"});
                EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNew(), fields, new object[] {"A", 5, "a"});
                Assert.IsFalse(listeners[1].IsInvoked);
                Assert.IsFalse(listeners[2].IsInvoked);
                Assert.IsFalse(listeners[3].IsInvoked);
                EPAssertionUtil.AssertProps(listeners[4].AssertOneGetOld(), fields, new object[] {"D", 5, "d"});
                EPAssertionUtil.AssertProps(listeners[4].AssertOneGetNew(), fields, new object[] {"E", 5, "e"});
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"E", 5, "e"});
                Reset(listeners);
                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateMapIndexProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionSetMapPropsBean(env);
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    RunAssertionUpdateIStreamSetMapProps(env, rep);
                }

                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    RunAssertionNamedWindowSetMapProps(env, rep);
                }
            }
        }

        private static void RunAssertionSetMapPropsBean(RegressionEnvironment env)
        {
            // test update-istream with bean
            var path = new RegressionPath();
            env.CompileDeployWBusPublicType("create schema MyMapPropEvent as " + typeof(MyMapPropEvent).MaskTypeName(), path);
            env.CompileDeploy("insert into MyStream select * from MyMapPropEvent", path);
            env.CompileDeploy("@Name('s0') update istream MyStream set Props('abc') = 1, Array[2] = 10", path).AddListener("s0");
            env.SendEventBean(new MyMapPropEvent());
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertPairGetIRAndReset(),
                "Props('abc'),Array[2]".SplitCsv(),
                new object[] {1, 10},
                new object[] {null, null});
            env.UndeployAll();
        }

        private static void RunAssertionUpdateIStreamSetMapProps(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var mapType = typeof(IDictionary<string, object>).CleanName();
            // test update-istream with map
            var path = new RegressionPath();
            var eplType = eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMapProp>() +
                          $" @name('type') create schema MyInfraTypeWithMapProp(simple string, myarray int[], mymap `{mapType}`);\n";
            env.CompileDeployWBusPublicType(eplType, path);
            env.CompileDeploy("@Name('update') update istream MyInfraTypeWithMapProp set simple='A', mymap('abc') = 1, myarray[2] = 10", path)
                .AddListener("update");
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {null, new int[10], new Dictionary<string, object>()}, "MyInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(MakeMapEvent(new Dictionary<string, object>(), new int[10]), "MyInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = SupportAvroUtil.GetAvroSchema(env.Runtime.EventTypeService.GetEventType(env.DeploymentId("type"), "MyInfraTypeWithMapProp"))
                    .AsRecordSchema();
                var @event = new GenericRecord(schema);
                @event.Put("myarray", Arrays.AsList(0, 0, 0, 0, 0));
                @event.Put("mymap", new Dictionary<string, object>());
                env.SendEventAvro(@event, "MyInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                var myarray = new JArray(new JValue(0), new JValue(0), new JValue(0), new JValue(0), new JValue(0));
                var mymap = new JObject();
                var @event = new JObject(new JProperty("myarray", myarray), new JProperty("mymap", mymap));
                env.SendEventJson(@event.ToString(), "MyInfraTypeWithMapProp");
            }
            else {
                Assert.Fail();
            }

            EPAssertionUtil.AssertProps(
                env.Listener("update").AssertPairGetIRAndReset(),
                "simple,mymap('abc'),myarray[2]".SplitCsv(),
                new object[] {"A", 1, 10},
                new object[] {null, null, 0});
            env.UndeployAll();
        }

        private static void RunAssertionNamedWindowSetMapProps(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var mapType = typeof(IDictionary<string, object>).CleanName();
            // test named-window update
            var path = new RegressionPath();
            var eplTypes = eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMapProp>() +
                           $" @name('type') create schema MyNWInfraTypeWithMapProp(simple String, myarray int[], mymap `{mapType}`)";
            env.CompileDeployWBusPublicType(eplTypes, path);
            env.CompileDeploy("@Name('window') create window MyWindowWithMapProp#keepall as MyNWInfraTypeWithMapProp", path);
            env.CompileDeploy(
                eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMapProp>() +
                " insert into MyWindowWithMapProp select * from MyNWInfraTypeWithMapProp",
                path);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {null, new int[10], new Dictionary<string, object>()}, "MyNWInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(MakeMapEvent(new Dictionary<string, object>(), new int[10]), "MyNWInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = SupportAvroUtil.GetAvroSchema(env.Runtime.EventTypeService.GetEventType(env.DeploymentId("type"), "MyNWInfraTypeWithMapProp"))
                    .AsRecordSchema();
                var @event = new GenericRecord(schema);
                @event.Put("myarray", Arrays.AsList(0, 0, 0, 0, 0));
                @event.Put("mymap", new Dictionary<string, object>());
                env.SendEventAvro(@event, "MyNWInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                var myarray = new JArray(new JValue(0), new JValue(0), new JValue(0), new JValue(0), new JValue(0));
                var mymap = new JObject();
                var @event = new JObject(new JProperty("myarray", myarray), new JProperty("mymap", mymap));
                env.SendEventJson(@event.ToString(), "MyNWInfraTypeWithMapProp");
            }
            else {
                Assert.Fail();
            }

            env.CompileDeploy("on SupportBean update MyWindowWithMapProp set simple='A', mymap('abc') = IntPrimitive, myarray[2] = IntPrimitive", path);
            env.SendEventBean(new SupportBean("E1", 10));
            EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("window"), "simple,mymap('abc'),myarray[2]".SplitCsv(), new[] {new object[] {"A", 10, 10}});
            // test null and array too small
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {null, new int[2], null}, "MyNWInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(MakeMapEvent(null, new int[2]), "MyNWInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var @event = new GenericRecord(
                    SchemaBuilder.Record(
                        "name",
                        TypeBuilder.OptionalString("simple"),
                        TypeBuilder.Field("myarray", TypeBuilder.Array(TypeBuilder.LongType())),
                        TypeBuilder.Field("mymap", TypeBuilder.Map(TypeBuilder.StringType()))));
                @event.Put("myarray", Arrays.AsList(0, 0));
                @event.Put("mymap", null);
                env.SendEventAvro(@event, "MyNWInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                var myarray = new JArray(new JValue(0), new JValue(0));
                var @event = new JObject(new JProperty("myarray", myarray));
                env.SendEventJson(@event.ToString(), "MyNWInfraTypeWithMapProp");
            }
            else {
                Assert.Fail();
            }

            env.SendEventBean(new SupportBean("E2", 20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("window"),
                "simple,mymap('abc'),myarray[2]".SplitCsv(),
                new[] {new object[] {"A", 20, 20}, new object[] {"A", null, null}});
            env.UndeployAll();
        }

        private static IDictionary<string, object> MakeMapEvent(
            IDictionary<string, object> mymap,
            int[] myarray)
        {
            IDictionary<string, object> map = new LinkedHashMap<string, object>();
            map.Put("mymap", mymap);
            map.Put("myarray", myarray);
            return map;
        }

        private static void TryAssertionFieldsWithPriority(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var path = new RegressionPath();
            var prefix = eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedSB>();
            env.CompileDeploy(prefix + " insert into MyStream select TheString, IntPrimitive from SupportBean(TheString not like 'Z%')", path);
            env.CompileDeploy(prefix + " insert into MyStream select 'AX'||TheString as TheString, IntPrimitive from SupportBean(TheString like 'Z%')", path);
            env.CompileDeploy(prefix + " @Name('a') @Priority(12) update istream MyStream set IntPrimitive=-2 where IntPrimitive=-1", path);
            env.CompileDeploy(prefix + " @Name('b') @Priority(11) update istream MyStream set IntPrimitive=-1 where TheString like 'D%'", path);
            env.CompileDeploy(prefix + " @Name('c') @Priority(9) update istream MyStream set IntPrimitive=9 where TheString like 'A%'", path);
            env.CompileDeploy(
                prefix + " @Name('d') @Priority(8) update istream MyStream set IntPrimitive=8 where TheString like 'A%' or TheString like 'C%'",
                path);
            env.CompileDeploy(" @Name('e') @Priority(10) update istream MyStream set IntPrimitive=10 where TheString like 'A%'", path);
            env.CompileDeploy(" @Name('f') @Priority(7) update istream MyStream set IntPrimitive=7 where TheString like 'A%' or TheString like 'C%'", path);
            env.CompileDeploy(" @Name('g') @Priority(6) update istream MyStream set IntPrimitive=6 where TheString like 'A%'", path);
            env.CompileDeploy(" @Name('h') @Drop update istream MyStream set IntPrimitive=6 where TheString like 'B%'", path);
            env.CompileDeploy("@Name('s0') select * from MyStream where IntPrimitive > 0", path).AddListener("s0");
            var fields = "TheString,IntPrimitive".SplitCsv();
            env.SendEventBean(new SupportBean("A1", 0));
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"A1", 10});
            env.SendEventBean(new SupportBean("B1", 0));
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            env.SendEventBean(new SupportBean("C1", 0));
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"C1", 8});
            env.SendEventBean(new SupportBean("D1", 100));
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            env.UndeployModuleContaining("s0");
            env.CompileDeploy("@Name('s0') select * from MyStream", path).AddListener("s0");
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("s0").EventType.UnderlyingType));
            env.SendEventBean(new SupportBean("D1", -2));
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"D1", -2});
            env.SendEventBean(new SupportBean("Z1", -3));
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"AXZ1", 10});
            env.UndeployModuleContaining("e");
            env.SendEventBean(new SupportBean("Z2", 0));
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"AXZ2", 9});
            env.UndeployModuleContaining("c");
            env.UndeployModuleContaining("d");
            env.UndeployModuleContaining("f");
            env.UndeployModuleContaining("g");
            env.SendEventBean(new SupportBean("Z3", 0));
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {"AXZ3", 0});
            env.UndeployAll();
        }

        private static void Reset(SupportUpdateListener[] listeners)
        {
            foreach (var listener in listeners) {
                listener.Reset();
            }
        }

        private static IDictionary<string, object> MakeMap(
            string prop1,
            object val1,
            string prop2,
            object val2,
            string prop3,
            object val3)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put(prop1, val1);
            map.Put(prop2, val2);
            map.Put(prop3, val3);
            return map;
        }

        private static IDictionary<string, object> MakeMap(
            string prop1,
            object val1,
            string prop2,
            object val2)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put(prop1, val1);
            map.Put(prop2, val2);
            return map;
        }

        private static IDictionary<string, object> MakeMap(
            string prop1,
            object val1)
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put(prop1, val1);
            return map;
        }

        private static SupportBean MakeSupportBean(
            string theString,
            int intPrimitive,
            double doublePrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }

        public interface BaseInterface
        {
            string I { get; set; }
        }

        [Serializable]
        public class BaseOne : BaseInterface
        {
            public BaseOne()
            {
            }

            public BaseOne(
                string i,
                string p)
            {
                this.I = i;
                this.P = p;
            }

            public string I { get; set; }

            public string P { get; set; }
        }

        [Serializable]
        public class BaseTwo : BaseInterface
        {
            public BaseTwo()
            {
            }

            public BaseTwo(string p)
            {
                this.P = p;
            }

            public string I { get; set; }

            public string P { get; set; }
        }

        [Serializable]
        public class BaseOneA : BaseOne
        {
            public BaseOneA()
            {
            }

            public BaseOneA(
                string i,
                string p,
                string pa) : base(i, p)
            {
                this.Pa = pa;
            }

            public string Pa { get; set; }
        }

        [Serializable]
        public class BaseOneB : BaseOne
        {
            public BaseOneB()
            {
            }

            public BaseOneB(
                string i,
                string p,
                string pb) : base(i, p)
            {
                this.Pb = pb;
            }

            public string Pb { get; set; }
        }

        public static void SetIntBoxedValue(
            SupportBean sb,
            int value)
        {
            sb.IntBoxed = value;
        }

        [Serializable]
        public class MyMapPropEvent
        {
            private IDictionary<string, object> props = new Dictionary<string, object>();
            private object[] array = new object[10];

            public IDictionary<string, object> Props {
                get => props;
                set => props = value;
            }

            public object[] Array {
                get => array;
                set => array = value;
            }

            public void SetProps(
                string name,
                object value)
            {
                props.Put(name, value);
            }

            public void SetArray(
                int index,
                object value)
            {
                array[index] = value;
            }

            public IDictionary<string, object> GetProps()
            {
                return props;
            }

            public void SetProps(IDictionary<string, object> props)
            {
                this.props = props;
            }

            public object[] GetArray()
            {
                return array;
            }

            public void SetArray(object[] array)
            {
                this.array = array;
            }

            public object GetArray(int index)
            {
                return array[index];
            }
        }

        [Serializable]
        public class MyLocalJsonProvidedMapProp
        {
            public string Simple;
            public int?[] Myarray;
            public IDictionary<string, object> Mymap;
        }

        [Serializable]
        public class MyLocalJsonProvidedSB
        {
            public string TheString;
            public int IntPrimitive;
        }
    }
} // end of namespace