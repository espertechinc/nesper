///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

using Avro.Util;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.compat.magic;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.events;
using com.espertech.esper.regressionlib.support.expreval;
using com.espertech.esper.regressionlib.support.schedule;
using com.espertech.esper.runtime.client;



using NUnit.Framework;

using SupportMarkerInterface = com.espertech.esper.regressionlib.support.bean.SupportMarkerInterface;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreCast {

	    public static ICollection<RegressionExecution> Executions() {
	        var executions = new List<RegressionExecution>();
	        executions.Add(new ExprCoreCastDates());
	        executions.Add(new ExprCoreCastSimple());
	        executions.Add(new ExprCoreCastSimpleMoreTypes());
	        executions.Add(new ExprCoreCastAsParse());
	        executions.Add(new ExprCoreCastDoubleAndNullOM());
	        executions.Add(new ExprCoreCastInterface());
	        executions.Add(new ExprCoreCastStringAndNullCompile());
	        executions.Add(new ExprCoreCastBoolean());
	        executions.Add(new ExprCoreCastWStaticType());
	        executions.Add(new ExprCoreCastWArray(false));
	        executions.Add(new ExprCoreCastWArray(true));
	        executions.Add(new ExprCoreCastGeneric());
	        executions.Add(new ExprCoreCastDecimalBigInt());
	        return executions;
	    }

	    private class ExprCoreCastDecimalBigInt : RegressionExecution
	    {
		    public void Run(RegressionEnvironment env)
		    {
			    var epl =
				    "@buseventtype @public create schema MyEvent(value System.Object);\n" +
			        "@name('s0') select" +
				    " cast(value, decimal) as c0," +
				    " cast(value, BigInteger) as c1 from MyEvent;\n";
			    env.CompileDeploy(epl).AddListener("s0");

			    SendAssert(env, 1, 1m, new BigInteger(1));
			    SendAssert(env, 2L, 2m, new BigInteger(2L));
			    SendAssert(env, 2.4d, 2.4m, new BigInteger(2.4d.AsInt64()));

			    var bdOne = 156.78m;
			    SendAssert(env, bdOne, bdOne, bdOne.AsBigInteger());

			    var biOne = BigInteger.Parse("200");
			    SendAssert(env, biOne, 200.0m, biOne);

			    var bdTwo = ((decimal)Math.Pow(2d, 500500)) + 0.1m;
			    SendAssert(env, bdTwo, bdTwo, bdTwo.AsBigInteger());

			    var biTwo = BigInteger.Pow(new BigInteger(2), 500500);
			    //SendAssert(env, biTwo, 2m, new BigDecimal(biTwo), biTwo);

			    SendAssert(env, null, null, null);

			    env.UndeployAll();
		    }

		    private void SendAssert(
			    RegressionEnvironment env,
			    object value,
			    decimal? c0Expected,
			    BigInteger? c1Expected)
		    {
			    env.SendEventMap(Collections.SingletonMap("value", value), "MyEvent");
			    env.AssertEventNew(
				    "s0",
				    @event => {
					    Assert.AreEqual(c0Expected, @event.Get("c0"));
					    Assert.AreEqual(c1Expected, @event.Get("c1"));
				    });
		    }
	    }

	    private class ExprCoreCastGeneric : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var schema = new StringBuilder();
	            schema.Append("@Public @buseventtype create schema MyEvent(");
	            var delimiter = "";
	            foreach (var pair in SupportGenericColUtil.NAMESANDTYPES) {
	                schema.Append(delimiter).Append(pair.Name).Append(" System.Object");
	                delimiter = ",";
	            }
	            schema.Append(");\n");

	            delimiter = "";
	            var cast = new StringBuilder();
	            cast.Append("@name('s0') select ");
	            foreach (var pair in SupportGenericColUtil.NAMESANDTYPES) {
		            cast
			            .Append(delimiter)
			            .Append("cast(")
			            .Append(pair.Name)
			            .Append(",")
			            .Append(pair.TypeName)
			            .Append(") as ")
			            .Append(pair.Name);
	                delimiter = ",";
	            }
	            cast.Append(" from MyEvent;\n");

	            var epl = schema.ToString() + cast.ToString();
	            env.CompileDeploy(epl).AddListener("s0");

	            env.AssertStatement("s0", statement => SupportGenericColUtil.AssertPropertyTypes(statement.EventType));

	            env.SendEventMap(SupportGenericColUtil.GetSampleEvent(), "MyEvent");
	            env.AssertEventNew("s0", @event =>SupportGenericColUtil.Compare(@event));

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.SERDEREQUIRED);
	        }
	    }

	    private class ExprCoreCastWArray : RegressionExecution {
	        private bool soda;

	        public ExprCoreCastWArray(bool soda) {
	            this.soda = soda;
	        }

	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                    "@Public @buseventtype " +
	                    "create schema MyEvent(arr_string System.Object, arr_primitive System.Object, " +
	                    "arr_boxed_one System.Object, arr_boxed_two System.Object, arr_object System.Object," +
	                    "arr_2dim_primitive System.Object, arr_2dim_object System.Object," +
	                    "arr_3dim_primitive System.Object, arr_3dim_object System.Object" +
	                    ");\n" +
	                    "@Public create schema MyArrayEvent as " + typeof(MyArrayEvent).FullName + ";\n";
	            env.CompileDeploy(epl, path);

	            var insert = "@name('s0') insert into MyArrayEvent select " +
	                         "cast(arr_string,string[]) as c0, " +
	                         "cast(arr_primitive,int[primitive]) as c1, " +
	                         "cast(arr_boxed_one,int[]) as c2, " +
				             "cast(arr_boxed_two, System.Int32[]) as c3, " +
				             "cast(arr_object, System.Object[]) as c4," +
	                         "cast(arr_2dim_primitive,int[primitive][]) as c5, " +
				             "cast(arr_2dim_object, System.Object[][]) as c6," +
	                         "cast(arr_3dim_primitive,int[primitive][][]) as c7, " +
				             "cast(arr_3dim_object, System.Object[][][]) as c8 " +
	                         "from MyEvent";
	            env.CompileDeploy(soda, insert, path).AddListener("s0");

	            env.AssertStatement("s0", stmt => {
	                var eventType = stmt.EventType;
	                Assert.AreEqual(typeof(string[]), eventType.GetPropertyType("c0"));
	                Assert.AreEqual(typeof(int[]), eventType.GetPropertyType("c1"));
	                Assert.AreEqual(typeof(int?[]), eventType.GetPropertyType("c2"));
	                Assert.AreEqual(typeof(int?[]), eventType.GetPropertyType("c3"));
	                Assert.AreEqual(typeof(object[]), eventType.GetPropertyType("c4"));
	                Assert.AreEqual(typeof(int[][]), eventType.GetPropertyType("c5"));
	                Assert.AreEqual(typeof(object[][]), eventType.GetPropertyType("c6"));
	                Assert.AreEqual(typeof(int[][][]), eventType.GetPropertyType("c7"));
	                Assert.AreEqual(typeof(object[][][]), eventType.GetPropertyType("c8"));
	            });

	            IDictionary<string, object> map = new Dictionary<string, object>();
	            map.Put("arr_string", new string[]{"a"});
	            map.Put("arr_primitive", new int[]{1});
	            map.Put("arr_boxed_one", new int?[]{2});
	            map.Put("arr_boxed_two", new int?[]{3});
	            map.Put("arr_object", new SupportBean[]{new SupportBean("E1", 0)});
	            map.Put("arr_2dim_primitive", new int[][]{ new int[] {10}});
	            map.Put("arr_2dim_object", new int?[][]{ new int?[] {11}});
	            map.Put("arr_3dim_primitive", new int[][][]{new int[][] { new int[] {12}}});
	            map.Put("arr_3dim_object", new int?[][][]{new int?[][]{new int?[]{13}}});

	            env.SendEventMap(map, "MyEvent");

	            env.AssertEventNew("s0", @event => {
	                var mae = (MyArrayEvent) @event.Underlying;
	                Assert.AreEqual("a", mae.C0[0]);
	                Assert.AreEqual(1, mae.C1[0]);
	                Assert.AreEqual(2, mae.C2[0].AsInt32());
	                Assert.AreEqual(3, mae.C3[0].AsInt32());
	                Assert.AreEqual(new SupportBean("E1", 0), mae.C4[0]);
	                Assert.AreEqual(10, mae.C5[0][0]);
	                Assert.AreEqual(11, mae.C6[0][0]);
	                Assert.AreEqual(12, mae.C7[0][0][0]);
	                Assert.AreEqual(13, mae.C8[0][0][0]);
	            });

				env.SendEventMap(EmptyDictionary<string, object>.Instance, "MyEvent");

	            env.UndeployAll();
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "soda=" + soda +
	                '}';
	        }
	    }

	    private class ExprCoreCastWStaticType : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmt = "@name('s0') select " +
	                       "cast(anInt, int) as intVal, " +
	                       "cast(anDouble, double) as doubleVal, " +
	                       "cast(anLong, long) as longVal, " +
	                       "cast(anFloat, float) as floatVal, " +
	                       "cast(anByte, byte) as byteVal, " +
	                       "cast(anShort, short) as shortVal, " +
				           "cast(intPrimitive, int) as IntOne, " +
	                       "cast(intBoxed, int) as intTwo, " +
				           "cast(intPrimitive, System.Int64) as longOne, " +
	                       "cast(intBoxed, long) as longTwo " +
	                       "from StaticTypeMapEvent";

	            env.CompileDeploy(stmt).AddListener("s0");

	            IDictionary<string, object> map = new Dictionary<string, object>();
	            map.Put("anInt", "100");
	            map.Put("anDouble", "1.4E-1");
	            map.Put("anLong", "-10");
	            map.Put("anFloat", "1.001");
	            map.Put("anByte", "0x0A");
	            map.Put("anShort", "223");
	            map.Put("intPrimitive", 10);
	            map.Put("intBoxed", 11);

	            env.SendEventMap(map, "StaticTypeMapEvent");
	            env.AssertEventNew("s0", row => {
	                Assert.AreEqual(100, row.Get("intVal"));
	                Assert.AreEqual(0.14d, row.Get("doubleVal"));
	                Assert.AreEqual(-10L, row.Get("longVal"));
	                Assert.AreEqual(1.001f, row.Get("floatVal"));
	                Assert.AreEqual((byte) 10, row.Get("byteVal"));
	                Assert.AreEqual((short) 223, row.Get("shortVal"));
					Assert.AreEqual(10, row.Get("IntOne"));
	                Assert.AreEqual(11, row.Get("intTwo"));
	                Assert.AreEqual(10L, row.Get("longOne"));
	                Assert.AreEqual(11L, row.Get("longTwo"));
	            });

	            env.UndeployAll();
	        }
	    }

	    private class ExprCoreCastSimpleMoreTypes : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8".SplitCsv();
	            var builder = new SupportEvalBuilder("SupportBean")
					.WithExpression(fields[0], "cast(IntPrimitive, float)")
					.WithExpression(fields[1], "cast(IntPrimitive, short)")
					.WithExpression(fields[2], "cast(IntPrimitive, byte)")
					.WithExpression(fields[3], "cast(TheString, char)")
					.WithExpression(fields[4], "cast(TheString, boolean)")
					.WithExpression(fields[5], "cast(IntPrimitive, BigInteger)")
					.WithExpression(fields[6], "cast(IntPrimitive, decimal)")
					.WithExpression(fields[7], "cast(DoublePrimitive, decimal)")
					.WithExpression(fields[8], "cast(TheString, char)");

	            builder.WithStatementConsumer(
		            stmt => AssertTypes(
			            stmt,
			            fields,
			            typeof(float?),
			            typeof(short?),
			            typeof(byte?),
			            typeof(char?),
			            typeof(bool?),
			            typeof(BigInteger?),
			            typeof(decimal?),
			            typeof(decimal?),
			            typeof(char?)
		            ));

	            var bean = new SupportBean("true", 1);
	            bean.DoublePrimitive = 1;
	            builder.WithAssertion(bean).Expect(fields, 1.0f, (short) 1, (byte) 1, 't', true, new BigInteger(1), 1.0m, 1m, 't');

	            builder.Run(env);
	            env.UndeployAll();
	        }
	    }

	    private class ExprCoreCastSimple : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "c0,c1,c2,c3,c4,c5,c6,c7".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean")
					.WithExpression(fields[0], "cast(TheString as string)")
					.WithExpression(fields[1], "cast(IntBoxed, int)")
					.WithExpression(fields[2], "cast(FloatBoxed, System.Single)")
					.WithExpression(fields[3], "cast(TheString, System.String)")
					.WithExpression(fields[4], "cast(IntPrimitive, System.Int32)")
					.WithExpression(fields[5], "cast(IntPrimitive, long)")
					.WithExpression(fields[6], "cast(IntPrimitive, System.Object)")
					.WithExpression(fields[7], "cast(FloatBoxed, long)");

	            builder.WithStatementConsumer(stmt => {
	                var type = stmt.EventType;
	                Assert.AreEqual(typeof(string), type.GetPropertyType("c0"));
	                Assert.AreEqual(typeof(int?), type.GetPropertyType("c1"));
	                Assert.AreEqual(typeof(float?), type.GetPropertyType("c2"));
	                Assert.AreEqual(typeof(string), type.GetPropertyType("c3"));
	                Assert.AreEqual(typeof(int?), type.GetPropertyType("c4"));
	                Assert.AreEqual(typeof(long?), type.GetPropertyType("c5"));
	                Assert.AreEqual(typeof(object), type.GetPropertyType("c6"));
	                Assert.AreEqual(typeof(long?), type.GetPropertyType("c7"));
	            });

	            var bean = new SupportBean("abc", 100);
	            bean.FloatBoxed = 9.5f;
	            bean.IntBoxed = 3;
	            builder.WithAssertion(bean).Expect(fields, "abc", 3, 9.5f, "abc", 100, 100L, 100, 9L);

	            bean = new SupportBean(null, 100);
	            bean.FloatBoxed = null;
	            bean.IntBoxed = null;
	            builder.WithAssertion(bean).Expect(fields, null, null, null, null, 100, 100L, 100, null);

	            builder.Run(env);
	            env.UndeployAll();

	            // test cast with chained and null
	            var epl = "@name('s0') select" +
	                      " cast(One as " + typeof(SupportBean).FullName + ").GetTheString() as t0," +
	                      " cast(null, " + typeof(SupportBean).FullName + ") as t1" +
	                      " from SupportBeanObject";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.SendEventBean(new SupportBeanObject(new SupportBean("E1", 1)));
	            env.AssertPropsNew("s0", "t0,t1".SplitCsv(), new object[]{"E1", null});
	            env.AssertStatement("s0", statement => Assert.AreEqual(typeof(SupportBean), statement.EventType.GetPropertyType("t1")));

	            env.UndeployAll();
	        }
	    }

	    private class ExprCoreCastDoubleAndNullOM : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "select cast(item?,double) as t0 from SupportBeanDynRoot";

	            var model = new EPStatementObjectModel();
	            model.SelectClause = SelectClause.Create().Add(Expressions.Cast("item?", "double"), "t0");
	            model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportBeanDynRoot)));
	            model = env.CopyMayFail(model);
	            Assert.AreEqual(epl, model.ToEPL());

	            model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
	            env.CompileDeploy(model).AddListener("s0");

	            env.AssertStmtType("s0", "t0", typeof(double?));

	            env.SendEventBean(new SupportBeanDynRoot(100));
	            env.AssertEqualsNew("s0", "t0", 100d);

	            env.SendEventBean(new SupportBeanDynRoot((byte) 2));
	            env.AssertEqualsNew("s0", "t0", 2d);

	            env.SendEventBean(new SupportBeanDynRoot(77.7777));
	            env.AssertEqualsNew("s0", "t0", 77.7777d);

	            env.SendEventBean(new SupportBeanDynRoot(6L));
	            env.AssertEqualsNew("s0", "t0", 6d);

	            env.SendEventBean(new SupportBeanDynRoot(null));
	            env.AssertEqualsNew("s0", "t0", null);

	            env.SendEventBean(new SupportBeanDynRoot("abc"));
	            env.AssertEqualsNew("s0", "t0", null);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCoreCastDates : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();

	            RunAssertionDatetimeBaseTypes(env, true, milestone);

	            RunAssertionDatetimeVariance(env, milestone);

	            RunAssertionDatetimeRenderOutCol(env, milestone);

	            RunAssertionDynamicDateFormat(env);

	            RunAssertionConstantDate(env, milestone);

	            RunAssertionISO8601Date(env, milestone);

	            RunAssertionDateformatNonString(env, milestone);

	            RunAssertionDatetimeInvalid(env);
	        }
	    }

	    private class ExprCoreCastAsParse : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select cast(theString, int) as t0 from SupportBean";
	            env.CompileDeploy(epl).AddListener("s0");
	            env.AssertStmtType("s0", "t0", typeof(int?));

	            env.SendEventBean(new SupportBean("12", 1));
	            env.AssertPropsNew("s0", "t0".SplitCsv(), new object[]{12});

	            env.UndeployAll();
	        }
	    }

	    private class ExprCoreCastInterface : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select" +
	                      " cast(item?, " + typeof(SupportMarkerInterface).FullName + ") as t0, " +
	                      " cast(item?, " + typeof(ISupportA).FullName + ") as t1, " +
	                      " cast(item?, " + typeof(ISupportBaseAB).FullName + ") as t2, " +
	                      " cast(item?, " + typeof(ISupportBaseABImpl).FullName + ") as t3, " +
	                      " cast(item?, " + typeof(ISupportC).FullName + ") as t4, " +
	                      " cast(item?, " + typeof(ISupportD).FullName + ") as t5, " +
	                      " cast(item?, " + typeof(ISupportAImplSuperG).FullName + ") as t6, " +
	                      " cast(item?, " + typeof(ISupportAImplSuperGImplPlus).FullName + ") as t7 " +
	                      " from SupportBeanDynRoot";

	            env.CompileDeploy(epl).AddListener("s0");

	            env.AssertStatement("s0", statement => {
	                var type = statement.EventType;
	                Assert.AreEqual(typeof(SupportMarkerInterface), type.GetPropertyType("t0"));
	                Assert.AreEqual(typeof(ISupportA), type.GetPropertyType("t1"));
	                Assert.AreEqual(typeof(ISupportBaseAB), type.GetPropertyType("t2"));
	                Assert.AreEqual(typeof(ISupportBaseABImpl), type.GetPropertyType("t3"));
	                Assert.AreEqual(typeof(ISupportC), type.GetPropertyType("t4"));
	                Assert.AreEqual(typeof(ISupportD), type.GetPropertyType("t5"));
	                Assert.AreEqual(typeof(ISupportAImplSuperG), type.GetPropertyType("t6"));
	                Assert.AreEqual(typeof(ISupportAImplSuperGImplPlus), type.GetPropertyType("t7"));
	            });

	            object beanOne = new SupportBeanDynRoot("abc");
	            env.SendEventBean(new SupportBeanDynRoot(beanOne));
	            env.AssertEventNew("s0", theEvent => AssertResults(theEvent, new object[]{beanOne, null, null, null, null, null, null, null}));

	            object beanTwo = new ISupportDImpl("", "", "");
	            env.SendEventBean(new SupportBeanDynRoot(beanTwo));
	            env.AssertEventNew("s0", theEvent => AssertResults(theEvent, new object[]{null, null, null, null, null, beanTwo, null, null}));

	            object beanThree = new ISupportBCImpl("", "", "");
	            env.SendEventBean(new SupportBeanDynRoot(beanThree));
	            env.AssertEventNew("s0", theEvent => AssertResults(theEvent, new object[]{null, null, beanThree, null, beanThree, null, null, null}));

	            object beanFour = new ISupportAImplSuperGImplPlus();
	            env.SendEventBean(new SupportBeanDynRoot(beanFour));
	            env.AssertEventNew("s0", theEvent => AssertResults(theEvent, new object[]{null, beanFour, beanFour, null, beanFour, null, beanFour, beanFour}));

	            object beanFive = new ISupportBaseABImpl("");
	            env.SendEventBean(new SupportBeanDynRoot(beanFive));
	            env.AssertEventNew("s0", theEvent => AssertResults(theEvent, new object[]{null, null, beanFive, beanFive, null, null, null, null}));

	            env.UndeployAll();
	        }
	    }

	    private class ExprCoreCastStringAndNullCompile : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select cast(item?,System.String) as t0 from SupportBeanDynRoot";

	            env.EplToModelCompileDeploy(epl).AddListener("s0");

	            env.AssertStmtType("s0", "t0", typeof(string));

	            env.SendEventBean(new SupportBeanDynRoot(100));
	            env.AssertEqualsNew("s0", "t0", "100");

	            env.SendEventBean(new SupportBeanDynRoot((byte) 2));
	            env.AssertEqualsNew("s0", "t0", "2");

	            env.SendEventBean(new SupportBeanDynRoot(77.7777));
	            env.AssertEqualsNew("s0", "t0", "77.7777");

	            env.SendEventBean(new SupportBeanDynRoot(6L));
	            env.AssertEqualsNew("s0", "t0", "6");

	            env.SendEventBean(new SupportBeanDynRoot(null));
	            env.AssertEqualsNew("s0", "t0", null);

	            env.SendEventBean(new SupportBeanDynRoot("abc"));
	            env.AssertEqualsNew("s0", "t0", "abc");

	            env.UndeployAll();
	        }
	    }

	    private class ExprCoreCastBoolean : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select cast(boolPrimitive as java.lang.Boolean) as t0, " +
	                      " cast(boolBoxed | boolPrimitive, boolean) as t1, " +
	                      " cast(boolBoxed, string) as t2 " +
	                      " from SupportBean";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.AssertStatement("s0", statement => {
	                var type = statement.EventType;
	                Assert.AreEqual(typeof(bool?), type.GetPropertyType("t0"));
	                Assert.AreEqual(typeof(bool?), type.GetPropertyType("t1"));
	                Assert.AreEqual(typeof(string), type.GetPropertyType("t2"));
	            });

	            var bean = new SupportBean("abc", 100);
	            bean.BoolPrimitive = true;
	            bean.BoolBoxed = true;
	            env.SendEventBean(bean);
	            env.AssertEventNew("s0", theEvent => AssertResults(theEvent, new object[]{true, true, "true"}));

	            bean = new SupportBean(null, 100);
	            bean.BoolPrimitive = false;
	            bean.BoolBoxed = false;
	            env.SendEventBean(bean);
	            env.AssertEventNew("s0", theEvent => AssertResults(theEvent, new object[]{false, false, "false"}));

	            bean = new SupportBean(null, 100);
	            bean.BoolPrimitive = true;
	            bean.BoolBoxed = null;
	            env.SendEventBean(bean);
	            env.AssertEventNew("s0", theEvent => AssertResults(theEvent, new object[]{true, null, null}));

	            env.UndeployAll();
	        }
	    }

	    private static void RunAssertionDatetimeBaseTypes(RegressionEnvironment env, bool soda, AtomicLong milestone) {
			var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8".SplitCsv();
			var builder = new SupportEvalBuilder("MyDateType")
				.WithExpression(fields[0], "cast(yyyymmdd,System.DateTimeOffset,dateformat:\"yyyyMMdd\")")
				.WithExpression(fields[1], "cast(yyyymmdd,System.DateTime,dateformat:\"yyyyMMdd\")")
	            .WithExpression(fields[2], "cast(yyyymmdd,long,dateformat:\"yyyyMMdd\")")
				.WithExpression(fields[3], "cast(yyyymmdd,System.Int64,dateformat:\"yyyyMMdd\")")
				.WithExpression(fields[4], "cast(yyyymmdd,dateTimeEx,dateformat:\"yyyyMMdd\")")
				.WithExpression(fields[5], "cast(yyyymmdd,dtx,dateformat:\"yyyyMMdd\")")
				.WithExpression(fields[6], "cast(yyyymmdd,datetime,dateformat:\"yyyyMMdd\").get(\"month\")")
				.WithExpression(fields[7], "cast(yyyymmdd,dtx,dateformat:\"yyyyMMdd\").get(\"month\")")
	            .WithExpression(fields[8], "cast(yyyymmdd,long,dateformat:\"yyyyMMdd\").get(\"month\")");

	        var formatYYYYMMdd = new SimpleDateFormat("yyyyMMdd");
			var dateYYMMddDate = formatYYYYMMdd.Parse("20100510");
			var dtxYYMMddDate = DateTimeEx.GetInstance(TimeZoneInfo.Utc, dateYYMMddDate);

	        IDictionary<string, object> values = new Dictionary<string, object>();
	        values.Put("yyyymmdd", "20100510");
			builder.WithAssertion(values)
				.Expect(
					fields,
					dateYYMMddDate.DateTime, // c0
					dateYYMMddDate.DateTime.DateTime, // c1
					dateYYMMddDate.UtcMillis, // c2
					dateYYMMddDate.UtcMillis, // c3
					dtxYYMMddDate, // c4
					dtxYYMMddDate, // c5
					5,  // c6
					5,  // c7
					5); // c8

	        builder.Run(env);
	        env.UndeployAll();
	    }

		private static void RunAssertionDatetimeVariance(
			RegressionEnvironment env,
			AtomicLong milestone)
		{
			var fields = "c0,c1,c2,c3,c4,c5,c6,c7".SplitCsv();
			var builder = new SupportEvalBuilder("MyDateType")
				.WithExpression(fields[0], "cast(yyyymmdd,datetimeoffset,dateformat:\"yyyyMMdd\")")
				.WithExpression(fields[1], "cast(yyyymmdd,System.DateTimeOffset,dateformat:\"yyyyMMdd\")")
				.WithExpression(fields[2], "cast(yyyymmddhhmmss,datetimeoffset,dateformat:\"yyyyMMddHHmmss\")")
				.WithExpression(fields[3], "cast(yyyymmddhhmmss,System.DateTimeOffset,dateformat:\"yyyyMMddHHmmss\")")
				.WithExpression(fields[4], "cast(hhmmss,datetimeoffset,dateformat:\"HHmmss\")")
				.WithExpression(fields[5], "cast(hhmmss,System.DateTimeOffset,dateformat:\"HHmmss\")")
				.WithExpression(fields[6], "cast(yyyymmddhhmmsszz,datetimeoffset,dateformat:\"yyyyMMddHHmmsszzz\")")
				.WithExpression(fields[7], "cast(yyyymmddhhmmsszz,System.DateTimeOffset,dateformat:\"yyyyMMddHHmmsszzz\")");

	        var yyyymmdd = "20100510";
	        var yyyymmddhhmmss = "20100510141516";
			var yyyymmddhhmmsszz = "20100510141516-00:00";
	        var hhmmss = "141516";
			
			// Send an event with all of the "values" - keep in mind, these are just strings so
			// the intent is to have the cast properly parse the values using the given format
			// and then return the correct data type as specified
			
			var values = new Dictionary<string, object>();
	        values.Put("yyyymmdd", yyyymmdd);
	        values.Put("yyyymmddhhmmss", yyyymmddhhmmss);
	        values.Put("hhmmss", hhmmss);
			values.Put("yyyymmddhhmmsszz", yyyymmddhhmmsszz);

			var yyyymmddDtx = new SimpleDateFormat("yyyyMMdd").Parse(yyyymmdd);
			var yyyymmddhhmmssDtx = new SimpleDateFormat("yyyyMMddHHmmss").Parse(yyyymmddhhmmss);
			var mmhhssDtx = new SimpleDateFormat("HHmmss").Parse(hhmmss);
			var yyyymmddhhmmsszzDtx = new SimpleDateFormat("yyyyMMddHHmmsszzz").Parse(yyyymmddhhmmsszz);
	        
			builder.WithAssertion(values)
				.Expect(
					fields,
					yyyymmddDtx.DateTime, // c0
					yyyymmddDtx.DateTime, // c1
					yyyymmddhhmmssDtx.DateTime, // c2
					yyyymmddhhmmssDtx.DateTime, // c3
					mmhhssDtx.DateTime, // c4
					mmhhssDtx.DateTime, // c5
					yyyymmddhhmmsszzDtx.DateTime, // c6
					yyyymmddhhmmsszzDtx.DateTime // c7
				);

	        builder.Run(env);
	        env.UndeployAll();
	    }

	    private static void RunAssertionDynamicDateFormat(RegressionEnvironment env) {

			var fields = "c0,c1,c2,c3".SplitCsv();
			var builder = new SupportEvalBuilder("SupportBean_StringAlphabetic")
				.WithExpression(fields[0], "cast(A,long,dateformat:B)")
				.WithExpression(fields[1], "cast(A,datetime,dateformat:B)")
				.WithExpression(fields[2], "cast(A,datetimeoffset,dateformat:B)")
				.WithExpression(fields[3], "cast(A,dtx,dateformat:B)");

	        AssertDynamicDateFormat(builder, fields, "20100502", "yyyyMMdd");
	        AssertDynamicDateFormat(builder, fields, "20100502101112", "yyyyMMddhhmmss");
	        AssertDynamicDateFormat(builder, fields, null, "yyyyMMdd");

	        builder.Run(env);

	        // invalid date
	        try {
	            env.SendEventBean(new SupportBean_StringAlphabetic("x", "yyyyMMddhhmmss"));
	        } catch (EPException ex) {
	            SupportMessageAssertUtil.AssertMessageContains(ex, "Exception parsing date 'x' format 'yyyyMMddhhmmss': Unparseable date: \"x\"");
	        }

	        // invalid format
	        try {
	            env.SendEventBean(new SupportBean_StringAlphabetic("20100502", "UUHHYY"));
	        } catch (EPException ex) {
	            SupportMessageAssertUtil.AssertMessageContains(ex, "Illegal pattern character 'U'");
	        }

	        env.UndeployAll();
	    }

	    private static void RunAssertionDatetimeRenderOutCol(
		    RegressionEnvironment env,
		    AtomicLong milestone)
	    {
		    var epl = "@name('s0') select cast(yyyymmdd,datetime,dateformat:\"yyyyMMdd\") from MyDateType";
		    env.CompileDeploy(epl).AddListener("s0").Milestone(milestone.GetAndIncrement());
		    env.AssertStatement("s0", statement => Assert.AreEqual("cast(yyyymmdd,datetime,dateformat:\"yyyyMMdd\")", statement.EventType.PropertyNames[0]));
		    env.UndeployAll();
	    }

	    private static void AssertDynamicDateFormat(SupportEvalBuilder builder, string[] fields, string date, string format) {

	        var dateFormat = new SimpleDateFormat(format);
			var expectedDateTimeEx = date == null ? null : dateFormat.Parse(date);
			var expectedDateTimeOffset = expectedDateTimeEx?.DateTime;
			var expectedDateTime = expectedDateTimeOffset?.DateTime;
			var expectedLong = expectedDateTimeEx?.UtcMillis;

	        builder
		        .WithAssertion(new SupportBean_StringAlphabetic(date, format))
				.Expect(fields, expectedLong, expectedDateTime, expectedDateTimeOffset, expectedDateTimeEx);
	    }

	    private static void RunAssertionConstantDate(RegressionEnvironment env, AtomicLong milestone) {
	        var fields = "c0".SplitCsv();
			var builder = new SupportEvalBuilder("SupportBean")
				.WithExpressions(fields, "cast('20030201',dtx,dateformat:\"yyyyMMdd\")");

	        var dateFormat = new SimpleDateFormat("yyyyMMdd");
			var expectedDate = dateFormat.Parse("20030201");

	        builder.WithAssertion(new SupportBean("E1", 1)).Expect(fields, expectedDate);

	        builder.Run(env);
	        env.UndeployAll();
	    }

	    private static void RunAssertionISO8601Date(RegressionEnvironment env, AtomicLong milestone) {
	        var epl = "@name('s0') select " +
			          "cast('1997-07-16T19:20:30Z',dtx,dateformat:'iso') as c0," +
			          "cast('1997-07-16T19:20:30+01:00',dtx,dateformat:'iso') as c1," +
			          "cast('1997-07-16T19:20:30',dtx,dateformat:'iso') as c2," +
			          "cast('1997-07-16T19:20:30.45Z',dtx,dateformat:'iso') as c3," +
			          "cast('1997-07-16T19:20:30.45+01:00',dtx,dateformat:'iso') as c4," +
			          "cast('1997-07-16T19:20:30.45',dtx,dateformat:'iso') as c5," +
	                  "cast('1997-07-16T19:20:30.45',long,dateformat:'iso') as c6," +
			          "cast('1997-07-16T19:20:30.45',datetime,dateformat:'iso') as c7," +
			          "cast(TheString,dtx,dateformat:'iso') as c8," +
			          "cast(TheString,long,dateformat:'iso') as c9," +
			          "cast(TheString,datetime,dateformat:'iso') as c10," +
			          "cast('1997-07-16T19:20:30.45',datetimeoffset,dateformat:'iso') as c11," +
			          "cast('1997-07-16T19:20:30+01:00',datetime,dateformat:'iso') as c12," +
			          "cast('1997-07-16',datetimeoffset,dateformat:'iso') as c13," +
			          "cast('19:20:30',datetimeoffset,dateformat:'iso') as c14" +
	                  " from SupportBean";
	        env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

	        env.SendEventBean(new SupportBean());
	        env.AssertEventNew("s0", @event => {
		        SupportDateTimeUtil.CompareDate((DateTimeEx) @event.Get("c0"), 1997, 7, 16, 19, 20, 30, 0, "UTC");
		        SupportDateTimeUtil.CompareDate((DateTimeEx) @event.Get("c1"), 1997, 7, 16, 19, 20, 30, 0, "GMT+01:00");
		        SupportDateTimeUtil.CompareDate((DateTimeEx) @event.Get("c2"), 1997, 7, 16, 19, 20, 30, 0, "UTC");
		        SupportDateTimeUtil.CompareDate((DateTimeEx) @event.Get("c3"), 1997, 7, 16, 19, 20, 30, 450, "UTC");
		        SupportDateTimeUtil.CompareDate((DateTimeEx) @event.Get("c4"), 1997, 7, 16, 19, 20, 30, 450, "GMT+01:00");
		        SupportDateTimeUtil.CompareDate((DateTimeEx) @event.Get("c5"), 1997, 7, 16, 19, 20, 30, 450, "UTC");
		        
		        Assert.That(@event.Get("c6"), Is.InstanceOf<long>());
		        Assert.That(@event.Get("c7"), Is.InstanceOf<DateTime>());

		        foreach (var prop in "c8,c9,c10".SplitCsv()) {
	                Assert.IsNull(@event.Get(prop));
	            }
		        
		        var isoDateTimeFormat = DateTimeFormat.ISO_DATE_TIME;

		        var expectedC11 = isoDateTimeFormat.Parse("1997-07-16T19:20:30.45").DateTime;
		        var expectedC12 = isoDateTimeFormat.Parse("1997-07-16T19:20:30+01:00").DateTime.DateTime;
		        var expectedC13 = DateTimeParsingFunctions.ParseDefault("1997-07-16");
		        var expectedC14 = DateTimeParsingFunctions.ParseDefault("19:20:30");

		        Assert.That(@event.Get("c11"), Is.EqualTo(expectedC11));
		        Assert.That(@event.Get("c12"), Is.EqualTo(expectedC12));
		        Assert.That(@event.Get("c13"), Is.EqualTo(expectedC13));
		        Assert.That(@event.Get("c14"), Is.EqualTo(expectedC14));
	        });

	        env.UndeployAll();
	    }

	    private static void RunAssertionDateformatNonString(RegressionEnvironment env, AtomicLong milestone) {
	        var sdt = SupportDateTime.Make("2002-05-30T09:00:00.000");
			var sdfDate = sdt.DateTimeEx.DateTime.ToString("s");
			var sdf = typeof(SimpleDateFormat).FullName;

	        var epl = "@name('s0') select " +
			          $"cast('{sdfDate}',dtx,dateformat:{sdf}.GetInstance()) as c0," +
			          $"cast('{sdfDate}',datetimeoffset,dateformat:{sdf}.GetInstance()) as c1," +
			          $"cast('{sdfDate}',datetime,dateformat:{sdf}.GetInstance()) as c2," +
			          $"cast('{sdfDate}',long,dateformat:{sdf}.GetInstance()) as c3" +
	                  " from SupportBean";
	        env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

	        env.SendEventBean(new SupportBean());
	        env.AssertPropsNew(
		        "s0",
		        "c0,c1,c2,c3".SplitCsv(),
		        new object[] {
			        sdt.DateTimeEx,
			        sdt.DateTimeEx.DateTime,
			        sdt.DateTimeEx.DateTime.DateTime,
			        sdt.DateTimeEx.UtcMillis
		        });

	        env.UndeployAll();
	    }

	    private static void RunAssertionDatetimeInvalid(RegressionEnvironment env) {
	        // not a valid named parameter
			env.TryInvalidCompile(
				"select cast(TheString, datetime, x:1) from SupportBean",
				"Failed to validate select-clause expression 'cast(TheString,datetime,x:1)': Unexpected named parameter 'x', expecting any of the following: [dateformat]");

#if INVALID // we do not validate date format patterns
	        // invalid date format
			env.TryInvalidCompile(
				"select cast(TheString, datetime, dateformat:'BBBBMMDD') from SupportBean",
				"Failed to validate select-clause expression 'cast(TheString,datetime,dateformat:\"BBB...(42 chars)': Invalid date format 'BBBBMMDD' (as obtained from new SimpleDateFormat): Illegal pattern character 'B'");
#endif
			
		    env.TryInvalidCompile(
				"select cast(TheString, datetime, dateformat:1) from SupportBean",
				"Failed to validate select-clause expression 'cast(TheString,datetime,dateformat:1)': Failed to validate named parameter 'dateformat', expected a single expression returning any of the following types: string,DateFormat,DateTimeFormat");

	        // invalid input
	        env.TryInvalidCompile(
				"select cast(IntPrimitive, datetime, dateformat:'yyyyMMdd') from SupportBean",
				"Failed to validate select-clause expression 'cast(IntPrimitive,datetime,dateform...(49 chars)': Use of the 'dateformat' named parameter requires a string-type input");

	        env.TryInvalidCompile(
				"select cast(TheString, int, dateformat:'yyyyMMdd') from SupportBean",
				"Failed to validate select-clause expression 'cast(TheString,int,dateformat:\"yyyy...(41 chars)': Use of the 'dateformat' named parameter requires a target type of long, DateTime, DateTimeOffset or DateEx");

#if INVALID // completely valid, DateTimeFormat implements DateFormat
	        // invalid parser
            env.TryInvalidCompile(
                $"select cast('xx', datetime, dateformat:{typeof(DateTimeFormat).FullName}.For(\"yyyyMMddHHmmssVV\")) from SupportBean",
                $"Failed to validate select-clause expression 'cast(\"xx\",datetime,dateformat:com.espertech...(91 chars)': Invalid format, expected string-format or DateFormat but received {typeof(DateTimeFormat).FullName}");
            
            env.TryInvalidCompile(
                $"select cast('xx', datetimeoffset, dateformat:SimpleDateFormat.GetInstance()) from SupportBean",
                $"Failed to validate select-clause expression 'cast(\"xx\",datetimeoffset,dateformat:...(66 chars)': Invalid format, expected string-format or DateTimeFormatter but received java.text.DateFormat");
#endif
	    }

	    private static void AssertResults(EventBean theEvent, object[] result) {
	        for (var i = 0; i < result.Length; i++) {
	            Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
	        }
	    }

	    private static void AssertTypes(EPStatement stmt, string[] fields, params Type[] types) {
	        for (var i = 0; i < fields.Length; i++) {
	            Assert.AreEqual(types[i], stmt.EventType.GetPropertyType(fields[i]), "failed for " + i);
	        }
	    }

		public class MyArrayEvent
		{
			public MyArrayEvent(
				string[] c0,
				int[] c1,
				int?[] c2,
				int?[] c3,
				object[] c4,
				int[][] c5,
				object[][] c6,
				int[][][] c7,
				object[][][] c8)
			{
				C0 = c0;
				C1 = c1;
				C2 = c2;
				C3 = c3;
				C4 = c4;
				C5 = c5;
				C6 = c6;
				C7 = c7;
				C8 = c8;
	        }

			[PropertyName("c0")]
			public string[] C0 { get; }

			[PropertyName("c1")]
			public int[] C1 { get; }

			[PropertyName("c2")]
			public int?[] C2 { get; }

			[PropertyName("c3")]
			public int?[] C3 { get; }

			[PropertyName("c4")]
			public object[] C4 { get; }

			[PropertyName("c5")]
			public int[][] C5 { get; }

			[PropertyName("c6")]
			public object[][] C6 { get; }

			[PropertyName("c7")]
			public int[][][] C7 { get; }

			[PropertyName("c8")]
			public object[][][] C8 { get; }
	    }
	}
} // end of namespace
