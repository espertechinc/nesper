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
using com.espertech.esper.type;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprRelationalOpNode 
    {
        private ExprRelationalOpNode _opNode;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _opNode = new ExprRelationalOpNodeImpl(RelationalOpEnum.GE);
        }
    
        [Test]
        public void TestGetType()
        {
            _opNode.AddChildNode(new SupportExprNode(typeof(long)));
            _opNode.AddChildNode(new SupportExprNode(typeof(int)));
            Assert.AreEqual(typeof(bool?), _opNode.ReturnType);
        }
    
        [Test]
        public void TestValidate()
        {
            // Test success
            _opNode.AddChildNode(new SupportExprNode(typeof(String)));
            _opNode.AddChildNode(new SupportExprNode(typeof(String)));
            _opNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
    
            _opNode.SetChildNodes(new SupportExprNode(typeof(String)));
    
            // Test too few nodes under this node
            try
            {
                _opNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // Expected
            }
    
            // Test mismatch type
            _opNode.AddChildNode(new SupportExprNode(typeof(int)));
            try
            {
                _opNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
    
            // As of 5.2, booleans are valid because they implement the IComparable interface
#if false
            // Test type cannot be compared
            _opNode.ChildNodes = new ExprNode[]{ new SupportExprNode(typeof(bool?)) };
            _opNode.AddChildNode(new SupportExprNode(typeof(bool?)));
    
            try
            {
                _opNode.Validate(ExprValidationContextFactory.MakeEmpty());
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }
#endif
        }
    
        [Test]
        public void TestEvaluate()
        {
            SupportExprNode childOne = new SupportExprNode("d");
            SupportExprNode childTwo = new SupportExprNode("c");
            _opNode.AddChildNode(childOne);
            _opNode.AddChildNode(childTwo);
            _opNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));       // Type initialization

            var eparams = new EvaluateParams(null, false, null);

            Assert.AreEqual(true, _opNode.Evaluate(eparams));
    
            childOne.Value = "c";
            Assert.AreEqual(true, _opNode.Evaluate(eparams));
    
            childOne.Value = "b";
            Assert.AreEqual(false, _opNode.Evaluate(eparams));
    
            _opNode = MakeNode(null, typeof(int), 2, typeof(int));
            Assert.AreEqual(null, _opNode.Evaluate(eparams));
            _opNode = MakeNode(1, typeof(int), null, typeof(int));
            Assert.AreEqual(null, _opNode.Evaluate(eparams));
            _opNode = MakeNode(null, typeof(int), null, typeof(int));
            Assert.AreEqual(null, _opNode.Evaluate(eparams));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            _opNode.AddChildNode(new SupportExprNode(10));
            _opNode.AddChildNode(new SupportExprNode(5));
            Assert.AreEqual("10>=5", _opNode.ToExpressionStringMinPrecedenceSafe());
        }
    
        private ExprRelationalOpNode MakeNode(Object valueLeft, Type typeLeft, Object valueRight, Type typeRight)
        {
            ExprRelationalOpNode relOpNode = new ExprRelationalOpNodeImpl(RelationalOpEnum.GE);
            relOpNode.AddChildNode(new SupportExprNode(valueLeft, typeLeft));
            relOpNode.AddChildNode(new SupportExprNode(valueRight, typeRight));
            SupportExprNodeUtil.Validate(relOpNode);
            return relOpNode;
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(_opNode.EqualsNode(_opNode, false));
            Assert.IsFalse(_opNode.EqualsNode(new ExprRelationalOpNodeImpl(RelationalOpEnum.LE), false));
            Assert.IsFalse(_opNode.EqualsNode(new ExprOrNode(), false));
        }
    }
}
