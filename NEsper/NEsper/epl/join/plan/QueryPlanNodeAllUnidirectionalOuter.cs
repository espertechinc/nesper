///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.join.plan
{
    public class QueryPlanNodeAllUnidirectionalOuter : QueryPlanNode
    {
        private readonly int _streamNum;

        public QueryPlanNodeAllUnidirectionalOuter(int streamNum)
        {
            _streamNum = streamNum;
        }

        public override ExecNode MakeExec(
            string statementName,
            int statementId,
            Attribute[] annotations,
            IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream,
            EventType[] streamTypes,
            Viewable[] streamViews,
            HistoricalStreamIndexList[] historicalStreamIndexLists,
            VirtualDWView[] viewExternal,
            ILockable[] tableSecondaryIndexLocks)
        {
            return new ExecNodeAllUnidirectionalOuter(_streamNum, streamTypes.Length);
        }

        public override void AddIndexes(ISet<TableLookupIndexReqKey> usedIndexes)
        {
        }

        public override void Print(IndentWriter writer)
        {
            writer.WriteLine("Unidirectional Full-Outer-Join-All Execution");
        }
    }
} // end of namespace
