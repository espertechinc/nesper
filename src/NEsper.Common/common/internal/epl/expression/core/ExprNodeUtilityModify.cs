///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.chain;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeUtilityModify
    {
        public static void ReplaceChildNode(
            ExprNode parentNode,
            ExprNode nodeToReplace,
            ExprNode newNode)
        {
            var index = FindChildNode(parentNode, nodeToReplace);
            if (index == -1) {
                parentNode.ReplaceUnlistedChildNode(nodeToReplace, newNode);
            }
            else {
                parentNode.SetChildNode(index, newNode);
            }
        }

        private static int FindChildNode(
            ExprNode parentNode,
            ExprNode childNode)
        {
            for (var i = 0; i < parentNode.ChildNodes.Length; i++) {
                if (parentNode.ChildNodes[i] == childNode) {
                    return i;
                }
            }

            return -1;
        }

        public static void ReplaceChainChildNode(
            ExprNode nodeToReplace,
            ExprNode newNode,
            IList<Chainable> chainSpec)
        {
            foreach (Chainable chained in chainSpec) {
                var parametersOrEmpty = chained.GetParametersOrEmpty();
                var index = parametersOrEmpty.IndexOf(nodeToReplace);
                if (index != -1) {
                    parametersOrEmpty[index] = newNode;
                }
            }
        }
    }
} // end of namespace