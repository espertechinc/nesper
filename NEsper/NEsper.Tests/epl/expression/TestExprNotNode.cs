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
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.support.epl;
using com.espertech.esper.type;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression
{
    [TestFixture]
    public class TestExprNotNode 
    {
        private ExprNotNode _notNode;
    
        [SetUp]
        public void SetUp()
        {
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
                _notNode.Validate(ExprValidationContextFactory.MakeEmpty());
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }
    
            // fails with too many sub-expressions
            _notNode.AddChildNode(new SupportExprNode(typeof(Boolean)));
            _notNode.AddChildNode(new SupportExprNode(typeof(Boolean)));
            try
            {
                _notNode.Validate(ExprValidationContextFactory.MakeEmpty());
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }
    
            // test failure, type mismatch
            _notNode = new ExprNotNode();
            _notNode.AddChildNode(new SupportExprNode(typeof(string)));
            try
            {
                _notNode.Validate(ExprValidationContextFactory.MakeEmpty());
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }
    
            // validates
            _notNode = new ExprNotNode();
            _notNode.AddChildNode(new SupportExprNode(typeof(Boolean)));
            _notNode.Validate(ExprValidationContextFactory.MakeEmpty());
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
            Assert.IsTrue(_notNode.EqualsNode(_notNode));
            Assert.IsFalse(_notNode.EqualsNode(new ExprMinMaxRowNode(MinMaxTypeEnum.MIN)));
            Assert.IsFalse(_notNode.EqualsNode(new ExprOrNode()));
            Assert.IsTrue(_notNode.EqualsNode(new ExprNotNode()));
        }
    }
}
