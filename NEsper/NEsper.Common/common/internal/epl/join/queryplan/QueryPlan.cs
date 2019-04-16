///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>Contains the query plan for all streams. </summary>
    public class QueryPlan
    {
        /// <summary>Ctor. </summary>
        /// <param name="indexSpecs">specs for indexes to create</param>
        /// <param name="execNodeSpecs">specs for execution nodes to create</param>
        public QueryPlan(
            QueryPlanIndex[] indexSpecs,
            QueryPlanNode[] execNodeSpecs)
        {
            IndexSpecs = indexSpecs;
            ExecNodeSpecs = execNodeSpecs;
        }

        /// <summary>Return index specs. </summary>
        /// <value>index specs</value>
        public QueryPlanIndex[] IndexSpecs { get; private set; }

        /// <summary>Return execution node specs. </summary>
        /// <value>execution node specs</value>
        public QueryPlanNode[] ExecNodeSpecs { get; private set; }

        public override String ToString()
        {
            return ToQueryPlan();
        }

        public String ToQueryPlan()
        {
            var buffer = new StringBuilder();
            buffer.Append("QueryPlanNode\n");
            buffer.Append(QueryPlanIndex.Print(IndexSpecs));
            buffer.Append(QueryPlanNode.Print(ExecNodeSpecs));
            return buffer.ToString();
        }
    }
}