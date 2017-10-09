///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression
{
    [TestFixture]
    public class TestExprAndNode 
    {
        private ExprAndNode _andNode;
    
        [SetUp]
        public void SetUp()
        {
            _andNode = new ExprAndNodeImpl();
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(bool?), _andNode.ReturnType);
        }
    
        [Test]
        public void TestValidate()
        {
            // test success
            _andNode.AddChildNode(new SupportExprNode(typeof(Boolean)));
            _andNode.AddChildNode(new SupportExprNode(typeof(Boolean)));
            _andNode.Validate(SupportExprValidationContextFactory.MakeEmpty());
    
            // test failure, type mismatch
            _andNode.AddChildNode(new SupportExprNode(typeof(string)));
            try
            {
                _andNode.Validate(SupportExprValidationContextFactory.MakeEmpty());
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }
    
            // test failed - with just one child
            _andNode = new ExprAndNodeImpl();
            _andNode.AddChildNode(new SupportExprNode(typeof(Boolean)));
            try
            {
                _andNode.Validate(SupportExprValidationContextFactory.MakeEmpty());
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }
        }
    
        [Test]
        public void TestEvaluate()
        {
            _andNode.AddChildNode(new SupportBoolExprNode(true));
            _andNode.AddChildNode(new SupportBoolExprNode(true));
            SupportExprNodeUtil.Validate(_andNode);
            Assert.IsTrue(_andNode.Evaluate(new EvaluateParams(null, false, null)).AsBoolean());
    
            _andNode = new ExprAndNodeImpl();
            _andNode.AddChildNode(new SupportBoolExprNode(true));
            _andNode.AddChildNode(new SupportBoolExprNode(false));
            SupportExprNodeUtil.Validate(_andNode);
            Assert.IsFalse(_andNode.Evaluate(new EvaluateParams(null, false, null)).AsBoolean());
        }
    
        [Test]
        public void TestToExpressionString()
        {
            _andNode.AddChildNode(new SupportExprNode(true));
            _andNode.AddChildNode(new SupportExprNode(false));
    
            Assert.AreEqual("True and False", _andNode.ToExpressionStringMinPrecedenceSafe());
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(_andNode.EqualsNode(new ExprAndNodeImpl()));
            Assert.IsFalse(_andNode.EqualsNode(new ExprOrNode()));
        }
    }
}
