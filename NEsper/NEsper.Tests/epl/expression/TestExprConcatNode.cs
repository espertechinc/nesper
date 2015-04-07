///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.support.epl;
using com.espertech.esper.type;

using NUnit.Framework;


namespace com.espertech.esper.epl.expression
{
    [TestFixture]
    public class TestExprConcatNode 
    {
        private ExprConcatNode _concatNode;
    
        [SetUp]
        public void SetUp()
        {
            _concatNode = new ExprConcatNode();
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(string), _concatNode.ReturnType);
        }
    
        [Test]
        public void TestToExpressionString()
        {
            _concatNode = new ExprConcatNode();
            _concatNode.AddChildNode(new SupportExprNode("a"));
            _concatNode.AddChildNode(new SupportExprNode("b"));
            Assert.AreEqual("\"a\"||\"b\"", _concatNode.ToExpressionStringMinPrecedenceSafe());
            _concatNode.AddChildNode(new SupportExprNode("c"));
            Assert.AreEqual("\"a\"||\"b\"||\"c\"", _concatNode.ToExpressionStringMinPrecedenceSafe());
        }
    
        [Test]
        public void TestValidate()
        {
            // Must have 2 or more String subnodes
            try
            {
                _concatNode.Validate(ExprValidationContextFactory.MakeEmpty());
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }
    
            // Must have only string-type subnodes
            _concatNode.AddChildNode(new SupportExprNode(typeof(string)));
            _concatNode.AddChildNode(new SupportExprNode(typeof(int)));
            try
            {
                _concatNode.Validate(ExprValidationContextFactory.MakeEmpty());
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
            _concatNode.AddChildNode(new SupportExprNode("x"));
            _concatNode.AddChildNode(new SupportExprNode("y"));
            SupportExprNodeUtil.Validate(_concatNode);
            Assert.AreEqual("xy", _concatNode.Evaluate(new EvaluateParams(null, false, null)));
    
            _concatNode.AddChildNode(new SupportExprNode("z"));
            SupportExprNodeUtil.Validate(_concatNode);
            Assert.AreEqual("xyz", _concatNode.Evaluate(new EvaluateParams(null, false, null)));
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(_concatNode.EqualsNode(_concatNode));
            Assert.IsFalse(_concatNode.EqualsNode(new ExprMathNode(MathArithTypeEnum.DIVIDE, false, false)));
        }
    }
}
