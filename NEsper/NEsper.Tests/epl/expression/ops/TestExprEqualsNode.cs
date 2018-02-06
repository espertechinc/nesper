///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprEqualsNode 
    {
        private ExprEqualsNode[] _equalsNodes;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _equalsNodes = new ExprEqualsNode[4];
            _equalsNodes[0] = new ExprEqualsNodeImpl(false, false);
    
            _equalsNodes[1] = new ExprEqualsNodeImpl(false, false);
            _equalsNodes[1].AddChildNode(new SupportExprNode(1L));
            _equalsNodes[1].AddChildNode(new SupportExprNode(1));
            _equalsNodes[1].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
    
            _equalsNodes[2] = new ExprEqualsNodeImpl(true, false);
            _equalsNodes[2].AddChildNode(new SupportExprNode(1.5D));
            _equalsNodes[2].AddChildNode(new SupportExprNode(1));
            _equalsNodes[2].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
    
            _equalsNodes[3] = new ExprEqualsNodeImpl(false, false);
            _equalsNodes[3].AddChildNode(new SupportExprNode(1D));
            _equalsNodes[3].AddChildNode(new SupportExprNode(1));
            _equalsNodes[3].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(bool?), _equalsNodes[1].ExprEvaluator.ReturnType);
        }
    
        [Test]
        public void TestValidate()
        {
            // Test success
            _equalsNodes[0].AddChildNode(new SupportExprNode(typeof(String)));
            _equalsNodes[0].AddChildNode(new SupportExprNode(typeof(String)));
            _equalsNodes[0].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));

            _equalsNodes[1].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            _equalsNodes[2].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            _equalsNodes[3].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));

            _equalsNodes[0].ChildNodes = new ExprNode[]
            {
                new SupportExprNode(typeof (String))
            };

            // Test too few nodes under this node
            try
            {
                _equalsNodes[0].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
    
            // Test mismatch type
            _equalsNodes[0].AddChildNode(new SupportExprNode(typeof(Boolean)));
            try
            {
                _equalsNodes[0].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    
        [Test]
        public void TestEvaluateEquals()
        {
            var eparams = new EvaluateParams(null, false, null);

            _equalsNodes[0] = MakeNode(true, false, false);
            Assert.IsFalse(_equalsNodes[0].ExprEvaluator.Evaluate(eparams).AsBoolean());
    
            _equalsNodes[0] = MakeNode(false, false, false);
            Assert.IsTrue(_equalsNodes[0].ExprEvaluator.Evaluate(eparams).AsBoolean());
    
            _equalsNodes[0] = MakeNode(true, true, false);
            Assert.IsTrue(_equalsNodes[0].ExprEvaluator.Evaluate(eparams).AsBoolean());
    
            _equalsNodes[0] = MakeNode(true, typeof(bool), null, typeof(bool), false);
            Assert.IsNull(_equalsNodes[0].ExprEvaluator.Evaluate(eparams));
    
            _equalsNodes[0] = MakeNode(null, typeof(String), "ss", typeof(String), false);
            Assert.IsNull(_equalsNodes[0].ExprEvaluator.Evaluate(eparams));
    
            _equalsNodes[0] = MakeNode(null, typeof(String), null, typeof(String), false);
            Assert.IsNull(_equalsNodes[0].ExprEvaluator.Evaluate(eparams));
    
            // try a long and int
            _equalsNodes[1].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            Assert.IsTrue(_equalsNodes[1].ExprEvaluator.Evaluate(eparams).AsBoolean());
    
            // try a double and int
            _equalsNodes[2].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            Assert.IsTrue(_equalsNodes[2].ExprEvaluator.Evaluate(eparams).AsBoolean());

            _equalsNodes[3].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            Assert.IsTrue(_equalsNodes[3].ExprEvaluator.Evaluate(eparams).AsBoolean());
        }
    
        [Test]
        public void TestEvaluateNotEquals()
        {
            var eparams = new EvaluateParams(null, false, null);

            _equalsNodes[0] = MakeNode(true, false, true);
            Assert.IsTrue(_equalsNodes[0].ExprEvaluator.Evaluate(eparams).AsBoolean());
    
            _equalsNodes[0] = MakeNode(false, false, true);
            Assert.IsFalse(_equalsNodes[0].ExprEvaluator.Evaluate(eparams).AsBoolean());
    
            _equalsNodes[0] = MakeNode(true, true, true);
            Assert.IsFalse(_equalsNodes[0].ExprEvaluator.Evaluate(eparams).AsBoolean());
    
            _equalsNodes[0] = MakeNode(true, typeof(Boolean), null, typeof(Boolean), true);
            Assert.IsNull(_equalsNodes[0].ExprEvaluator.Evaluate(eparams));
    
            _equalsNodes[0] = MakeNode(null, typeof(String), "ss", typeof(String), true);
            Assert.IsNull(_equalsNodes[0].ExprEvaluator.Evaluate(eparams));
    
            _equalsNodes[0] = MakeNode(null, typeof(String), null, typeof(String), true);
            Assert.IsNull(_equalsNodes[0].ExprEvaluator.Evaluate(eparams));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            _equalsNodes[0].AddChildNode(new SupportExprNode(true));
            _equalsNodes[0].AddChildNode(new SupportExprNode(false));
            Assert.AreEqual("True=False", _equalsNodes[0].ToExpressionStringMinPrecedenceSafe());
        }
    
        private ExprEqualsNode MakeNode(Object valueLeft, Object valueRight, bool isNot)
        {
            ExprEqualsNode equalsNode = new ExprEqualsNodeImpl(isNot, false);
            equalsNode.AddChildNode(new SupportExprNode(valueLeft));
            equalsNode.AddChildNode(new SupportExprNode(valueRight));
            SupportExprNodeUtil.Validate(equalsNode);
            return equalsNode;
        }
    
        private ExprEqualsNode MakeNode(Object valueLeft, Type typeLeft, Object valueRight, Type typeRight, bool isNot)
        {
            ExprEqualsNode equalsNode = new ExprEqualsNodeImpl(isNot, false);
            equalsNode.AddChildNode(new SupportExprNode(valueLeft, typeLeft));
            equalsNode.AddChildNode(new SupportExprNode(valueRight, typeRight));
            SupportExprNodeUtil.Validate(equalsNode);
            return equalsNode;
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(_equalsNodes[0].EqualsNode(_equalsNodes[1], false));
            Assert.IsFalse(_equalsNodes[0].EqualsNode(_equalsNodes[2], false));
        }
    }
}
