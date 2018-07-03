///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.variable;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprVariableNode 
    {
        private ExprVariableNodeImpl _varNode;
        private VariableService _variableService;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _variableService = new VariableServiceImpl(_container, 100, null, null, null);
            _variableService.CreateNewVariable(null, "var1", "string", true, false, false, null, null);
            _variableService.CreateNewVariable(null, "dummy", "string", true, false, false, null, null);
            _variableService.CreateNewVariable(null, "IntPrimitive", "int", true, false, false, null, null);
            _varNode = new ExprVariableNodeImpl(_variableService.GetVariableMetaData("var1"), null);
        }
    
        [Test]
        public void TestGetType()  
        {
            SupportExprNodeFactory.Validate3Stream(_varNode);
            Assert.AreEqual(typeof(string), _varNode.ConstantType);
        }
    
        [Test]
        public void TestEvaluate()
        {
            SupportExprNodeFactory.Validate3Stream(_varNode);
            Assert.AreEqual(_varNode.Evaluate(new EvaluateParams(null, true, null)),"my_variable_value");
        }
    
        [Test]
        public void TestEquals()  
        {
            ExprInNode otherInNode = SupportExprNodeFactory.MakeInSetNode(false);
            ExprVariableNode otherVarOne = new ExprVariableNodeImpl(_variableService.GetVariableMetaData("dummy"), null);
            ExprVariableNode otherVarTwo = new ExprVariableNodeImpl(_variableService.GetVariableMetaData("var1"), null);
            ExprVariableNode otherVarThree = new ExprVariableNodeImpl(_variableService.GetVariableMetaData("var1"), "abc");
    
            Assert.IsTrue(_varNode.EqualsNode(_varNode, false));
            Assert.IsTrue(_varNode.EqualsNode(otherVarTwo, false));
            Assert.IsFalse(_varNode.EqualsNode(otherVarOne, false));
            Assert.IsFalse(_varNode.EqualsNode(otherInNode, false));
            Assert.IsFalse(otherVarTwo.EqualsNode(otherVarThree, false));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("var1", _varNode.ToExpressionStringMinPrecedenceSafe());
        }

        private static void TryInvalidValidate(ExprVariableNodeImpl varNode)
        {
            try {
                SupportExprNodeFactory.Validate3Stream(varNode);
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // expected
            }
        }
    }
}
