///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.epl.expression.visitor
{
    /// <summary>
    /// Visitor for expression node trees that determines if the expressions within contain a variable.
    /// </summary>
    public class ExprNodeVariableVisitor : ExprNodeVisitor
    {
        private readonly VariableService _variableService;
        private ISet<string> _variableNames;

        public ExprNodeVariableVisitor(VariableService variableService)
        {
            _variableService = variableService;
        }

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        /// <summary>
        /// Returns true if the visitor finds a variable value.
        /// </summary>
        /// <value>true for variable present in expression</value>
        public bool HasVariables
        {
            get { return _variableNames != null && !_variableNames.IsEmpty(); }
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprDotNode)
            {
                var exprDotNode = (ExprDotNode) exprNode;
                var variableName = exprDotNode.IsVariableOpGetName(_variableService);
                if (variableName != null)
                {
                    AddVariableName(variableName);
                }
            }
            if (exprNode is ExprVariableNode)
            {
                var variableNode = (ExprVariableNode) exprNode;
                AddVariableName(variableNode.VariableName);
            }
        }

        /// <summary>
        /// Returns the set of variable names encoountered.
        /// </summary>
        /// <value>variable names</value>
        public ISet<string> VariableNames
        {
            get { return _variableNames; }
        }

        private void AddVariableName(string name)
        {
            if (_variableNames == null)
            {
                _variableNames = new HashSet<string>();
            }
            _variableNames.Add(name);
        }
    }
} // end of namespace
