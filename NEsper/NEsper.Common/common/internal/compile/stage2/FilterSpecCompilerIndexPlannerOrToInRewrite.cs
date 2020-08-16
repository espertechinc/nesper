///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerIndexPlannerOrToInRewrite
    {
        public static ExprNode RewriteOrToInIfApplicable(
            ExprNode constituent,
            bool rewriteRegardlessOfLookupable)
        {
            if (!(constituent is ExprOrNode) || constituent.ChildNodes.Length < 2) {
                return constituent;
            }

            // check eligibility
            var childNodes = constituent.ChildNodes;
            foreach (var child in childNodes) {
                if (!(child is ExprEqualsNode)) {
                    return constituent;
                }

                var equalsNode = (ExprEqualsNode) child;
                if (equalsNode.IsIs || equalsNode.IsNotEquals) {
                    return constituent;
                }
            }

            // find common-expression node
            ExprNode commonExpressionNode;
            var lhs = childNodes[0].ChildNodes[0];
            var rhs = childNodes[0].ChildNodes[1];
            if (ExprNodeUtilityCompare.DeepEquals(lhs, rhs, false)) {
                return constituent;
            }

            if (IsExprExistsInAllEqualsChildNodes(childNodes, lhs)) {
                commonExpressionNode = lhs;
            }
            else if (IsExprExistsInAllEqualsChildNodes(childNodes, rhs)) {
                commonExpressionNode = rhs;
            }
            else {
                return constituent;
            }

            // if the common expression doesn't reference an event property, no need to rewrite
            if (!rewriteRegardlessOfLookupable) {
                var lookupableVisitor = new FilterSpecExprNodeVisitorLookupableLimitedExpr();
                commonExpressionNode.Accept(lookupableVisitor);
                if (!lookupableVisitor.HasStreamZeroReference || !lookupableVisitor.IsLimited) {
                    return constituent;
                }
            }

            // build node
            var @in = new ExprInNodeImpl(false);
            @in.AddChildNode(commonExpressionNode);
            for (var i = 0; i < constituent.ChildNodes.Length; i++) {
                var child = constituent.ChildNodes[i];
                var nodeindex = ExprNodeUtilityCompare.DeepEquals(commonExpressionNode, childNodes[i].ChildNodes[0], false) ? 1 : 0;
                @in.AddChildNode(child.ChildNodes[nodeindex]);
            }

            // validate
            try {
                @in.ValidateWithoutContext();
            }
            catch (ExprValidationException) {
                return constituent;
            }

            return @in;
        }

        private static bool IsExprExistsInAllEqualsChildNodes(
            ExprNode[] childNodes,
            ExprNode search)
        {
            foreach (var child in childNodes) {
                var lhs = child.ChildNodes[0];
                var rhs = child.ChildNodes[1];
                if (!ExprNodeUtilityCompare.DeepEquals(lhs, search, false) && !ExprNodeUtilityCompare.DeepEquals(rhs, search, false)) {
                    return false;
                }

                if (ExprNodeUtilityCompare.DeepEquals(lhs, rhs, false)) {
                    return false;
                }
            }

            return true;
        }
    }
} // end of namespace