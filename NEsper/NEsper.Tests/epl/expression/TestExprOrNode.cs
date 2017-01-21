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
    public class TestExprOrNode 
    {
        private ExprOrNode _orNode;
    
        [SetUp]
        public void SetUp()
        {
            _orNode = new ExprOrNode();
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(bool?), _orNode.ReturnType);
        }
    
        [Test]
        public void TestValidate()
        {
            // test success
            _orNode.AddChildNode(new SupportExprNode(typeof(Boolean)));
            _orNode.AddChildNode(new SupportExprNode(typeof(Boolean)));
            _orNode.Validate(ExprValidationContextFactory.MakeEmpty());
    
            // test failure, type mismatch
            _orNode.AddChildNode(new SupportExprNode(typeof(string)));
            try
            {
                _orNode.Validate(ExprValidationContextFactory.MakeEmpty());
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }
    
            // test failed - with just one child
            _orNode = new ExprOrNode();
            _orNode.AddChildNode(new SupportExprNode(typeof(Boolean)));
            try
            {
                _orNode.Validate(ExprValidationContextFactory.MakeEmpty());
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
            _orNode.AddChildNode(new SupportBoolExprNode(true));
            _orNode.AddChildNode(new SupportBoolExprNode(false));
            SupportExprNodeUtil.Validate(_orNode);
            Assert.IsTrue(_orNode.Evaluate(new EvaluateParams(null, false, null)).AsBoolean());
    
            _orNode = new ExprOrNode();
            _orNode.AddChildNode(new SupportBoolExprNode(false));
            _orNode.AddChildNode(new SupportBoolExprNode(false));
            SupportExprNodeUtil.Validate(_orNode);
            Assert.IsFalse(_orNode.Evaluate(new EvaluateParams(null, false, null)).AsBoolean());
    
            _orNode = new ExprOrNode();
            _orNode.AddChildNode(new SupportExprNode(null, typeof(Boolean)));
            _orNode.AddChildNode(new SupportExprNode(false));
            SupportExprNodeUtil.Validate(_orNode);
            Assert.IsNull(_orNode.Evaluate(new EvaluateParams(null, false, null)));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            _orNode.AddChildNode(new SupportExprNode(true));
            _orNode.AddChildNode(new SupportExprNode(false));
            Assert.AreEqual("True or False", _orNode.ToExpressionStringMinPrecedenceSafe());
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(_orNode.EqualsNode(_orNode));
            Assert.IsFalse(_orNode.EqualsNode(new ExprMinMaxRowNode(MinMaxTypeEnum.MIN)));
            Assert.IsTrue(_orNode.EqualsNode(new ExprOrNode()));
        }
    }
}
