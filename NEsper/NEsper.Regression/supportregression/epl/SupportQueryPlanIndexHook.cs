///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.util;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.epl
{
    public class SupportQueryPlanIndexHook : QueryPlanIndexHook
    {
        private static readonly IList<QueryPlanIndexDescSubquery> Subqueries = new List<QueryPlanIndexDescSubquery>();
        private static readonly IList<QueryPlanIndexDescOnExpr> OnExprs = new List<QueryPlanIndexDescOnExpr>();
        private static readonly IList<QueryPlanIndexDescFAF> FafSnapshots = new List<QueryPlanIndexDescFAF>();
        private static readonly IList<QueryPlan> Joins = new List<QueryPlan>();

        private static readonly IList<QueryPlanIndexDescHistorical> _historical =
            new List<QueryPlanIndexDescHistorical>();

        public static string ResetGetClassName()
        {
            Reset();
            return typeof (SupportQueryPlanIndexHook).FullName;
        }

        public static void Reset()
        {
            Subqueries.Clear();
            OnExprs.Clear();
            FafSnapshots.Clear();
            Joins.Clear();
            _historical.Clear();
        }

        public void Historical(QueryPlanIndexDescHistorical historicalPlan)
        {
            _historical.Add(historicalPlan);
        }

        public void Subquery(QueryPlanIndexDescSubquery subquery)
        {
            Subqueries.Add(subquery);
        }

        public void InfraOnExpr(QueryPlanIndexDescOnExpr onexprdesc)
        {
            OnExprs.Add(onexprdesc);
        }

        public void FireAndForget(QueryPlanIndexDescFAF fafdesc)
        {
            FafSnapshots.Add(fafdesc);
        }

        public void Join(QueryPlan join)
        {
            Joins.Add(join);
        }

        public static List<QueryPlanIndexDescSubquery> GetAndResetSubqueries()
        {
            List<QueryPlanIndexDescSubquery> copy = new List<QueryPlanIndexDescSubquery>(Subqueries);
            Reset();
            return copy;
        }

        public static QueryPlanIndexDescOnExpr GetAndResetOnExpr()
        {
            QueryPlanIndexDescOnExpr onexpr = OnExprs[0];
            Reset();
            return onexpr;
        }

        public static void AssertSubquery(
            QueryPlanIndexDescSubquery subquery,
            int subqueryNum,
            string tableName,
            string indexBackingClass)
        {
            if (indexBackingClass == null)
            {
                Assert.AreEqual(0, subquery.Tables.Length);
                return;
            }
            Assert.AreEqual(tableName, subquery.Tables[0].IndexName);
            Assert.AreEqual(subqueryNum, subquery.SubqueryNum);
            Assert.AreEqual(indexBackingClass, subquery.Tables[0].IndexDesc);
        }

        public static void AssertSubqueryBackingAndReset(int subqueryNum, string tableName, string indexBackingClass)
        {
            Assert.IsTrue(Subqueries.Count == 1);
            QueryPlanIndexDescSubquery subquery = Subqueries[0];
            AssertSubquery(subquery, subqueryNum, tableName, indexBackingClass);
            Reset();
        }

        public static QueryPlanIndexDescSubquery AssertSubqueryAndReset()
        {
            Assert.IsTrue(Subqueries.Count == 1);
            QueryPlanIndexDescSubquery subquery = Subqueries[0];
            Reset();
            return subquery;
        }

        public static void AssertOnExprTableAndReset(string indexName, string indexDescription)
        {
            Assert.IsTrue(OnExprs.Count == 1);
            QueryPlanIndexDescOnExpr onexp = OnExprs[0];
            if (indexDescription != null)
            {
                Assert.AreEqual(indexDescription, onexp.Tables[0].IndexDesc);
                Assert.AreEqual(indexName, onexp.Tables[0].IndexName); // can be null
            }
            else
            {
                Assert.IsNull(onexp.Tables);
                Assert.IsNull(indexDescription);
            }
            Reset();
        }

        public static QueryPlanIndexDescOnExpr AssertOnExprAndReset()
        {
            Assert.IsTrue(OnExprs.Count == 1);
            QueryPlanIndexDescOnExpr onexp = OnExprs[0];
            Reset();
            return onexp;
        }

        public static void AssertFAFAndReset(string tableName, string indexBackingClass)
        {
            Assert.IsTrue(FafSnapshots.Count == 1);
            QueryPlanIndexDescFAF fafdesc = FafSnapshots[0];
            Assert.AreEqual(tableName, fafdesc.Tables[0].IndexName);
            Assert.AreEqual(indexBackingClass, fafdesc.Tables[0].IndexDesc);
            Reset();
        }

        public static void AssertJoinOneStreamAndReset(bool unique)
        {
            Assert.IsTrue(Joins.Count == 1);
            QueryPlan join = Joins[0];
            QueryPlanIndex first = join.IndexSpecs[1];
            TableLookupIndexReqKey firstName = first.Items.Keys.First();
            QueryPlanIndexItem index = first.Items.Get(firstName);
            Assert.AreEqual(unique, index.IsUnique);
            Reset();
        }

        public static QueryPlan AssertJoinAndReset()
        {
            Assert.IsTrue(Joins.Count == 1);
            QueryPlan join = Joins[0];
            Reset();
            return join;
        }

        public static void AssertJoinAllStreamsAndReset(bool unique)
        {
            Assert.IsTrue(Joins.Count == 1);
            QueryPlan join = Joins[0];
            foreach (QueryPlanIndex index in join.IndexSpecs)
            {
                TableLookupIndexReqKey firstName = index.Items.Keys.First();
                QueryPlanIndexItem indexDesc = index.Items.Get(firstName);
                Assert.AreEqual(unique, indexDesc.IsUnique);
            }
            Reset();
        }

        public static QueryPlanIndexDescHistorical AssertHistoricalAndReset()
        {
            QueryPlanIndexDescHistorical item = _historical[0];
            Reset();
            return item;
        }
    }
} // end of namespace
