///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.@join.queryplanbuild;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Plan to perform a nested iteration over child nodes.
    /// </summary>
    public class NestedIterationNodeForge : QueryPlanNodeForge
    {
        private readonly List<QueryPlanNodeForge> childNodes;
        private readonly int[] nestingOrder;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="nestingOrder">order of streams in nested iteration</param>
        public NestedIterationNodeForge(int[] nestingOrder)
        {
            this.nestingOrder = nestingOrder;
            childNodes = new List<QueryPlanNodeForge>();

            if (nestingOrder.Length == 0) {
                throw new ArgumentException("Invalid empty nesting order");
            }
        }

        /// <summary>
        ///     Returns list of child nodes.
        /// </summary>
        /// <returns>list of child nodes</returns>
        public List<QueryPlanNodeForge> ChildNodes => childNodes;

        /// <summary>
        ///     Adds a child node.
        /// </summary>
        /// <param name="childNode">is the child evaluation tree node to add</param>
        public void AddChildNode(QueryPlanNodeForge childNode)
        {
            childNodes.Add(childNode);
        }

        public override void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes)
        {
            foreach (var child in childNodes) {
                child.AddIndexes(usedIndexes);
            }
        }

        protected override void Print(IndentWriter indentWriter)
        {
            indentWriter.WriteLine("NestedIterationNode with nesting order " + nestingOrder.RenderAny());
            indentWriter.IncrIndent();
            foreach (var child in childNodes) {
                child.Print(indentWriter);
            }

            indentWriter.DecrIndent();
        }

        public override CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var childNodeArray = CodegenMakeableUtil.MakeArray(
                "childNodes", typeof(QueryPlanNode), childNodes.ToArray(),
                GetType(), parent, symbols, classScope);
            return NewInstance(typeof(NestedIterationNode), childNodeArray, Constant(nestingOrder));
        }

        public override void Accept(QueryPlanNodeForgeVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var child in childNodes) {
                child.Accept(visitor);
            }
        }
    }
} // end of namespace