///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.util.LambdaAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
	public class ExprEnumToMap
	{
		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			WithEvent(execs);
			WithScalar(execs);
			WithInvalid(execs);
			return execs;
		}

		public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprEnumToMapInvalid());
			return execs;
		}

		public static IList<RegressionExecution> WithScalar(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprEnumToMapScalar());
			return execs;
		}

		public static IList<RegressionExecution> WithEvent(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprEnumToMapEvent());
			return execs;
		}

		internal class ExprEnumToMapEvent : RegressionExecution
		{
			public bool ExcludeWhenInstrumented()
			{
				return true;
			}

			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportBean_ST0_Container");
				builder.WithExpression(fields[0], "Contained.toMap(c => Id, d => P00)");
				builder.WithExpression(fields[1], "Contained.toMap((c, index) => Id || '_' || Convert.ToString(index), (d, index) => P00 + 10*index)");
				builder.WithExpression(
					fields[2],
					"Contained.toMap((c, index, size) => Id || '_' || Convert.ToString(index) || '_' || Convert.ToString(size), (d, index, size) => P00 + 10*index + 100*size)");

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(IDictionary<string, int>)));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E3,12", "E2,5"))
					.Verify("c0", val => CompareMap(val, "E1,E3,E2", 1, 12, 5))
					.Verify("c1", val => CompareMap(val, "E1_0,E3_1,E2_2", 1, 22, 25))
					.Verify("c2", val => CompareMap(val, "E1_0_3,E3_1_3,E2_2_3", 301, 322, 325));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E3,4", "E2,7", "E1,2"))
					.Verify("c0", val => CompareMap(val, "E1,E3,E2", 2, 4, 7))
					.Verify("c1", val => CompareMap(val, "E1_0,E3_1,E2_2,E1_3", 1, 14, 27, 32))
					.Verify("c2", val => CompareMap(val, "E1_0_4,E3_1_4,E2_2_4,E1_3_4", 401, 414, 427, 432));

				builder.WithAssertion(new SupportBean_ST0_Container(Collections.SingletonList(new SupportBean_ST0(null, null))))
					.Verify("c0", val => CompareMap(val, "E1,E2,E3", null, null, null))
					.Verify("c1", val => CompareMap(val, "E1,E2,E3", null, null, null))
					.Verify("c2", val => CompareMap(val, "E1,E2,E3", null, null, null));

				builder.Run(env);
			}
		}

		internal class ExprEnumToMapScalar : RegressionExecution
		{
			public bool ExcludeWhenInstrumented()
			{
				return true;
			}

			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Strvals.toMap(k => k, v => extractNum(v))");
				builder.WithExpression(fields[1], "Strvals.toMap((k, i) => k || '_' || Convert.ToString(i), (v, idx) => extractNum(v) + 10*idx)");
				builder.WithExpression(
					fields[2],
					"Strvals.toMap((k, i, s) => k || '_' || Convert.ToString(i) || '_' || Convert.ToString(s), (v, idx, sz) => extractNum(v) + 10*idx + 100*sz)");

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(IDictionary<string, int>)));

				builder.WithAssertion(SupportCollection.MakeString("E2,E1,E3"))
					.Verify("c0", val => CompareMap(val, "E1,E2,E3", 1, 2, 3))
					.Verify("c1", val => CompareMap(val, "E1_1,E2_0,E3_2", 11, 2, 23))
					.Verify("c2", val => CompareMap(val, "E1_1_3,E2_0_3,E3_2_3", 311, 302, 323));

				builder.WithAssertion(SupportCollection.MakeString("E1"))
					.Verify("c0", val => CompareMap(val, "E1", 1))
					.Verify("c1", val => CompareMap(val, "E1_0", 1))
					.Verify("c2", val => CompareMap(val, "E1_0_1", 101));

				builder.WithAssertion(SupportCollection.MakeString(null))
					.Verify("c0", Assert.IsNull)
					.Verify("c1", Assert.IsNull)
					.Verify("c2", Assert.IsNull);

				builder.WithAssertion(SupportCollection.MakeString(""))
					.Verify("c0", val => CompareMap(val, ""))
					.Verify("c1", val => CompareMap(val, ""))
					.Verify("c2", val => CompareMap(val, ""));

				builder.Run(env);
			}
		}

		internal class ExprEnumToMapInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "select Strvals.toMap(k => k, (v, i) => extractNum(v)) from SupportCollection";
				TryInvalidCompile(
					env,
					epl,
					"Failed to validate select-clause expression 'Strvals.toMap(,)': Parameters mismatch for enumeration method 'toMap', the method requires a lambda expression providing key-selector and a lambda expression providing value-selector, but receives a lambda expression and a 2-parameter lambda expression");
			}
		}

		private static void CompareMap(
			object received,
			string keyCSV,
			params object[] values)
		{
			var keys = string.IsNullOrWhiteSpace(keyCSV) ? new string[0] : keyCSV.SplitCsv();
			var receivedDictionary = received.AsObjectDictionary();
			EPAssertionUtil.AssertPropsMap(receivedDictionary, keys, values);
		}
	}
} // end of namespace
