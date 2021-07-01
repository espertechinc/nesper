///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerIndexPlannerUnwindAndOr
    {
        public static IList<ExprNode> UnwindAndOr(IList<ExprNode> nodes)
        {
            IList<ExprNode> unwound = new List<ExprNode>(nodes.Count);
            foreach (var node in nodes) {
                var result = Unwind(node);
                unwound.Add(result);
            }

            return unwound;
        }

        private static ExprNode Unwind(ExprNode node)
        {
            var isOr = node is ExprOrNode;
            var isAnd = node is ExprAndNode;
            if (!isOr && !isAnd) {
                return node;
            }

            var needsUnwind = false;
            foreach (var child in node.ChildNodes) {
                if (child is ExprOrNode && isOr || child is ExprAndNode && isAnd) {
                    needsUnwind = true;
                    break;
                }
            }

            if (!needsUnwind) {
                return node;
            }

            if (isOr) {
                var unwoundX = new ExprOrNode();
                foreach (var child in node.ChildNodes) {
                    if (child is ExprOrNode) {
                        foreach (var orChild in child.ChildNodes) {
                            var unwoundChild = Unwind(orChild);
                            if (unwoundChild is ExprOrNode) {
                                unwoundX.AddChildNodes(Arrays.AsList(unwoundChild.ChildNodes));
                            }
                            else {
                                unwoundX.AddChildNode(unwoundChild);
                            }
                        }
                    }
                    else {
                        unwoundX.AddChildNode(Unwind(child));
                    }
                }

                return unwoundX;
            }

            ExprAndNode unwound = new ExprAndNodeImpl();
            foreach (var child in node.ChildNodes) {
                if (child is ExprAndNode) {
                    foreach (var andChild in child.ChildNodes) {
                        var unwoundChild = Unwind(andChild);
                        if (unwoundChild is ExprAndNode) {
                            unwound.AddChildNodes(Arrays.AsList(unwoundChild.ChildNodes));
                        }
                        else {
                            unwound.AddChildNode(unwoundChild);
                        }
                    }
                }
                else {
                    unwound.AddChildNode(Unwind(child));
                }
            }

            return unwound;
        }
    }
} // end of namespace