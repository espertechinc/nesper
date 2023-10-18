///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

using SupportBean_N = com.espertech.esper.regressionlib.support.bean.SupportBean_N;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
	public class EPLInsertIntoPopulateUnderlying
	{

		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EPLInsertIntoCtor());
			execs.Add(new EPLInsertIntoCtorWithPattern());
			execs.Add(new EPLInsertIntoBeanJoin());
			execs.Add(new EPLInsertIntoPopulateBeanSimple());
			execs.Add(new EPLInsertIntoBeanWildcard());
			execs.Add(new EPLInsertIntoPopulateBeanObjects());
			execs.Add(new EPLInsertIntoPopulateUnderlyingSimple());
			execs.Add(new EPLInsertIntoCharSequenceCompat());
			execs.Add(new EPLInsertIntoBeanFactoryMethod());
			execs.Add(new EPLInsertIntoArrayPONOInsert());
			execs.Add(new EPLInsertIntoArrayMapInsert());
			execs.Add(new EPLInsertIntoWindowAggregationAtEventBean());
			execs.Add(new EPLInsertIntoInvalid());
			return execs;
		}

		private class EPLInsertIntoWindowAggregationAtEventBean : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.CompileDeploy(
						"@name('s0') insert into SupportBeanArrayEvent select window(*) @eventbean from SupportBean#keepall")
					.AddListener("s0");

				var e1 = new SupportBean("E1", 1);
				env.SendEventBean(e1);
				env.AssertEventNew("s0", @event => AssertMyEventTargetWithArray(@event, e1));

				var e2 = new SupportBean("E2", 2);
				env.SendEventBean(e2);
				env.AssertEventNew("s0", @event => AssertMyEventTargetWithArray(@event, e1, e2));

				env.UndeployAll();
			}

			private static void AssertMyEventTargetWithArray(
				EventBean eventBean,
				params SupportBean[] beans)
			{
				var und = (SupportBeanArrayEvent)eventBean.Underlying;
				EPAssertionUtil.AssertEqualsExactOrder(und.Array, beans);
			}
		}

		private class EPLInsertIntoCtor : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				// simple type and null values
				var eplOne =
					"@name('s0') insert into SupportBeanCtorOne select theString, intBoxed, intPrimitive, boolPrimitive from SupportBean";
				env.CompileDeploy(eplOne).AddListener("s0");

				SendReceive(env, "E1", 2, true, 100);
				SendReceive(env, "E2", 3, false, 101);
				SendReceive(env, null, 4, true, null);
				env.UndeployModuleContaining("s0");

				// boxable type and null values
				var eplTwo =
					"@name('s0') insert into SupportBeanCtorOne select theString, null, intBoxed from SupportBean";
				env.CompileDeploy(eplTwo).AddListener("s0");
				SendReceiveTwo(env, "E1", 100);
				env.UndeployModuleContaining("s0");

				// test join wildcard
				var eplThree =
					"@name('s0') insert into SupportBeanCtorTwo select * from SupportBean_ST0#lastevent, SupportBean_ST1#lastevent";
				env.CompileDeploy(eplThree).AddListener("s0");

				env.SendEventBean(new SupportBean_ST0("ST0", 1));
				env.SendEventBean(new SupportBean_ST1("ST1", 2));
				env.AssertEventNew(
					"s0",
					@event => {
						var theEvent = (SupportBeanCtorTwo)@event.Underlying;
						Assert.IsNotNull(theEvent.St0);
						Assert.IsNotNull(theEvent.St1);
					});
				env.UndeployModuleContaining("s0");

				// test (should not use column names)
				var eplFour =
					"@name('s0') insert into SupportBeanCtorOne(theString, intPrimitive) select 'E1', 5 from SupportBean";
				env.CompileDeploy(eplFour).AddListener("s0");
				env.SendEventBean(new SupportBean("x", -1));
				env.AssertEventNew(
					"s0",
					@event => {
						var eventOne = (SupportBeanCtorOne)@event.Underlying;
						Assert.AreEqual("E1", eventOne.TheString);
						Assert.AreEqual(99, eventOne.IntPrimitive);
						Assert.AreEqual((int?)5, eventOne.IntBoxed);
					});

				// test Ctor accepting same types
				env.UndeployAll();
				var epl =
					"@name('s0') insert into SupportEventWithCtorSameType select c1,c2 from SupportBean(theString='b1')#lastevent as c1, SupportBean(theString='b2')#lastevent as c2";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean("b1", 1));
				env.SendEventBean(new SupportBean("b2", 2));
				env.AssertEventNew(
					"s0",
					@event => {
						var result = (SupportEventWithCtorSameType)@event.Underlying;
						Assert.AreEqual(1, result.B1.IntPrimitive);
						Assert.AreEqual(2, result.B2.IntPrimitive);
					});

				env.UndeployAll();
			}
		}

		private class EPLInsertIntoCtorWithPattern : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// Test valid case of array insert
				var epl = "@name('s0') insert into SupportBeanCtorThree select s, e FROM PATTERN [" +
				          "every s=SupportBean_ST0 -> [2] e=SupportBean_ST1]";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventBean(new SupportBean_ST0("E0", 1));
				env.SendEventBean(new SupportBean_ST1("E1", 2));
				env.SendEventBean(new SupportBean_ST1("E2", 3));
				env.AssertEventNew(
					"s0",
					@event => {
						var three = (SupportBeanCtorThree)@event.Underlying;
						Assert.AreEqual("E0", three.St0.Id);
						Assert.AreEqual(2, three.St1.Length);
						Assert.AreEqual("E1", three.St1[0].Id);
						Assert.AreEqual("E2", three.St1[1].Id);
					});

				env.UndeployAll();
			}
		}

		private class EPLInsertIntoBeanJoin : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var n1 = new SupportBean_N(1, 10, 100d, 1000d, true, true);
				// test wildcard
				var stmtTextOne =
					"@name('s0') insert into SupportBeanObject select * from SupportBean_N#lastevent as one, SupportBean_S0#lastevent as two";
				env.CompileDeploy(stmtTextOne).AddListener("s0");

				env.SendEventBean(n1);
				var s01 = new SupportBean_S0(1);
				env.SendEventBean(s01);
				env.AssertEventNew(
					"s0",
					@event => {
						var theEvent = (SupportBeanObject)@event.Underlying;
						Assert.AreSame(n1, theEvent.One);
						Assert.AreSame(s01, theEvent.Two);
					});
				env.UndeployModuleContaining("s0");

				// test select stream names
				stmtTextOne =
					"@name('s0') insert into SupportBeanObject select one, two from SupportBean_N#lastevent as one, SupportBean_S0#lastevent as two";
				env.CompileDeploy(stmtTextOne).AddListener("s0");

				env.SendEventBean(n1);
				env.SendEventBean(s01);
				env.AssertEventNew(
					"s0",
					@event => {
						var theEvent = (SupportBeanObject)@event.Underlying;
						Assert.AreSame(n1, theEvent.One);
						Assert.AreSame(s01, theEvent.Two);
					});
				env.UndeployModuleContaining("s0");

				// test fully-qualified class name as target
				stmtTextOne =
					"@name('s0') insert into SupportBeanObject select one, two from SupportBean_N#lastevent as one, SupportBean_S0#lastevent as two";
				env.CompileDeploy(stmtTextOne).AddListener("s0");

				env.SendEventBean(n1);
				env.SendEventBean(s01);
				env.AssertEventNew(
					"s0",
					@event => {
						var theEvent = (SupportBeanObject)@event.Underlying;
						Assert.AreSame(n1, theEvent.One);
						Assert.AreSame(s01, theEvent.Two);
					});
				env.UndeployModuleContaining("s0");

				// test local class and auto-import
				stmtTextOne = "@name('s0') insert into " +
				              typeof(EPLInsertIntoPopulateUnderlying).FullName +
				              "$MyLocalTarget select 1 as value from SupportBean_N";
				env.CompileDeploy(stmtTextOne).AddListener("s0");
				env.SendEventBean(n1);
				env.AssertEventNew(
					"s0",
					@event => {
						var eventLocal = (MyLocalTarget)@event.Underlying;
						Assert.AreEqual(1, eventLocal.Value);
					});
				env.UndeployAll();
			}
		}

		private class EPLInsertIntoInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				var text = "insert into SupportBeanCtorOne select 1 from SupportBean";
				env.TryInvalidCompile(
					text,
					"Failed to find a suitable constructor for class '" +
					typeof(SupportBeanCtorOne).FullName +
					"': Could not find constructor in class '" +
					typeof(SupportBeanCtorOne).FullName +
					"' with matching parameter number and expected parameter type(s) 'int'");

				text = "insert into SupportBean(intPrimitive) select 1L from SupportBean";
				env.TryInvalidCompile(
					text,
					"Invalid assignment of column 'intPrimitive' of type 'long' to event property 'intPrimitive' typed as 'int', column and parameter types mismatch [insert into SupportBean(intPrimitive) select 1L from SupportBean]");

				text = "insert into SupportBean(intPrimitive) select null from SupportBean";
				env.TryInvalidCompile(
					text,
					"Invalid assignment of column 'intPrimitive' of null type to event property 'intPrimitive' typed as 'int', nullable type mismatch [insert into SupportBean(intPrimitive) select null from SupportBean]");

				text = "insert into SupportBeanReadOnly select 'a' as geom from SupportBean";
				env.TryInvalidCompile(
					text,
					"Failed to find a suitable constructor for class '" +
					typeof(SupportBeanReadOnly).FullName +
					"': Could not find constructor in class '" +
					typeof(SupportBeanReadOnly).FullName +
					"' with matching parameter number and expected parameter type(s) 'String' (nearest matching constructor taking no parameters) [insert into SupportBeanReadOnly select 'a' as geom from SupportBean]");

				text = "insert into SupportBean select 3 as dummyField from SupportBean";
				env.TryInvalidCompile(
					text,
					"Column 'dummyField' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean select 3 as dummyField from SupportBean]");

				text = "insert into SupportBean select 3 from SupportBean";
				env.TryInvalidCompile(
					text,
					"Column '3' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean select 3 from SupportBean]");

				text = "insert into SupportBeanInterfaceProps(isa) select isbImpl from MyMap";
				env.TryInvalidCompile(
					text,
					"Invalid assignment of column 'isa' of type '" +
					typeof(ISupportBImpl).FullName +
					"' to event property 'isa' typed as '" +
					typeof(ISupportA).FullName +
					"', column and parameter types mismatch [insert into SupportBeanInterfaceProps(isa) select isbImpl from MyMap]");

				text = "insert into SupportBeanInterfaceProps(isg) select isabImpl from MyMap";
				env.TryInvalidCompile(
					text,
					"Invalid assignment of column 'isg' of type '" +
					typeof(ISupportBaseABImpl).FullName +
					"' to event property 'isg' typed as '" +
					typeof(ISupportAImplSuperG).FullName +
					"', column and parameter types mismatch [insert into SupportBeanInterfaceProps(isg) select isabImpl from MyMap]");

				text = "insert into SupportBean(dummy) select 3 from SupportBean";
				env.TryInvalidCompile(
					text,
					"Column 'dummy' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?) [insert into SupportBean(dummy) select 3 from SupportBean]");

				text = "insert into SupportBeanReadOnly(side) select 'E1' from MyMap";
				env.TryInvalidCompile(
					text,
					"Failed to find a suitable constructor for class '" +
					typeof(SupportBeanReadOnly).FullName +
					"': Could not find constructor in class '" +
					typeof(SupportBeanReadOnly).FullName +
					"' with matching parameter number and expected parameter type(s) 'String' (nearest matching constructor taking no parameters) [insert into SupportBeanReadOnly(side) select 'E1' from MyMap]");

				var path = new RegressionPath();
				env.CompileDeploy("@public insert into ABCStream select *, 1+1 from SupportBean", path);
				text = "@public insert into ABCStream(string) select 'E1' from MyMap";
				env.TryInvalidCompile(
					path,
					text,
					"Event type named 'ABCStream' has already been declared with differing column name or type information: Type by name 'ABCStream' is not a compatible type (target type underlying is '" +
					typeof(Pair<object, IDictionary<string, object>>).FullName +
					"') [@public insert into ABCStream(string) select 'E1' from MyMap]");

				text = "insert into xmltype select 1 from SupportBean";
				env.TryInvalidCompile(
					text,
					"Event type named 'xmltype' has already been declared with differing column name or type information: Type by name 'xmltype' is not a compatible type (target type underlying is '" +
					typeof(XmlNode).FullName +
					"') [insert into xmltype select 1 from SupportBean]");

				text = "insert into MyMap(dummy) select 1 from SupportBean";
				env.TryInvalidCompile(
					text,
					"Event type named 'MyMap' has already been declared with differing column name or type information: Type by name 'MyMap' in property 'dummy' property name not found in target");

				text = "@public create window MyWindow#keepall (c0 null, c1 int);\n" +
				       "insert into MyWindow select 1 as c0 from SupportBean;\n";
				env.TryInvalidCompile(
					text,
					"Event type named 'MyWindow' has already been declared with differing column name or type information: Type by name 'MyWindow' in property 'c0' expects a null-value but receives 'Integer'");

				// setter throws exception
				var stmtTextOne = "@name('s0') insert into SupportBeanErrorTestingTwo(value) select 'E1' from MyMap";
				env.CompileDeploy(stmtTextOne).AddListener("s0");

				try {
					env.SendEventMap(new Dictionary<string, object>(), "MyMap");
					Assert.Fail();
				}
				catch (EPException ex) {
					// expected
				}

				env.UndeployAll();

				// surprise - wrong type than defined
				stmtTextOne = "@name('s0') insert into SupportBean(intPrimitive) select anint from MyMap";
				env.CompileDeploy(stmtTextOne).AddListener("s0");
				IDictionary<string, object> map = new Dictionary<string, object>();
				map.Put("anint", "notAnInt");
				try {
					env.SendEventBean(map, "MyMap");
					env.AssertEqualsNew("s0", "intPrimitive", 0);
				}
				catch (Exception ex) {
					// an exception is possible and up to the implementation.
				}

				// ctor throws exception
				env.UndeployAll();
				var stmtTextThree = "@name('s0') insert into SupportBeanCtorOne select 'E1' from SupportBean";
				env.CompileDeploy(stmtTextThree).AddListener("s0");
				try {
					env.SendEventBean(new SupportBean("E1", 1));
					Assert.Fail(); // rethrowing handler registered
				}
				catch (Exception ex) {
					// expected
				}

				// allow automatic cast of same-type event
				path.Clear();
				env.CompileDeploy("@public create schema MapOneA as (prop1 string)", path);
				env.CompileDeploy("@public create schema MapTwoA as (prop1 string)", path);
				env.CompileDeploy("insert into MapOneA select * from MapTwoA", path);

				env.UndeployAll();
			}

			public ISet<RegressionFlag> Flags()
			{
				return Collections.Set(RegressionFlag.INVALIDITY);
			}
		}

		private class EPLInsertIntoPopulateBeanSimple : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// test select column names
				var stmtTextOne = "@name('i1') insert into SupportBean select " +
				                  "'E1' as theString, 1 as intPrimitive, 2 as intBoxed, 3L as longPrimitive," +
				                  "null as longBoxed, true as boolPrimitive, " +
				                  "'x' as charPrimitive, 0xA as bytePrimitive, " +
				                  "8.0f as floatPrimitive, 9.0d as doublePrimitive, " +
				                  "0x05 as shortPrimitive, SupportEnum.ENUM_VALUE_2 as enumValue " +
				                  " from MyMap";
				env.CompileDeploy(stmtTextOne);

				var stmtTextTwo = "@name('s0') select * from SupportBean";
				env.CompileDeploy(stmtTextTwo).AddListener("s0");

				env.SendEventMap(new Dictionary<string, object>(), "MyMap");
				env.AssertEventNew(
					"s0",
					@event => {
						var received = (SupportBean)@event.Underlying;
						Assert.AreEqual("E1", received.TheString);
						SupportBean.Compare(
							received,
							"intPrimitive,intBoxed,longPrimitive,longBoxed,boolPrimitive,charPrimitive,bytePrimitive,floatPrimitive,doublePrimitive,shortPrimitive,enumValue"
								.SplitCsv(),
							new object[]
								{ 1, 2, 3L, null, true, 'x', (byte)10, 8f, 9d, (short)5, SupportEnum.ENUM_VALUE_2 });
					});
				// test insert-into column names
				env.UndeployModuleContaining("s0");
				env.UndeployModuleContaining("i1");

				stmtTextOne = "@name('s0') insert into SupportBean(theString, intPrimitive, intBoxed, longPrimitive," +
				              "longBoxed, boolPrimitive, charPrimitive, bytePrimitive, floatPrimitive, doublePrimitive, " +
				              "shortPrimitive, enumValue) select " +
				              "'E1', 1, 2, 3L," +
				              "null, true, " +
				              "'x', 0xA, " +
				              "8.0f, 9.0d, " +
				              "0x05 as shortPrimitive, SupportEnum.ENUM_VALUE_2 " +
				              " from MyMap";
				env.CompileDeploy(stmtTextOne).AddListener("s0");

				env.SendEventMap(new Dictionary<string, object>(), "MyMap");
				env.AssertEventNew(
					"s0",
					@event => {
						var received = (SupportBean)@event.Underlying;
						Assert.AreEqual("E1", received.TheString);
						SupportBean.Compare(
							received,
							"intPrimitive,intBoxed,longPrimitive,longBoxed,boolPrimitive,charPrimitive,bytePrimitive,floatPrimitive,doublePrimitive,shortPrimitive,enumValue"
								.SplitCsv(),
							new object[]
								{ 1, 2, 3L, null, true, 'x', (byte)10, 8f, 9d, (short)5, SupportEnum.ENUM_VALUE_2 });
					});

				// test convert Integer boxed to Long boxed
				env.UndeployModuleContaining("s0");
				stmtTextOne =
					"@name('s0') insert into SupportBean(longBoxed, doubleBoxed) select intBoxed, floatBoxed from MyMap";
				env.CompileDeploy(stmtTextOne).AddListener("s0");

				IDictionary<string, object> vals = new Dictionary<string, object>();
				vals.Put("intBoxed", 4);
				vals.Put("floatBoxed", 0f);
				env.SendEventMap(vals, "MyMap");
				env.AssertPropsNew("s0", "longBoxed,doubleBoxed".SplitCsv(), new object[] { 4L, 0d });
				env.UndeployAll();

				// test new-to-map conversion
				env.CompileDeploy(
						"@name('s0') insert into MyEventWithMapFieldSetter(id, themap) " +
						"select 'test' as id, new {somefield = theString} as themap from SupportBean")
					.AddListener("s0");

				env.SendEventBean(new SupportBean("E1", 1));
				env.AssertEventNew(
					"s0",
					@event => EPAssertionUtil.AssertPropsMap(
						(IDictionary<string, object>)@event.Get("themap"),
						"somefield".SplitCsv(),
						"E1"));

				env.UndeployAll();
			}
		}

		private class EPLInsertIntoBeanWildcard : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				var stmtTextOne = "@name('s0') insert into SupportBean select * from MySupportMap";
				env.CompileDeploy(stmtTextOne).AddListener("s0");

				IDictionary<string, object> vals = new Dictionary<string, object>();
				vals.Put("intPrimitive", 4);
				vals.Put("longBoxed", 100L);
				vals.Put("theString", "E1");
				vals.Put("boolPrimitive", true);

				env.SendEventMap(vals, "MySupportMap");
				env.AssertPropsNew(
					"s0",
					"intPrimitive,longBoxed,theString,boolPrimitive".SplitCsv(),
					new object[] { 4, 100L, "E1", true });

				env.UndeployAll();
			}
		}

		private class EPLInsertIntoPopulateBeanObjects : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// arrays and maps
				var stmtTextOne =
					"@name('s0') insert into SupportBeanComplexProps(arrayProperty,objectArray,mapProperty) select " +
					"intArr,{10,20,30},mapProp" +
					" from MyMap as m";
				env.CompileDeploy(stmtTextOne).AddListener("s0");

				IDictionary<string, object> mymapVals = new Dictionary<string, object>();
				mymapVals.Put("intArr", new int[] { -1, -2 });
				IDictionary<string, object> inner = new Dictionary<string, object>();
				inner.Put("mykey", "myval");
				mymapVals.Put("mapProp", inner);
				env.SendEventMap(mymapVals, "MyMap");
				env.AssertEventNew(
					"s0",
					@event => {
						var theEvent = (SupportBeanComplexProps)@event.Underlying;
						Assert.AreEqual(-2, theEvent.ArrayProperty[1]);
						Assert.AreEqual(20, theEvent.ObjectArray[1]);
						Assert.AreEqual("myval", theEvent.MapProperty.Get("mykey"));
					});
				env.UndeployModuleContaining("s0");

				// inheritance
				stmtTextOne = "@name('s0') insert into SupportBeanInterfaceProps(isa,isg) select " +
				              "isaImpl,isgImpl" +
				              " from MyMap";
				env.CompileDeploy(stmtTextOne).AddListener("s0");

				mymapVals = new Dictionary<string, object>();
				mymapVals.Put("mapProp", inner);
				env.SendEventMap(mymapVals, "MyMap");
				env.AssertEventNew(
					"s0",
					@event => {
						Assert.IsTrue(@event.Underlying is SupportBeanInterfaceProps);
						Assert.AreEqual(typeof(SupportBeanInterfaceProps), @event.EventType.UnderlyingType);
					});
				env.UndeployModuleContaining("s0");

				// object values from Map same type
				stmtTextOne = "@name('s0') insert into SupportBeanComplexProps(nested) select nested from MyMap";
				env.CompileDeploy(stmtTextOne).AddListener("s0");

				mymapVals = new Dictionary<string, object>();
				mymapVals.Put("nested", new SupportBeanComplexProps.SupportBeanSpecialGetterNested("111", "222"));
				env.SendEventMap(mymapVals, "MyMap");
				env.AssertEventNew(
					"s0",
					@event => {
						var eventThree = (SupportBeanComplexProps)@event.Underlying;
						Assert.AreEqual("111", eventThree.Nested.NestedValue);
					});
				env.UndeployModuleContaining("s0");

				// object to Object
				stmtTextOne =
					"@name('s0') insert into SupportBeanArrayCollMap(anyObject) select nested from SupportBeanComplexProps";
				env.CompileDeploy(stmtTextOne).AddListener("s0");

				env.SendEventBean(SupportBeanComplexProps.MakeDefaultBean());
				env.AssertEventNew(
					"s0",
					@event => {
						var eventFour = (SupportBeanArrayCollMap)@event.Underlying;
						Assert.AreEqual(
							"nestedValue",
							((SupportBeanComplexProps.SupportBeanSpecialGetterNested)eventFour.AnyObject).NestedValue);
					});
				env.UndeployModuleContaining("s0");

				// test null value
				var stmtTextThree =
					"@name('s0') insert into SupportBean select 'B' as theString, intBoxed as intPrimitive from SupportBean(theString='A')";
				env.CompileDeploy(stmtTextThree).AddListener("s0");

				env.SendEventBean(new SupportBean("A", 0));
				env.AssertEventNew(
					"s0",
					@event => {
						var received = (SupportBean)@event.Underlying;
						Assert.AreEqual(0, received.IntPrimitive);
					});

				var bean = new SupportBean("A", 1);
				bean.IntBoxed = 20;
				env.SendEventBean(bean);
				env.AssertEventNew(
					"s0",
					@event => {
						var received = (SupportBean)@event.Underlying;
						Assert.AreEqual(20, received.IntPrimitive);
					});

				env.UndeployAll();
			}
		}

		private class EPLInsertIntoPopulateUnderlyingSimple : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryAssertionPopulateUnderlying(env, "MyMapType");
				TryAssertionPopulateUnderlying(env, "MyOAType");
				TryAssertionPopulateUnderlying(env, "MyAvroType");
			}
		}

		private class EPLInsertIntoCharSequenceCompat : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
					if (rep.IsJsonEvent() || rep.IsJsonProvidedClassEvent()) {
						continue; // Json doesn't allow CharSequence by itself unless registering an adapter
					}

					var path = new RegressionPath();
					env.CompileDeploy(
						rep.GetAnnotationText() + "create schema ConcreteType as (value java.lang.CharSequence)",
						path);
					env.CompileDeploy("insert into ConcreteType select \"Test\" as value from SupportBean", path);
					env.UndeployAll();
				}
			}
		}

		private class EPLInsertIntoBeanFactoryMethod : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// test factory method on the same event class
				var stmtTextOne = "@name('s0') insert into SupportBeanString select 'abc' as theString from MyMap";
				env.CompileDeploy(stmtTextOne).AddListener("s0").SetSubscriber("s0");

				env.SendEventMap(new Dictionary<string, object>(), "MyMap");
				env.AssertEventNew("s0", @event => { Assert.AreEqual("abc", @event.Get("theString")); });
				env.AssertSubscriber("s0", subscriber => Assert.AreEqual("abc", subscriber.AssertOneGetNewAndReset()));
				env.UndeployModuleContaining("s0");

				// test factory method fully-qualified
				stmtTextOne = "@name('s0') insert into SupportSensorEvent(id, type, device, measurement, confidence)" +
				              "select 2, 'A01', 'DHC1000', 100, 5 from MyMap";
				env.CompileDeploy(stmtTextOne).AddListener("s0");

				env.SendEventMap(new Dictionary<string, object>(), "MyMap");
				env.AssertPropsNew(
					"s0",
					"id,type,device,measurement,confidence".SplitCsv(),
					new object[] { 2, "A01", "DHC1000", 100.0, 5.0 });

                Assert.That(
                    () => TypeHelper.Instantiate(typeof(SupportBeanString)),
                    Throws.InstanceOf<TypeInstantiationException>());

                Assert.That(
                    () => TypeHelper.Instantiate(typeof(SupportSensorEvent)),
                    Throws.InstanceOf<TypeInstantiationException>());

				env.UndeployAll();
			}
		}

		private class EPLInsertIntoArrayPONOInsert : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{

				var path = new RegressionPath();
				var epl = "@public create schema FinalEventInvalidNonArray as " +
				          typeof(FinalEventInvalidNonArray).FullName +
				          ";\n" +
				          "@public create schema FinalEventInvalidArray as " +
				          typeof(FinalEventInvalidArray).FullName +
				          ";\n" +
				          "@public create schema FinalEventValid as " +
				          typeof(FinalEventValid).FullName +
				          ";\n";
				env.CompileDeploy(epl, path);
				env.AdvanceTime(0);

				// Test valid case of array insert
				var validEpl =
					"@name('s0') INSERT INTO FinalEventValid SELECT s as startEvent, e as endEvent FROM PATTERN [" +
					"every s=SupportBean_S0 -> e=SupportBean(theString=s.p00) until timer:interval(10 sec)]";
				env.CompileDeploy(validEpl, path).AddListener("s0");

				env.SendEventBean(new SupportBean_S0(1, "G1"));
				env.SendEventBean(new SupportBean("G1", 2));
				env.SendEventBean(new SupportBean("G1", 3));
				env.AdvanceTime(10000);

				env.AssertEventNew(
					"s0",
					@event => {
						var outEvent = (FinalEventValid)@event.Underlying;
						Assert.AreEqual(1, outEvent.StartEvent.Id);
						Assert.AreEqual("G1", outEvent.StartEvent.P00);
						Assert.AreEqual(2, outEvent.EndEvent.Length);
						Assert.AreEqual(2, outEvent.EndEvent[0].IntPrimitive);
						Assert.AreEqual(3, outEvent.EndEvent[1].IntPrimitive);
					});

				// Test invalid case of non-array destination insert
				var invalidEpl =
					"INSERT INTO FinalEventInvalidNonArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
					"every s=SupportBean_S0 -> e=SupportBean(theString=s.p00) until timer:interval(10 sec)]";
				env.TryInvalidCompile(
					path,
					invalidEpl,
					"Invalid assignment of column 'endEvent' of type '" +
					typeof(SupportBean).FullName +
					"[]' to event property 'endEvent' typed as '" +
					typeof(SupportBean).FullName +
					"', column and parameter types mismatch [INSERT INTO FinalEventInvalidNonArray SELECT s as startEvent, e as endEvent FROM PATTERN [every s=SupportBean_S0 -> e=SupportBean(theString=s.p00) until timer:interval(10 sec)]]");

				// Test invalid case of array destination insert from non-array var
				var invalidEplTwo =
					"INSERT INTO FinalEventInvalidArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
					"every s=SupportBean_S0 -> e=SupportBean(theString=s.p00) until timer:interval(10 sec)]";
				env.TryInvalidCompile(
					path,
					invalidEplTwo,
					"Invalid assignment of column 'startEvent' of type '" +
					typeof(SupportBean_S0).FullName +
					"' to event property 'startEvent' typed as '" +
					typeof(SupportBean_S0).FullName +
					"[]', column and parameter types mismatch [INSERT INTO FinalEventInvalidArray SELECT s as startEvent, e as endEvent FROM PATTERN [every s=SupportBean_S0 -> e=SupportBean(theString=s.p00) until timer:interval(10 sec)]]");

				env.UndeployAll();
			}
		}

		private class EPLInsertIntoArrayMapInsert : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
					if (rep.IsAvroEvent()) {
						env.AssertThat(() => { TryAssertionArrayMapInsert(env, rep); });
					}
					else {
						TryAssertionArrayMapInsert(env, rep);
					}
				}
			}
		}

		private static void TryAssertionArrayMapInsert(
			RegressionEnvironment env,
			EventRepresentationChoice eventRepresentationEnum)
		{

			var path = new RegressionPath();
			var schema =
				eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedEventOne)) +
				" @public @buseventtype create schema EventOne(id string);\n" +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedEventTwo)) +
				" @public @buseventtype create schema EventTwo(id string, val int);\n" +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedFinalEventValid)) +
				" @public @buseventtype create schema FinalEventValid (startEvent EventOne, endEvent EventTwo[]);\n" +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided(
					typeof(MyLocalJsonProvidedFinalEventInvalidNonArray)) +
				" @public @buseventtype create schema FinalEventInvalidNonArray (startEvent EventOne, endEvent EventTwo);\n" +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided(
					typeof(MyLocalJsonProvidedFinalEventInvalidArray)) +
				" @public @buseventtype create schema FinalEventInvalidArray (startEvent EventOne, endEvent EventTwo);\n";
			env.CompileDeploy(schema, path);

			env.AdvanceTime(0);

			// Test valid case of array insert
			var validEpl =
				"@name('s0') INSERT INTO FinalEventValid SELECT s as startEvent, e as endEvent FROM PATTERN [" +
				"every s=EventOne -> e=EventTwo(id=s.id) until timer:interval(10 sec)]";
			env.CompileDeploy(validEpl, path).AddListener("s0");

			SendEventOne(env, eventRepresentationEnum, "G1");
			SendEventTwo(env, eventRepresentationEnum, "G1", 2);
			SendEventTwo(env, eventRepresentationEnum, "G1", 3);
			env.AdvanceTime(10000);

			env.AssertEventNew(
				"s0",
				@event => {
					EventBean startEventOne;
					EventBean endEventOne;
					EventBean endEventTwo;
					if (eventRepresentationEnum.IsObjectArrayEvent()) {
						var outArray = (object[])@event.Underlying;
						startEventOne = (EventBean)outArray[0];
						endEventOne = ((EventBean[])outArray[1])[0];
						endEventTwo = ((EventBean[])outArray[1])[1];
					}
					else if (eventRepresentationEnum.IsMapEvent()) {
						var outMap = @event.Underlying.AsStringDictionary();
						startEventOne = (EventBean)outMap.Get("startEvent");
						endEventOne = ((EventBean[])outMap.Get("endEvent"))[0];
						endEventTwo = ((EventBean[])outMap.Get("endEvent"))[1];
					}
					else if (eventRepresentationEnum.IsAvroEvent() ||
					         eventRepresentationEnum.IsJsonEvent() ||
					         eventRepresentationEnum.IsJsonProvidedClassEvent()) {
						startEventOne = (EventBean)@event.GetFragment("startEvent");
						var endEvents = (EventBean[])@event.GetFragment("endEvent");
						endEventOne = endEvents[0];
						endEventTwo = endEvents[1];
					}
					else {
						throw new IllegalStateException("Unrecognized enum " + eventRepresentationEnum);
					}

					Assert.AreEqual("G1", startEventOne.Get("id"));
					Assert.AreEqual(2, endEventOne.Get("val"));
					Assert.AreEqual(3, endEventTwo.Get("val"));
				});

			// Test invalid case of non-array destination insert
			var invalidEplOne =
				"INSERT INTO FinalEventInvalidNonArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
				"every s=EventOne -> e=EventTwo(id=s.id) until timer:interval(10 sec)]";
			env.AssertThat(
				() => {
					try {
						env.CompileWCheckedEx(invalidEplOne, path);
						Assert.Fail();
					}
					catch (EPCompileException ex) {
						string expected;
						if (eventRepresentationEnum.IsAvroEvent()) {
							expected =
								"Property 'endEvent' is incompatible, expecting an array of compatible schema 'EventTwo' but received schema 'EventTwo'";
						}
						else {
							expected =
								"Event type named 'FinalEventInvalidNonArray' has already been declared with differing column name or type information: Type by name 'FinalEventInvalidNonArray' in property 'endEvent' expected event type 'EventTwo' but receives event type array 'EventTwo'";
						}

						SupportMessageAssertUtil.AssertMessage(ex, expected);
					}
				});

			// Test invalid case of array destination insert from non-array var
			var invalidEplTwo =
				"INSERT INTO FinalEventInvalidArray SELECT s as startEvent, e as endEvent FROM PATTERN [" +
				"every s=EventOne -> e=EventTwo(id=s.id) until timer:interval(10 sec)]";
			env.AssertThat(
				() => {
					try {
						env.CompileWCheckedEx(invalidEplTwo, path);
						Assert.Fail();
					}
					catch (EPCompileException ex) {
						string expected;
						if (eventRepresentationEnum.IsAvroEvent()) {
							expected =
								"Property 'endEvent' is incompatible, expecting an array of compatible schema 'EventTwo' but received schema 'EventTwo'";
						}
						else {
							expected =
								"Event type named 'FinalEventInvalidArray' has already been declared with differing column name or type information: Type by name 'FinalEventInvalidArray' in property 'endEvent' expected event type 'EventTwo' but receives event type array 'EventTwo'";
						}

						SupportMessageAssertUtil.AssertMessage(ex, expected);
					}
				});

			env.UndeployAll();
		}

		private static void SendEventTwo(
			RegressionEnvironment env,
			EventRepresentationChoice
				eventRepresentationEnum, string id,
			int val)
		{
			if (eventRepresentationEnum.IsObjectArrayEvent()) {
				env.SendEventObjectArray(new object[] { id, val }, "EventTwo");
			}
			else if (eventRepresentationEnum.IsMapEvent()) {
				IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
				theEvent.Put("id", id);
				theEvent.Put("val", val);
				env.SendEventMap(theEvent, "EventTwo");
			}
			else if (eventRepresentationEnum.IsAvroEvent()) {
				var schema = SchemaBuilder.Record("name", RequiredString("id"), RequiredInt("val")).AsRecordSchema();
				var record = new GenericRecord(schema);
				record.Put("id", id);
				record.Put("val", val);
				env.SendEventAvro(record, "EventTwo");
			}
			else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
				var @object = new JObject();
				@object.Add("id", id);
				@object.Add("val", val);
				env.SendEventJson(@object.ToString(), "EventTwo");
			}
			else {
				Assert.Fail();
			}
		}

		private static void SendEventOne(
			RegressionEnvironment env,
			EventRepresentationChoice
				eventRepresentationEnum, string id)
		{
			if (eventRepresentationEnum.IsObjectArrayEvent()) {
				env.SendEventObjectArray(new object[] { id }, "EventOne");
			}
			else if (eventRepresentationEnum.IsMapEvent()) {
				IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
				theEvent.Put("id", id);
				env.SendEventMap(theEvent, "EventOne");
			}
			else if (eventRepresentationEnum.IsAvroEvent()) {
				var schema = SchemaBuilder.Record("name", RequiredString("id"));
				var record = new GenericRecord(schema);
				record.Put("id", id);
				env.SendEventAvro(record, "EventOne");
			}
			else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
				var @object = new JObject();
				@object.Add("id", id);
				env.SendEventJson(@object.ToString(), "EventOne");
			}
			else {
				Assert.Fail();
			}
		}

		public class FinalEventInvalidNonArray
		{
			private SupportBean_S0 startEvent;
			private SupportBean endEvent;

			public SupportBean_S0 StartEvent {
				get => startEvent;
				set => startEvent = value;
			}

			public SupportBean EndEvent {
				get => endEvent;
				set => endEvent = value;
			}
		}

		public class FinalEventInvalidArray
		{
			private SupportBean_S0[] startEvent;
			private SupportBean[] endEvent;

			public SupportBean_S0[] StartEvent {
				get => startEvent;
				set => startEvent = value;
			}

			public SupportBean[] EndEvent {
				get => endEvent;
				set => endEvent = value;
			}
		}

		public class FinalEventValid
		{
			private SupportBean_S0 startEvent;
			private SupportBean[] endEvent;

			public SupportBean_S0 StartEvent {
				get => startEvent;
				set => startEvent = value;
			}

			public SupportBean[] EndEvent {
				get => endEvent;
				set => endEvent = value;
			}
		}

		public class MyLocalTarget
		{
			public int value;

			public int Value {
				get => value;
				set => this.value = value;
			}
		}

		private static void SendReceiveTwo(
			RegressionEnvironment env,
			string theString,
			int? intBoxed)
		{
			var bean = new SupportBean(theString, -1);
			bean.IntBoxed = intBoxed;
			env.SendEventBean(bean);
			env.AssertEventNew(
				"s0",
				@event => {
					var theEvent = (SupportBeanCtorOne)@event.Underlying;
					Assert.AreEqual(theString, theEvent.TheString);
					Assert.AreEqual(null, theEvent.IntBoxed);
					Assert.AreEqual(intBoxed, (int?)theEvent.IntPrimitive);
				});
		}

		private static void SendReceive(
			RegressionEnvironment env,
			string theString,
			int intPrimitive,
			bool boolPrimitive,
			int? intBoxed)
		{
			var bean = new SupportBean(theString, intPrimitive);
			bean.BoolPrimitive = boolPrimitive;
			bean.IntBoxed = intBoxed;
			env.SendEventBean(bean);
			env.AssertEventNew(
				"s0",
				@event => {
					var theEvent = (SupportBeanCtorOne)@event.Underlying;
					Assert.AreEqual(theString, theEvent.TheString);
					Assert.AreEqual(intBoxed, theEvent.IntBoxed);
					Assert.AreEqual(boolPrimitive, theEvent.BoolPrimitive);
					Assert.AreEqual(intPrimitive, theEvent.IntPrimitive);
				});
		}

		private static void TryAssertionPopulateUnderlying(
			RegressionEnvironment env,
			string typeName)
		{
			env.CompileDeploy("@name('select') select * from " + typeName);

			var stmtTextOne = "@name('s0') insert into " +
			                  typeName +
			                  " select intPrimitive as intVal, theString as stringVal, doubleBoxed as doubleVal from SupportBean";
			env.CompileDeploy(stmtTextOne).AddListener("s0");

			env.AssertThat(() => Assert.AreSame(env.Statement("select").EventType, env.Statement("s0").EventType));

			var bean = new SupportBean();
			bean.IntPrimitive = 1000;
			bean.TheString = "E1";
			bean.DoubleBoxed = 1001d;
			env.SendEventBean(bean);

			env.AssertPropsNew("s0", "intVal,stringVal,doubleVal".SplitCsv(), new object[] { 1000, "E1", 1001d });
			env.UndeployAll();
		}

		[Serializable]
		public class MyLocalJsonProvidedEventOne
		{
			public string id;
		}

		[Serializable]
		public class MyLocalJsonProvidedEventTwo
		{
			public string id;
			public int val;
		}

		[Serializable]
		public class MyLocalJsonProvidedFinalEventValid
		{
			public MyLocalJsonProvidedEventOne startEvent;
			public MyLocalJsonProvidedEventTwo[] endEvent;
		}

		[Serializable]
		public class MyLocalJsonProvidedFinalEventInvalidNonArray
		{
			public MyLocalJsonProvidedEventOne startEvent;
			public MyLocalJsonProvidedEventTwo endEvent;
		}

		[Serializable]
		public class MyLocalJsonProvidedFinalEventInvalidArray
		{
			public MyLocalJsonProvidedEventOne startEvent;
			public MyLocalJsonProvidedEventTwo endEvent;
		}
	}
} // end of namespace
