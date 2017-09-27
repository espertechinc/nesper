///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.schedule;
using com.espertech.esper.supportunit.core;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression
{
    [TestFixture]
    public class TestExprTimestampNode 
    {
        private ExprTimestampNode _node;
        private ExprEvaluatorContext _context;
    
        [SetUp]
        public void SetUp()
        {
            _node = new ExprTimestampNode();
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(long?), _node.ReturnType);
        }
    
        [Test]
        public void TestValidate()
        {
            // Test too many nodes
            _node.AddChildNode(new SupportExprNode(1));
            try
            {
                _node.Validate(SupportExprValidationContextFactory.MakeEmpty());
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
            TimeProvider provider = new ProxyTimeProvider
            {
                Get = () => 99
            };

            _context = new SupportExprEvaluatorContext(provider);
            _node.Validate(new ExprValidationContext(null, null, null, null, provider, null, null, null, null, null, 1, null, null, null, false, false, false, false, null, false));

            Assert.AreEqual(99L, _node.Evaluate(new EvaluateParams(null, false, _context)));
        }
    
        [Test]
        public void TestEquals()
        {
            Assert.IsFalse(_node.EqualsNode(new ExprEqualsNodeImpl(true, false)));
            Assert.IsTrue(_node.EqualsNode(new ExprTimestampNode()));
        }
    
        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("current_timestamp()", _node.ToExpressionStringMinPrecedenceSafe());
        }
    }
}
