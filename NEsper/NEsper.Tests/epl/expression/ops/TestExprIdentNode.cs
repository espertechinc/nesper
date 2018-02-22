///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;

using com.espertech.esper.compat.logging;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprIdentNode 
    {
        private ExprIdentNode[] _identNodes;
        private StreamTypeService _streamTypeService;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _identNodes = new ExprIdentNode[4];
            _identNodes[0] = new ExprIdentNodeImpl("Mapped('a')");
            _identNodes[1] = new ExprIdentNodeImpl("NestedValue", "Nested");
            _identNodes[2] = new ExprIdentNodeImpl("Indexed[1]", "s2");
            _identNodes[3] = new ExprIdentNodeImpl("IntPrimitive", "s0");
    
            _streamTypeService = new SupportStreamTypeSvc3Stream();
        }
    
        [Test]
        public void TestValidateInvalid()
        {
            try
            {
                var xx = _identNodes[0].StreamId;
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // expected
            }
    
            Assert.IsNull(_identNodes[0].ExprEvaluator);
    
            try
            {
                var xx = _identNodes[0].ResolvedStreamName;
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // expected
            }
    
            try
            {
                var xx = _identNodes[0].ResolvedPropertyName;
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // expected
            }
        }
    
        [Test]
        public void TestValidate()
        {
            _identNodes[0].Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
            Assert.AreEqual(2, _identNodes[0].StreamId);
            Assert.AreEqual(typeof(string), _identNodes[0].ExprEvaluator.ReturnType);
            Assert.AreEqual("Mapped('a')", _identNodes[0].ResolvedPropertyName);

            _identNodes[1].Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
            Assert.AreEqual(2, _identNodes[1].StreamId);
            Assert.AreEqual(typeof(string), _identNodes[1].ExprEvaluator.ReturnType);
            Assert.AreEqual("Nested.NestedValue", _identNodes[1].ResolvedPropertyName);

            _identNodes[2].Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
            Assert.AreEqual(2, _identNodes[2].StreamId);
            Assert.AreEqual(typeof(int), _identNodes[2].ExprEvaluator.ReturnType);
            Assert.AreEqual("Indexed[1]", _identNodes[2].ResolvedPropertyName);

            _identNodes[3].Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
            Assert.AreEqual(0, _identNodes[3].StreamId);
            Assert.AreEqual(typeof(int), _identNodes[3].ExprEvaluator.ReturnType);
            Assert.AreEqual("IntPrimitive", _identNodes[3].ResolvedPropertyName);
    
            TryInvalidValidate(new ExprIdentNodeImpl(""));
            TryInvalidValidate(new ExprIdentNodeImpl("dummy"));
            TryInvalidValidate(new ExprIdentNodeImpl("Nested", "s0"));
            TryInvalidValidate(new ExprIdentNodeImpl("dummy", "s2"));
            TryInvalidValidate(new ExprIdentNodeImpl("IntPrimitive", "s2"));
            TryInvalidValidate(new ExprIdentNodeImpl("IntPrimitive", "s3"));
        }
    
        [Test]
        public void TestGetType()
        {
            // test success
            _identNodes[0].Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
            Assert.AreEqual(typeof(string), _identNodes[0].ExprEvaluator.ReturnType);
        }
    
        [Test]
        public void TestEvaluate()
        {
            EventBean[] events = new EventBean[] {MakeEvent(10)};

            _identNodes[3].Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
            Assert.AreEqual(10, _identNodes[3].ExprEvaluator.Evaluate(new EvaluateParams(events, false, null)));
            Assert.IsNull(_identNodes[3].ExprEvaluator.Evaluate(new EvaluateParams(new EventBean[2], false, null)));
        }
    
        [Test]
        public void TestEvaluatePerformance()
        {
            // test performance of evaluate for indexed events
            // fails if the getter is not in place

            EventBean[] events = SupportStreamTypeSvc3Stream.SampleEvents;
            _identNodes[2].Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
    
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 100000; i++)
            {
                _identNodes[2].ExprEvaluator.Evaluate(new EvaluateParams(events, false, null));
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;
            Log.Info(".testEvaluate delta=" + delta);
            Assert.IsTrue(delta < 500);
        }
    
        [Test]
        public void TestToExpressionString()
        {
            for (int i = 0; i < _identNodes.Length; i++)
            {
                _identNodes[i].Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
            }
            Assert.AreEqual("Mapped('a')", _identNodes[0].ToExpressionStringMinPrecedenceSafe());
            Assert.AreEqual("Nested.NestedValue", _identNodes[1].ToExpressionStringMinPrecedenceSafe());
            Assert.AreEqual("s2.Indexed[1]", _identNodes[2].ToExpressionStringMinPrecedenceSafe());
            Assert.AreEqual("s0.IntPrimitive", _identNodes[3].ToExpressionStringMinPrecedenceSafe());
        }
    
        [Test]
        public void TestEqualsNode()
        {
            _identNodes[0].Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
            _identNodes[2].Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
            _identNodes[3].Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
            Assert.IsTrue(_identNodes[3].EqualsNode(_identNodes[3], false));
            Assert.IsFalse(_identNodes[0].EqualsNode(_identNodes[2], false));
        }
    
        protected internal static EventBean MakeEvent(int intPrimitive)
        {
            SupportBean theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            return SupportEventBeanFactory.CreateObject(theEvent);
        }
    
        private void TryInvalidValidate(ExprIdentNode identNode)
        {
            try
            {
                identNode.Validate(SupportExprValidationContextFactory.Make(_container, _streamTypeService));
                Assert.Fail();
            }
            catch(ExprValidationException)
            {
                // expected
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
