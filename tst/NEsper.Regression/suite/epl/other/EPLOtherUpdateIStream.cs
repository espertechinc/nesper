///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

using Newtonsoft.Json.Linq;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherUpdateIStream
    {
        public static List<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
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
            WithMapSetMapPropsBean(execs);
            WithMapSetMapPropsRep(execs);
            WithNWSetMapProps(execs);
            WithArrayElement(execs);
            WithArrayElementBoxed(execs);
            WithArrayElementInvalid(execs);
            WithExpression(execs);
            With(IStreamEnumAnyOf)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithIStreamEnumAnyOf(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateIStreamEnumAnyOf());
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

        public static IList<RegressionExecution> WithNWSetMapProps(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateNWSetMapProps());
            return execs;
        }

        public static IList<RegressionExecution> WithMapSetMapPropsRep(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateMapSetMapPropsRep());
            return execs;
        }

        public static IList<RegressionExecution> WithMapSetMapPropsBean(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateMapSetMapPropsBean());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateSubqueryMultikeyWArray());
            return execs;
        }

        public static IList<RegressionExecution> WithListenerDeliveryMultiupdateMixed(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherUpdateListenerDeliveryMultiupdateMixed());
            return execs;
        }

        public static IList<RegressionExecution> WithListenerDeliveryMultiupdate(
            IList<RegressionExecution> execs = null)
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

        public static IList<RegressionExecution> WithInsertDirectBeanTypeInheritance(
            IList<RegressionExecution> execs = null)
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

        internal class EPLOtherUpdateIStreamEnumAnyOf : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema MyEvent as " +
                          typeof(SupportEventWithListOfObject).MaskTypeName() +
                          ";\n" +
                          "@name('update') update istream MyEvent set updated = true where mylist.anyOf(e -> e is not null); \n" +
                          "@name('s0') select updated from MyEvent;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(
                    new SupportEventWithListOfObject(Arrays.AsList<object>("first", "second")),
                    "MyEvent");
                env.AssertEqualsNew("s0", "updated", true);

                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateArrayElementInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplSchema =
                    "@name('create') @public create schema MySchema(doublearray double[primitive], intarray int[primitive], notAnArray int)";
                env.Compile(eplSchema, path);

                // invalid property
                env.TryInvalidCompile(
                    path,
                    "update istream MySchema set c1[0]=1",
                    "Failed to validate assignment expression 'c1[0]=1': Property 'c1[0]' is not available for write access");

                // index expression is not Integer
                env.TryInvalidCompile(
                    path,
                    "update istream MySchema set doublearray[null]=1",
                    "Incorrect index expression for array operation, expected an expression returning an integer value but the expression 'null' returns 'null (any type)' for expression 'doublearray'");

                // type incompatible cannot assign
                env.TryInvalidCompile(
                    path,
                    "update istream MySchema set intarray[notAnArray]='x'",
                    "Failed to validate assignment expression 'intarray[notAnArray]=\"x\"': Invalid assignment of column '\"x\"' of type 'System.String' to event property 'intarray' typed as 'System.Int32', column and parameter types mismatch [update istream MySchema set intarray[notAnArray]='x']");

                // not-an-array
                env.TryInvalidCompile(
                    path,
                    "update istream MySchema set notAnArray[notAnArray]=1",
                    "Failed to validate assignment expression 'notAnArray[notAnArray]=1': Property 'notAnArray' type is not array");

                // not found
                env.TryInvalidCompile(
                    path,
                    "update istream MySchema set dummy[IntPrimitive]=1",
                    "Failed to validate update assignment expression 'IntPrimitive': Property named 'IntPrimitive' is not valid in any stream");

                path.Clear();

                // runtime-behavior for index-overflow and null-array and null-index and
                var epl =
                    "@name('create') @public @buseventtype create schema MySchema(doublearray double[primitive], indexvalue int, rhsvalue int);\n" +
                    "update istream MySchema set doublearray[indexvalue]=rhsvalue;\n";
                env.CompileDeploy(epl);

                // index returned is too large
                try {
                    env.SendEventMap(
                        CollectionUtil.BuildMap("doublearray", new double[3], "indexvalue", 10, "rhsvalue", 1),
                        "MySchema");
                    Assert.Fail();
                }
                catch (Exception ex) {
                    ClassicAssert.IsTrue(ex.Message.Contains("Array length 3 less than index 10 for property 'doublearray'"));
                }

                // index returned null
                env.SendEventMap(
                    CollectionUtil.BuildMap("doublearray", new double[3], "indexvalue", null, "rhsvalue", 1),
                    "MySchema");

                // rhs returned null for array-of-primitive
                env.SendEventMap(
                    CollectionUtil.BuildMap("doublearray", new double[3], "indexvalue", 1, "rhsvalue", null),
                    "MySchema");

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
                    "  public class Helper {\n" +
                    "    public static void Swap(System.Collections.Generic.IDictionary<string, object> @event) {\n" +
                    "      var temp = @event[\"a\"];\n" +
                    "      @event[\"a\"] = @event[\"b\"];\n" +
                    "      @event[\"b\"] = temp;\n" +
                    "    }\n" +
                    "  }\n" +
                    "\"\"\"\n" +
                    "update istream MyEvent as me set Helper.Swap(me);\n" +
                    "@name('s0') select * from MyEvent;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventMap(CollectionUtil.BuildMap("a", 1, "b", 10), "MyEvent");
                env.AssertPropsNew("s0", "a,b".SplitCsv(), new object[] { 10, 1 });

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
                    "@name('s0') select dbls as c0 from MyEvent;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventMap(Collections.SingletonDataMap("dbls", new double?[3]), "MyEvent");
                env.AssertEventNew(
                    "s0",
                    @event => ClassicAssert.AreEqual(new double?[] { null, 1d, null }, (double?[])@event.Get("c0")));

                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateArrayElement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema Arriving(position int, intarray int[], objectarray System.Object[]);\n" +
                    "update istream Arriving set intarray[position] = 1, objectarray[position] = 1;\n" +
                    "@name('s0') select * from Arriving;\n";
                env.CompileDeploy(epl).AddListener("s0");

                AssertUpdate(env, 1, new int[] { 0, 1, 0 }, new object[] { null, 1, null });
                AssertUpdate(env, 0, new int[] { 1, 0, 0 }, new object[] { 1, null, null });
                AssertUpdate(env, 2, new int[] { 0, 0, 1 }, new object[] { null, null, 1 });

                env.UndeployAll();
            }

            private void AssertUpdate(
                RegressionEnvironment env,
                int position,
                int[] expectedInt,
                object[] expectedObj)
            {
                env.SendEventMap(
                    CollectionUtil.BuildMap("position", position, "intarray", new int[3], "objectarray", new object[3]),
                    "Arriving");
                env.AssertPropsNew(
                    "s0",
                    "position,intarray,objectarray".SplitCsv(),
                    new object[] { position, expectedInt, expectedObj });
            }
        }

        internal class EPLOtherUpdateSubqueryMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema Arriving(Value int);\n" +
                          "update istream Arriving set Value = (select sum(Value) as c0 from SupportEventWithIntArray#keepall group by Array);\n" +
                          "@name('s0') select * from Arriving;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportEventWithIntArray("E1", new int[] { 1, 2 }, 10));
                env.SendEventBean(new SupportEventWithIntArray("E2", new int[] { 1, 2 }, 11));

                env.Milestone(0);
                AssertUpdate(env, 21);

                env.SendEventBean(new SupportEventWithIntArray("E3", new int[] { 1, 2 }, 12));
                AssertUpdate(env, 33);

                env.Milestone(1);

                env.SendEventBean(new SupportEventWithIntArray("E4", new int[] { 1 }, 13));
                AssertUpdate(env, null);

                env.UndeployAll();
            }

            private void AssertUpdate(
                RegressionEnvironment env,
                int? expected)
            {
                env.SendEventMap(new Dictionary<string, object>(), "Arriving");
                env.AssertEqualsNew("s0", "Value", expected);
            }
        }

        internal class EPLOtherUpdateBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var text = "@name('Insert') @public insert into MyStream select * from SupportBean";
                env.CompileDeploy(text, path).AddListener("Insert");

                text =
                    "@name('Update') update istream MyStream set IntPrimitive=10, TheString='O_' || TheString where IntPrimitive=1";
                env.CompileDeploy(text, path).AddListener("Update");

                text = "@name('Select') select * from MyStream";
                env.CompileDeploy(text, path).AddListener("Select");

                var fields = "TheString,IntPrimitive".SplitCsv();
                env.SendEventBean(new SupportBean("E1", 9));
                env.AssertPropsNew("Select", fields, new object[] { "E1", 9 });
                env.AssertPropsNew("Insert", fields, new object[] { "E1", 9 });
                env.AssertListenerNotInvoked("Update");

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertPropsNew("Select", fields, new object[] { "O_E2", 10 });
                env.AssertPropsNew("Insert", fields, new object[] { "E2", 1 });
                env.AssertPropsIRPair("Update", fields, new object[] { "O_E2", 10 }, new object[] { "E2", 1 });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 1));
                env.AssertPropsNew("Select", fields, new object[] { "O_E3", 10 });
                env.AssertPropsNew("Insert", fields, new object[] { "E3", 1 });
                env.AssertPropsIRPair("Update", fields, new object[] { "O_E3", 10 }, new object[] { "E3", 1 });

                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateFieldUpdateOrder : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@name('update') update istream SupportBean " +
                    "set IntPrimitive=myvar, IntBoxed=IntPrimitive");
                env.AssertStatement(
                    "update",
                    statement => ClassicAssert.AreEqual(
                        StatementType.UPDATE,
                        statement.GetProperty(StatementProperty.STATEMENTTYPE)));

                env.CompileDeploy("@name('s0') select * from SupportBean").AddListener("s0");
                var fields = "IntPrimitive,IntBoxed".SplitCsv();

                env.SendEventBean(MakeSupportBean("E1", 1, 2));
                env.AssertPropsNew("s0", fields, new object[] { 10, 1 });

                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public insert into SupportBeanStream select * from SupportBean", path);
                env.CompileDeploy(
                    "@public insert into SupportBeanStreamTwo select * from pattern[a=SupportBean -> b=SupportBean]",
                    path);
                env.CompileDeploy("@public insert into SupportBeanStreamRO select * from SupportBeanReadOnly", path);

                env.TryInvalidCompile(
                    path,
                    "update istream SupportBeanStream set IntPrimitive=LongPrimitive",
                    "Failed to validate assignment expression 'IntPrimitive=LongPrimitive': Invalid assignment of column 'LongPrimitive' of type 'System.Nullable<System.Int64>' to event property 'IntPrimitive' typed as 'System.Int32', column and parameter types mismatch [update istream SupportBeanStream set IntPrimitive=LongPrimitive]");
                env.TryInvalidCompile(
                    path,
                    "update istream SupportBeanStream set xxx='abc'",
                    "Failed to validate assignment expression 'xxx=\"abc\"': Property 'xxx' is not available for write access [update istream SupportBeanStream set xxx='abc']");
                env.TryInvalidCompile(
                    path,
                    "update istream SupportBeanStream set IntPrimitive=null",
                    "Failed to validate assignment expression 'IntPrimitive=null': Invalid assignment of column 'null' of null type to event property 'IntPrimitive' typed as 'System.Int32', nullable type mismatch [update istream SupportBeanStream set IntPrimitive=null]");
                env.TryInvalidCompile(
                    path,
                    "update istream SupportBeanStreamTwo set a.IntPrimitive=10",
                    "Failed to validate assignment expression 'a.IntPrimitive=10': Property 'a.IntPrimitive' is not available for write access [update istream SupportBeanStreamTwo set a.IntPrimitive=10]");
                env.TryInvalidCompile(
                    path,
                    "update istream SupportBeanStreamRO set side='a'",
                    "Failed to validate assignment expression 'side=\"a\"': Property 'side' is not available for write access [update istream SupportBeanStreamRO set side='a']");
                env.TryInvalidCompile(
                    path,
                    "update istream SupportBean set LongPrimitive=sum(IntPrimitive)",
                    "Aggregation functions may not be used within update-set [update istream SupportBean set LongPrimitive=sum(IntPrimitive)]");
                env.TryInvalidCompile(
                    path,
                    "update istream SupportBean set LongPrimitive=LongPrimitive where sum(IntPrimitive) = 1",
                    "Aggregation functions may not be used within an update-clause [update istream SupportBean set LongPrimitive=LongPrimitive where sum(IntPrimitive) = 1]");
                env.TryInvalidCompile(
                    path,
                    "update istream SupportBean set LongPrimitive=prev(1, LongPrimitive)",
                    "Failed to validate update assignment expression 'prev(1,LongPrimitive)': Previous function cannot be used in this context [update istream SupportBean set LongPrimitive=prev(1, LongPrimitive)]");
                env.TryInvalidCompile(
                    path,
                    "update istream MyXmlEvent set abc=1",
                    "Failed to validate assignment expression 'abc=1': Property 'abc' is not available for write access [update istream MyXmlEvent set abc=1]");
