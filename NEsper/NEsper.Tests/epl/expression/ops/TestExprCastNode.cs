///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprCastNode 
    {
        private ExprCastNode[] _castNodes;
    
        [SetUp]
        public void SetUp()
        {
            _castNodes = new ExprCastNode[2];
    
            _castNodes[0] = new ExprCastNode("long");
            _castNodes[0].AddChildNode(new SupportExprNode(10L, typeof(long)));
    
            _castNodes[1] = new ExprCastNode("" + typeof(int?).FullName + "");
            _castNodes[1].AddChildNode(new SupportExprNode(0x10, typeof(byte)));
        }
    
        [Test]
        public void TestGetType()
        {
            for (int i = 0; i < _castNodes.Length; i++)
            {
                _castNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty());
            }

            Assert.AreEqual(typeof(long?), _castNodes[0].TargetType);
            Assert.AreEqual(typeof(int?), _castNodes[1].TargetType);
        }
    
        [Test]
        public void TestValidate()
        {
            ExprCastNode castNode = new ExprCastNode("int");
    
            // Test too few nodes under this node
            try
            {
                castNode.Validate(SupportExprValidationContextFactory.MakeEmpty());
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
            for (int i = 0; i < _castNodes.Length; i++)
            {
                _castNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty());
            }
    
            Assert.AreEqual(10L, _castNodes[0].ExprEvaluator.Evaluate(new EvaluateParams(null, false, null)));
            Assert.AreEqual(16, _castNodes[1].ExprEvaluator.Evaluate(new EvaluateParams(null, false, null)));
        }
    
        [Test]
        public void TestEquals()
        {
            Assert.IsFalse(_castNodes[0].EqualsNode(new ExprEqualsNodeImpl(true, false)));
            Assert.IsFalse(_castNodes[0].EqualsNode(_castNodes[1]));
            Assert.IsFalse(_castNodes[0].EqualsNode(new ExprCastNode("" + typeof(int?).FullName + "")));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            _castNodes[0].Validate(SupportExprValidationContextFactory.MakeEmpty());
            Assert.AreEqual("cast(10,long)", _castNodes[0].ToExpressionStringMinPrecedenceSafe());
        }
    }
}
