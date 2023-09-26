///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    /// <summary>
    ///     Represents the factory of a node in a tree responsible for assembling outer join query results.
    ///     <para />
    ///     The tree of factory nodes is double-linked, child nodes know each parent and parent know all child nodes.
    /// </summary>
    public abstract class BaseAssemblyNodeFactory : CodegenMakeable
    {
        /// <summary>
        ///     Child nodes.
        /// </summary>
        internal readonly IList<BaseAssemblyNodeFactory> childNodes;

        /// <summary>
        ///     Number of streams in statement.
        /// </summary>
        internal readonly int numStreams;

        /// <summary>
        ///     Stream number.
        /// </summary>
        internal readonly int streamNum;

        /// <summary>
        ///     Parent node.
        /// </summary>
        internal BaseAssemblyNodeFactory parentNode;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">stream number of the event stream that this node assembles results for.</param>
        /// <param name="numStreams">number of streams</param>
        protected BaseAssemblyNodeFactory(
            int streamNum,
            int numStreams)
        {
            this.streamNum = streamNum;
            this.numStreams = numStreams;
            childNodes = new List<BaseAssemblyNodeFactory>(4);
        }

        /// <summary>
        ///     Set parent node.
        /// </summary>
        /// <value>parent node</value>
        public BaseAssemblyNodeFactory Parent {
            get => parentNode;
            set => parentNode = value;
        }

        public BaseAssemblyNodeFactory ParentNode {
            get => parentNode;
            set => parentNode = value;
        }

        /// <summary>
        ///     Returns the stream number.
        /// </summary>
        /// <value>stream number</value>
        public int StreamNum => streamNum;

        /// <summary>
        ///     Returns child nodes.
        /// </summary>
        /// <returns>child nodes</returns>
        public IList<BaseAssemblyNodeFactory> ChildNodes => childNodes;

        public abstract CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbolsArg,
            CodegenClassScope classScope);

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenSymbolProvider symbolsArg,
            CodegenClassScope classScope)
        {
            return Make(parent, (SAIFFInitializeSymbol)symbolsArg, classScope);
        }

        public abstract BaseAssemblyNode MakeAssemblerUnassociated();

        /// <summary>
        ///     Output this node using writer, not outputting child nodes.
        /// </summary>
        /// <param name="indentWriter">to use for output</param>
        public abstract void Print(IndentWriter indentWriter);

        /// <summary>
        ///     Add a child node.
        /// </summary>
        /// <param name="childNode">to add</param>
        public virtual void AddChild(BaseAssemblyNodeFactory childNode)
        {
            childNode.parentNode = this;
            childNodes.Add(childNode);
        }

        /// <summary>
        ///     Output this node and all descendent nodes using writer, outputting child nodes.
        /// </summary>
        /// <param name="indentWriter">to output to</param>
        public void PrintDescendends(IndentWriter indentWriter)
        {
            Print(indentWriter);
            foreach (var child in childNodes) {
                indentWriter.IncrIndent();
                child.Print(indentWriter);
                indentWriter.DecrIndent();
            }
        }

        /// <summary>
        ///     Returns all descendent nodes to the top node in a list in which the utmost descendants are
        ///     listed first and the top node itself is listed last.
        /// </summary>
        /// <param name="topNode">is the root node of a tree structure</param>
        /// <returns>list of nodes with utmost descendants first ordered by level of depth in tree with top node last</returns>
        public static IList<BaseAssemblyNodeFactory> GetDescendentNodesBottomUp(BaseAssemblyNodeFactory topNode)
        {
            IList<BaseAssemblyNodeFactory> result = new List<BaseAssemblyNodeFactory>();

            // Map to hold per level of the node (1 to N depth) of node a list of nodes, if any
            // exist at that level
            var nodesPerLevel =
                new OrderedListDictionary<int, IList<BaseAssemblyNodeFactory>>();

            // Recursively enter all aggregate functions and their level into map
            RecursiveAggregateEnter(topNode, nodesPerLevel, 1);

            // Done if none found
            if (nodesPerLevel.IsEmpty()) {
                throw new IllegalStateException("Empty collection for nodes per level");
            }

            // From the deepest (highest) level to the lowest, add aggregates to list
            var deepLevel = nodesPerLevel.Keys.Last();
            for (var i = deepLevel; i >= 1; i--) {
                var list = nodesPerLevel.Get(i);
                if (list == null) {
                    continue;
                }

                result.AddAll(list);
            }

            return result;
        }

        private static void RecursiveAggregateEnter(
            BaseAssemblyNodeFactory currentNode,
            IDictionary<int, IList<BaseAssemblyNodeFactory>> nodesPerLevel,
            int currentLevel)
        {
            // ask all child nodes to enter themselves
            foreach (var node in currentNode.childNodes) {
                RecursiveAggregateEnter(node, nodesPerLevel, currentLevel + 1);
            }

            // Add myself to list
            var aggregates = nodesPerLevel.Get(currentLevel);
            if (aggregates == null) {
                aggregates = new List<BaseAssemblyNodeFactory>();
                nodesPerLevel.Put(currentLevel, aggregates);
            }

            aggregates.Add(currentNode);
        }
    }
} // end of namespace