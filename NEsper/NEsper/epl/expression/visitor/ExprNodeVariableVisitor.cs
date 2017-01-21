///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.visitor
{
    /// <summary>
    /// Visitor for expression node trees that determines if the expressions within contain a variable.
    /// </summary>
    public class ExprNodeVariableVisitor : ExprNodeVisitor
    {
        /// <summary> Returns the set of variable names encoountered.</summary>
        /// <returns>variable names</returns>
        public ISet<string> VariableNames { get; private set; }

        /// <summary>Returns true if the visitor finds a variable value.</summary>
        /// <returns>true for variable present in expression</returns>
        public bool HasVariables { get; private set; }
        
        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        public void Visit(ExprNode exprNode)
        {
            if (!(exprNode is ExprVariableNode))
            {
                return;
            }
            HasVariables = true;

            var variableNode = (ExprVariableNode)exprNode;
            if (VariableNames == null)
            {
                VariableNames = new HashSet<string>();
            }
            VariableNames.Add(variableNode.VariableName);
        }

    }
} // End of namespace
