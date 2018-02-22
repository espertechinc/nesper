///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
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
    public class TestExprBitWiseNode
    {
    	private ExprBitWiseNode _bitWiseNode;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
    		_bitWiseNode = new ExprBitWiseNode(BitWiseOpEnum.BAND);
    	}
    
        [Test]
        public void TestValidate()
        {
            // Must have exactly 2 subnodes
            try
            {
                _bitWiseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            	Log.Debug("No nodes in the expression");
            }
    
            // Must have only number or boolean-type subnodes
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(string)));
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(int)));
            try
            {
                _bitWiseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    
        [Test]
        public void TestGetType()
        {
        	Log.Debug(".testGetType");
        	_bitWiseNode = new ExprBitWiseNode(BitWiseOpEnum.BAND);
        	_bitWiseNode.AddChildNode(new SupportExprNode(typeof(double?)));
        	_bitWiseNode.AddChildNode(new SupportExprNode(typeof(int)));
            try
            {
                _bitWiseNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            	Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
            _bitWiseNode = new ExprBitWiseNode(BitWiseOpEnum.BAND);
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(long)));
            _bitWiseNode.AddChildNode(new SupportExprNode(typeof(long)));
            ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.SELECT, _bitWiseNode, SupportExprValidationContextFactory.MakeEmpty(_container));
            Assert.AreEqual(typeof(long?), _bitWiseNode.ReturnType);
        }
    
        [Test]
        public void TestEvaluate()
        {
        	Log.Debug(".testEvaluate");
        	_bitWiseNode.AddChildNode(new SupportExprNode(10));
        	_bitWiseNode.AddChildNode(new SupportExprNode(12));
            ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.SELECT, _bitWiseNode, SupportExprValidationContextFactory.MakeEmpty(_container));
            Assert.AreEqual(8, _bitWiseNode.Evaluate(new EvaluateParams(null, false, null)));
        }
    
        [Test]
        public void TestEqualsNode()
        {
        	Log.Debug(".testEqualsNode");
        	_bitWiseNode = new ExprBitWiseNode(BitWiseOpEnum.BAND);
            Assert.IsTrue(_bitWiseNode.EqualsNode(_bitWiseNode, false));
            Assert.IsFalse(_bitWiseNode.EqualsNode(new ExprBitWiseNode(BitWiseOpEnum.BXOR), false));
        }
    
        [Test]
        public void TestToExpressionString()
        {
        	Log.Debug(".testToExpressionString");
        	_bitWiseNode = new ExprBitWiseNode(BitWiseOpEnum.BAND);
        	_bitWiseNode.AddChildNode(new SupportExprNode(4));
        	_bitWiseNode.AddChildNode(new SupportExprNode(2));
            Assert.AreEqual("4&2", _bitWiseNode.ToExpressionStringMinPrecedenceSafe());
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
