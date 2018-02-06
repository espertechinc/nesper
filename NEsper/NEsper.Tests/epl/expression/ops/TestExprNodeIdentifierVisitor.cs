///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprNodeIdentifierVisitor 
    {
        private ExprNode exprNode;
    
        [SetUp]
        public void SetUp()
        {
            exprNode = SupportExprNodeFactory.MakeMathNode();
        }
    
        [Test]
        public void TestVisit()
        {
            // test without aggregation nodes
            ExprNodeIdentifierVisitor visitor = new ExprNodeIdentifierVisitor(false);
            exprNode.Accept(visitor);
    
            Assert.AreEqual(2, visitor.ExprProperties.Count);
            Assert.AreEqual(0, (Object) visitor.ExprProperties[0].First);
            Assert.AreEqual("IntBoxed", (Object) visitor.ExprProperties[0].Second);
            Assert.AreEqual(0, (Object) visitor.ExprProperties[1].First);
            Assert.AreEqual("IntPrimitive", (Object) visitor.ExprProperties[1].Second);
    
            // test with aggregation nodes, such as "IntBoxed * sum(IntPrimitive)"
            exprNode = SupportExprNodeFactory.MakeSumAndFactorNode();
            visitor = new ExprNodeIdentifierVisitor(true);
            exprNode.Accept(visitor);
            Assert.AreEqual(2, visitor.ExprProperties.Count);
            Assert.AreEqual("IntBoxed", (Object) visitor.ExprProperties[0].Second);
            Assert.AreEqual("IntPrimitive", (Object) visitor.ExprProperties[1].Second);
    
            visitor = new ExprNodeIdentifierVisitor(false);
            exprNode.Accept(visitor);
            Assert.AreEqual(1, visitor.ExprProperties.Count);
            Assert.AreEqual("IntBoxed", (Object) visitor.ExprProperties[0].Second);
        }
    }
}