#if false
                env.TryInvalidCompile(
                    path,
                    "update istream SupportBeanErrorTestingOne set Value='1'",
                    "The update-clause requires the underlying event representation to support copy (via Serializable by default) [update istream SupportBeanErrorTestingOne set value='1']");
#endif
                env.TryInvalidCompile(
                    path,
                    "update istream SupportBean set LongPrimitive=(select P0 from MyMapTypeInv#lastevent where TheString=P3)",
                    "Failed to plan subquery number 1 querying MyMapTypeInv: Failed to validate filter expression 'TheString=P3': Property named 'TheString' must be prefixed by a stream name, use the stream name itself or use the as-clause to name the stream with the property in the format \"stream.property\" [update istream SupportBean set LongPrimitive=(select P0 from MyMapTypeInv#lastevent where TheString=P3)]");
                env.TryInvalidCompile(
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
                env.CompileDeploy("@name('insert') @public insert into MyStreamBW select * from SupportBean", path);
                env.AddListener("insert");

                env.CompileDeploy(
                    "@name('update_1') update istream MyStreamBW set IntPrimitive=10, TheString='O_' || TheString where IntPrimitive=1",
                    path);
                env.AddListener("update_1");

                env.CompileDeploy("@name('s0') select * from MyStreamBW", path);
                env.AddListener("s0");

                var fields = "TheString,IntPrimitive".SplitCsv();
                env.SendEventBean(new SupportBean("E1", 9));
                env.AssertPropsNew("s0", fields, new object[] { "E1", 9 });
                env.AssertPropsNew("insert", fields, new object[] { "E1", 9 });
                env.AssertListenerNotInvoked("update_1");

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertPropsNew("s0", fields, new object[] { "O_E2", 10 });
                env.AssertPropsNew("insert", fields, new object[] { "E2", 1 });
                env.AssertPropsIRPair("update_1", fields, new object[] { "O_E2", 10 }, new object[] { "E2", 1 });
                env.ListenerReset("update_1");

                env.SendEventBean(new SupportBean("E3", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E3", 2 });
                env.AssertPropsNew("insert", fields, new object[] { "E3", 2 });
                env.AssertListenerNotInvoked("update_1");

                env.SendEventBean(new SupportBean("E4", 1));
                env.AssertPropsNew("s0", fields, new object[] { "O_E4", 10 });
                env.AssertPropsNew("insert", fields, new object[] { "E4", 1 });
                env.AssertPropsIRPair("update_1", fields, new object[] { "O_E4", 10 }, new object[] { "E4", 1 });

                env.CompileDeploy(
                    "@name('update_2') update istream MyStreamBW as xyz set IntPrimitive=xyz.IntPrimitive + 1000 where IntPrimitive=2",
                    path);
                env.AddListener("update_2");

                env.SendEventBean(new SupportBean("E5", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E5", 1002 });
                env.AssertPropsNew("insert", fields, new object[] { "E5", 2 });
                env.AssertPropsIRPair("update_2", fields, new object[] { "E5", 1002 }, new object[] { "E5", 2 });

                env.UndeployModuleContaining("update_1");

                env.SendEventBean(new SupportBean("E6", 1));
                env.AssertPropsNew("s0", fields, new object[] { "E6", 1 });
                env.AssertPropsNew("insert", fields, new object[] { "E6", 1 });
                env.AssertListenerNotInvoked("update_2");

                env.SendEventBean(new SupportBean("E7", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E7", 1002 });
                env.AssertPropsNew("insert", fields, new object[] { "E7", 2 });
                env.AssertPropsIRPair("update_2", fields, new object[] { "E7", 1002 }, new object[] { "E7", 2 });
                env.ListenerReset("update_2");
                env.AssertIterator("update_2", iterator => ClassicAssert.IsFalse(iterator.MoveNext()));

                env.SendEventBean(new SupportBean("E8", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E8", 1002 });
                env.AssertPropsNew("insert", fields, new object[] { "E8", 2 });

                env.SetSubscriber("update_2");

                env.SendEventBean(new SupportBean("E9", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E9", 1002 });
                env.AssertPropsNew("insert", fields, new object[] { "E9", 2 });
                env.AssertSubscriber(
                    "update_2",
                    subscriber => {
                        SupportBean.Compare(subscriber.OldDataListFlattened[0], "E9", 2);
                        SupportBean.Compare(subscriber.NewDataListFlattened[0], "E9", 1002);
                        subscriber.Reset();
                    });

                env.UndeployModuleContaining("update_2");

                env.SendEventBean(new SupportBean("E10", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E10", 2 });
                env.AssertPropsNew("insert", fields, new object[] { "E10", 2 });

                env.CompileDeploy("@name('update_3') update istream MyStreamBW set IntPrimitive=IntBoxed", path);
                env.AddListener("update_3");

                env.SendEventBean(new SupportBean("E11", 2));
                env.AssertListener(
                    "update_3",
                    listener => EPAssertionUtil.AssertProps(
                        listener.AssertOneGetNew(),
                        fields,
                        new object[] { "E11", 2 }));

                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateInsertIntoWMapNoWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@name('insert') @public insert into MyStreamII select * from MyMapTypeII", path)
                    .AddListener("insert");

                var update = env.Compile("@name('update') update istream MyStreamII set P0=P1, P1=P0", path);
                env.Deploy(update);

                env.CompileDeploy("@name('s0') select * from MyStreamII", path).AddListener("s0");

                var fields = "P0,P1,P2".SplitCsv();
                env.SendEventMap(MakeMap("P0", 10L, "P1", 1L, "P2", 100L), "MyMapTypeII");
                env.AssertPropsNew("s0", fields, new object[] { 1L, 10L, 100L });
                env.AssertPropsNew("insert", fields, new object[] { 10L, 1L, 100L });

                env.UndeployModuleContaining("update");
                env.Deploy(update).AddListener("update");

                env.SendEventMap(MakeMap("P0", 5L, "P1", 4L, "P2", 101L), "MyMapTypeII");
                env.AssertPropsNew("s0", fields, new object[] { 4L, 5L, 101L });
                env.AssertPropsNew("insert", fields, new object[] { 5L, 4L, 101L });

                env.UndeployModuleContaining("update");

                env.SendEventMap(MakeMap("P0", 20L, "P1", 0L, "P2", 102L), "MyMapTypeII");
                env.AssertPropsNew("s0", fields, new object[] { 20L, 0L, 102L });
                env.AssertPropsNew("insert", fields, new object[] { 20L, 0L, 102L });

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
                    "@public create schema BaseInterface as " + typeof(BaseInterface).MaskTypeName() + ";\n" +
                    "@public create schema BaseOne as " + typeof(BaseOne).MaskTypeName() + ";\n" +
                    "@public create schema BaseOneA as " + typeof(BaseOneA).MaskTypeName() + ";\n" +
                    "@public create schema BaseOneB as " + typeof(BaseOneB).MaskTypeName() + ";\n" +
                    "@public create schema BaseTwo as " + typeof(BaseTwo).MaskTypeName() + ";\n";
                env.CompileDeploy(epl, path);

                // test update applies to child types via interface
                env.CompileDeploy(
                    "@name('insert') insert into BaseOne select P0 as I, P1 as P from MyMapTypeIDB",
                    path);
                env.CompileDeploy("@name('a') update istream BaseInterface set I='XYZ' where I like 'E%'", path);
                env.CompileDeploy("@name('s0') select * from BaseOne", path).AddListener("s0");

                var fields = "I,P".SplitCsv();
                env.SendEventMap(MakeMap("P0", "E1", "P1", "E1"), "MyMapTypeIDB");
                env.AssertPropsNew("s0", fields, new object[] { "XYZ", "E1" });

                env.SendEventMap(MakeMap("P0", "F1", "P1", "E2"), "MyMapTypeIDB");
                env.AssertPropsNew("s0", fields, new object[] { "F1", "E2" });

                env.CompileDeploy("@Priority(2) @Name('b') update istream BaseOne set I='BLANK'", path);

                env.SendEventMap(MakeMap("P0", "somevalue", "P1", "E3"), "MyMapTypeIDB");
                env.AssertPropsNew("s0", fields, new object[] { "BLANK", "E3" });

                env.CompileDeploy("@Priority(3) @Name('c') update istream BaseOneA set I='FINAL'", path);

                env.SendEventMap(MakeMap("P0", "somevalue", "P1", "E4"), "MyMapTypeIDB");
                env.AssertPropsNew("s0", fields, new object[] { "BLANK", "E4" });

                env.UndeployModuleContaining("insert");
                env.CompileDeploy(
                    "@name('insert') insert into BaseOneA select P0 as I, P1 as P, 'a' as pa from MyMapTypeIDB",
                    path);

                env.SendEventMap(MakeMap("P0", "somevalue", "P1", "E5"), "MyMapTypeIDB");
                env.AssertPropsNew("s0", fields, new object[] { "FINAL", "E5" });

                env.UndeployModuleContaining("insert");
                env.CompileDeploy(
                    "@name('insert') insert into BaseOneB select P0 as I, P1 as P, 'b' as pb from MyMapTypeIDB",
                    path);

                env.SendEventMap(MakeMap("P0", "somevalue", "P1", "E6"), "MyMapTypeIDB");
                env.AssertPropsNew("s0", fields, new object[] { "BLANK", "E6" });

                env.UndeployModuleContaining("insert");
                env.CompileDeploy(
                    "@name('insert') insert into BaseTwo select P0 as I, P1 as P from MyMapTypeIDB",
                    path);

                env.UndeployModuleContaining("s0");
                env.CompileDeploy("@name('s0') select * from BaseInterface", path).AddListener("s0");

                env.SendEventMap(MakeMap("P0", "E2", "P1", "E7"), "MyMapTypeIDB");
                env.AssertPropsNew("s0", new string[] { "I" }, new object[] { "XYZ" });

                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "P0,P1".SplitCsv();
                var path = new RegressionPath();

                env.CompileDeploy(
                        "@name('window') @public create window AWindow#keepall select * from MyMapTypeNW",
                        path)
                    .AddListener("window");
                env.CompileDeploy("@name('insert') insert into AWindow select * from MyMapTypeNW", path)
                    .AddListener("insert");
                env.CompileDeploy("@name('select') select * from AWindow", path).AddListener("select");
                env.CompileDeploy("update istream AWindow set P1='newvalue'", path);

                env.Milestone(0);

                env.SendEventMap(MakeMap("P0", "E1", "P1", "oldvalue"), "MyMapTypeNW");
                env.AssertPropsNew("window", fields, new object[] { "E1", "newvalue" });
                env.AssertPropsNew("insert", fields, new object[] { "E1", "oldvalue" });
                env.AssertPropsNew("select", fields, new object[] { "E1", "newvalue" });

                env.CompileDeploy(
                        "@name('onselect') on SupportBean(TheString='A') select win.* from AWindow as win",
                        path)
                    .AddListener("onselect");
                env.SendEventBean(new SupportBean("A", 0));
                env.AssertPropsNew("onselect", fields, new object[] { "E1", "newvalue" });

                env.Milestone(1);

                env.CompileDeploy(
                        "@name('oninsert') @public on SupportBean(TheString='B') insert into MyOtherStream select win.* from AWindow as win",
                        path)
                    .AddListener("oninsert");
                env.SendEventBean(new SupportBean("B", 1));
                env.AssertPropsNew("oninsert", fields, new object[] { "E1", "newvalue" });

                env.Milestone(2);

                env.CompileDeploy("update istream MyOtherStream set P0='a', P1='b'", path);
                env.CompileDeploy("@name('s0') select * from MyOtherStream", path).AddListener("s0");
                env.SendEventBean(new SupportBean("B", 1));
                env.AssertPropsNew("oninsert", fields, new object[] { "E1", "newvalue" });
                env.AssertPropsNew("s0", fields, new object[] { "a", "b" });

                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateTypeWidener : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fields = "TheString,LongBoxed,IntBoxed".SplitCsv();

                env.CompileDeploy("@public insert into AStream select * from SupportBean", path);
                env.CompileDeploy("update istream AStream set LongBoxed=IntBoxed, IntBoxed=null", path);
                env.CompileDeploy("@name('s0') select * from AStream", path).AddListener("s0");

                var bean = new SupportBean("E1", 0);
                bean.LongBoxed = 888L;
                bean.IntBoxed = 999;
                env.SendEventBean(bean);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 999L, null });

                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateSendRouteSenderPreprocess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test map
                env.CompileDeploy("@name('s0') select * from MyMapTypeSR").AddListener("s0");
                env.CompileDeploy("update istream MyMapTypeSR set P0='a'");

                var fields = "P0,P1".SplitCsv();
                env.SendEventMap(MakeMap("P0", "E1", "P1", "E1"), "MyMapTypeSR");
                env.AssertPropsNew("s0", fields, new object[] { "a", "E1" });

                env.SendEventMap(MakeMap("P0", "E2", "P1", "E2"), "MyMapTypeSR");
                env.AssertPropsNew("s0", fields, new object[] { "a", "E2" });

                env.CompileDeploy("@name('trigger') select * from SupportBean");
                env.Statement("trigger").Events += (
                    sender,
                    args) => env.EventService.RouteEventMap(MakeMap("P0", "E3", "P1", "E3"), "MyMapTypeSR");
                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", fields, new object[] { "a", "E3" });

                env.CompileDeploy("@Drop @name('drop') update istream MyMapTypeSR set P0='a'");
                env.SendEventMap(MakeMap("P0", "E4", "P1", "E4"), "MyMapTypeSR");
                env.SendEventMap(MakeMap("P0", "E5", "P1", "E5"), "MyMapTypeSR");
                env.SendEventBean(new SupportBean());
                env.AssertListenerNotInvoked("s0");

                env.UndeployModuleContaining("drop");
                env.UndeployModuleContaining("s0");
                env.UndeployModuleContaining("trigger");

                // test bean
                env.CompileDeploy("@name('s0') select * from SupportBean").AddListener("s0");
                env.CompileDeploy("update istream SupportBean set IntPrimitive=999");

                fields = "TheString,IntPrimitive".SplitCsv();
                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsNew("s0", fields, new object[] { "E1", 999 });

                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertPropsNew("s0", fields, new object[] { "E2", 999 });

                env.CompileDeploy("@name('trigger') select * from MyMapTypeSR");
                env.Statement("trigger").Events += ((
                    sender,
                    args) => env.EventService.RouteEventBean(new SupportBean("E3", 0), "SupportBean"));
                env.SendEventMap(MakeMap("P0", "", "P1", ""), "MyMapTypeSR");
                env.AssertPropsNew("s0", fields, new object[] { "E3", 999 });

                env.CompileDeploy("@Drop update istream SupportBean set IntPrimitive=1");
                env.SendEventBean(new SupportBean("E4", 0));
                env.SendEventBean(new SupportBean("E4", 0));
                env.SendEventMap(MakeMap("P0", "", "P1", ""), "MyMapTypeSR");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        internal class EPLOtherUpdateSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.UpdateClause = UpdateClause.Create(
                    "MyMapTypeSODA",
                    Expressions.Eq(Expressions.Property("P1"), Expressions.Constant("newvalue")));
                model.UpdateClause.OptionalAsClauseStreamName = "mytype";
                model.UpdateClause.OptionalWhereClause = Expressions.Eq("P0", "E1");
                ClassicAssert.AreEqual(
                    "update istream MyMapTypeSODA as mytype set P1=\"newvalue\" where P0=\"E1\"",
                    model.ToEPL());

                // test map
                env.CompileDeploy("@name('s0') select * from MyMapTypeSODA").AddListener("s0");
                env.CompileDeploy(model);

                var fields = "P0,P1".SplitCsv();
                env.SendEventMap(MakeMap("P0", "E1", "P1", "E1"), "MyMapTypeSODA");
                env.AssertPropsNew("s0", fields, new object[] { "E1", "newvalue" });

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
                env.CompileDeploy(
                    "@public insert into ABCStreamXML select 1 as ValOne, 2 as ValTwo, * from MyXMLEvent",
                    path);
                env.CompileDeploy(
                    "update istream ABCStreamXML set ValOne = 987, ValTwo=123 where prop1='SAMPLE_V1'",
                    path);
                env.CompileDeploy("@name('s0') select * from ABCStreamXML", path).AddListener("s0");

                env.SendEventXMLDOM(simpleDoc, "MyXMLEvent");
                env.AssertPropsNew("s0", "ValOne,ValTwo,prop1".SplitCsv(), new object[] { 987, 123, "SAMPLE_V1" });

                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateWrappedObject : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public insert into ABCStreamWO select 1 as ValOne, 2 as ValTwo, * from SupportBean",
                    path);
                env.CompileDeploy("@name('update') update istream ABCStreamWO set ValOne = 987, ValTwo=123", path);
                env.CompileDeploy("@name('s0') select * from ABCStreamWO", path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsNew("s0", "ValOne,ValTwo,TheString".SplitCsv(), new object[] { 987, 123, "E1" });

                env.UndeployModuleContaining("update");
                env.CompileDeploy("@name('update') update istream ABCStreamWO set TheString = 'A'", path);

                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertPropsNew("s0", "ValOne,ValTwo,TheString".SplitCsv(), new object[] { 1, 2, "A" });

                env.UndeployModuleContaining("update");
                env.CompileDeploy("update istream ABCStreamWO set TheString = 'B', ValOne = 555", path);

                env.SendEventBean(new SupportBean("E3", 0));
                env.AssertPropsNew("s0", "ValOne,ValTwo,TheString".SplitCsv(), new object[] { 555, 2, "B" });

                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateCopyMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public insert into ABCStreamCM select * from SupportBeanCopyMethod", path);
                env.CompileDeploy("update istream ABCStreamCM set ValOne = 'x', ValTwo='y'", path);
                env.CompileDeploy("@name('s0') select * from ABCStreamCM", path).AddListener("s0");

                env.SendEventBean(new SupportBeanCopyMethod("1", "2"));
                env.AssertPropsNew("s0", "ValOne,ValTwo".SplitCsv(), new object[] { "x", "y" });

                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fields = "TheString,IntPrimitive".SplitCsv();
                env.CompileDeploy("@public insert into ABCStreamSQ select * from SupportBean", path);
                env.CompileDeploy(
                    "@name('update') update istream ABCStreamSQ set TheString = (select s0 from MyMapTypeSelect#lastevent) where IntPrimitive in (select w0 from MyMapTypeWhere#keepall)",
                    path);
                env.CompileDeploy("@name('s0') select * from ABCStreamSQ", path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsNew("s0", fields, new object[] { "E1", 0 });

                env.SendEventMap(MakeMap("w0", 1), "MyMapTypeWhere");
                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertPropsNew("s0", fields, new object[] { null, 1 });

                env.SendEventBean(new SupportBean("E3", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E3", 2 });

                env.SendEventMap(MakeMap("s0", "newvalue"), "MyMapTypeSelect");
                env.SendEventBean(new SupportBean("E4", 1));
                env.AssertPropsNew("s0", fields, new object[] { "newvalue", 1 });

                env.SendEventMap(MakeMap("s0", "othervalue"), "MyMapTypeSelect");
                env.SendEventBean(new SupportBean("E5", 1));
                env.AssertPropsNew("s0", fields, new object[] { "othervalue", 1 });

                // test correlated subquery
                env.UndeployModuleContaining("update");
                env.CompileDeploy(
                    "@name('update') update istream ABCStreamSQ set IntPrimitive = (select s1 from MyMapTypeSelect#keepall where s0 = ABCStreamSQ.TheString)",
                    path);

                // note that this will log an error (int primitive set to null), which is good, and leave the value unchanged
                env.SendEventBean(new SupportBean("E6", 8));
                env.AssertPropsNew("s0", fields, new object[] { "E6", 8 });

                env.SendEventMap(MakeMap("s0", "E7", "s1", 91), "MyMapTypeSelect");
                env.SendEventBean(new SupportBean("E7", 0));
                env.AssertPropsNew("s0", fields, new object[] { "E7", 91 });

                // test correlated with as-clause
                env.UndeployModuleContaining("update");
                env.CompileDeploy(
                    "@name('update') update istream ABCStreamSQ as mystream set IntPrimitive = (select s1 from MyMapTypeSelect#keepall where s0 = mystream.TheString)",
                    path);

                // note that this will log an error (int primitive set to null), which is good, and leave the value unchanged
                env.SendEventBean(new SupportBean("E8", 111));
                env.AssertPropsNew("s0", fields, new object[] { "E8", 111 });

                env.SendEventMap(MakeMap("s0", "E9", "s1", -1), "MyMapTypeSelect");
                env.SendEventBean(new SupportBean("E9", 0));
                env.AssertPropsNew("s0", fields, new object[] { "E9", -1 });

                env.UndeployAll();
            }
        }

        internal class EPLOtherUpdateUnprioritizedOrder : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "s0,s1".SplitCsv();
                var path = new RegressionPath();
                env.CompileDeploy("@public insert into ABCStreamUO select * from MyMapTypeUO", path);
                env.CompileDeploy("@name('A') update istream ABCStreamUO set s0='A'", path);
                env.CompileDeploy("@name('B') update istream ABCStreamUO set s0='B'", path);
                env.CompileDeploy("@name('C') update istream ABCStreamUO set s0='C'", path);
                env.CompileDeploy("@name('D') update istream ABCStreamUO set s0='D'", path);
                env.CompileDeploy("@name('s0') select * from ABCStreamUO", path).AddListener("s0");

                env.SendEventMap(MakeMap("s0", "", "s1", 1), "MyMapTypeUO");
                env.AssertPropsNew("s0", fields, new object[] { "D", 1 });

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
                env.CompileDeploy(
                        "@name('insert') @public insert into ABCStreamLD select *, 'orig' as value1 from SupportBean",
                        path)
                    .Statement("insert")
                    .AddListener(listenerInsert);
                env.CompileDeploy(
                        "@name('A') update istream ABCStreamLD set TheString='A', value1='a' where IntPrimitive in (1,2)",
                        path)
                    .Statement("A")
                    .AddListener(listeners[0]);
                env.CompileDeploy(
                        "@name('B') update istream ABCStreamLD set TheString='B', value1='b' where IntPrimitive in (1,3)",
                        path)
                    .Statement("B")
                    .AddListener(listeners[1]);
                env.CompileDeploy(
                        "@name('C') update istream ABCStreamLD set TheString='C', value1='c' where IntPrimitive in (2,3)",
                        path)
                    .Statement("C")
                    .AddListener(listeners[2]);
                env.CompileDeploy("@name('s0') select * from ABCStreamLD", path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("insert", fields, new object[] { "E1", 1, "orig" });
                EPAssertionUtil.AssertProps(listeners[0].AssertOneGetOld(), fields, new object[] { "E1", 1, "orig" });
                EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNew(), fields, new object[] { "A", 1, "a" });
                EPAssertionUtil.AssertProps(listeners[1].AssertOneGetOld(), fields, new object[] { "A", 1, "a" });
                EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNew(), fields, new object[] { "B", 1, "b" });
                ClassicAssert.IsFalse(listeners[2].IsInvoked);
                env.AssertPropsNew("s0", fields, new object[] { "B", 1, "b" });
                Reset(listeners);

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("insert", fields, new object[] { "E2", 2, "orig" });
                EPAssertionUtil.AssertProps(listeners[0].AssertOneGetOld(), fields, new object[] { "E2", 2, "orig" });
                EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNew(), fields, new object[] { "A", 2, "a" });
                ClassicAssert.IsFalse(listeners[1].IsInvoked);
                EPAssertionUtil.AssertProps(listeners[2].AssertOneGetOld(), fields, new object[] { "A", 2, "a" });
                EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNew(), fields, new object[] { "C", 2, "c" });
                env.AssertPropsNew("s0", fields, new object[] { "C", 2, "c" });
                Reset(listeners);

                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertPropsNew("insert", fields, new object[] { "E3", 3, "orig" });
                ClassicAssert.IsFalse(listeners[0].IsInvoked);
                EPAssertionUtil.AssertProps(listeners[1].AssertOneGetOld(), fields, new object[] { "E3", 3, "orig" });
                EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNew(), fields, new object[] { "B", 3, "b" });
                EPAssertionUtil.AssertProps(listeners[2].AssertOneGetOld(), fields, new object[] { "B", 3, "b" });
                EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNew(), fields, new object[] { "C", 3, "c" });
                env.AssertPropsNew("s0", fields, new object[] { "C", 3, "c" });
                Reset(listeners);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
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
                env.CompileDeploy(
                        "@name('insert') @public insert into ABCStreamLDM select *, 'orig' as value1 from SupportBean",
                        path)
                    .Statement("insert")
                    .AddListener(listenerInsert);
                env.CompileDeploy("@name('s0') select * from ABCStreamLDM", path).AddListener("s0");

                env.CompileDeploy("@name('A') update istream ABCStreamLDM set TheString='A', value1='a'", path);
                env.CompileDeploy("@name('B') update istream ABCStreamLDM set TheString='B', value1='b'", path)
                    .Statement("B")
                    .AddListener(listeners[1]);
                env.CompileDeploy("@name('C') update istream ABCStreamLDM set TheString='C', value1='c'", path);
                env.CompileDeploy("@name('D') update istream ABCStreamLDM set TheString='D', value1='d'", path)
                    .Statement("D")
                    .AddListener(listeners[3]);
                env.CompileDeploy("@name('E') update istream ABCStreamLDM set TheString='E', value1='e'", path);

                env.SendEventBean(new SupportBean("E4", 4));
                env.AssertPropsNew("insert", fields, new object[] { "E4", 4, "orig" });
                ClassicAssert.IsFalse(listeners[0].IsInvoked);
                EPAssertionUtil.AssertProps(listeners[1].AssertOneGetOld(), fields, new object[] { "A", 4, "a" });
                EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNew(), fields, new object[] { "B", 4, "b" });
                ClassicAssert.IsFalse(listeners[2].IsInvoked);
                EPAssertionUtil.AssertProps(listeners[3].AssertOneGetOld(), fields, new object[] { "C", 4, "c" });
                EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNew(), fields, new object[] { "D", 4, "d" });
                ClassicAssert.IsFalse(listeners[4].IsInvoked);
                env.AssertPropsNew("s0", fields, new object[] { "E", 4, "e" });
                Reset(listeners);

                env.Statement("B").RemoveAllListeners();
                env.Statement("D").RemoveAllListeners();
                env.Statement("A").AddListener(listeners[0]);
                env.Statement("E").AddListener(listeners[4]);

                env.SendEventBean(new SupportBean("E5", 5));
                env.AssertPropsNew("insert", fields, new object[] { "E5", 5, "orig" });
                EPAssertionUtil.AssertProps(listeners[0].AssertOneGetOld(), fields, new object[] { "E5", 5, "orig" });
                EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNew(), fields, new object[] { "A", 5, "a" });
                ClassicAssert.IsFalse(listeners[1].IsInvoked);
                ClassicAssert.IsFalse(listeners[2].IsInvoked);
                ClassicAssert.IsFalse(listeners[3].IsInvoked);
                EPAssertionUtil.AssertProps(listeners[4].AssertOneGetOld(), fields, new object[] { "D", 5, "d" });
                EPAssertionUtil.AssertProps(listeners[4].AssertOneGetNew(), fields, new object[] { "E", 5, "e" });
                env.AssertPropsNew("s0", fields, new object[] { "E", 5, "e" });
                Reset(listeners);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        internal class EPLOtherUpdateNWSetMapProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    RunAssertionNamedWindowSetMapProps(env, rep);
                }
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        internal class EPLOtherUpdateMapSetMapPropsBean : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionSetMapPropsBean(env);
            }
        }

        internal class EPLOtherUpdateMapSetMapPropsRep : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    RunAssertionUpdateIStreamSetMapProps(env, rep);
                }
            }
        }

        private static void RunAssertionSetMapPropsBean(RegressionEnvironment env)
        {
            // test update-istream with bean
            var path = new RegressionPath();
            env.CompileDeploy(
                "@public @buseventtype create schema MyMapPropEvent as " + typeof(MyMapPropEvent).MaskTypeName(),
                path);
            env.CompileDeploy("@public insert into MyStream select * from MyMapPropEvent", path);
            env.CompileDeploy("@name('s0') update istream MyStream set props('abc') = 1, array[2] = 10", path)
                .AddListener("s0");

            env.SendEventBean(new MyMapPropEvent());
            env.AssertPropsIRPair(
                "s0",
                "props('abc'),array[2]".SplitCsv(),
                new object[] { 1, 10 },
                new object[] { null, null });

            env.UndeployAll();
        }

        private static void RunAssertionUpdateIStreamSetMapProps(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            // test update-istream with map
            var path = new RegressionPath();
            var eplType = eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMapProp)) +
                          " @name('type') @public @buseventtype create schema MyInfraTypeWithMapProp(simple String, myarray int[], mymap System.Collections.Generic.IDictionary)";
            env.CompileDeploy(eplType, path);

            env.CompileDeploy(
                    "@name('update') update istream MyInfraTypeWithMapProp set simple='A', mymap('abc') = 1, myarray[2] = 10",
                    path)
                .AddListener("update");

            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(
                    new object[] { null, new int[10], new Dictionary<string, object>() },
                    "MyInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(MakeMapEvent(new Dictionary<string, object>(), new int[10]), "MyInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = env.RuntimeAvroSchemaByDeployment("type", "MyInfraTypeWithMapProp").AsRecordSchema();
                var @event = new GenericRecord(schema);
                @event.Put("myarray", Arrays.AsList(0, 0, 0, 0, 0));
                @event.Put("mymap", new Dictionary<string, object>());
                @event.Put("simple", "");
                env.SendEventAvro(@event, "MyInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                var myarray = new JArray(0, 0, 0, 0);
                var mymap = new JObject();
                var @event = new JObject(
                    new JProperty("myarray", myarray),
                    new JProperty("mymap", mymap));
                env.SendEventJson(@event.ToString(), "MyInfraTypeWithMapProp");
            }
            else {
                Assert.Fail();
            }

            var simpleExpected = eventRepresentationEnum.IsAvroEvent() ? "" : null;
            env.AssertPropsIRPair(
                "update",
                "simple,mymap('abc'),myarray[2]".SplitCsv(),
                new object[] { "A", 1, 10 },
                new object[] { simpleExpected, null, 0 });

            env.UndeployAll();
        }

        private static void RunAssertionNamedWindowSetMapProps(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            // test named-window update
            var path = new RegressionPath();
            var eplTypes = eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMapProp)) +
                           " @name('type') @public @buseventtype create schema MyNWInfraTypeWithMapProp(simple String, myarray int[], mymap System.Collections.Generic.IDictionary)";
            env.CompileDeploy(eplTypes, path);

            env.CompileDeploy(
                "@name('window') @public create window MyWindowWithMapProp#keepall as MyNWInfraTypeWithMapProp",
                path);
            env.CompileDeploy(
                eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedMapProp)) +
                " insert into MyWindowWithMapProp select * from MyNWInfraTypeWithMapProp",
                path);

            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(
                    new object[] { null, new int[10], new Dictionary<string, object>() },
                    "MyNWInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(
                    MakeMapEvent(new Dictionary<string, object>(), new int[10]),
                    "MyNWInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = env.RuntimeAvroSchemaByDeployment("type", "MyNWInfraTypeWithMapProp");
                var @event = new GenericRecord(schema.AsRecordSchema());
                @event.Put("myarray", Arrays.AsList(0, 0, 0, 0, 0));
                @event.Put("mymap", new Dictionary<string, object>());
                env.SendEventAvro(@event, "MyNWInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                var myarray = new JArray(0, 0, 0, 0);
                var mymap = new JObject();
                var @event = new JObject(new JProperty("myarray", myarray), new JProperty("mymap", mymap));
                env.SendEventJson(@event.ToString(), "MyNWInfraTypeWithMapProp");
            }
            else {
                Assert.Fail();
            }

            env.CompileDeploy(
                "on SupportBean update MyWindowWithMapProp set simple='A', mymap('abc') = IntPrimitive, myarray[2] = IntPrimitive",
                path);
            env.SendEventBean(new SupportBean("E1", 10));
            env.AssertPropsPerRowIterator(
                "window",
                "simple,mymap('abc'),myarray[2]".SplitCsv(),
                new object[][] { new object[] { "A", 10, 10 } });

            // test null and array too small
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] { null, new int[2], null }, "MyNWInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsMapEvent()) {
                env.SendEventMap(MakeMapEvent(null, new int[2]), "MyNWInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var @event = new GenericRecord(
                    SchemaBuilder.Record(
                        "name",
                        OptionalString("simple"),
                        Field("myarray", Array(LongType())),
                        Field("mymap", Map(StringType()))));

                @event.Put("myarray", Arrays.AsList(0, 0));
                @event.Put("mymap", null);
                env.SendEventAvro(@event, "MyNWInfraTypeWithMapProp");
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                var myarray = new JArray(0, 0);
                var @event = new JObject(new JProperty("myarray", myarray));
                env.SendEventJson(@event.ToString(), "MyNWInfraTypeWithMapProp");
            }
            else {
                Assert.Fail();
            }

            env.SendEventBean(new SupportBean("E2", 20));
            env.AssertPropsPerRowIteratorAnyOrder(
                "window",
                "simple,mymap('abc'),myarray[2]".SplitCsv(),
                new object[][] { new object[] { "A", 20, 20 }, new object[] { "A", null, null } });

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
            var prefix = eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedSB));
            env.CompileDeploy(
                prefix +
                " @public insert into MyStream select TheString, IntPrimitive from SupportBean(TheString not like 'Z%')",
                path);
            env.CompileDeploy(
                prefix +
                " @public insert into MyStream select 'AX'||TheString as TheString, IntPrimitive from SupportBean(TheString like 'Z%')",
                path);
            env.CompileDeploy(
                prefix + " @Name('a') @Priority(12) update istream MyStream set IntPrimitive=-2 where IntPrimitive=-1",
                path);
            env.CompileDeploy(
                prefix +
                " @Name('b') @Priority(11) update istream MyStream set IntPrimitive=-1 where TheString like 'D%'",
                path);
            env.CompileDeploy(
                prefix +
                " @Name('c') @Priority(9) update istream MyStream set IntPrimitive=9 where TheString like 'A%'",
                path);
            env.CompileDeploy(
                prefix +
                " @Name('d') @Priority(8) update istream MyStream set IntPrimitive=8 where TheString like 'A%' or TheString like 'C%'",
                path);
            env.CompileDeploy(
                " @Name('e') @Priority(10) update istream MyStream set IntPrimitive=10 where TheString like 'A%'",
                path);
            env.CompileDeploy(
                " @Name('f') @Priority(7) update istream MyStream set IntPrimitive=7 where TheString like 'A%' or TheString like 'C%'",
                path);
            env.CompileDeploy(
                " @Name('g') @Priority(6) update istream MyStream set IntPrimitive=6 where TheString like 'A%'",
                path);
            env.CompileDeploy(
                " @Name('h') @Drop update istream MyStream set IntPrimitive=6 where TheString like 'B%'",
                path);

            env.CompileDeploy("@name('s0') select * from MyStream where IntPrimitive > 0", path).AddListener("s0");

            var fields = "TheString,IntPrimitive".SplitCsv();
            env.SendEventBean(new SupportBean("A1", 0));
            env.AssertPropsNew("s0", fields, new object[] { "A1", 10 });

            env.SendEventBean(new SupportBean("B1", 0));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean("C1", 0));
            env.AssertPropsNew("s0", fields, new object[] { "C1", 8 });

            env.SendEventBean(new SupportBean("D1", 100));
            env.AssertListenerNotInvoked("s0");

            env.UndeployModuleContaining("s0");
            env.CompileDeploy("@name('s0') select * from MyStream", path).AddListener("s0");
            env.AssertStatement(
                "s0",
                statement => ClassicAssert.IsTrue(eventRepresentationEnum.MatchesClass(statement.EventType.UnderlyingType)));

            env.SendEventBean(new SupportBean("D1", -2));
            env.AssertPropsNew("s0", fields, new object[] { "D1", -2 });

            env.SendEventBean(new SupportBean("Z1", -3));
            env.AssertPropsNew("s0", fields, new object[] { "AXZ1", 10 });

            env.UndeployModuleContaining("e");
            env.SendEventBean(new SupportBean("Z2", 0));
            env.AssertPropsNew("s0", fields, new object[] { "AXZ2", 9 });

            env.UndeployModuleContaining("c");
            env.UndeployModuleContaining("d");
            env.UndeployModuleContaining("f");
            env.UndeployModuleContaining("g");
            env.SendEventBean(new SupportBean("Z3", 0));
            env.AssertPropsNew("s0", fields, new object[] { "AXZ3", 0 });

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
            public string I { get; set; }
        }

        public class BaseOne : BaseInterface
        {
            private string i;
            private string p;

            public string I {
                get => i;
                set => i = value;
            }

            public string P {
                get => p;
                set => p = value;
            }

            public BaseOne()
            {
            }

            public BaseOne(
                string i,
                string p)
            {
                this.I = i;
                this.p = p;
            }
        }

        public class BaseTwo : BaseInterface
        {
            private string i;
            private string p;

            public BaseTwo()
            {
            }

            public BaseTwo(string p)
            {
                this.p = p;
            }

            public string P {
                get => p;
                set => this.p = value;
            }

            public string I {
                get => i;
                set => this.i = value;
            }
        }

        public class BaseOneA : BaseOne
        {
            private string pa;

            public BaseOneA()
            {
            }

            public BaseOneA(
                string i,
                string p,
                string pa) : base(i, p)
            {
                this.pa = pa;
            }

            public string Pa {
                get => pa;
                set => this.pa = value;
            }
        }

        public class BaseOneB : BaseOne
        {
            private string pb;

            public BaseOneB()
            {
            }

            public BaseOneB(
                string i,
                string p,
                string pb) : base(i, p)
            {
                this.pb = pb;
            }

            public string Pb {
                get => pb;
                set => this.pb = value;
            }
        }

        public static void SetIntBoxedValue(
            SupportBean sb,
            int value)
        {
            sb.IntBoxed = value;
        }

        public class MyMapPropEvent
        {
            private IDictionary<string, object> props = new Dictionary<string, object>();
            private object[] array = new object[10];

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

            public IDictionary<string, object> Props {
                get => props;
                set => this.props = value;
            }

            public object[] Array {
                get => array;
                set => this.array = value;
            }

            public object GetArray(int index)
            {
                return array[index];
            }
        }

        public class MyLocalJsonProvidedMapProp
        {
            public string simple;
            public int?[] myarray;
            public IDictionary<string, object> mymap;
        }

        public class MyLocalJsonProvidedSB
        {
            public string TheString;
            public int IntPrimitive;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization
        /// is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        public class SupportEventWithListOfObject
        {
            private IList<object> mylist;
            private bool updated;

            public SupportEventWithListOfObject(IList<object> mylist)
            {
                this.mylist = mylist;
                this.updated = false;
            }

            public bool IsUpdated {
                get => updated;
                set => this.updated = value;
            }

            public IList<object> Mylist {
                get => mylist;
                set => this.mylist = value;
            }
        }
    }
} // end of namespace