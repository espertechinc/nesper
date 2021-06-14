///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.variable;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    /// <summary>
    /// Visitor for expression node trees that determines if the expressions within contain a variable.
    /// </summary>
    public class ExprNodeVariableVisitor : ExprNodeVisitor
    {
        private readonly VariableCompileTimeResolver variableCompileTimeResolver;
        private IDictionary<string, VariableMetaData> variableNames;

        public ExprNodeVariableVisitor(VariableCompileTimeResolver variableCompileTimeResolver)
        {
            this.variableCompileTimeResolver = variableCompileTimeResolver;
        }

        public bool IsWalkDeclExprParam => true;

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        /// <summary>
        /// Returns true if the visitor finds a variable value.
        /// </summary>
        /// <returns>true for variable present in expression</returns>
        public bool IsVariables {
            get => variableNames != null && !variableNames.IsEmpty();
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprDotNode) {
                ExprDotNode exprDotNode = (ExprDotNode) exprNode;
                VariableMetaData metadata = exprDotNode.IsVariableOpGetName(variableCompileTimeResolver);
                if (metadata != null) {
                    AddVariableName(metadata);
                }
            }

            if (exprNode is ExprVariableNode) {
                ExprVariableNode variableNode = (ExprVariableNode) exprNode;
                VariableMetaData metadata = variableNode.VariableMetadata;
                AddVariableName(metadata);
            }
        }

        public IDictionary<string, VariableMetaData> VariableNames {
            get { return variableNames; }
        }

        /// <summary>
        /// Returns the set of variable names encoountered.
        /// </summary>
        /// <returns>variable names</returns>
        private void AddVariableName(VariableMetaData meta)
        {
            if (variableNames == null) {
                variableNames = new LinkedHashMap<string, VariableMetaData>();
            }

            variableNames.Put(meta.VariableName, meta);
        }
    }
} // end of namespace