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
using System.Text;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.@event;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.filtersvcimpl;
using com.espertech.esper.runtime.@internal.kernel.service;
using com.espertech.esper.runtime.@internal.kernel.statement;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.filter
{
    public class SupportFilterHelper
    {
        public static void AssertFilterCount(
            RegressionEnvironment env,
            int count,
            string stmtName)
        {
            var statement = env.Statement(stmtName);
            if (statement == null) {
                Assert.Fail("Statement not found '" + stmtName + "'");
            }

            Assert.AreEqual(count, GetFilterCountAnyType(statement));
        }

        public static string GetFilterToString(
            RegressionEnvironment env,
            string name)
        {
            var statementSPI = (EPStatementSPI) env.Statement(name);
            var filterServiceSPI = (FilterServiceSPI) statementSPI.StatementContext.FilterService;
            var set = filterServiceSPI.Get(Collections.SingletonSet(statementSPI.StatementId));

            ISet<string> sorted = new SortedSet<string>();
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

        public static int GetFilterCount(
            EPStatement statement,
            string eventTypeName)
        {
            var statementSPI = (EPStatementSPI) statement;
            var typeId = SupportEventTypeHelper.GetTypeIdForName(statementSPI.StatementContext, eventTypeName);
            var filterServiceSPI = (FilterServiceSPI) statementSPI.StatementContext.FilterService;
            var set = filterServiceSPI.Get(Collections.SingletonSet(statementSPI.StatementId));
            foreach (var entry in set) {
                if (entry.Key.Equals(typeId)) {
                    var list = entry.Value.Get(statementSPI.StatementId);
                    return list.Count;
                }
            }

            return 0;
        }

        public static int GetFilterCountAnyType(EPStatement statement)
        {
            var statementSPI = (EPStatementSPI) statement;
            var filterServiceSPI = (FilterServiceSPI) statementSPI.StatementContext.FilterService;
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

        public static void AssertFilterTwo(
            EPStatement statement,
            string epl,
            string expressionOne,
            FilterOperator opOne,
            string expressionTwo,
            FilterOperator opTwo)
        {
            var statementSPI = (EPStatementSPI) statement;
            var multi = GetFilterMulti(statementSPI);
            Assert.AreEqual(2, multi.Length);
            Assert.AreEqual(opOne, multi[0].Op);
            Assert.AreEqual(expressionOne, multi[0].Name);
            Assert.AreEqual(opTwo, multi[1].Op);
            Assert.AreEqual(expressionTwo, multi[1].Name);
        }

        public static FilterItem GetFilterSingle(EPStatement statement)
        {
            var @params = GetFilterMulti((EPStatementSPI) statement);
            Assert.AreEqual(1, @params.Length);
            return @params[0];
        }

        public static FilterItem[] GetFilterMulti(EPStatementSPI statementSPI)
        {
            var statementId = statementSPI.StatementContext.StatementId;
            var filterServiceSPI = (FilterServiceSPI) statementSPI.StatementContext.FilterService;
            var set = filterServiceSPI.Get(Collections.SingletonSet(statementId));
            Assert.AreEqual(1, set.Count);
            var filters = set.Values.First();
            Assert.IsTrue(filters.ContainsKey(statementId));
            Assert.AreEqual(1, filters.Count);
            var paths = filters.Get(statementId);
            Assert.AreEqual(1, paths.Count);
            return paths.First();
        }

        public static void AssertFilterMulti(
            EPStatement statement,
            string eventTypeName,
            FilterItem[][] expected)
        {
            var spi = (EPStatementSPI) statement;
            var typeId = SupportEventTypeHelper.GetTypeIdForName(spi.StatementContext, eventTypeName);
            var statementId = spi.StatementContext.StatementId;
            var filterServiceSPI = (FilterServiceSPI) spi.StatementContext.FilterService;
            var set = filterServiceSPI.Get(Collections.SingletonSet(statementId));

            IDictionary<int, IList<FilterItem[]>> filters = null;
            foreach (var entry in set) {
                if (entry.Key.Equals(typeId)) {
                    filters = entry.Value;
                }
            }

            Assert.IsNotNull(filters);
            Assert.IsFalse(filters.IsEmpty());

            var @params = filters.Get(statementId);
            Assert.IsFalse(@params.IsEmpty());

            Comparison<FilterItem> comparator = (
                    o1,
                    o2) => o1.Name == o2.Name
                    ? o1.Op.CompareTo(o2.Op)
                    : string.Compare(o1.Name, o2.Name, StringComparison.Ordinal);

            var found = @params.ToArray();
            for (var i = 0; i < found.Length; i++) {
                Array.Sort(found[i], comparator);
            }

            for (var i = 0; i < expected.Length; i++) {
                Array.Sort(expected[i], comparator);
            }

            EPAssertionUtil.AssertEqualsAnyOrder(expected, found);
        }

        public static int GetFilterCountApprox(RegressionEnvironment env)
        {
            var spi = ((EPRuntimeSPI) env.Runtime).ServicesContext.FilterService;
            return spi.FilterCountApprox;
        }

        public static string GetFilterAll(EPRuntime epService)
        {
            var deployments = epService.DeploymentService.Deployments;
            ISet<int> statements = new HashSet<int>();
            foreach (var deployment in deployments) {
                var info = epService.DeploymentService.GetDeployment(deployment);
                foreach (var statement in info.Statements) {
                    var spi = (EPStatementSPI) statement;
                    statements.Add(spi.StatementId);
                }
            }

            var filterService = ((EPRuntimeSPI) epService).ServicesContext.FilterService;
            var pairs = filterService.Get(statements);
            return pairs.ToString();
        }
    }
} // end of namespace