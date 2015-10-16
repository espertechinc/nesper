///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Superclass of all nodes in an evaluation tree representing an event pattern expression. 
    /// Follows the Composite pattern. Child nodes do not carry references to parent nodes, 
    /// the tree is unidirectional.
    /// </summary>
    public interface EvalFactoryNode
    {
        /// <summary>Adds a child node. </summary>
        /// <param name="childNode">is the child evaluation tree node to add</param>
        void AddChildNode(EvalFactoryNode childNode);

        /// <summary>Returns list of child nodes </summary>
        /// <value>list of child nodes</value>
        IList<EvalFactoryNode> ChildNodes { get; }

        void AddChildNodes(IEnumerable<EvalFactoryNode> childNodes);
    
        EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode);

        bool IsFilterChildNonQuitting { get; }

        short FactoryNodeId { get; set; }

        bool IsStateful { get; }

        /// <summary>Returns precendence. </summary>
        /// <value>precendence</value>
        PatternExpressionPrecedenceEnum Precedence { get; }

        /// <summary>Write expression considering precendence. </summary>
        /// <param name="writer">to use</param>
        /// <param name="parentPrecedence">precendence</param>
        void ToEPL(TextWriter writer, PatternExpressionPrecedenceEnum parentPrecedence);
    }
}
