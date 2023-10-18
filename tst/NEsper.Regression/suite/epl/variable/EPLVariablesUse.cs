///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.variable;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.threading;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.runtime.@internal.kernel.service;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.variable {
	public class EPLVariablesUse {

		public static IList<RegressionExecution> Executions () {
			IList<RegressionExecution> execs = new List<RegressionExecution> ();
			execs.Add (new EPLVariableUseSimplePreconfigured ());
			execs.Add (new EPLVariableUseSimpleSameModule ());
			execs.Add (new EPLVariableUseSimpleTwoModules ());
			execs.Add (new EPLVariableUseEPRuntime ());
			execs.Add (new EPLVariableUseDotSeparateThread ());
			execs.Add (new EPLVariableUseInvokeMethod ());
			execs.Add (new EPLVariableUseConstantVariable ());
			execs.Add (new EPLVariableUseVariableInFilterBoolean ());
			execs.Add (new EPLVariableUseVariableInFilter ());
			execs.Add (new EPLVariableUseFilterConstantCustomTypePreconfigured ());
			execs.Add (new EPLVariableUseWVarargs ());
			return execs;
		}

		private class EPLVariableUseWVarargs : RegressionExecution {
			public void Run (RegressionEnvironment env) {
				var epl = "@name('s0') select * from SupportBean(varargsTestClient.functionWithVarargs(longBoxed, varargsTestClient.getTestObject(theString))) as t";
				env.CompileDeploy (epl).AddListener ("s0");

				var sb = new SupportBean ("5", 0);
				sb.LongBoxed = 5L;
				env.SendEventBean (sb);
				env.AssertEventNew ("s0", @event => { });

				env.UndeployAll ();
			}
		}

		private class EPLVariableUseFilterConstantCustomTypePreconfigured : RegressionExecution {
			public void Run (RegressionEnvironment env) {
				env.CompileDeploy ("@name('s0') select * from MyVariableCustomEvent(name=my_variable_custom_typed)").AddListener ("s0");

				env.SendEventBean (new MyVariableCustomEvent (MyVariableCustomType.Of ("abc")));
				env.AssertListenerInvoked ("s0");

				env.UndeployAll ();
			}
		}

		private class EPLVariableUseSimplePreconfigured : RegressionExecution {
			public void Run (RegressionEnvironment env) {
				env.CompileDeploy ("@name('s0') select var_simple_preconfig_const as c0 from SupportBean").AddListener ("s0");

				env.Milestone (0);

				env.SendEventBean (new SupportBean ("E1", 0));
				env.AssertEqualsNew ("s0", "c0", true);

				env.Milestone (1);

				env.UndeployAll ();
			}
		}

		private class EPLVariableUseSimpleSameModule : RegressionExecution {
			public void Run (RegressionEnvironment env) {
				var epl = "create variable boolean var_simple_module_const = true;\n" +
				          "@name('s0') select var_simple_module_const as c0 from SupportBean;\n";
				env.CompileDeploy (epl).AddListener ("s0");
				env.Milestone (0);
				env.SendEventBean (new SupportBean ("E1", 0));
				env.AssertEqualsNew ("s0", "c0", true);
				env.UndeployAll ();
			}
		}

		private class EPLVariableUseSimpleTwoModules : RegressionExecution {
			public void Run (RegressionEnvironment env) {
				var path = new RegressionPath ();
				env.CompileDeploy ("@public create variable boolean var_simple_twomodule_const = true", path);
				env.CompileDeploy ("@name('s0') select var_simple_twomodule_const as c0 from SupportBean", path);
				env.AddListener ("s0");
				env.Milestone (0);
				env.SendEventBean (new SupportBean ("E1", 0));
				env.AssertEqualsNew ("s0", "c0", true);
				env.UndeployAll ();
			}
		}

		private class EPLVariableUseDotSeparateThread : RegressionExecution {
			public void Run (RegressionEnvironment env) {

				env.RuntimeSetVariable (null, "mySimpleVariableService", new EPLVariablesUse.MySimpleVariableService ());

				var epStatement = env.CompileDeploy ("@name('s0') select mySimpleVariableService.doSomething() as c0 from SupportBean").Statement ("s0");

				var latch = new CountDownLatch (1);
				IList<string> values = new List<string> ();
				epStatement.Subscriber = new Action<EventBean>(
					(@event) => {
						var value = (string)@event.Get("c0");
						values.Add(value);
						latch.CountDown();
					});

				var executorService = Executors.NewSingleThreadExecutor ();
				executorService.Submit(() => env.SendEventBean(new SupportBean()));
				
				try {
					latch.Await ();
				} catch (ThreadInterruptedException e) {
					Assert.Fail ();
				}
				executorService.Shutdown ();

				Assert.AreEqual (1, values.Count);
				Assert.AreEqual ("hello", values[0]);

				env.UndeployAll ();
			}

			public ISet<RegressionFlag> Flags () {
				return Collections.Set (RegressionFlag.RUNTIMEOPS);
			}
		}

		private class EPLVariableUseInvokeMethod : RegressionExecution {
			public void Run (RegressionEnvironment env) {
				// declared via EPL
				var path = new RegressionPath ();
				env.CompileDeploy ("@public create constant variable MySimpleVariableService myService = MySimpleVariableServiceFactory.makeService()", path);

				// exercise
				var epl = "@name('s0') select " +
				          "myService.doSomething() as c0, " +
				          "myInitService.doSomething() as c1 " +
				          "from SupportBean";
				env.CompileDeploy (epl, path).AddListener ("s0");

				env.SendEventBean (new SupportBean ("E1", 1));
				env.AssertPropsNew ("s0", "c0,c1".Split (","), new object[] { "hello", "hello" });

				env.UndeployAll ();
			}
		}

		private class EPLVariableUseConstantVariable : RegressionExecution {
			public void Run (RegressionEnvironment env) {
				var path = new RegressionPath ();

				env.CompileDeploy ("@public create const variable int MYCONST = 10", path);
				TryOperator (env, path, "MYCONST = intBoxed", new object[][] { new object[] { 10, true }, new object[] { 9, false }, new object[] { null, false } });

				TryOperator (env, path, "MYCONST > intBoxed", new object[][] { new object[] { 11, false }, new object[] { 10, false }, new object[] { 9, true }, new object[] { 8, true } });
				TryOperator (env, path, "MYCONST >= intBoxed", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, true }, new object[] { 8, true } });
				TryOperator (env, path, "MYCONST < intBoxed", new object[][] { new object[] { 11, true }, new object[] { 10, false }, new object[] { 9, false }, new object[] { 8, false } });
				TryOperator (env, path, "MYCONST <= intBoxed", new object[][] { new object[] { 11, true }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, false } });

				TryOperator (env, path, "intBoxed < MYCONST", new object[][] { new object[] { 11, false }, new object[] { 10, false }, new object[] { 9, true }, new object[] { 8, true } });
				TryOperator (env, path, "intBoxed <= MYCONST", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, true }, new object[] { 8, true } });
				TryOperator (env, path, "intBoxed > MYCONST", new object[][] { new object[] { 11, true }, new object[] { 10, false }, new object[] { 9, false }, new object[] { 8, false } });
				TryOperator (env, path, "intBoxed >= MYCONST", new object[][] { new object[] { 11, true }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, false } });

				TryOperator (env, path, "intBoxed in (MYCONST)", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, false } });
				TryOperator (env, path, "intBoxed between MYCONST and MYCONST", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, false } });

				TryOperator (env, path, "MYCONST != intBoxed", new object[][] { new object[] { 10, false }, new object[] { 9, true }, new object[] { null, false } });
				TryOperator (env, path, "intBoxed != MYCONST", new object[][] { new object[] { 10, false }, new object[] { 9, true }, new object[] { null, false } });

				TryOperator (env, path, "intBoxed not in (MYCONST)", new object[][] { new object[] { 11, true }, new object[] { 10, false }, new object[] { 9, true }, new object[] { 8, true } });
				TryOperator (env, path, "intBoxed not between MYCONST and MYCONST", new object[][] { new object[] { 11, true }, new object[] { 10, false }, new object[] { 9, true }, new object[] { 8, true } });

				TryOperator (env, path, "MYCONST is intBoxed", new object[][] { new object[] { 10, true }, new object[] { 9, false }, new object[] { null, false } });
				TryOperator (env, path, "intBoxed is MYCONST", new object[][] { new object[] { 10, true }, new object[] { 9, false }, new object[] { null, false } });

				TryOperator (env, path, "MYCONST is not intBoxed", new object[][] { new object[] { 10, false }, new object[] { 9, true }, new object[] { null, true } });
				TryOperator (env, path, "intBoxed is not MYCONST", new object[][] { new object[] { 10, false }, new object[] { 9, true }, new object[] { null, true } });

				// try coercion
				TryOperator (env, path, "MYCONST = shortBoxed", new object[][] { new object[] {
						(short) 10, true }, new object[] {
						(short) 9, false }, new object[] { null, false } });
				TryOperator (env, path, "shortBoxed = MYCONST", new object[][] { new object[] {
						(short) 10, true }, new object[] {
						(short) 9, false }, new object[] { null, false } });

				TryOperator (env, path, "MYCONST > shortBoxed", new object[][] { new object[] {
						(short) 11, false }, new object[] {
						(short) 10, false }, new object[] {
						(short) 9, true }, new object[] {
						(short) 8, true } });
				TryOperator (env, path, "shortBoxed < MYCONST", new object[][] { new object[] {
						(short) 11, false }, new object[] {
						(short) 10, false }, new object[] {
						(short) 9, true }, new object[] {
						(short) 8, true } });

				TryOperator (env, path, "shortBoxed in (MYCONST)", new object[][] { new object[] {
						(short) 11, false }, new object[] {
						(short) 10, true }, new object[] {
						(short) 9, false }, new object[] {
						(short) 8, false } });

				// test SODA
				env.UndeployAll ();

				var epl = "@name('variable') create constant variable int MYCONST = 10";
				env.EplToModelCompileDeploy (epl);

				// test invalid
				env.TryInvalidCompile (path, "on SupportBean set MYCONST = 10",
					"Failed to validate assignment expression 'MYCONST=10': Variable by name 'MYCONST' is declared constant and may not be set [on SupportBean set MYCONST = 10]");
				env.TryInvalidCompile (path, "select * from SupportBean output when true then set MYCONST=1",
					"Failed to validate the output rate limiting clause: Failed to validate assignment expression 'MYCONST=1': Variable by name 'MYCONST' is declared constant and may not be set [select * from SupportBean output when true then set MYCONST=1]");

				// assure no update via API
				TryInvalidSetAPI (env, env.DeploymentId ("variable"), "MYCONST", 1);

				// add constant variable via config API
				TryInvalidSetAPI (env, null, "MYCONST_TWO", "dummy");
				TryInvalidSetAPI (env, null, "MYCONST_THREE", false);

				// try ESPER-653
				env.CompileDeploy ("@name('s0') @public create constant variable java.util.Date START_TIME = java.util.Calendar.getInstance().getTime()");
				env.AssertIterator ("s0", en => Assert.IsNotNull (en.Advance ().Get ("START_TIME")));
				env.UndeployModuleContaining ("s0");

				// test array constant
				env.UndeployAll ();
				env.CompileDeploy ("@public create constant variable string[] var_strings = {'E1', 'E2'}", path);
				env.CompileDeploy ("@name('s0') select var_strings from SupportBean", path);
				env.AssertStatement ("s0", statement => Assert.AreEqual (typeof (string[]), statement.EventType.GetPropertyType ("var_strings")));
				env.UndeployModuleContaining ("s0");

				TryAssertionArrayVar (env, path, "var_strings");

				TryOperator (env, path, "intBoxed in (10, 8)", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, true } });

				env.CompileDeploy ("@public create constant variable int [ ] var_ints = {8, 10}", path);
				TryOperator (env, path, "intBoxed in (var_ints)", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, false }, new object[] { 8, true } });

				env.CompileDeploy ("@public create constant variable int[]  var_intstwo = {9}", path);
				TryOperator (env, path, "intBoxed in (var_ints, var_intstwo)", new object[][] { new object[] { 11, false }, new object[] { 10, true }, new object[] { 9, true }, new object[] { 8, true } });

				env.TryInvalidCompile ("create constant variable SupportBean[] var_beans",
					"Cannot create variable 'var_beans', type 'SupportBean' cannot be declared as an array type and cannot receive type parameters as it is an event type");

				// test array of primitives
				env.CompileDeploy ("@name('s0') @public create variable byte[] myBytesBoxed");
				env.AssertStatement ("s0", statement => {
					var expectedType = new object[][] { new object[] { "myBytesBoxed", typeof (byte[]) } };
					SupportEventTypeAssertionUtil.AssertEventTypeProperties (expectedType, statement.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
				});
				env.UndeployModuleContaining ("s0");

				env.CompileDeploy ("@name('s0') @public create variable byte[primitive] myBytesPrimitive");
				env.AssertStatement ("s0", statement => {
					var expectedType = new object[][] { new object[] { "myBytesPrimitive", typeof (byte[]) } };
					SupportEventTypeAssertionUtil.AssertEventTypeProperties (expectedType, statement.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
				});
				env.UndeployAll ();

				// test enum constant
				env.CompileDeploy ("@public create constant variable SupportEnum var_enumone = SupportEnum.ENUM_VALUE_2", path);
				TryOperator (env, path, "var_enumone = enumValue", new object[][] { new object[] { SupportEnum.ENUM_VALUE_3, false }, new object[] { SupportEnum.ENUM_VALUE_2, true }, new object[] { SupportEnum.ENUM_VALUE_1, false } });

				env.CompileDeploy ("@public create constant variable SupportEnum[] var_enumarr = {SupportEnum.ENUM_VALUE_2, SupportEnum.ENUM_VALUE_1}", path);
				TryOperator (env, path, "enumValue in (var_enumarr, var_enumone)", new object[][] { new object[] { SupportEnum.ENUM_VALUE_3, false }, new object[] { SupportEnum.ENUM_VALUE_2, true }, new object[] { SupportEnum.ENUM_VALUE_1, true } });

				env.CompileDeploy ("@public create variable SupportEnum var_enumtwo = SupportEnum.ENUM_VALUE_2", path);
				env.CompileDeploy ("on SupportBean set var_enumtwo = enumValue", path);

				env.UndeployAll ();
			}

			private static void TryAssertionArrayVar (RegressionEnvironment env, RegressionPath path, string varName) {
				env.CompileDeploy ("@name('s0') select * from SupportBean(theString in (" + varName + "))", path).AddListener ("s0");

				SendBeanAssert (env, "E1", true);
				SendBeanAssert (env, "E2", true);
				SendBeanAssert (env, "E3", false);

				env.UndeployAll ();
			}

			private static void SendBeanAssert (RegressionEnvironment env, string theString, bool expected) {
				env.SendEventBean (new SupportBean (theString, 1));
				env.AssertListenerInvokedFlag ("s0", expected);
			}

			public ISet<RegressionFlag> Flags () {
				return Collections.Set (RegressionFlag.RUNTIMEOPS);
			}
		}

		private class EPLVariableUseEPRuntime : RegressionExecution {
			public void Run (RegressionEnvironment env) {
				var runtimeSPI = (EPVariableServiceSPI) env.Runtime.VariableService;
				var types = runtimeSPI.VariableTypeAll;
				Assert.AreEqual (typeof (int?), types.Get (new DeploymentIdNamePair (null, "var1")));
				Assert.AreEqual (typeof (string), types.Get (new DeploymentIdNamePair (null, "var2")));

				Assert.AreEqual (typeof (int?), runtimeSPI.GetVariableType (null, "var1"));
				Assert.AreEqual (typeof (string), runtimeSPI.GetVariableType (null, "var2"));

				var stmtTextSet = "on SupportBean set var1 = intPrimitive, var2 = theString";
				env.CompileDeploy (stmtTextSet);

				AssertVariableValuesPreconfigured (env, new string[] { "var1", "var2" }, new object[] {-1, "abc" });
				SendSupportBean (env, null, 99);
				AssertVariableValuesPreconfigured (env, new string[] { "var1", "var2" }, new object[] { 99, null });

				env.RuntimeSetVariable (null, "var2", "def");
				AssertVariableValuesPreconfigured (env, new string[] { "var1", "var2" }, new object[] { 99, "def" });

				env.Milestone (0);

				AssertVariableValuesPreconfigured (env, new string[] { "var1", "var2" }, new object[] { 99, "def" });
				env.RuntimeSetVariable (null, "var1", 123);
				AssertVariableValuesPreconfigured (env, new string[] { "var1", "var2" }, new object[] { 123, "def" });

				env.Milestone (1);

				AssertVariableValuesPreconfigured (env, new string[] { "var1", "var2" }, new object[] { 123, "def" });
				IDictionary<DeploymentIdNamePair, object> newValues = new Dictionary<DeploymentIdNamePair, object> ();
				newValues.Put (new DeploymentIdNamePair (null, "var1"), 20);
				env.Runtime.VariableService.SetVariableValue(newValues);
				AssertVariableValuesPreconfigured (env, new string[] { "var1", "var2" }, new object[] { 20, "def" });

				newValues.Put (new DeploymentIdNamePair (null, "var1"), (byte) 21);
				newValues.Put (new DeploymentIdNamePair (null, "var2"), "test");
				env.Runtime.VariableService.SetVariableValue(newValues);
				AssertVariableValuesPreconfigured (env, new string[] { "var1", "var2" }, new object[] { 21, "test" });

				newValues.Put (new DeploymentIdNamePair (null, "var1"), null);
				newValues.Put (new DeploymentIdNamePair (null, "var2"), null);
				env.Runtime.VariableService.SetVariableValue(newValues);
				AssertVariableValuesPreconfigured (env, new string[] { "var1", "var2" }, new object[] { null, null });

				// try variable not found
				try {
					env.RuntimeSetVariable (null, "dummy", null);
					Assert.Fail ();
				} catch (VariableNotFoundException ex) {
					// expected
					Assert.AreEqual ("Variable by name 'dummy' has not been declared", ex.Message);
				}

				// try variable not found
				try {
					newValues.Put (new DeploymentIdNamePair (null, "dummy2"), 20);
					env.Runtime.VariableService.SetVariableValue(newValues);
					Assert.Fail ();
				} catch (VariableNotFoundException ex) {
					// expected
					Assert.AreEqual ("Variable by name 'dummy2' has not been declared", ex.Message);
				}

				// create new variable on the fly
				env.CompileDeploy ("@name('create') create variable int dummy = 20 + 20");
				Assert.AreEqual (40, env.Runtime.VariableService.GetVariableValue (env.DeploymentId ("create"), "dummy"));

				// try type coercion
				try {
					env.Runtime.VariableService.SetVariableValue (env.DeploymentId ("create"), "dummy", "abc");
					Assert.Fail ();
				} catch (VariableValueException ex) {
					// expected
					Assert.AreEqual ("Variable 'dummy' of declared type Integer cannot be assigned a value of type String", ex.Message);
				}

				try {
					env.Runtime.VariableService.SetVariableValue (env.DeploymentId ("create"), "dummy", 100L);
					Assert.Fail ();
				} catch (VariableValueException ex) {
					// expected
					Assert.AreEqual ("Variable 'dummy' of declared type Integer cannot be assigned a value of type Long", ex.Message);
				}

				try {
					env.RuntimeSetVariable (null, "var2", 0);
					Assert.Fail ();
				} catch (VariableValueException ex) {
					// expected
					Assert.AreEqual ("Variable 'var2' of declared type String cannot be assigned a value of type Integer", ex.Message);
				}

				// coercion
				env.RuntimeSetVariable (null, "var1", (short) - 1);
				AssertVariableValuesPreconfigured (env, new string[] { "var1", "var2" }, new object[] {-1, null });

				// rollback for coercion failed
				newValues = new LinkedHashMap<DeploymentIdNamePair,object>(); // preserve order
				newValues.Put (new DeploymentIdNamePair (null, "var2"), "xyz");
				newValues.Put (new DeploymentIdNamePair (null, "var1"), 4.4d);
				try {
					env.Runtime.VariableService.SetVariableValue(newValues);
					Assert.Fail ();
				} catch (VariableValueException ex) {
					// expected
				}
				AssertVariableValuesPreconfigured (env, new string[] { "var1", "var2" }, new object[] {-1, null });

				// rollback for variable not found
				newValues = new LinkedHashMap<DeploymentIdNamePair,object>(); // preserve order
				newValues.Put (new DeploymentIdNamePair (null, "var2"), "xyz");
				newValues.Put (new DeploymentIdNamePair (null, "var1"), 1);
				newValues.Put (new DeploymentIdNamePair (null, "notfoundvariable"), null);
				try {
					env.Runtime.VariableService.SetVariableValue(newValues);
					Assert.Fail ();
				} catch (VariableNotFoundException ex) {
					// expected
				}
				AssertVariableValuesPreconfigured (env, new string[] { "var1", "var2" }, new object[] {-1, null });

				env.UndeployAll ();
			}

			public ISet<RegressionFlag> Flags () {
				return Collections.Set (RegressionFlag.RUNTIMEOPS);
			}
		}

		private class EPLVariableUseVariableInFilterBoolean : RegressionExecution {
			public void Run (RegressionEnvironment env) {

				var stmtTextSet = "@name('set') on SupportBean_S0 set var1IFB = p00, var2IFB = p01";
				env.CompileDeploy (stmtTextSet).AddListener ("set");
				var fieldsVar = new string[] { "var1IFB", "var2IFB" };
				env.AssertPropsPerRowIterator ("set", fieldsVar, new object[][] { new object[] { null, null } });

				var stmtTextSelect = "@name('s0') select theString, intPrimitive from SupportBean(theString = var1IFB or theString = var2IFB)";
				var fieldsSelect = new string[] { "theString", "intPrimitive" };
				env.CompileDeploy (stmtTextSelect).AddListener ("s0");

				SendSupportBean (env, null, 1);
				env.AssertListenerNotInvoked ("s0");

				env.Milestone (0);

				SendSupportBeanS0NewThread (env, 100, "a", "b");
				env.AssertPropsNew ("set", fieldsVar, new object[] { "a", "b" });

				SendSupportBean (env, "a", 2);
				env.AssertPropsNew ("s0", fieldsSelect, new object[] { "a", 2 });

				SendSupportBean (env, null, 1);
				env.AssertListenerNotInvoked ("s0");

				env.Milestone (1);

				SendSupportBean (env, "b", 3);
				env.AssertPropsNew ("s0", fieldsSelect, new object[] { "b", 3 });

				SendSupportBean (env, "c", 4);
				env.AssertListenerNotInvoked ("s0");

				env.Milestone (2);

				SendSupportBeanS0NewThread (env, 100, "e", "c");
				env.AssertPropsNew ("set", fieldsVar, new object[] { "e", "c" });

				SendSupportBean (env, "c", 5);
				env.AssertPropsNew ("s0", fieldsSelect, new object[] { "c", 5 });

				SendSupportBean (env, "e", 6);
				env.AssertPropsNew ("s0", fieldsSelect, new object[] { "e", 6 });

				env.UndeployAll ();
			}
		}

		private class EPLVariableUseVariableInFilter : RegressionExecution {
			public void Run (RegressionEnvironment env) {

				var stmtTextSet = "@name('set') on SupportBean_S0 set var1IF = p00";
				env.CompileDeploy (stmtTextSet).AddListener ("set");
				var fieldsVar = new string[] { "var1IF" };
				env.AssertPropsPerRowIterator ("set", fieldsVar, new object[][] { new object[] { null } });

				var stmtTextSelect = "@name('s0') select theString, intPrimitive from SupportBean(theString = var1IF)";
				var fieldsSelect = new string[] { "theString", "intPrimitive" };
				env.CompileDeploy (stmtTextSelect).AddListener ("s0");

				SendSupportBean (env, null, 1);
				env.AssertListenerNotInvoked ("s0");

				SendSupportBeanS0NewThread (env, 100, "a", "b");
				env.AssertPropsNew ("set", fieldsVar, new object[] { "a" });

				SendSupportBean (env, "a", 2);
				env.AssertPropsNew ("s0", fieldsSelect, new object[] { "a", 2 });

				env.Milestone (0);

				SendSupportBean (env, null, 1);
				env.AssertListenerNotInvoked ("s0");

				SendSupportBeanS0NewThread (env, 100, "e", "c");
				env.AssertPropsNew ("set", fieldsVar, new object[] { "e" });

				env.Milestone (1);

				SendSupportBean (env, "c", 5);
				env.AssertListenerNotInvoked ("s0");

				SendSupportBean (env, "e", 6);
				env.AssertPropsNew ("s0", fieldsSelect, new object[] { "e", 6 });

				env.UndeployAll ();
			}
		}

		private static SupportBean SendSupportBean (RegressionEnvironment env, string theString, int intPrimitive) {
			var bean = new SupportBean ();
			bean.TheString = theString;
			bean.IntPrimitive = intPrimitive;
			env.SendEventBean (bean);
			return bean;
		}

		private static void SendSupportBeanS0NewThread (RegressionEnvironment env, int id, string p00, string p01) {
			try {
				var t = new Thread(() => { env.SendEventBean (new SupportBean_S0 (id, p00, p01)); });
				t.Start ();
				t.Join ();
			} catch (ThreadInterruptedException ex) {
				Assert.Fail (ex.Message);
			}
		}

		private static SupportBean MakeSupportBean (string theString, int intPrimitive, int? intBoxed) {
			var bean = new SupportBean ();
			bean.TheString = theString;
			bean.IntPrimitive = intPrimitive;
			bean.IntBoxed = intBoxed;
			return bean;
		}

		private static void AssertVariableValuesPreconfigured (RegressionEnvironment env, string[] names, object[] values) {
			Assert.AreEqual (names.Length, values.Length);

			// assert one-by-one
			for (var i = 0; i < names.Length; i++) {
				Assert.AreEqual (values[i], env.Runtime.VariableService.GetVariableValue (null, names[i]));
			}

			// get and assert all
			var all = env.Runtime.VariableService.GetVariableValueAll();
			for (var i = 0; i < names.Length; i++) {
				Assert.AreEqual (values[i], all.Get (new DeploymentIdNamePair (null, names[i])));
			}

			// get by request
			ISet<DeploymentIdNamePair> nameSet = new HashSet<DeploymentIdNamePair>();
			foreach (var name in names) {
				nameSet.Add (new DeploymentIdNamePair (null, name));
			}
			var valueSet = env.Runtime.VariableService.GetVariableValue (nameSet);
			for (var i = 0; i < names.Length; i++) {
				Assert.AreEqual (values[i], valueSet.Get (new DeploymentIdNamePair (null, names[i])));
			}
		}

		[Serializable] public class A {
			public string GetValue () {
				return "";
			}
		}

		public class B { }

		private static void TryInvalidSetAPI (RegressionEnvironment env, string deploymentId, string variableName, object newValue) {
			try {
				env.Runtime.VariableService.SetVariableValue (deploymentId, variableName, newValue);
				Assert.Fail ();
			} catch (VariableConstantValueException ex) {
				Assert.AreEqual (ex.Message, "Variable by name '" + variableName + "' is declared as constant and may not be assigned a new value");
			}
			try {
				env.Runtime.VariableService.SetVariableValue(Collections.SingletonMap<DeploymentIdNamePair, object>(
					new DeploymentIdNamePair(deploymentId, variableName),
					newValue));
				Assert.Fail ();
			} catch (VariableConstantValueException ex) {
				Assert.AreEqual (ex.Message, "Variable by name '" + variableName + "' is declared as constant and may not be assigned a new value");
			}
		}

		private static void TryOperator (RegressionEnvironment env, RegressionPath path, string @operator, object[][] testdata) {
			env.CompileDeploy ("@name('s0') select theString as c0,intPrimitive as c1 from SupportBean(" + @operator + ")", path);
			env.AddListener ("s0");

			// initiate
			env.SendEventBean (new SupportBean_S0 (10, "S01"));

			for (var i = 0; i < testdata.Length; i++) {
				var bean = new SupportBean ();
				var testValue = testdata[i][0];
				if (testValue is int?) {
					bean.IntBoxed = (int?) testValue;
				} else if (testValue is SupportEnum) {
					bean.EnumValue = (SupportEnum) testValue;
				} else {
					bean.ShortBoxed = (short?) testValue;
				}
				var expected = (bool) testdata[i][1];

				env.SendEventBean (bean);
				env.AssertListenerInvokedFlag ("s0", expected, "Failed at " + i);
			}

			// assert type of expression
			env.AssertThat (() => {
				var item = SupportFilterServiceHelper.GetFilterSvcSingle (env.Statement ("s0"));
				Assert.IsTrue (item.Op != FilterOperator.BOOLEAN_EXPRESSION);
			});

			env.UndeployModuleContaining ("s0");
		}

		public class MySimpleVariableServiceFactory {
			public static MySimpleVariableService MakeService () {
				return new MySimpleVariableService ();
			}
		}

		/// <summary>
		/// Test service; only serializable because it *may* go over the wire some time when running tests and serialization is just convenient
		/// </summary>
		[Serializable] public class MySimpleVariableService {

			public string DoSomething () {
				return "hello";
			}
		}

		public enum MyEnumWithOverride {
			LONG,
			SHORT
		}

		public static class MyEnumWithOverrideExtensions {
			public static int GetValue (MyEnumWithOverride value) {
				switch (value) {
					case MyEnumWithOverride.LONG:
						return 1;
					case MyEnumWithOverride.SHORT:
						return -1;
				}

				throw new ArgumentException ("invalid value", nameof (value));
			}
		}

		/// <summary>
		/// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
		/// </summary>
		[Serializable] public class MyVariableCustomEvent {
			private readonly MyVariableCustomType name;

			internal MyVariableCustomEvent (MyVariableCustomType name) {
				this.name = name;
			}

			public MyVariableCustomType GetName () {
				return name;
			}
		}

		/// <summary>
		/// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
		/// </summary>
		[Serializable] public class MyVariableCustomType {
			private readonly string name;

			MyVariableCustomType (string name) {
				this.name = name;
			}

			public static MyVariableCustomType Of (string name) {
				return new MyVariableCustomType (name);
			}

			public string GetName () {
				return name;
			}

			public override bool Equals (object o) {
				if (this == o) return true;
				if (o == null || GetType () != o.GetType ()) return false;
				var myType = (MyVariableCustomType) o;
				return object.Equals (name, myType.name);
			}

			public override int GetHashCode () {
				return HashCode.Combine(name);
			}
		}

		/// <summary>
		/// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
		/// </summary>
		[Serializable] public class SupportVarargsObject {

			private long? value;

			public SupportVarargsObject (long? value) {
				this.value = value;
			}

			public long? Value {
				get => value;
				set => this.value = value;
			}
		}

		public interface SupportVarargsClient {

			bool FunctionWithVarargs (long? longValue, params object[] objects);

			SupportVarargsObject GetTestObject (string stringValue);
		}

		/// <summary>
		/// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
		/// </summary>
		public class SupportVarargsClientImpl : SupportVarargsClient {

			public bool FunctionWithVarargs (long? longValue, params object[] objects) {
				var obj = (SupportVarargsObject) objects[0];
				return longValue.Equals (obj.Value);
			}

			public SupportVarargsObject GetTestObject (string stringValue) {
				return new SupportVarargsObject (long.Parse (stringValue));
			}
		}
	}
} // end of namespace