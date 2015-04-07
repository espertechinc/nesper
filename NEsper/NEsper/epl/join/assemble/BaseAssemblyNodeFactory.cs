///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.assemble
{
	/// <summary>
	/// Represents the factory of a node in a tree responsible for assembling outer join query results.
	/// <para />The tree of factory nodes is double-linked, child nodes know each parent and parent know all child nodes.
	/// </summary>
	public abstract class BaseAssemblyNodeFactory
	{
	    /// <summary>
	    /// Parent node.
	    /// </summary>
	    private BaseAssemblyNodeFactory _parentNode;

	    /// <summary>
	    /// Child nodes.
	    /// </summary>
	    private readonly IList<BaseAssemblyNodeFactory> _childNodes;

	    /// <summary>
	    /// Stream number.
	    /// </summary>
	    private readonly int _streamNum;

	    /// <summary>
	    /// Number of streams in statement.
	    /// </summary>
	    private readonly int _numStreams;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="streamNum">stream number of the event stream that this node assembles results for.</param>
	    /// <param name="numStreams">number of streams</param>
	    protected BaseAssemblyNodeFactory(int streamNum, int numStreams)
	    {
	        _streamNum = streamNum;
	        _numStreams = numStreams;
	        _childNodes = new List<BaseAssemblyNodeFactory>(4);
	    }

	    public abstract BaseAssemblyNode MakeAssemblerUnassociated();

	    /// <summary>
	    /// Output this node using writer, not outputting child nodes.
	    /// </summary>
	    /// <param name="indentWriter">to use for output</param>
	    public abstract void Print(IndentWriter indentWriter);

	    /// <summary>
	    /// Set parent node.
	    /// </summary>
	    public void SetParent(BaseAssemblyNodeFactory parent)
	    {
	        _parentNode = parent;
	    }

	    public BaseAssemblyNodeFactory ParentNode
	    {
	        get { return _parentNode; }
	    }

	    public int NumStreams
	    {
	        get { return _numStreams; }
	    }

	    /// <summary>
	    /// Add a child node.
	    /// </summary>
	    /// <param name="childNode">to add</param>
	    public virtual void AddChild(BaseAssemblyNodeFactory childNode)
	    {
	        childNode._parentNode = this;
	        _childNodes.Add(childNode);
	    }

	    /// <summary>
	    /// Returns the stream number.
	    /// </summary>
	    /// <value>stream number</value>
	    protected internal int StreamNum
	    {
	        get { return _streamNum; }
	    }

	    /// <summary>
	    /// Returns child nodes.
	    /// </summary>
	    /// <value>child nodes</value>
	    public IList<BaseAssemblyNodeFactory> ChildNodes
	    {
	        get { return _childNodes; }
	    }

	    /// <summary>
	    /// Output this node and all descendent nodes using writer, outputting child nodes.
	    /// </summary>
	    /// <param name="indentWriter">to output to</param>
	    public void PrintDescendends(IndentWriter indentWriter)
	    {
	        Print(indentWriter);
	        foreach (BaseAssemblyNodeFactory child in _childNodes)
	        {
	            indentWriter.IncrIndent();
	            child.Print(indentWriter);
	            indentWriter.DecrIndent();
	        }
	    }

	    /// <summary>
	    /// Returns all descendent nodes to the top node in a list in which the utmost descendants are
	    /// listed first and the top node itself is listed last.
	    /// </summary>
	    /// <param name="topNode">is the root node of a tree structure</param>
	    /// <returns>list of nodes with utmost descendants first ordered by level of depth in tree with top node last</returns>
	    public static IList<BaseAssemblyNodeFactory> GetDescendentNodesBottomUp(BaseAssemblyNodeFactory topNode)
	    {
	        var result = new List<BaseAssemblyNodeFactory>();

	        // Map to hold per level of the node (1 to N depth) of node a list of nodes, if any
	        // exist at that level
            var nodesPerLevel = new OrderedDictionary<int, ICollection<BaseAssemblyNodeFactory>>();

	        // Recursively enter all aggregate functions and their level into map
	        RecursiveAggregateEnter(topNode, nodesPerLevel, 1);

	        // Done if none found
	        if (nodesPerLevel.IsEmpty())
	        {
	            throw new IllegalStateException("Empty collection for nodes per level");
	        }

	        // From the deepest (highest) level to the lowest, add aggregates to list
	        int deepLevel = nodesPerLevel.Keys.Last();
	        for (int i = deepLevel; i >= 1; i--)
	        {
	            var list = nodesPerLevel.Get(i);
	            if (list == null)
	            {
	                continue;
	            }
	            result.AddAll(list);
	        }

	        return result;
	    }

	    private static void RecursiveAggregateEnter(BaseAssemblyNodeFactory currentNode, IDictionary<int, ICollection<BaseAssemblyNodeFactory>> nodesPerLevel, int currentLevel)
	    {
	        // ask all child nodes to enter themselves
	        foreach (BaseAssemblyNodeFactory node in currentNode._childNodes)
	        {
	            RecursiveAggregateEnter(node, nodesPerLevel, currentLevel + 1);
	        }

	        // Add myself to list
	        var aggregates = nodesPerLevel.Get(currentLevel);
	        if (aggregates == null)
	        {
	            aggregates = new LinkedList<BaseAssemblyNodeFactory>();
	            nodesPerLevel.Put(currentLevel, aggregates);
	        }
	        aggregates.Add(currentNode);
	    }
	}
} // end of namespace
