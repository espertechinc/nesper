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
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>Plan to perform a nested iteration over child nodes. </summary>
    public class NestedIterationNode : QueryPlanNode
    {
        private readonly List<QueryPlanNode> _childNodes;
        private readonly int[] _nestingOrder;

        /// <summary>Ctor. </summary>
        /// <param name="nestingOrder">order of streams in nested iteration</param>
        public NestedIterationNode(int[] nestingOrder)
        {
            _nestingOrder = nestingOrder;
            _childNodes = new List<QueryPlanNode>();

            if (nestingOrder.Length == 0)
            {
                throw new ArgumentException("Invalid empty nesting order");
            }
        }

        /// <summary>Returns list of child nodes. </summary>
        /// <value>list of child nodes</value>
        protected internal List<QueryPlanNode> ChildNodes
        {
            get { return _childNodes; }
        }

        /// <summary>Adds a child node. </summary>
        /// <param name="childNode">is the child evaluation tree node to add</param>
        public void AddChildNode(QueryPlanNode childNode)
        {
            _childNodes.Add(childNode);
        }

        public override ExecNode MakeExec(string statementName, int statementId, Attribute[] annotations, IDictionary<TableLookupIndexReqKey, EventTable>[] indexPerStream, EventType[] streamTypes, Viewable[] streamViews, HistoricalStreamIndexList[] historicalStreamIndexList, VirtualDWView[] viewExternal, ILockable[] tableSecondaryIndexLocks)
        {
            if (_childNodes.IsEmpty())
            {
                throw new IllegalStateException("Zero child nodes for nested iteration");
            }

            var execNode = new NestedIterationExecNode(_nestingOrder);
            foreach (QueryPlanNode child in _childNodes)
            {
                ExecNode childExec = child.MakeExec(
                    statementName, statementId, annotations, indexPerStream, streamTypes, streamViews,
                    historicalStreamIndexList, viewExternal, tableSecondaryIndexLocks);
                execNode.AddChildNode(childExec);
            }
            return execNode;
        }

        public override void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes)
        {
            foreach (QueryPlanNode child in _childNodes)
            {
                child.AddIndexes(usedIndexes);
            }
        }

        protected internal override void Print(IndentWriter indentWriter)
        {
            indentWriter.WriteLine("NestedIterationNode with nesting order " + _nestingOrder.Render());
            indentWriter.IncrIndent();
            foreach (QueryPlanNode child in _childNodes)
            {
                child.Print(indentWriter);
            }
            indentWriter.DecrIndent();
        }
    }
}