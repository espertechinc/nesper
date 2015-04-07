///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Result of analysis of pattern expression node tree.
    /// </summary>
    public class EvalNodeAnalysisResult
    {
        private readonly IList<EvalFactoryNode> _activeNodes = new List<EvalFactoryNode>();

        /// <summary>Add a node found. </summary>
        /// <param name="node">found</param>
        public void AddNode(EvalFactoryNode node)
        {
            _activeNodes.Add(node);
        }

        /// <summary>Returns all nodes found. </summary>
        /// <value>pattern nodes</value>
        public IList<EvalFactoryNode> ActiveNodes
        {
            get { return _activeNodes; }
        }

        /// <summary>Returns filter nodes. </summary>
        /// <value>filter nodes</value>
        public IList<EvalFilterFactoryNode> FilterNodes
        {
            get
            {
                return _activeNodes.OfType<EvalFilterFactoryNode>().ToList();
            }
        }

        /// <summary>Returns the repeat-nodes. </summary>
        /// <value>repeat nodes</value>
        public IList<EvalMatchUntilFactoryNode> RepeatNodes
        {
            get
            {
                return _activeNodes.OfType<EvalMatchUntilFactoryNode>().ToList();
            }
        }
    }
}