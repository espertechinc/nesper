///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.LambdaAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
	public class ExprEnumGroupBy
	{
		public static ICollection<RegressionExecution> Executions()
		{
			var execs = new List<RegressionExecution>();
			WithOneParamEvent(execs);
			WithOneParamScalar(execs);
			WithTwoParamEvent(execs);
			WithTwoParamScalar(execs);
			return execs;
		}

		public static IList<RegressionExecution> WithTwoParamScalar(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprEnumGroupByTwoParamScalar());
			return execs;
		}

		public static IList<RegressionExecution> WithTwoParamEvent(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprEnumGroupByTwoParamEvent());
			return execs;
		}

		public static IList<RegressionExecution> WithOneParamScalar(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprEnumGroupByOneParamScalar());
			return execs;
		}

		public static IList<RegressionExecution> WithOneParamEvent(IList<RegressionExecution> execs = null)
		{
			execs ??= new List<RegressionExecution>();
			execs.Add(new ExprEnumGroupByOneParamEvent());
			return execs;
		}

		internal class ExprEnumGroupByOneParamEvent : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2,c3,c4".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
				builder.WithExpression(fields[0], "Contained.groupBy(c => Id)");
				builder.WithExpression(fields[1], "Contained.groupBy((c, i) => Id || '_' || Convert.ToString(i))");
				builder.WithExpression(fields[2], "Contained.groupBy((c, i, s) => Id || '_' || ConvertInfraTableInvalid.ToString(i) || '_' || Convert.ToString(s))");
				builder.WithExpression(fields[3], "Contained.groupBy(c => null)");
				builder.WithExpression(fields[4], "Contained.groupBy((c, i) => case when i > 1 then null else id end)");
				
				var inner = typeof(ICollection<SupportBean_ST0>);
				var mapOfString = typeof(IDictionary<string, ICollection<SupportBean_ST0>>);
				var mapOfObject = typeof(IDictionary<object, ICollection<SupportBean_ST0>>);
				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypes(
					stmt.EventType, fields, new []{ mapOfString, mapOfString, mapOfString, mapOfObject, mapOfString }));

				EPAssertionUtil.AssertionCollectionValueString extractorEvents = collectionItem => {
					var p00 = ((SupportBean_ST0) collectionItem).P00;
					return Convert.ToString(p00);
				};

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E1,2", "E2,5"))
					.Verify("c0", val => CompareMaps(val, "E1,E2", new[] {"1,2", "5"}, extractorEvents))
					.Verify("c1", val => CompareMaps(val, "E1_0,E1_1,E2_2", new[] {"1", "2", "5"}, extractorEvents))
					.Verify("c2", val => CompareMaps(val, "E1_0_3,E1_1_3,E2_2_3", new[] {"1", "2", "5"}, extractorEvents))
					.Verify("c3", val => CompareMaps(val, "null", new[]{"1,2,5"}, extractorEvents))
					.Verify("c4", val => CompareMaps(val, "E1,null", new[]{"1,2", "5"}, extractorEvents));

				var assertionNull = builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull());
				foreach (var field in fields) {
					assertionNull.Verify(field, Assert.IsNull);
				}

				var assertionEmpty = builder.WithAssertion(SupportBean_ST0_Container.Make2Value());
				foreach (var field in fields) {
					assertionEmpty.Verify(field, val => CompareMaps(val, "", new string[0], extractorEvents));
				}

				builder.Run(env);
			}
		}

		internal class ExprEnumGroupByOneParamScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2".SplitCsv();
				var builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Strvals.groupBy(c => extractAfterUnderscore(c))");
				builder.WithExpression(fields[1], "Strvals.groupBy((c, i) => extractAfterUnderscore(c) || '_' || Convert.ToString(i))");
				builder.WithExpression(
					fields[2],
					"Strvals.groupBy((c, i, s) => extractAfterUnderscore(c) || '_' || Convert.ToString(i) || '_' || Convert.ToString(s))");

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(IDictionary<string, ICollection<string>>)));

				builder.WithAssertion(SupportCollection.MakeString("E1_2,E2_1,E3_2"))
					.Verify("c0", val => CompareMaps(val, "2,1", new[] {"E1_2,E3_2", "E2_1"}, GetExtractorScalar()))
					.Verify("c1", val => CompareMaps(val, "2_0,1_1,2_2", new[] {"E1_2", "E2_1", "E3_2"}, GetExtractorScalar()))
					.Verify("c2", val => CompareMaps(val, "2_0_3,1_1_3,2_2_3", new[] {"E1_2", "E2_1", "E3_2"}, GetExtractorScalar()));

				var assertionNull = builder.WithAssertion(SupportCollection.MakeString(null));
				foreach (var field in fields) {
					assertionNull.Verify(field, Assert.IsNull);
				}

				var assertionEmpty = builder.WithAssertion(SupportCollection.MakeString(""));
				foreach (var field in fields) {
					assertionEmpty.Verify(field, val => CompareMaps(val, "", new string[0], GetExtractorScalar()));
				}

				builder.Run(env);
			}
		}

		internal class ExprEnumGroupByTwoParamEvent : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
				builder.WithExpression(fields[0], "Contained.groupBy(k => Id, v => P00)");
				builder.WithExpression(fields[1], "Contained.groupBy((k, i) => Id || '_' || Convert.ToString(i), (v, i) => P00 + i*10)");
				builder.WithExpression(
					fields[2],
					"Contained.groupBy((k, i, s) => Id || '_' || Convert.ToString(i) || '_' || Convert.ToString(s), (v, i, s) => P00 + i*10 + s*100)");

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(IDictionary<string, ICollection<int>>)));

				EPAssertionUtil.AssertionCollectionValueString extractor = collectionItem => {
					var p00 = collectionItem.AsInt32();
					return Convert.ToString(p00);
				};

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E1,2", "E2,5"))
					.Verify("c0", val => CompareMaps(val, "E1,E2", new[] {"1,2", "5"}, extractor))
					.Verify("c1", val => CompareMaps(val, "E1_0,E1_1,E2_2", new[] {"1", "12", "25"}, extractor))
					.Verify("c2", val => CompareMaps(val, "E1_0_3,E1_1_3,E2_2_3", new[] {"301", "312", "325"}, extractor));

				var assertionNull = builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull());
				foreach (var field in fields) {
					assertionNull.Verify(field, Assert.IsNull);
				}

				var assertionEmpty = builder.WithAssertion(SupportBean_ST0_Container.Make2Value());
				foreach (var field in fields) {
					assertionEmpty.Verify(field, val => CompareMaps(val, "", new string[0], GetExtractorScalar()));
				}

				builder.Run(env);
			}
		}

		internal class ExprEnumGroupByTwoParamScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2".SplitCsv();
				var builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Strvals.groupBy(k => extractAfterUnderscore(k), v => v)");
				builder.WithExpression(
					fields[1],
					"Strvals.groupBy((k, i) => extractAfterUnderscore(k) || '_' || Convert.ToString(i), (v, i) => v || '_' || Convert.ToString(i))");
				builder.WithExpression(
					fields[2],
					"Strvals.groupBy((k, i, s) => extractAfterUnderscore(k) || '_' || Convert.ToString(i) || '_' || Convert.ToString(s), (v, i, s) => v || '_' || Convert.ToString(i) || '_' || Convert.ToString(s))");

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(IDictionary<string, ICollection<string>>)));

				builder.WithAssertion(SupportCollection.MakeString("E1_2,E2_1,E3_2"))
					.Verify("c0", val => CompareMaps(val, "2,1", new[] {"E1_2,E3_2", "E2_1"}, GetExtractorScalar()))
					.Verify("c1", val => CompareMaps(val, "2_0,1_1,2_2", new[] {"E1_2_0", "E2_1_1", "E3_2_2"}, GetExtractorScalar()))
					.Verify("c2", val => CompareMaps(val, "2_0_3,1_1_3,2_2_3", new[] {"E1_2_0_3", "E2_1_1_3", "E3_2_2_3"}, GetExtractorScalar()));

				var assertionNull = builder.WithAssertion(SupportCollection.MakeString(null));
				foreach (var field in fields) {
					assertionNull.Verify(field, Assert.IsNull);
				}

				var assertionEmpty = builder.WithAssertion(SupportCollection.MakeString(""));
				foreach (var field in fields) {
					assertionEmpty.Verify(field, val => CompareMaps(val, "", new string[0], GetExtractorScalar()));
				}

				builder.Run(env);
			}
		}

		public static string ExtractAfterUnderscore(string @string)
		{
			var indexUnderscore = @string.IndexOf('_');
			if (indexUnderscore == -1) {
				Assert.Fail();
			}

			return @string.Substring(indexUnderscore + 1);
		}

		private static EPAssertionUtil.AssertionCollectionValueString GetExtractorScalar()
		{
			return collectionItem => collectionItem.ToString();
		}

		private static void CompareMaps(
			object val,
			string keyCSV,
			string[] values,
			EPAssertionUtil.AssertionCollectionValueString extractorEvents)
		{
			var keys = string.IsNullOrWhiteSpace(keyCSV) ? new string[0] : keyCSV.SplitCsv().Select(_ => _ == "null" ? null : _).ToArray();
			var dictionary = val.UnwrapDictionary();
			EPAssertionUtil.AssertMapOfCollection(dictionary, keys, values, extractorEvents);
		}
	}
} // end of namespace
