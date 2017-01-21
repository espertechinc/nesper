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

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Superclass of all nodes in an evaluation tree representing an event pattern expression. 
    /// Follows the Composite pattern. Child nodes do not carry references to parent nodes, 
    /// the tree is unidirectional.
    /// </summary>
    [Serializable]
    public abstract class EvalNodeFactoryBase
        : EvalFactoryNode
    {
        private readonly List<EvalFactoryNode> _childNodes;
        private short _factoryNodeId;
    
        public abstract EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode);
        public abstract void ToPrecedenceFreeEPL(TextWriter writer);
    
        /// <summary>Constructor creates a list of child nodes. </summary>
        protected EvalNodeFactoryBase()
        {
            _childNodes = new List<EvalFactoryNode>();
        }
    
        /// <summary>Adds a child node. </summary>
        /// <param name="childNode">is the child evaluation tree node to add</param>
        public void AddChildNode(EvalFactoryNode childNode)
        {
            _childNodes.Add(childNode);
        }
    
        public void AddChildNodes(IEnumerable<EvalFactoryNode> childNodesToAdd)
        {
            _childNodes.AddRange(childNodesToAdd);
        }

        /// <summary>Returns list of child nodes. </summary>
        /// <value>list of child nodes</value>
        public IList<EvalFactoryNode> ChildNodes
        {
            get { return _childNodes; }
        }

        public short FactoryNodeId
        {
            get { return _factoryNodeId; }
            set { _factoryNodeId = value; }
        }

        public void ToEPL(TextWriter writer, PatternExpressionPrecedenceEnum parentPrecedence)
        {
            if (Precedence.GetLevel() < parentPrecedence.GetLevel()) {
                writer.Write("(");
                ToPrecedenceFreeEPL(writer);
                writer.Write(")");
            }
            else {
                ToPrecedenceFreeEPL(writer);
            }
        }

        public abstract bool IsFilterChildNonQuitting { get; }
        public abstract bool IsStateful { get; }
        public abstract PatternExpressionPrecedenceEnum Precedence { get; }
    }
}
