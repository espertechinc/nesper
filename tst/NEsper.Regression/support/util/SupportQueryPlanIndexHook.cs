///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.join.support;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.util
{
	public class SupportQueryPlanIndexHook : QueryPlanIndexHook {

	    private static readonly IList<QueryPlanIndexDescSubquery> SUBQUERIES = new List<QueryPlanIndexDescSubquery>();
	    private static readonly IList<QueryPlanIndexDescOnExpr> ONEXPRS = new List<QueryPlanIndexDescOnExpr>();
	    private static readonly IList<QueryPlanIndexDescFAF> FAFSNAPSHOTS = new List<QueryPlanIndexDescFAF>();
	    private static readonly IList<QueryPlanForge> JOINS = new List<QueryPlanForge>();
	    private static readonly IList<QueryPlanIndexDescHistorical> HISTORICALS = new List<QueryPlanIndexDescHistorical>();

	    public static string ResetGetClassName() {
	        Reset();
	        return typeof(SupportQueryPlanIndexHook).FullName;
	    }

	    public static void Reset() {
	        SUBQUERIES.Clear();
	        ONEXPRS.Clear();
	        FAFSNAPSHOTS.Clear();
	        JOINS.Clear();
	        HISTORICALS.Clear();
	    }

	    public void Historical(QueryPlanIndexDescHistorical historicalPlan) {
	        HISTORICALS.Add(historicalPlan);
	    }

	    public void Subquery(QueryPlanIndexDescSubquery subquery) {
	        SUBQUERIES.Add(subquery);
	    }

	    public void InfraOnExpr(QueryPlanIndexDescOnExpr onexprdesc) {
	        ONEXPRS.Add(onexprdesc);
	    }

	    public void FireAndForget(QueryPlanIndexDescFAF fafdesc) {
	        FAFSNAPSHOTS.Add(fafdesc);
	    }

	    public void Join(QueryPlanForge join) {
	        JOINS.Add(join);
	    }

	    public static IList<QueryPlanIndexDescSubquery> GetAndResetSubqueries() {
	        IList<QueryPlanIndexDescSubquery> copy = new List<QueryPlanIndexDescSubquery>(SUBQUERIES);
	        Reset();
	        return copy;
	    }

	    public static QueryPlanIndexDescOnExpr GetAndResetOnExpr() {
	        var onexpr = ONEXPRS[0];
	        Reset();
	        return onexpr;
	    }

	    public static void AssertSubquery(QueryPlanIndexDescSubquery subquery, int subqueryNum, string tableName, string indexBackingClass) {
	        if (indexBackingClass == null) {
	            Assert.AreEqual(0, subquery.Tables.Length);
	            return;
	        }
	        Assert.AreEqual(tableName, subquery.Tables[0].IndexName);
	        Assert.AreEqual(subqueryNum, subquery.SubqueryNum);
	        Assert.AreEqual(indexBackingClass, subquery.Tables[0].IndexDesc);
	    }

	    public static void AssertSubqueryBackingAndReset(int subqueryNum, string tableName, string indexBackingClass) {
	        Assert.AreEqual(1, SUBQUERIES.Count);
	        var subquery = SUBQUERIES[0];
	        AssertSubquery(subquery, subqueryNum, tableName, indexBackingClass);
	        Reset();
	    }

	    public static QueryPlanIndexDescSubquery AssertSubqueryAndReset() {
	        Assert.AreEqual(1, SUBQUERIES.Count);
	        var subquery = SUBQUERIES[0];
	        Reset();
	        return subquery;
	    }

	    public static void AssertOnExprTableAndReset(string indexName, string indexDescription) {
	        Assert.AreEqual(1, ONEXPRS.Count);
	        var onexp = ONEXPRS[0];
	        if (indexDescription != null) {
	            Assert.AreEqual(indexDescription, onexp.Tables[0].IndexDesc);
	            Assert.AreEqual(indexName, onexp.Tables[0].IndexName); // can be null
	        } else {
	            Assert.IsNull(onexp.Tables);
	            Assert.IsNull(onexp.TableLookupStrategy);
	        }
	        Reset();
	    }

	    public static QueryPlanIndexDescOnExpr AssertOnExprAndReset() {
	        Assert.IsTrue(ONEXPRS.Count == 1);
	        var onexp = ONEXPRS[0];
	        Reset();
	        return onexp;
	    }

	    public static void AssertFAFAndReset(string tableName, string indexBackingClassStartsWith) {
	        Assert.IsTrue(FAFSNAPSHOTS.Count == 1);
	        var fafdesc = FAFSNAPSHOTS[0];
	        Assert.AreEqual(tableName, fafdesc.Tables[0].IndexName);
	        var name = fafdesc.Tables[0].IndexDesc;
	        if (indexBackingClassStartsWith != null) {
	            Assert.IsTrue(name.StartsWith(indexBackingClassStartsWith));
	        }
	        Reset();
	    }

	    public static void AssertJoinOneStreamAndReset(bool unique) {
	        Assert.IsTrue(JOINS.Count == 1);
	        var join = JOINS[0];
	        var first = join.IndexSpecs[1];
	        var firstName = first.Items.Keys.First();
	        var index = first.Items.Get(firstName);
	        Assert.AreEqual(unique, index.IsUnique);
	        Reset();
	    }

	    public static QueryPlanForge AssertJoinAndReset() {
	        Assert.IsTrue(JOINS.Count == 1);
	        var join = JOINS[0];
	        Reset();
	        return join;
	    }

	    public static void AssertJoinAllStreamsAndReset(bool unique) {
	        Assert.IsTrue(JOINS.Count == 1);
	        var join = JOINS[0];
	        foreach (var index in join.IndexSpecs) {
	            var firstName = index.Items.Keys.First();
	            var indexDesc = index.Items.Get(firstName);
	            Assert.AreEqual(unique, indexDesc.IsUnique);
	        }
	        Reset();
	    }

	    public static QueryPlanIndexDescHistorical AssertHistoricalAndReset() {
	        var item = HISTORICALS[0];
	        Reset();
	        return item;
	    }
	}
} // end of namespace
