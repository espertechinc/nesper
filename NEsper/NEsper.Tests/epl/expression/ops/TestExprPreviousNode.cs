///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprPreviousNode
    {
        private ExprPreviousNode _prevNode;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _prevNode = SupportExprNodeFactory.MakePreviousNode();
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(double?), _prevNode.ReturnType);
        }
    
        [Test]
        public void TestValidate()
        {
            _prevNode = new ExprPreviousNode(ExprPreviousNodePreviousType.PREV);
    
            // No subnodes: Exception is thrown.
            TryInvalidValidate(_prevNode);
    
            // singe child node not possible, must be 2 at least
            _prevNode.AddChildNode(new SupportExprNode(4));
            TryInvalidValidate(_prevNode);
        }
    
        [Test]
        public void TestEquals() 
        {
            ExprPreviousNode node1 = new ExprPreviousNode(ExprPreviousNodePreviousType.PREV);
            Assert.IsTrue(node1.EqualsNode(_prevNode, false));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("prev(s1.IntPrimitive,s1.DoublePrimitive)", _prevNode.ToExpressionStringMinPrecedenceSafe());
        }
    
        private void TryInvalidValidate(ExprPreviousNode exprPrevNode)
        {
            try {
                exprPrevNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // expected
            }
        }
    }
}
