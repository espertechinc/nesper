///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.@event;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.filtersvcimpl;
using com.espertech.esper.runtime.@internal.kernel.service;
using com.espertech.esper.runtime.@internal.kernel.statement;

// assertTrue
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.filter
{
	public class SupportFilterServiceHelper
	{

		public static void AssertFilterSvcCount(
			RegressionEnvironment env,
			int count,
			string stmtName)
		{
			env.AssertStatement(
				stmtName,
				statement => ClassicAssert.AreEqual(count, SupportFilterServiceHelper.GetFilterSvcCountAnyType(statement)));
		}

		public static string GetFilterSvcToString(
			RegressionEnvironment env,
			string statementName)
		{
			var statementSPI = (EPStatementSPI)env.Statement(statementName);
			var filterServiceSPI = (FilterServiceSPI)statementSPI.StatementContext.FilterService;
			var set = filterServiceSPI.Get(Collections.SingletonSet(statementSPI.StatementId));

			var sorted = new SortedSet<string>();
			foreach (var typeEntry in set) {
				foreach (var filterItems in typeEntry.Value.Values) {
					foreach (var itemArray in filterItems) {
						var type = SupportEventTypeHelper.GetEventTypeForTypeId(
							statementSPI.StatementContext,
							typeEntry.Key);

						var builder = new StringBuilder();
						builder.Append(type.Name).Append("(");
						var delimiter = "";
						foreach (var item in itemArray) {
							builder.Append(delimiter);
							builder.Append(item.Name);
							builder.Append(item.Op.GetTextualOp());
							builder.Append(item.OptionalValue);
							delimiter = ",";
						}

						builder.Append(")");
						sorted.Add(builder.ToString());
					}
				}
			}

			return sorted.RenderAny();
		}

		public static int GetFilterSvcCount(
			EPStatement statement,
			string eventTypeName)
		{
			var statementSPI = (EPStatementSPI)statement;
			var typeId = SupportEventTypeHelper.GetTypeIdForName(statementSPI.StatementContext, eventTypeName);
			var filterServiceSPI = (FilterServiceSPI)statementSPI.StatementContext.FilterService;
			var set = filterServiceSPI.Get(Collections.SingletonSet(statementSPI.StatementId));
			foreach (var entry in set) {
				if (entry.Key.Equals(typeId)) {
					var list = entry.Value.Get(statementSPI.StatementId);
					return list.Count;
				}
			}

			return 0;
		}

		public static int GetFilterSvcCountAnyType(EPStatement statement)
		{
			var statementSPI = (EPStatementSPI)statement;
			var filterServiceSPI = (FilterServiceSPI)statementSPI.StatementContext.FilterService;
			var set = filterServiceSPI.Get(Collections.SingletonSet(statementSPI.StatementId));
			var total = 0;
			foreach (var entry in set) {
				var list = entry.Value.Get(statementSPI.StatementId);
				if (list != null) {
					total += list.Count;
				}
			}

			return total;
		}

		public static void AssertFilterSvcTwo(
			RegressionEnvironment env,
			string statementName,
			string expressionOne,
			FilterOperator opOne,
			string expressionTwo,
			FilterOperator opTwo)
		{
			env.AssertStatement(
				statementName,
				statement => {
					var statementSPI = (EPStatementSPI)statement;
					var multi = GetFilterSvcMultiAssertNonEmpty(statementSPI);
					ClassicAssert.AreEqual(2, multi.Length);
					ClassicAssert.AreEqual(opOne, multi[0].Op);
					ClassicAssert.AreEqual(expressionOne, multi[0].Name);
					ClassicAssert.AreEqual(opTwo, multi[1].Op);
					ClassicAssert.AreEqual(expressionTwo, multi[1].Name);
				});
		}

		public static FilterItem GetFilterSvcSingle(EPStatement statement)
		{
			var @params = GetFilterSvcMultiAssertNonEmpty((EPStatementSPI)statement);
			ClassicAssert.AreEqual(1, @params.Length);
			return @params[0];
		}

		public static void AssertFilterSvcSingle(
			RegressionEnvironment env,
			string stmtName,
			string expression,
			FilterOperator op)
		{
			env.AssertStatement(
				stmtName,
				statement => {
					var statementSPI = (EPStatementSPI)statement;
					var param = GetFilterSvcSingle(statementSPI);
					ClassicAssert.AreEqual(op, param.Op);
					ClassicAssert.AreEqual(expression, param.Name);
				});
		}

		public static FilterItem[] GetFilterSvcMultiAssertNonEmpty(EPStatement statementSPI)
		{
			var ctx = ((EPStatementSPI)statementSPI).StatementContext;
			var statementId = ctx.StatementId;
			var filterServiceSPI = (FilterServiceSPI)ctx.FilterService;
			var set =
				filterServiceSPI.Get(Collections.SingletonSet(statementId));
			ClassicAssert.AreEqual(1, set.Count);
			var filters = set.Values.First();
			ClassicAssert.IsTrue(filters.ContainsKey(statementId));
			ClassicAssert.AreEqual(1, filters.Count);
			var paths = filters.Get(statementId);
			ClassicAssert.AreEqual(1, paths.Count);
			return paths.First();
		}

		public static void AssertFilterSvcByTypeSingle(
			RegressionEnvironment env,
			string statementName,
			string eventTypeName,
			FilterItem expected)
		{
			env.AssertStatement(
				statementName,
				statement => {
					var filtersAll = GetFilterSvcMultiAssertNonEmpty(statement, eventTypeName);
					ClassicAssert.AreEqual(1, filtersAll.Length);
					var filters = filtersAll[0];
					ClassicAssert.AreEqual(1, filters.Length);
					ClassicAssert.AreEqual(expected, filters[0]);
				});
		}

		public static void AssertFilterSvcByTypeMulti(
			RegressionEnvironment env,
			string statementName,
			string eventTypeName,
			FilterItem[][] expected)
		{
			Comparison<FilterItem> comparator = (
				o1,
                o2) => o1.Name == o2.Name
                ? o1.Op.CompareTo(o2.Op)
                : string.Compare(o1.Name, o2.Name, StringComparison.Ordinal);

			env.AssertStatement(
				statementName,
				statement => {
					var found = GetFilterSvcMultiAssertNonEmpty(statement, eventTypeName);

					for (var i = 0; i < found.Length; i++) {
						Array.Sort(found[i], comparator);
					}

					for (var i = 0; i < expected.Length; i++) {
						Array.Sort(expected[i], comparator);
					}

					EPAssertionUtil.AssertEqualsAnyOrder(expected, found);
				});
		}

		public static int GetFilterSvcCountApprox(RegressionEnvironment env)
		{
			var runtimeSpi = (EPRuntimeSPI)env.Runtime;
			var spi = (FilterServiceSPI)runtimeSpi.ServicesContext.FilterService;
			return spi.FilterCountApprox;
		}

		public static IDictionary<EventTypeIdPair, IDictionary<int, IList<FilterItem[]>>> GetFilterSvcAllStmt(
			EPRuntime runtime)
		{
			var deployments = runtime.DeploymentService.Deployments;
			ISet<int> statements = new HashSet<int>();
			EPStatementSPI statementSPI = null;
			foreach (var deployment in deployments) {
				var info = runtime.DeploymentService.GetDeployment(deployment);
				foreach (var statement in info.Statements) {
					statementSPI = (EPStatementSPI)statement;
					statements.Add(statementSPI.StatementId);
				}
			}

			if (statementSPI == null) {
				throw new IllegalStateException("Empty statements");
			}

			var runtimeSpi = ((EPRuntimeSPI)runtime);
			var filterService = (FilterServiceSPI)runtimeSpi.ServicesContext.FilterService;
			return filterService.Get(statements);
		}

		public static IDictionary<int, IList<FilterItem[]>> GetFilterSvcAllStmtForType(
			EPRuntime runtime,
			string eventTypeName)
		{
			var pairs = GetFilterSvcAllStmt(runtime);
			var eventType = runtime.EventTypeService.GetBusEventType(eventTypeName);
			var typeId = eventType.Metadata.EventTypeIdPair;
			return pairs.Get(typeId);
		}

		public static IDictionary<string, FilterItem> GetFilterSvcAllStmtForTypeSingleFilter(
			EPRuntime runtime,
			string eventTypeName)
		{
			var pairs = GetFilterSvcAllStmt(runtime);

			var eventType = runtime.EventTypeService.GetBusEventType(eventTypeName);
			var typeId = eventType.Metadata.EventTypeIdPair;
			var filters = pairs.Get(typeId);

			var deployments = runtime.DeploymentService.Deployments;
			IDictionary<string, FilterItem> statements = new Dictionary<string, FilterItem>();
			foreach (var deployment in deployments) {
				var info = runtime.DeploymentService.GetDeployment(deployment);
				foreach (var statement in info.Statements) {
					var list = filters.Get(((EPStatementSPI)statement).StatementId);
					if (list != null) {
						ClassicAssert.AreEqual(1, list.Count);
						ClassicAssert.AreEqual(1, list[0].Length);
						statements.Put(statement.Name, list[0][0]);
					}
				}
			}

			return statements;
		}

		public static IDictionary<string, FilterItem[]> GetFilterSvcAllStmtForTypeMulti(
			EPRuntime runtime,
			string eventTypeName)
		{
			var pairs = GetFilterSvcAllStmt(runtime);

			var eventType = runtime.EventTypeService.GetBusEventType(eventTypeName);
			var typeId = eventType.Metadata.EventTypeIdPair;
			var filters = pairs.Get(typeId);

			var deployments = runtime.DeploymentService.Deployments;
			IDictionary<string, FilterItem[]> statements = new Dictionary<string, FilterItem[]>();
			foreach (var deployment in deployments) {
				var info = runtime.DeploymentService.GetDeployment(deployment);
				foreach (var statement in info.Statements) {
					var list = filters.Get(((EPStatementSPI)statement).StatementId);
					if (list != null) {
						ClassicAssert.AreEqual(1, list.Count);
						statements.Put(statement.Name, list[0]);
					}
				}
			}

			return statements;
		}

		public static void AssertFilterSvcEmpty(
			RegressionEnvironment env,
			string statementName,
			string eventTypeName)
		{
			env.AssertStatement(
				statementName,
				statement => {
					var filters = GetFilterSvcMultiAssertNonEmpty(statement, eventTypeName);
					ClassicAssert.AreEqual(1, filters.Length);
					ClassicAssert.AreEqual(0, filters[0].Length);
				});
		}

		public static void AssertFilterSvcNone(
			RegressionEnvironment env,
			string statementName,
			string eventTypeName)
		{
			env.AssertStatement(
				statementName,
				statement => {
					var set = GetFilterSvcForStatement(statement);
					var typeId = SupportEventTypeHelper.GetTypeIdForName(
						((EPStatementSPI)statement).StatementContext,
						eventTypeName);
					ClassicAssert.IsFalse(set.ContainsKey(typeId));
				});
		}

		public static FilterItem[][] GetFilterSvcMultiAssertNonEmpty(
			EPStatement statement,
			string eventTypeName)
		{
			var set = GetFilterSvcForStatement(statement);
			var spi = (EPStatementSPI)statement;
			var typeId = SupportEventTypeHelper.GetTypeIdForName(spi.StatementContext, eventTypeName);

			IDictionary<int, IList<FilterItem[]>> filters = null;
			foreach (var entry in set) {
				if (entry.Key.Equals(typeId)) {
					filters = entry.Value;
				}
			}

			ClassicAssert.IsNotNull(filters);
			ClassicAssert.IsFalse(filters.IsEmpty());

			var @params = filters.Get(spi.StatementId);
			ClassicAssert.IsFalse(@params.IsEmpty());

			return @params.ToArray();
		}

		public static void AssertFilterSvcMultiSameIndexDepthOne(
			RegressionEnvironment env,
			string stmtName,
			string eventType,
			int numEntries,
			string expression,
			FilterOperator @operator)
		{
			env.AssertStatement(
				stmtName,
				stmt => {
					var items = GetFilterSvcMultiAssertNonEmpty(stmt, eventType);
					ClassicAssert.AreEqual(numEntries, items.Length);
					for (var i = 0; i < numEntries; i++) {
						var entries = items[i];
						ClassicAssert.AreEqual(1, entries.Length);
						var item = entries[0];
						ClassicAssert.AreEqual(expression, item.Name);
						ClassicAssert.AreEqual(@operator, item.Op);
						ClassicAssert.AreSame(items[0][0].Index, item.Index);
					}
				});
		}

		public static void AssertFilterSvcMultiSameIndexDepthOne(
			IDictionary<int, IList<FilterItem[]>> filters,
			int numEntries,
			string expression,
			FilterOperator @operator)
		{
			ClassicAssert.AreEqual(numEntries, filters.Count);
			FilterItem first = null;
			foreach (var stmtEntry in filters) {
				var entriesStmt = stmtEntry.Value.ToArray();
				ClassicAssert.AreEqual(1, entriesStmt.Length);
				var entries = entriesStmt[0];
				ClassicAssert.AreEqual(1, entries.Length);
				var item = entries[0];
				ClassicAssert.AreEqual(expression, item.Name);
				ClassicAssert.AreEqual(@operator, item.Op);
				if (first == null) {
					first = item;
				}

				ClassicAssert.AreSame(first.Index, item.Index);
			}
		}

		public static IDictionary<EventTypeIdPair, IDictionary<int, IList<FilterItem[]>>> GetFilterSvcForStatement(
			EPStatement statement)
		{
			var spi = (EPStatementSPI)statement;
			var statementId = spi.StatementContext.StatementId;
			var filterServiceSPI = (FilterServiceSPI)spi.StatementContext.FilterService;
			return filterServiceSPI.Get(Collections.SingletonSet(statementId));
		}
	}
} // end of namespace
