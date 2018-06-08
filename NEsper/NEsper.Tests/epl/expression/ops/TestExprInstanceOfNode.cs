///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprInstanceOfNode 
    {
        private ExprInstanceofNode[] _isNodes;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _isNodes = new ExprInstanceofNode[5];
    
            _isNodes[0] = new ExprInstanceofNode(new String[] {"long"}, _container);
            _isNodes[0].AddChildNode(new SupportExprNode(1L, typeof(long)));
    
            _isNodes[1] = new ExprInstanceofNode(new String[] {typeof(SupportBean).FullName, "int", "string"}, _container);
            _isNodes[1].AddChildNode(new SupportExprNode("", typeof(string)));
    
            _isNodes[2] = new ExprInstanceofNode(new String[] {"string"}, _container);
            _isNodes[2].AddChildNode(new SupportExprNode(null, typeof(Boolean)));
    
            _isNodes[3] = new ExprInstanceofNode(new String[] {"string", "char"}, _container);
            _isNodes[3].AddChildNode(new SupportExprNode(new SupportBean(), typeof(Object)));
    
            _isNodes[4] = new ExprInstanceofNode(new String[] {"int", "float", typeof(SupportBean).FullName}, _container);
            _isNodes[4].AddChildNode(new SupportExprNode(new SupportBean(), typeof(Object)));
        }
    
        [Test]
        public void TestGetType()
        {
            for (int i = 0; i < _isNodes.Length; i++)
            {
                _isNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.AreEqual(typeof(bool?), _isNodes[i].ReturnType);
            }
        }
    
        [Test]
        public void TestValidate()
        {
            ExprInstanceofNode isNode = new ExprInstanceofNode(new String[0], _container);
            isNode.AddChildNode(new SupportExprNode(1));
    
            // Test too few nodes under this node
            try
            {
                isNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
    
            // Test node result type not fitting
            isNode.AddChildNode(new SupportExprNode("s"));
            try
            {
                isNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    
        [Test]
        public void TestEvaluate()
        {
            for (int i = 0; i < _isNodes.Length; i++)
            {
                _isNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            }
    
            Assert.AreEqual(true, _isNodes[0].Evaluate(new EvaluateParams(null, false, null)));
            Assert.AreEqual(true, _isNodes[1].Evaluate(new EvaluateParams(null, false, null)));
            Assert.AreEqual(false, _isNodes[2].Evaluate(new EvaluateParams(null, false, null)));
            Assert.AreEqual(false, _isNodes[3].Evaluate(new EvaluateParams(null, false, null)));
            Assert.AreEqual(true, _isNodes[4].Evaluate(new EvaluateParams(null, false, null)));
        }
    
        [Test]
        public void TestEquals()
        {
            Assert.IsFalse(_isNodes[0].EqualsNode(new ExprEqualsNodeImpl(true, false), false));
            Assert.IsFalse(_isNodes[0].EqualsNode(_isNodes[1], false));
            Assert.IsTrue(_isNodes[0].EqualsNode(_isNodes[0], false));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("instanceof(\"\"," + typeof(SupportBean).FullName + ",int,string)", _isNodes[1].ToExpressionStringMinPrecedenceSafe());
        }
    }
}
