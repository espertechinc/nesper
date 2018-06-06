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
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.type;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprOrNode 
    {
        private ExprOrNode _orNode;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
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
            _orNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
    
            // test failure, type mismatch
            _orNode.AddChildNode(new SupportExprNode(typeof(string)));
            try
            {
                _orNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
    
            // test failed - with just one child
            _orNode = new ExprOrNode();
            _orNode.AddChildNode(new SupportExprNode(typeof(Boolean)));
            try
            {
                _orNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
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
            Assert.IsTrue(_orNode.EqualsNode(_orNode, false));
            Assert.IsFalse(_orNode.EqualsNode(new ExprMinMaxRowNode(MinMaxTypeEnum.MIN), false));
            Assert.IsTrue(_orNode.EqualsNode(new ExprOrNode(), false));
        }
    }
}
