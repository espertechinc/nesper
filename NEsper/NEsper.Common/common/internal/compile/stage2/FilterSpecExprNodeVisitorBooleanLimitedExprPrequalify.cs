///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.funcs;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.expression.variable;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.script.core;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecExprNodeVisitorBooleanLimitedExprPrequalify : ExprNodeVisitor
    {
        public bool IsLimited { get; private set; } = true;

        public bool IsVisit(ExprNode exprNode)
        {
            return IsLimited;
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprVariableNode) {
                var node = (ExprVariableNode) exprNode;
                if (!node.VariableMetadata.IsConstant) {
                    IsLimited = false;
                }
            }
            else if (exprNode is ExprTableAccessNode ||
                     exprNode is ExprSubselectNode ||
                     exprNode is ExprLambdaGoesNode ||
                     exprNode is ExprNodeScript ||
                     exprNode is ExprDeclaredNode) {
                IsLimited = false;
            }
            else if (exprNode is ExprPlugInSingleRowNode) {
                var plugIn = (ExprPlugInSingleRowNode) exprNode;
                if (plugIn.Config != null && plugIn.Config.FilterOptimizable == ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizable.DISABLED) {
                    IsLimited = false;
                }

                if (plugIn.IsLocalInlinedClass) {
                    IsLimited = false;
                }
            }
            else if (exprNode is ExprDotNode) {
                var node = (ExprDotNode) exprNode;
                if (node.IsLocalInlinedClass) {
                    IsLimited = false;
                }
            }

            // we don't process enumeration methods
            if (exprNode is ExprNodeWithChainSpec) {
                if (!((ExprNodeWithChainSpec) exprNode).ChainSpec.IsEmpty()) {
                    IsLimited = false;
                }
            }
        }
    }
} // end of namespace