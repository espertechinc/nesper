///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
	public class InfraTableResetAggregationState
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new InfraTableResetRowSum());
			execs.Add(new InfraTableResetRowSumWTableAlias());
			execs.Add(new InfraTableResetSelective());
			execs.Add(new InfraTableResetVariousAggs());
			execs.Add(new InfraTableResetInvalid());
			execs.Add(new InfraTableResetDocSample());
			return execs;
		}

		private class InfraTableResetDocSample : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create table IntrusionCountTable (\n" +
				             "  fromAddress string primary key,\n" +
				             "  toAddress string primary key,\n" +
				             "  countIntrusion10Sec count(*),\n" +
				             "  countIntrusion60Sec count(*)\n" +
				             ");\n" +
				             "create schema IntrusionReset(fromAddress string, toAddress string);\n" +
				             "on IntrusionReset as resetEvent merge IntrusionCountTable as tableRow\n" +
				             "where resetEvent.fromAddress = tableRow.fromAddress and resetEvent.toAddress = tableRow.toAddress\n" +
				             "when matched then update set countIntrusion10Sec.reset(), countIntrusion60Sec.reset();\n" +
				             "" +
				             "on IntrusionReset as resetEvent merge IntrusionCountTable as tableRow\n" +
				             "where resetEvent.fromAddress = tableRow.fromAddress and resetEvent.toAddress = tableRow.toAddress\n" +
				             "when matched then update set tableRow.reset();\n";
				env.Compile(epl);
			}
		}

		private class InfraTableResetInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string prefix = "@Name('table') create table MyTable(asum sum(int));\n";

				string invalidSelectAggReset = prefix + "on SupportBean_S0 merge MyTable when matched then insert into MyStream select asum.reset()";
				TryInvalidCompile(
					env,
					invalidSelectAggReset,
					"Failed to validate select-clause expression 'asum.reset()': The table aggregation'reset' method is only available for the on-merge update action");

				string invalidSelectRowReset = prefix + "on SupportBean_S0 merge MyTable as mt when matched then insert into MyStream select mt.reset()";
				TryInvalidCompile(env, invalidSelectRowReset, "Failed to validate select-clause expression 'mt.reset()'");

				string invalidAggResetWParams = prefix + "on SupportBean_S0 merge MyTable as mt when matched then update set asum.reset(1)";
				TryInvalidCompile(
					env,
					invalidAggResetWParams,
					"Failed to validate update assignment expression 'asum.reset(1)': The table aggregation 'reset' method does not allow parameters");

				string invalidRowResetWParams = prefix + "on SupportBean_S0 merge MyTable as mt when matched then update set mt.reset(1)";
				TryInvalidCompile(
					env,
					invalidRowResetWParams,
					"Failed to validate update assignment expression 'mt.reset(1)': The table aggregation 'reset' method does not allow parameters");
			}
		}

		private class InfraTableResetVariousAggs : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('table') create table MyTable(" +
				             "  myAvedev avedev(int),\n" +
				             "  myCount count(*),\n" +
				             "  myCountDistinct count(distinct int),\n" +
				             "  myMax max(int),\n" +
				             "  myMedian median(int),\n" +
				             "  myStddev stddev(int),\n" +
				             "  myFirstEver firstever(string),\n" +
				             "  myCountEver countever(*)," +
				             "  myMaxByEver maxbyever(IntPrimitive) @type(SupportBean)," +
				             "  myPluginAggSingle myaggsingle(*)," +
				             "  myPluginAggAccess referenceCountedMap(string)," +
				             "  myWordcms countMinSketch()" +
				             ");\n" +
				             "into table MyTable select" +
				             "  avedev(IntPrimitive) as myAvedev," +
				             "  count(*) as myCount," +
				             "  count(distinct IntPrimitive) as myCountDistinct," +
				             "  max(IntPrimitive) as myMax," +
				             "  median(IntPrimitive) as myMedian," +
				             "  stddev(IntPrimitive) as myStddev," +
				             "  firstever(TheString) as myFirstEver," +
				             "  countever(*) as myCountEver," +
				             "  maxbyever(*) as myMaxByEver," +
				             "  myaggsingle(*) as myPluginAggSingle," +
				             "  referenceCountedMap(TheString) as myPluginAggAccess," +
				             "  countMinSketchAdd(TheString) as myWordcms" +
				             "   " +
				             "from SupportBean#keepall;\n" +
				             "on SupportBean_S0 merge MyTable mt when matched then update set mt.reset();\n" +
				             "@Name('s0') select MyTable.myWordcms.countMinSketchFrequency(p10) as c0 from SupportBean_S1;\n";
				env.CompileDeploy(epl).AddListener("s0");
				string[] fieldSetOne = "myAvedev,myCount,myCountDistinct,myMax,myMedian,myStddev,myFirstEver,myCountEver,myMaxByEver".SplitCsv();

				SendEventSetAssert(env, fieldSetOne);

				env.Milestone(0);

				SendResetAssert(env, fieldSetOne);

				env.Milestone(1);

				SendEventSetAssert(env, fieldSetOne);

				env.UndeployAll();
			}

			private void SendEventSetAssert(
				RegressionEnvironment env,
				string[] fieldSetOne)
			{
				SendBean(env, "E1", 10);
				SendBean(env, "E2", 10);
				SupportBean e3 = SendBean(env, "E3", 30);

				EventBean row = env.GetEnumerator("table").Advance();
				EPAssertionUtil.AssertProps(
					row,
					fieldSetOne,
					new object[] {8.88888888888889d, 3L, 2L, 30, 10.0, 11.547005383792515d, "E1", 3L, e3});
				Assert.AreEqual(-3, row.Get("myPluginAggSingle"));
				Assert.AreEqual(3, (row.Get("myPluginAggAccess").AsStringDictionary()).Count);

				AssertCountMinSketch(env, "E1", 1);
			}

			private void AssertCountMinSketch(
				RegressionEnvironment env,
				string theString,
				long expected)
			{
				env.SendEventBean(new SupportBean_S1(0, theString));
				Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
			}

			private void SendResetAssert(
				RegressionEnvironment env,
				string[] fieldSetOne)
			{
				env.SendEventBean(new SupportBean_S0(0));
				EventBean row = env.GetEnumerator("table").Advance();
				EPAssertionUtil.AssertProps(
					row,
					fieldSetOne,
					new object[] {null, 0L, 0L, null, null, null, null, 0L, null});
				Assert.AreEqual(0, row.Get("myPluginAggSingle"));
				Assert.AreEqual(0, row.Get("myPluginAggAccess").AsStringDictionary().Count);

				AssertCountMinSketch(env, "E1", 0);
			}
		}

		private class InfraTableResetSelective : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Name('table') create table MyTable(k string primary key, " +
				             "  avgone avg(int), avgtwo avg(int)," +
				             "  winone window(*) @type(SupportBean), wintwo window(*) @type(SupportBean)" +
				             ");\n" +
				             "into table MyTable select TheString, " +
				             "  avg(IntPrimitive) as avgone, avg(IntPrimitive) as avgtwo," +
				             "  window(*) as winone, window(*) as wintwo " +
				             "from SupportBean#keepall group by TheString;\n" +
				             "on SupportBean_S0 merge MyTable where p00 = k  when matched then update set avgone.reset(), winone.reset();\n" +
				             "on SupportBean_S1 merge MyTable where p10 = k  when matched then update set avgtwo.reset(), wintwo.reset();\n";
				env.CompileDeploy(epl);
				string[] propertyNames = "k,avgone,avgtwo,winone,wintwo".SplitCsv();

				SupportBean s0 = SendBean(env, "G1", 1);
				SupportBean s1 = SendBean(env, "G2", 10);
				SupportBean s2 = SendBean(env, "G2", 2);
				SupportBean s3 = SendBean(env, "G1", 20);
				EPAssertionUtil.AssertPropsPerRowAnyOrder(
					env.GetEnumerator("table"),
					propertyNames,
					new object[][] {
						new object[] {"G1", 10.5d, 10.5d, new SupportBean[] {s0, s3}, new SupportBean[] {s0, s3}},
						new object[] {"G2", 6d, 6d, new SupportBean[] {s1, s2}, new SupportBean[] {s1, s2}}
					});

				env.Milestone(0);

				env.SendEventBean(new SupportBean_S0(0, "G2"));
				EPAssertionUtil.AssertPropsPerRowAnyOrder(
					env.GetEnumerator("table"),
					propertyNames,
					new object[][] {
						new object[] {"G1", 10.5d, 10.5d, new SupportBean[] {s0, s3}, new SupportBean[] {s0, s3}},
						new object[] {"G2", null, 6d, null, new SupportBean[] {s1, s2}}
					});

				env.Milestone(1);

				env.SendEventBean(new SupportBean_S1(0, "G1"));
				EPAssertionUtil.AssertPropsPerRowAnyOrder(
					env.GetEnumerator("table"),
					propertyNames,
					new object[][] {
						new object[] {"G1", 10.5d, null, new SupportBean[] {s0, s3}, null},
						new object[] {"G2", null, 6d, null, new SupportBean[] {s1, s2}}
					});

				env.UndeployAll();
			}
		}

		private class InfraTableResetRowSumWTableAlias : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RunAssertionReset(env, "on SupportBean_S0 merge MyTable as mt when matched then update set mt.reset();\n");
			}
		}

		private class InfraTableResetRowSum : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RunAssertionReset(env, "on SupportBean_S0 merge MyTable when matched then update set asum.reset();\n");
			}
		}

		private static void RunAssertionReset(
			RegressionEnvironment env,
			string onMerge)
		{
			string epl = "@Name('table') create table MyTable(asum sum(int));\n" +
			             "into table MyTable select sum(IntPrimitive) as asum from SupportBean;\n" +
			             onMerge;
			env.CompileDeploy(epl);

			SendBeanAssertSum(env, 10, 10);
			SendBeanAssertSum(env, 11, 21);
			SendResetAssertSum(env);

			env.Milestone(0);

			AssertTableSum(env, null);
			SendBeanAssertSum(env, 20, 20);
			SendBeanAssertSum(env, 21, 41);
			SendResetAssertSum(env);

			env.Milestone(1);

			SendBeanAssertSum(env, 30, 30);

			env.UndeployAll();
		}

		private static SupportBean SendBean(
			RegressionEnvironment env,
			string theString,
			int intPrimitive)
		{
			SupportBean sb = new SupportBean(theString, intPrimitive);
			env.SendEventBean(sb);
			return sb;
		}

		private static void SendBeanAssertSum(
			RegressionEnvironment env,
			int intPrimitive,
			int expected)
		{
			env.SendEventBean(new SupportBean("E1", intPrimitive));
			AssertTableSum(env, expected);
		}

		private static void SendResetAssertSum(RegressionEnvironment env)
		{
			env.SendEventBean(new SupportBean_S0(0));
			AssertTableSum(env, null);
		}

		private static void AssertTableSum(
			RegressionEnvironment env,
			int? expected)
		{
			Assert.AreEqual(expected, env.GetEnumerator("table").Advance().Get("asum"));
		}
	}
} // end of namespace
