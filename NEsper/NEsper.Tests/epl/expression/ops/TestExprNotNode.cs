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
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.type;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprNotNode 
    {
        private ExprNotNode _notNode;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _notNode = new ExprNotNode();
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(bool?), _notNode.ReturnType);
        }
    
        [Test]
        public void TestValidate()
        {
            // fails with zero expressions
            try
            {
                _notNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
    
            // fails with too many sub-expressions
            _notNode.AddChildNode(new SupportExprNode(typeof(Boolean)));
            _notNode.AddChildNode(new SupportExprNode(typeof(Boolean)));
            try
            {
                _notNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
    
            // test failure, type mismatch
            _notNode = new ExprNotNode();
            _notNode.AddChildNode(new SupportExprNode(typeof(string)));
            try
            {
                _notNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
    
            // validates
            _notNode = new ExprNotNode();
            _notNode.AddChildNode(new SupportExprNode(typeof(Boolean)));
            _notNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
        }
    
        [Test]
        public void TestEvaluate()
        {
            _notNode.AddChildNode(new SupportBoolExprNode(true));
            SupportExprNodeUtil.Validate(_notNode);
            Assert.IsFalse(_notNode.Evaluate(new EvaluateParams(null, false, null)).AsBoolean());
    
            _notNode = new ExprNotNode();
            _notNode.AddChildNode(new SupportBoolExprNode(false));
            SupportExprNodeUtil.Validate(_notNode);
            Assert.IsTrue(_notNode.Evaluate(new EvaluateParams(null, false, null)).AsBoolean());
        }
    
        [Test]
        public void TestToExpressionString()
        {
            _notNode.AddChildNode(new SupportExprNode(true));
            Assert.AreEqual("not True", _notNode.ToExpressionStringMinPrecedenceSafe());
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(_notNode.EqualsNode(_notNode, false));
            Assert.IsFalse(_notNode.EqualsNode(new ExprMinMaxRowNode(MinMaxTypeEnum.MIN), false));
            Assert.IsFalse(_notNode.EqualsNode(new ExprOrNode(), false));
            Assert.IsTrue(_notNode.EqualsNode(new ExprNotNode(), false));
        }
    }
}
