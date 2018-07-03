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
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprCoalesceNode 
    {
        private ExprCoalesceNode[] _coalesceNodes;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _coalesceNodes = new ExprCoalesceNode[5];
    
            _coalesceNodes[0] = new ExprCoalesceNode();
            _coalesceNodes[0].AddChildNode(new SupportExprNode(null, typeof(long)));
            _coalesceNodes[0].AddChildNode(new SupportExprNode(null, typeof(int)));
            _coalesceNodes[0].AddChildNode(new SupportExprNode(4, typeof(byte)));
    
            _coalesceNodes[1] = new ExprCoalesceNode();
            _coalesceNodes[1].AddChildNode(new SupportExprNode(null, typeof(string)));
            _coalesceNodes[1].AddChildNode(new SupportExprNode("a", typeof(string)));
    
            _coalesceNodes[2] = new ExprCoalesceNode();
            _coalesceNodes[2].AddChildNode(new SupportExprNode(null, typeof(Boolean)));
            _coalesceNodes[2].AddChildNode(new SupportExprNode(true, typeof(bool)));
    
            _coalesceNodes[3] = new ExprCoalesceNode();
            _coalesceNodes[3].AddChildNode(new SupportExprNode(null, typeof(char)));
            _coalesceNodes[3].AddChildNode(new SupportExprNode(null, typeof(char)));
            _coalesceNodes[3].AddChildNode(new SupportExprNode(null, typeof(char)));
            _coalesceNodes[3].AddChildNode(new SupportExprNode('b', typeof(char)));
    
            _coalesceNodes[4] = new ExprCoalesceNode();
            _coalesceNodes[4].AddChildNode(new SupportExprNode(5, typeof(float)));
            _coalesceNodes[4].AddChildNode(new SupportExprNode(null, typeof(double?)));
        }
    
        [Test]
        public void TestGetType()
        {
            for (int i = 0; i < _coalesceNodes.Length; i++)
            {
                _coalesceNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            }

            Assert.AreEqual(typeof(long?), _coalesceNodes[0].ReturnType);
            Assert.AreEqual(typeof(string), _coalesceNodes[1].ReturnType);
            Assert.AreEqual(typeof(bool?), _coalesceNodes[2].ReturnType);
            Assert.AreEqual(typeof(char?), _coalesceNodes[3].ReturnType);
            Assert.AreEqual(typeof(double?), _coalesceNodes[4].ReturnType);
        }
    
        [Test]
        public void TestValidate()
        {
            ExprCoalesceNode coalesceNode = new ExprCoalesceNode();
            coalesceNode.AddChildNode(new SupportExprNode(1));
    
            // Test too few nodes under this node
            try
            {
                coalesceNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
    
            // Test node result type not fitting
            coalesceNode.AddChildNode(new SupportExprNode("s"));
            try
            {
                coalesceNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
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
            for (int i = 0; i < _coalesceNodes.Length; i++)
            {
                _coalesceNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            }
    
            Assert.AreEqual(4L, _coalesceNodes[0].Evaluate(new EvaluateParams(null, false, null)));
            Assert.AreEqual("a", _coalesceNodes[1].Evaluate(new EvaluateParams(null, false, null)));
            Assert.AreEqual(true, _coalesceNodes[2].Evaluate(new EvaluateParams(null, false, null)));
            Assert.AreEqual('b', _coalesceNodes[3].Evaluate(new EvaluateParams(null, false, null)));
            Assert.AreEqual(5D, _coalesceNodes[4].Evaluate(new EvaluateParams(null, false, null)));
        }
    
        [Test]
        public void TestEquals()
        {
            Assert.IsFalse(_coalesceNodes[0].EqualsNode(new ExprEqualsNodeImpl(true, false), false));
            Assert.IsTrue(_coalesceNodes[0].EqualsNode(_coalesceNodes[1], false));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            _coalesceNodes[0].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            Assert.AreEqual("coalesce(null,null,4)", _coalesceNodes[0].ToExpressionStringMinPrecedenceSafe());
        }
    }
}
