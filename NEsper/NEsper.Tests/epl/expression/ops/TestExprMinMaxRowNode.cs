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
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.type;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprMinMaxRowNode 
    {
        private ExprMinMaxRowNode _minMaxNode;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _minMaxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MAX);
        }
    
        [Test]
        public void TestGetType()
        {
            _minMaxNode.AddChildNode(new SupportExprNode(typeof(double?)));
            _minMaxNode.AddChildNode(new SupportExprNode(typeof(int)));
            _minMaxNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            Assert.AreEqual(typeof(double?), _minMaxNode.ReturnType);
    
            _minMaxNode.AddChildNode(new SupportExprNode(typeof(double?)));
            _minMaxNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            Assert.AreEqual(typeof(double?), _minMaxNode.ReturnType);
        }
    
        [Test]
        public void TestToExpressionString()
        {
            _minMaxNode.AddChildNode(new SupportExprNode(9d));
            _minMaxNode.AddChildNode(new SupportExprNode(6));
            Assert.AreEqual("max(9,6)", _minMaxNode.ToExpressionStringMinPrecedenceSafe());
            _minMaxNode.AddChildNode(new SupportExprNode(0.5d));
            Assert.AreEqual("max(9,6,0.5)", _minMaxNode.ToExpressionStringMinPrecedenceSafe());
        }
    
        [Test]
        public void TestValidate()
        {
            // Must have 2 or more subnodes
            try
            {
                _minMaxNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
    
            // Must have only number-type subnodes
            _minMaxNode.AddChildNode(new SupportExprNode(typeof(String)));
            _minMaxNode.AddChildNode(new SupportExprNode(typeof(int)));
            try
            {
                _minMaxNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
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
            var eparams = new EvaluateParams(null, false, null);

            _minMaxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MAX);
            SetupNode(_minMaxNode, 10, 1.5, null);
            Assert.AreEqual(10d, _minMaxNode.Evaluate(eparams));
    
            _minMaxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MAX);
            SetupNode(_minMaxNode, 1, 1.5, null);
            Assert.AreEqual(1.5d, _minMaxNode.Evaluate(eparams));
    
            _minMaxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MIN);
            SetupNode(_minMaxNode, 1, 1.5, null);
            Assert.AreEqual(1d, _minMaxNode.Evaluate(eparams));
    
            _minMaxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MAX);
            SetupNode(_minMaxNode, 1, 1.5, 2.0f);
            Assert.AreEqual(2.0d, _minMaxNode.Evaluate(eparams));
    
            _minMaxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MIN);
            SetupNode(_minMaxNode, 6, 3.5, 2.0f);
            Assert.AreEqual(2.0d, _minMaxNode.Evaluate(eparams));
    
            _minMaxNode = MakeNode(null, typeof(int), 5, typeof(int), 6, typeof(int));
            Assert.IsNull(_minMaxNode.Evaluate(eparams));
            _minMaxNode = MakeNode(7, typeof(int), null, typeof(int), 6, typeof(int));
            Assert.IsNull(_minMaxNode.Evaluate(eparams));
            _minMaxNode = MakeNode(3, typeof(int), 5, typeof(int), null, typeof(int));
            Assert.IsNull(_minMaxNode.Evaluate(eparams));
            _minMaxNode = MakeNode(null, typeof(int), null, typeof(int), null, typeof(int));
            Assert.IsNull(_minMaxNode.Evaluate(eparams));
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(_minMaxNode.EqualsNode(_minMaxNode, false));
            Assert.IsFalse(_minMaxNode.EqualsNode(new ExprMinMaxRowNode(MinMaxTypeEnum.MIN), false));
            Assert.IsFalse(_minMaxNode.EqualsNode(new ExprOrNode(), false));
        }
    
        private void SetupNode(ExprMinMaxRowNode nodeMin, int intValue, double doubleValue, float? floatValue)
        {
            nodeMin.AddChildNode(new SupportExprNode(intValue));
            nodeMin.AddChildNode(new SupportExprNode(doubleValue));
            if (floatValue != null)
            {
                nodeMin.AddChildNode(new SupportExprNode(floatValue));
            }
            nodeMin.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
        }
    
        private ExprMinMaxRowNode MakeNode(Object valueOne, Type typeOne,
                                           Object valueTwo, Type typeTwo,
                                           Object valueThree, Type typeThree)
        {
            var maxNode = new ExprMinMaxRowNode(MinMaxTypeEnum.MAX);
            maxNode.AddChildNode(new SupportExprNode(valueOne, typeOne));
            maxNode.AddChildNode(new SupportExprNode(valueTwo, typeTwo));
            maxNode.AddChildNode(new SupportExprNode(valueThree, typeThree));
            maxNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            return maxNode;
        }
    
    }
}
