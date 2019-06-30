///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    [TestFixture]
    public class TestColumnNamedNodeSwapper : CommonTest
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
            ExprIdentNode i1_1 = new ExprIdentNodeImpl("intPrimitive");
            ExprIdentNode i1_2 = new ExprIdentNodeImpl("intBoxed");
            topNode.AddChildNode(i1_1);
            topNode.AddChildNode(i1_2);
            return topNode;
        }

        [Test]
        public void TestPartReplaced()
        {
            exprTree = MakeEqualsNode();
            alias = "intPrimitive";
            resultingTree = ColumnNamedNodeSwapper.Swap(exprTree, alias, fullExpr);

            Assert.IsTrue(resultingTree == exprTree);
            var childNodes = resultingTree.ChildNodes;
            var oldChildNodes = exprTree.ChildNodes;
            Assert.IsTrue(childNodes.Length == 2);
            Assert.IsTrue(childNodes[0] == fullExpr);
            Assert.IsTrue(childNodes[1] == oldChildNodes[1]);

            exprTree = resultingTree;
            alias = "intBoxed";
            resultingTree = ColumnNamedNodeSwapper.Swap(exprTree, alias, fullExpr);
            childNodes = resultingTree.ChildNodes;
            Assert.IsTrue(childNodes.Length == 2);
            Assert.IsTrue(childNodes[0] == fullExpr);
            Assert.IsTrue(childNodes[1] == fullExpr);

            exprTree = resultingTree;
            ExprNode newFullExpr = new ExprIdentNodeImpl("new full expr");
            alias = "full expression";
            resultingTree = ColumnNamedNodeSwapper.Swap(exprTree, alias, newFullExpr);
            childNodes = resultingTree.ChildNodes;
            Assert.IsTrue(childNodes.Length == 2);
            Assert.IsTrue(childNodes[0] == newFullExpr);
            Assert.IsTrue(childNodes[1] == newFullExpr);
        }

        [Test]
        public void TestWholeReplaced()
        {
            exprTree = new ExprIdentNodeImpl("swapped");
            alias = "swapped";
            resultingTree = ColumnNamedNodeSwapper.Swap(exprTree, alias, fullExpr);
            Assert.IsTrue(resultingTree == fullExpr);
        }
    }
} // end of namespace