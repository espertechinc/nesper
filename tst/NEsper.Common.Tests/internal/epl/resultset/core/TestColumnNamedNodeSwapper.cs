///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    [TestFixture]
    public class TestColumnNamedNodeSwapper : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            fullExpr = new ExprIdentNodeImpl("full expression");
        }

        private ExprNode exprTree;
        private string alias;
        private ExprNode fullExpr;
        private ExprNode resultingTree;

        public static ExprEqualsNode MakeEqualsNode()
        {
            ExprEqualsNode topNode = new ExprEqualsNodeImpl(false, false);
            ExprIdentNode i1_1 = new ExprIdentNodeImpl("IntPrimitive");
            ExprIdentNode i1_2 = new ExprIdentNodeImpl("IntBoxed");
            topNode.AddChildNode(i1_1);
            topNode.AddChildNode(i1_2);
            return topNode;
        }

        [Test]
        public void TestPartReplaced()
        {
            exprTree = MakeEqualsNode();
            alias = "IntPrimitive";
            resultingTree = ColumnNamedNodeSwapper.Swap(exprTree, alias, fullExpr);

            ClassicAssert.IsTrue(resultingTree == exprTree);
            var childNodes = resultingTree.ChildNodes;
            var oldChildNodes = exprTree.ChildNodes;
            ClassicAssert.IsTrue(childNodes.Length == 2);
            ClassicAssert.IsTrue(childNodes[0] == fullExpr);
            ClassicAssert.IsTrue(childNodes[1] == oldChildNodes[1]);

            exprTree = resultingTree;
            alias = "IntBoxed";
            resultingTree = ColumnNamedNodeSwapper.Swap(exprTree, alias, fullExpr);
            childNodes = resultingTree.ChildNodes;
            ClassicAssert.IsTrue(childNodes.Length == 2);
            ClassicAssert.IsTrue(childNodes[0] == fullExpr);
            ClassicAssert.IsTrue(childNodes[1] == fullExpr);

            exprTree = resultingTree;
            ExprNode newFullExpr = new ExprIdentNodeImpl("new full expr");
            alias = "full expression";
            resultingTree = ColumnNamedNodeSwapper.Swap(exprTree, alias, newFullExpr);
            childNodes = resultingTree.ChildNodes;
            ClassicAssert.IsTrue(childNodes.Length == 2);
            ClassicAssert.IsTrue(childNodes[0] == newFullExpr);
            ClassicAssert.IsTrue(childNodes[1] == newFullExpr);
        }

        [Test]
        public void TestWholeReplaced()
        {
            exprTree = new ExprIdentNodeImpl("swapped");
            alias = "swapped";
            resultingTree = ColumnNamedNodeSwapper.Swap(exprTree, alias, fullExpr);
            ClassicAssert.IsTrue(resultingTree == fullExpr);
        }
    }
} // end of namespace
