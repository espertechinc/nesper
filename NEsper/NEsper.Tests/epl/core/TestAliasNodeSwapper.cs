///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.core
{
    [TestFixture]
    public class TestAliasNodeSwapper 
    {
    	private ExprNode _exprTree;
        private String _alias;
        private ExprNode _fullExpr;
        private ExprNode _resultingTree;
    
        [SetUp]
    	public void SetUp()
    	{
    		_fullExpr = new ExprIdentNodeImpl("full expression");
    	}
    
        [Test]
    	public void TestWholeReplaced()
    	{
    		_exprTree = new ExprIdentNodeImpl("swapped");
    		_alias = "swapped";
    		_resultingTree = ColumnNamedNodeSwapper.Swap(_exprTree, _alias, _fullExpr);
    		Assert.IsTrue(_resultingTree == _fullExpr);
    	}
    
        [Test]
    	public void TestPartReplaced()
    	{
    		_exprTree = MakeEqualsNode();
    		_alias = "IntPrimitive";
    		_resultingTree = ColumnNamedNodeSwapper.Swap(_exprTree, _alias, _fullExpr);
    
    		Assert.IsTrue(_resultingTree == _exprTree);
    		var childNodes = _resultingTree.ChildNodes;
    		var oldChildNodes = _exprTree.ChildNodes;
    		Assert.IsTrue(childNodes.Count == 2);
    		Assert.IsTrue(childNodes[0] == _fullExpr);
    		Assert.IsTrue(childNodes[1] == oldChildNodes[1]);
    
    		_exprTree = _resultingTree;
    		_alias = "IntBoxed";
    		_resultingTree = ColumnNamedNodeSwapper.Swap(_exprTree, _alias, _fullExpr);
    		childNodes = _resultingTree.ChildNodes;
    		Assert.IsTrue(childNodes.Count == 2);
    		Assert.IsTrue(childNodes[0] == _fullExpr);
    		Assert.IsTrue(childNodes[1] == _fullExpr);
    
    		_exprTree = _resultingTree;
    		ExprNode newFullExpr = new ExprIdentNodeImpl("new full expr");
    		_alias = "full expression";
    		_resultingTree = ColumnNamedNodeSwapper.Swap(_exprTree, _alias, newFullExpr);
    		childNodes = _resultingTree.ChildNodes;
    		Assert.IsTrue(childNodes.Count == 2);
    		Assert.IsTrue(childNodes[0] == newFullExpr);
    		Assert.IsTrue(childNodes[1] == newFullExpr);
    	}
    
        public static ExprEqualsNode MakeEqualsNode()
        {
            ExprEqualsNode topNode = new ExprEqualsNodeImpl(false, false);
            ExprIdentNode i1_1 = new ExprIdentNodeImpl("IntPrimitive");
            ExprIdentNode i1_2 = new ExprIdentNodeImpl("IntBoxed");
            topNode.AddChildNode(i1_1);
            topNode.AddChildNode(i1_2);
    
            SupportExprNodeFactory.Validate1StreamBean(topNode);
    
            return topNode;
        }
    
    }
}
