///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;


namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
	public class ResultSetOutputLimitRowLimit
	{

		public static ICollection<RegressionExecution> Executions()
		{
			var execs = new List<RegressionExecution>();
			execs.Add(new ResultSetLimitOneWithOrderOptimization());
			execs.Add(new ResultSetBatchNoOffsetNoOrder());
			execs.Add(new ResultSetOrderBy());
			execs.Add(new ResultSetBatchOffsetNoOrderOM());
			execs.Add(new ResultSetFullyGroupedOrdered());
			execs.Add(new ResultSetEventPerRowUnGrouped());
			execs.Add(new ResultSetGroupedSnapshot());
			execs.Add(new ResultSetGroupedSnapshotNegativeRowcount());
			execs.Add(new ResultSetInvalid());
			execs.Add(new ResultSetLengthOffsetVariable());
			return execs;
		}

		private class ResultSetLimitOneWithOrderOptimization : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// batch-window assertions
				var path = new RegressionPath();
				var eplWithBatchSingleKey =
					"@name('s0') select theString from SupportBean#length_batch(10) order by theString limit 1";
				TryAssertionLimitOneSingleKeySortBatch(env, path, eplWithBatchSingleKey);

				var eplWithBatchMultiKey =
					"@name('s0') select theString, intPrimitive from SupportBean#length_batch(5) order by theString asc, intPrimitive desc limit 1";
				TryAssertionLimitOneMultiKeySortBatch(env, path, eplWithBatchMultiKey);

				// context output-when-terminated assertions
				env.CompileDeploy(
					"@public create context StartS0EndS1 as start SupportBean_S0 end SupportBean_S1",
					path);

				var eplContextSingleKey = "@name('s0') context StartS0EndS1 " +
				                          "select theString from SupportBean#keepall " +
				                          "output snapshot when terminated " +
				                          "order by theString limit 1";
				TryAssertionLimitOneSingleKeySortBatch(env, path, eplContextSingleKey);

				var eplContextMultiKey = "@name('s0') context StartS0EndS1 " +
				                         "select theString, intPrimitive from SupportBean#keepall " +
				                         "output snapshot when terminated " +
				                         "order by theString asc, intPrimitive desc limit 1";
				TryAssertionLimitOneMultiKeySortBatch(env, path, eplContextMultiKey);

				env.UndeployAll();
			}

			private static void TryAssertionLimitOneMultiKeySortBatch(
				RegressionEnvironment env,
				RegressionPath path,
				string epl)
			{
				env.CompileDeploy(epl, path).AddListener("s0");

				SendSBSequenceAndAssert(
					env,
					"F",
					10,
					new object[][] {
						new object[] { "F", 10 }, new object[] { "X", 8 }, new object[] { "F", 8 },
						new object[] { "G", 10 }, new object[] { "X", 1 }
					});
				SendSBSequenceAndAssert(
					env,
					"G",
					12,
					new object[][] {
						new object[] { "X", 10 }, new object[] { "G", 12 }, new object[] { "H", 100 },
						new object[] { "G", 10 }, new object[] { "X", 1 }
					});
				SendSBSequenceAndAssert(
					env,
					"G",
					11,
					new object[][] {
						new object[] { "G", 10 }, new object[] { "G", 8 }, new object[] { "G", 8 },
						new object[] { "G", 10 }, new object[] { "G", 11 }
					});

				env.UndeployModuleContaining("s0");
			}

			private static void TryAssertionLimitOneSingleKeySortBatch(
				RegressionEnvironment env,
				RegressionPath path,
				string epl)
			{
				env.CompileDeploy(epl, path).AddListener("s0");

				SendSBSequenceAndAssert(env, "A", new string[] { "F", "Q", "R", "T", "M", "T", "A", "I", "P", "B" });
				SendSBSequenceAndAssert(env, "B", new string[] { "P", "Q", "P", "T", "P", "T", "P", "P", "P", "B" });
				SendSBSequenceAndAssert(
					env,
					"C",
					new string[] { "C", "P", "Q", "P", "T", "P", "T", "P", "P", "P", "X" });

				env.UndeployModuleContaining("s0");
			}
		}

		private class ResultSetBatchNoOffsetNoOrder : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select irstream * from SupportBean#length_batch(3) limit 1";
				env.CompileDeploy(epl).AddListener("s0");

				TryAssertion(env);
				env.UndeployAll();
			}
		}

		private class ResultSetLengthOffsetVariable : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.CompileDeploy("@public create variable int myrows = 2", path);
				env.CompileDeploy("@public create variable int myoffset = 1", path);
				env.CompileDeploy("on SupportBeanNumeric set myrows = intOne, myoffset = intTwo", path);

				string epl;

				epl = "@name('s0') select * from SupportBean#length(5) output every 5 events limit myoffset, myrows";
				env.CompileDeploy(epl, path).AddListener("s0");
				TryAssertionVariable(env);
				env.UndeployModuleContaining("s0");

				env.SendEventBean(new SupportBeanNumeric(2, 1));

				epl =
					"@name('s0') select * from SupportBean#length(5) output every 5 events limit myrows offset myoffset";
				env.CompileDeploy(epl, path).AddListener("s0");
				TryAssertionVariable(env);
				env.UndeployModuleContaining("s0");

				env.SendEventBean(new SupportBeanNumeric(2, 1));

				env.EplToModelCompileDeploy(epl, path).AddListener("s0");
				TryAssertionVariable(env);

				env.UndeployAll();
			}
		}

		private class ResultSetOrderBy : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@name('s0') select * from SupportBean#length(5) output every 5 events order by intPrimitive limit 2 offset 2";
				env.CompileDeploy(epl).AddListener("s0");

				var fields = "theString".SplitCsv();

				env.AssertPropsPerRowIterator("s0", fields, null);

				SendEvent(env, "E1", 90);
				env.AssertPropsPerRowIterator("s0", fields, null);

				SendEvent(env, "E2", 5);
				env.AssertPropsPerRowIterator("s0", fields, null);

				SendEvent(env, "E3", 60);
				env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });

				SendEvent(env, "E4", 99);
				env.AssertPropsPerRowIterator(
					"s0",
					fields,
					new object[][] { new object[] { "E1" }, new object[] { "E4" } });
				env.AssertListenerNotInvoked("s0");

				SendEvent(env, "E5", 6);
				env.AssertPropsPerRowIterator(
					"s0",
					fields,
					new object[][] { new object[] { "E3" }, new object[] { "E1" } });
				env.AssertPropsPerRowLastNew(
					"s0",
					fields,
					new object[][] { new object[] { "E3" }, new object[] { "E1" } });

				env.UndeployAll();
			}
		}

		private class ResultSetBatchOffsetNoOrderOM : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var model = new EPStatementObjectModel();
				model.SelectClause = SelectClause.CreateWildcard();
				model.SelectClause.StreamSelector = StreamSelector.RSTREAM_ISTREAM_BOTH;
				model.FromClause = FromClause.Create(
					FilterStream.Create("SupportBean").AddView("length_batch", Expressions.Constant(3)));
				model.RowLimitClause = RowLimitClause.Create(1);

				var epl = "select irstream * from SupportBean#length_batch(3) limit 1";
				Assert.AreEqual(epl, model.ToEPL());

				model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
				env.CompileDeploy(model).AddListener("s0");

				TryAssertion(env);
				env.UndeployAll();

				env.EplToModelCompileDeploy("@name('s0') " + epl).AddListener("s0");
				TryAssertion(env);
				env.UndeployAll();
			}
		}

		private class ResultSetFullyGroupedOrdered : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@name('s0') select theString, sum(intPrimitive) as mysum from SupportBean#length(5) group by theString order by sum(intPrimitive) limit 2";
				env.CompileDeploy(epl).AddListener("s0");

				var fields = "theString,mysum".SplitCsv();

				env.AssertPropsPerRowIterator("s0", fields, null);

				SendEvent(env, "E1", 90);
				env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1", 90 } });

				SendEvent(env, "E2", 5);
				env.AssertPropsPerRowIterator(
					"s0",
					fields,
					new object[][] { new object[] { "E2", 5 }, new object[] { "E1", 90 } });

				SendEvent(env, "E3", 60);
				env.AssertPropsPerRowIterator(
					"s0",
					fields,
					new object[][] { new object[] { "E2", 5 }, new object[] { "E3", 60 } });

				SendEvent(env, "E3", 40);
				env.AssertPropsPerRowIterator(
					"s0",
					fields,
					new object[][] { new object[] { "E2", 5 }, new object[] { "E1", 90 } });

				SendEvent(env, "E2", 1000);
				env.AssertPropsPerRowIterator(
					"s0",
					fields,
					new object[][] { new object[] { "E1", 90 }, new object[] { "E3", 100 } });

				env.UndeployAll();
			}
		}

		private class ResultSetEventPerRowUnGrouped : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SendTimer(env, 1000);
				var epl =
					"@name('s0') select theString, sum(intPrimitive) as mysum from SupportBean#length(5) output every 10 seconds order by theString desc limit 2";
				env.CompileDeploy(epl).AddListener("s0");

				var fields = "theString,mysum".SplitCsv();

				env.AssertPropsPerRowIterator("s0", fields, null);

				SendEvent(env, "E1", 10);
				SendEvent(env, "E2", 5);
				SendEvent(env, "E3", 20);
				SendEvent(env, "E4", 30);

				SendTimer(env, 11000);
				env.AssertPropsPerRowLastNew(
					"s0",
					fields,
					new object[][] { new object[] { "E4", 65 }, new object[] { "E3", 35 } });

				env.UndeployAll();
			}
		}

		private class ResultSetGroupedSnapshot : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SendTimer(env, 1000);
				var epl =
					"@name('s0') select theString, sum(intPrimitive) as mysum from SupportBean#length(5) group by theString output snapshot every 10 seconds order by sum(intPrimitive) desc limit 2";
				env.CompileDeploy(epl).AddListener("s0");

				var fields = "theString,mysum".SplitCsv();

				env.AssertPropsPerRowIterator("s0", fields, null);

				SendEvent(env, "E1", 10);
				SendEvent(env, "E2", 5);
				SendEvent(env, "E3", 20);
				SendEvent(env, "E1", 30);

				SendTimer(env, 11000);
				env.AssertPropsPerRowLastNew(
					"s0",
					fields,
					new object[][] { new object[] { "E1", 40 }, new object[] { "E3", 20 } });

				env.UndeployAll();
			}
		}

		private class ResultSetGroupedSnapshotNegativeRowcount : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SendTimer(env, 1000);
				var epl =
					"@name('s0') select theString, sum(intPrimitive) as mysum from SupportBean#length(5) group by theString output snapshot every 10 seconds order by sum(intPrimitive) desc limit -1 offset 1";
				env.CompileDeploy(epl).AddListener("s0");

				var fields = "theString,mysum".SplitCsv();

				env.AssertPropsPerRowIterator("s0", fields, null);

				SendEvent(env, "E1", 10);
				SendEvent(env, "E2", 5);
				SendEvent(env, "E3", 20);
				SendEvent(env, "E1", 30);

				SendTimer(env, 11000);
				env.AssertPropsPerRowLastNew(
					"s0",
					fields,
					new object[][] { new object[] { "E3", 20 }, new object[] { "E2", 5 } });

				env.UndeployAll();
			}
		}

		private class ResultSetInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.CompileDeploy("@public create variable string myrows = 'abc'", path);
				env.TryInvalidCompile(
					path,
					"select * from SupportBean limit myrows",
					"Limit clause requires a variable of numeric type [select * from SupportBean limit myrows]");
				env.TryInvalidCompile(
					path,
					"select * from SupportBean limit 1, myrows",
					"Limit clause requires a variable of numeric type [select * from SupportBean limit 1, myrows]");
				env.TryInvalidCompile(
					path,
					"select * from SupportBean limit dummy",
					"Limit clause variable by name 'dummy' has not been declared [select * from SupportBean limit dummy]");
				env.TryInvalidCompile(
					path,
					"select * from SupportBean limit 1,dummy",
					"Limit clause variable by name 'dummy' has not been declared [select * from SupportBean limit 1,dummy]");
				env.UndeployAll();
			}
		}

		private static void SendTimer(
			RegressionEnvironment env,
			long timeInMSec)
		{
			env.AdvanceTime(timeInMSec);
		}

		private static void TryAssertionVariable(RegressionEnvironment env)
		{
			var fields = "theString".SplitCsv();

			env.AssertPropsPerRowIterator("s0", fields, null);

			SendEvent(env, "E1", 1);
			SendEvent(env, "E2", 2);
			env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E2" } });

			SendEvent(env, "E3", 3);
			env.AssertPropsPerRowIterator(
				"s0",
				fields,
				new object[][] { new object[] { "E2" }, new object[] { "E3" } });

			SendEvent(env, "E4", 4);
			env.AssertPropsPerRowIterator(
				"s0",
				fields,
				new object[][] { new object[] { "E2" }, new object[] { "E3" } });
			env.AssertListenerNotInvoked("s0");

			SendEvent(env, "E5", 5);
			env.AssertPropsPerRowIterator(
				"s0",
				fields,
				new object[][] { new object[] { "E2" }, new object[] { "E3" } });
			env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "E2" }, new object[] { "E3" } });

			SendEvent(env, "E6", 6);
			env.AssertPropsPerRowIterator(
				"s0",
				fields,
				new object[][] { new object[] { "E3" }, new object[] { "E4" } });
			env.AssertListenerNotInvoked("s0");

			// change variable values
			env.SendEventBean(new SupportBeanNumeric(2, 3));
			SendEvent(env, "E7", 7);
			env.AssertPropsPerRowIterator(
				"s0",
				fields,
				new object[][] { new object[] { "E6" }, new object[] { "E7" } });
			env.AssertListenerNotInvoked("s0");

			env.SendEventBean(new SupportBeanNumeric(-1, 0));
			SendEvent(env, "E8", 8);
			env.AssertPropsPerRowIterator(
				"s0",
				fields,
				new object[][] {
					new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" }, new object[] { "E7" },
					new object[] { "E8" }
				});
			env.AssertListenerNotInvoked("s0");

			env.SendEventBean(new SupportBeanNumeric(10, 0));
			SendEvent(env, "E9", 9);
			env.AssertPropsPerRowIterator(
				"s0",
				fields,
				new object[][] {
					new object[] { "E5" }, new object[] { "E6" }, new object[] { "E7" }, new object[] { "E8" },
					new object[] { "E9" }
				});
			env.AssertListenerNotInvoked("s0");

			env.SendEventBean(new SupportBeanNumeric(6, 3));
			SendEvent(env, "E10", 10);
			env.AssertPropsPerRowIterator(
				"s0",
				fields,
				new object[][] { new object[] { "E9" }, new object[] { "E10" } });
			env.AssertPropsPerRowLastNew(
				"s0",
				fields,
				new object[][] { new object[] { "E9" }, new object[] { "E10" } });

			env.SendEventBean(new SupportBeanNumeric(1, 1));
			env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E7" } });

			env.SendEventBean(new SupportBeanNumeric(2, 1));
			env.AssertPropsPerRowIterator(
				"s0",
				fields,
				new object[][] { new object[] { "E7" }, new object[] { "E8" } });

			env.SendEventBean(new SupportBeanNumeric(1, 2));
			env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E8" } });

			env.SendEventBean(new SupportBeanNumeric(6, 6));
			env.AssertPropsPerRowIterator("s0", fields, null);

			env.SendEventBean(new SupportBeanNumeric(1, 4));
			env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E10" } });

			env.SendEventBean(new SupportBeanNumeric(null, null));
			env.AssertPropsPerRowIterator(
				"s0",
				fields,
				new object[][] {
					new object[] { "E6" }, new object[] { "E7" }, new object[] { "E8" }, new object[] { "E9" },
					new object[] { "E10" }
				});

			env.SendEventBean(new SupportBeanNumeric(null, 2));
			env.AssertPropsPerRowIterator(
				"s0",
				fields,
				new object[][] { new object[] { "E8" }, new object[] { "E9" }, new object[] { "E10" } });

			env.SendEventBean(new SupportBeanNumeric(2, null));
			env.AssertPropsPerRowIterator(
				"s0",
				fields,
				new object[][] { new object[] { "E6" }, new object[] { "E7" } });

			env.SendEventBean(new SupportBeanNumeric(-1, 4));
			env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E10" } });

			env.SendEventBean(new SupportBeanNumeric(-1, 0));
			env.AssertPropsPerRowIterator(
				"s0",
				fields,
				new object[][] {
					new object[] { "E6" }, new object[] { "E7" }, new object[] { "E8" }, new object[] { "E9" },
					new object[] { "E10" }
				});

			env.SendEventBean(new SupportBeanNumeric(0, 0));
			env.AssertPropsPerRowIterator("s0", fields, null);
		}

		private static void TryAssertion(RegressionEnvironment env)
		{
			var fields = "theString".SplitCsv();

			SendEvent(env, "E1", 1);
			env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });

			SendEvent(env, "E2", 2);
			env.AssertListenerNotInvoked("s0");
			env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });

			SendEvent(env, "E3", 3);
			env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "E1" } });
			env.AssertPropsPerRowIterator("s0", fields, null);

			SendEvent(env, "E4", 4);
			env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E4" } });

			SendEvent(env, "E5", 5);
			env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E4" } });

			SendEvent(env, "E6", 6);
			env.AssertPropsPerRowIRPair(
				"s0",
				fields,
				new object[][] { new object[] { "E4" } },
				new object[][] { new object[] { "E1" } });
			env.AssertPropsPerRowIterator("s0", fields, null);
		}

		private static void SendEvent(
			RegressionEnvironment env,
			string theString,
			int intPrimitive)
		{
			env.SendEventBean(new SupportBean(theString, intPrimitive));
		}

		private static void SendSBSequenceAndAssert(
			RegressionEnvironment env,
			string expected,
			string[] theStrings)
		{
			env.SendEventBean(new SupportBean_S0(0));
			foreach (var theString in theStrings) {
				SendEvent(env, theString, 0);
			}

			env.SendEventBean(new SupportBean_S1(0));
			env.AssertPropsNew("s0", "theString".SplitCsv(), new object[] { expected });
		}

		private static void SendSBSequenceAndAssert(
			RegressionEnvironment env,
			string expectedString,
			int expectedInt,
			object[][] rows)
		{
			env.SendEventBean(new SupportBean_S0(0));
			foreach (var row in rows) {
				SendEvent(env, row[0].ToString(), row[1].AsInt32());
			}

			env.SendEventBean(new SupportBean_S1(0));
			env.AssertPropsNew("s0", "theString,intPrimitive".SplitCsv(), new object[] { expectedString, expectedInt });
		}
	}
} // end of namespace
