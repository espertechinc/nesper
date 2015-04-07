///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.support.epl;

using NUnit.Framework;


namespace com.espertech.esper.epl.expression
{
    [TestFixture]
    public class TestExprPropertyExistsNode 
    {
        private ExprPropertyExistsNode[] _existsNodes;
    
        [SetUp]
        public void SetUp()
        {
            _existsNodes = new ExprPropertyExistsNode[2];
    
            _existsNodes[0] = new ExprPropertyExistsNode();
            _existsNodes[0].AddChildNode(SupportExprNodeFactory.MakeIdentNode("dummy?", "s0"));
    
            _existsNodes[1] = new ExprPropertyExistsNode();
            _existsNodes[1].AddChildNode(SupportExprNodeFactory.MakeIdentNode("BoolPrimitive?", "s0"));
        }
    
        [Test]
        public void TestGetType()
        {
            for (int i = 0; i < _existsNodes.Length; i++)
            {
                _existsNodes[i].Validate(ExprValidationContextFactory.MakeEmpty());
                Assert.AreEqual(typeof(bool?), _existsNodes[i].ReturnType);
            }
        }
    
        [Test]
        public void TestValidate()
        {
            ExprPropertyExistsNode castNode = new ExprPropertyExistsNode();
    
            // Test too few nodes under this node
            try
            {
                castNode.Validate(ExprValidationContextFactory.MakeEmpty());
                Assert.Fail();
            }
            catch (ExprValidationException ex)
            {
                // Expected
            }
    
            castNode.AddChildNode(new SupportExprNode(1));
            try
            {
                castNode.Validate(ExprValidationContextFactory.MakeEmpty());
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
            for (int i = 0; i < _existsNodes.Length; i++)
            {
                _existsNodes[i].Validate(ExprValidationContextFactory.MakeEmpty());
            }
    
            Assert.AreEqual(false, _existsNodes[0].Evaluate(new EvaluateParams(new EventBean[3], false, null)));
            Assert.AreEqual(false, _existsNodes[1].Evaluate(new EvaluateParams(new EventBean[3], false, null)));
    
            EventBean[] events = new EventBean[] {TestExprIdentNode.MakeEvent(10)};
            Assert.AreEqual(false, _existsNodes[0].Evaluate(new EvaluateParams(events, false, null)));
            Assert.AreEqual(true, _existsNodes[1].Evaluate(new EvaluateParams(events, false, null)));
        }
    
        [Test]
        public void TestEquals()
        {
            Assert.IsFalse(_existsNodes[0].EqualsNode(new ExprEqualsNodeImpl(true, false)));
            Assert.IsTrue(_existsNodes[0].EqualsNode(_existsNodes[1]));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            _existsNodes[0].Validate(ExprValidationContextFactory.MakeEmpty());
            Assert.AreEqual("exists(s0.dummy?)", _existsNodes[0].ToExpressionStringMinPrecedenceSafe());
        }
    }
}
