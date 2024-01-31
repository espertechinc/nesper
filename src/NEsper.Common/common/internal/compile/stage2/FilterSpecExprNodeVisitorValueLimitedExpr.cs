///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.funcs;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.expression.variable;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecExprNodeVisitorValueLimitedExpr : ExprNodeVisitor
    {
        public bool IsLimited { get; private set; } = true;

        public bool IsVisit(ExprNode exprNode)
        {
            return IsLimited;
        }

        public bool IsWalkDeclExprParam => true;

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprStreamRefNode streamRefNode) {
                var stream = streamRefNode.StreamReferencedIfAny;
                if (stream != null) {
                    if (stream == 0) {
                        IsLimited = false;
                    }
                }
            }

            if (exprNode is FilterSpecCompilerAdvIndexDescProvider) {
                IsLimited = false;
            }

            if (exprNode is ExprVariableNode variableNode) {
                if (!variableNode.VariableMetadata.IsConstant) {
                    IsLimited = false;
                }
            }
            else if (exprNode is ExprTableAccessNode ||
                     exprNode is ExprSubselectNode ||
                     exprNode is ExprLambdaGoesNode ||
                     exprNode is ExprWildcard ||
                     exprNode is ExprNodeScript ||
                     exprNode is ExprDeclaredNode) {
                IsLimited = false;
            }
            else if (exprNode is ExprPlugInSingleRowNode plugIn) {
                if (plugIn?.Config is {
                        FilterOptimizable: ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.DISABLED
                    }) {
                    IsLimited = false;
                }
            }
            else if (exprNode is ExprTimestampNode) {
                IsLimited = false;
            }
        }
    }
} // end of namespace