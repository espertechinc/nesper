///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.querygraph;

namespace com.espertech.esper.regressionlib.support.epl
{
    public class SupportExprNodeFactory
    {
        public static QueryGraphValueEntryHashKeyedForge MakeKeyed(string property)
        {
            return new QueryGraphValueEntryHashKeyedForgeExpr(new ExprIdentNodeImpl(property), false);
        }

        public static QueryGraphValueEntryRangeForge MakeRangeLess(string prop)
        {
            return new QueryGraphValueEntryRangeRelOpForge(
                QueryGraphRangeEnum.LESS,
                new ExprIdentNodeImpl(prop),
                false);
        }

        public static QueryGraphValueEntryRangeInForge MakeRangeIn(
            string start,
            string end)
        {
            return new QueryGraphValueEntryRangeInForge(
                QueryGraphRangeEnum.RANGE_OPEN,
                new ExprIdentNodeImpl(start),
                new ExprIdentNodeImpl(end),
                false);
        }

        public static ExprNode[] MakeIdentExprNodes(params string[] props)
        {
            var nodes = new ExprNode[props.Length];
            for (var i = 0; i < props.Length; i++) {
                nodes[i] = new ExprIdentNodeImpl(props[i]);
            }

            return nodes;
        }

        public static ExprNode[] MakeConstAndIdentNode(
            string constant,
            string property)
        {
            return new ExprNode[] {new ExprConstantNodeImpl(constant), new ExprIdentNodeImpl(property)};
        }

        public static ExprNode[] MakeConstAndConstNode(
            string constantOne,
            string constantTwo)
        {
            return new ExprNode[] {new ExprConstantNodeImpl(constantOne), new ExprConstantNodeImpl(constantTwo)};
        }

        public static ExprNode MakeIdentExprNode(string property)
        {
            return new ExprIdentNodeImpl(property);
        }

        public static ExprNode MakeConstExprNode(string constant)
        {
            return new ExprConstantNodeImpl(constant);
        }
    }
} // end of namespace