///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprConstantNode 
    {
        private ExprConstantNode _constantNode;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _constantNode = new ExprConstantNodeImpl("5");
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(string), _constantNode.ConstantType);
    
            _constantNode = new ExprConstantNodeImpl(null);
            Assert.IsNull(_constantNode.ConstantType);
        }
    
        [Test]
        public void TestValidate()
        {
            _constantNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
        }
    
        [Test]
        public void TestEvaluate()
        {
            Assert.AreEqual("5", _constantNode.GetConstantValue(null));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            _constantNode = new ExprConstantNodeImpl("5");
            Assert.AreEqual("\"5\"", _constantNode.ToExpressionStringMinPrecedenceSafe());
    
            _constantNode = new ExprConstantNodeImpl(10);
            Assert.AreEqual("10", _constantNode.ToExpressionStringMinPrecedenceSafe());        
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(_constantNode.EqualsNode(new ExprConstantNodeImpl("5"), false));
            Assert.IsFalse(_constantNode.EqualsNode(new ExprOrNode(), false));
            Assert.IsFalse(_constantNode.EqualsNode(new ExprConstantNodeImpl(null), false));
            Assert.IsFalse(_constantNode.EqualsNode(new ExprConstantNodeImpl(3), false));
    
            _constantNode = new ExprConstantNodeImpl(null);
            Assert.IsTrue(_constantNode.EqualsNode(new ExprConstantNodeImpl(null), false));
        }
    }
}
