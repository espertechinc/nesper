///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Specification node for a query execution plan to be extended by specific execution specification nodes.
    /// </summary>
    public abstract class QueryPlanNode
    {
        /// <summary>
        /// Make execution node from this specification.
        /// </summary>
        /// <param name="statementName">the statement name</param>
        /// <param name="statementId">the statement id</param>
        /// <param name="annotations">annotations</param>
        /// <param name="indexesPerStream">tables build for each stream</param>
        /// <param name="streamTypes">event type of each stream</param>
        /// <param name="streamViews">viewable per stream for access to historical data</param>
        /// <param name="historicalStreamIndexLists">index management for historical streams</param>
        /// <param name="viewExternal">@return execution node matching spec</param>
        /// <param name="tableSecondaryIndexLocks">The table secondary index locks.</param>
        /// <returns></returns>
        public abstract ExecNode MakeExec(string statementName, int statementId, Attribute[] annotations, IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream, EventType[] streamTypes, Viewable[] streamViews, HistoricalStreamIndexList[] historicalStreamIndexLists, VirtualDWView[] viewExternal, ILockable[] tableSecondaryIndexLocks);

        public abstract void AddIndexes(ISet<TableLookupIndexReqKey> usedIndexes);

        /// <summary>Print a long readable format of the query node to the supplied PrintWriter. </summary>
        /// <param name="writer">is the indentation writer to print to</param>
        public abstract void Print(IndentWriter writer);

        /// <summary>Print in readable format the execution plan spec. </summary>
        /// <param name="planNodeSpecs">plans to print</param>
        /// <returns>readable text with plans</returns>
        public static String Print(QueryPlanNode[] planNodeSpecs)
        {
            var buffer = new StringBuilder();
            buffer.Append("QueryPlanNode[]\n");

            for (int i = 0; i < planNodeSpecs.Length; i++)
            {
                buffer.Append("  node spec " + i + " :\n");

                var writer = new StringWriter();
                var indentWriter = new IndentWriter(writer, 4, 2);

                if (planNodeSpecs[i] != null)
                {
                    planNodeSpecs[i].Print(indentWriter);
                }
                else
                {
                    indentWriter.WriteLine("no plan (historical)");
                }

                buffer.Append(writer.ToString());
            }

            return buffer.ToString();
        }
    }
}
