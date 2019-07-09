///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    [TestFixture]
    public class TestExprIdentNode : AbstractTestBase
    {
        private ExprIdentNode[] identNodes;
        private SupportStreamTypeSvc3Stream streamTypeService;

        [SetUp]
        public void SetUp()
        {
            identNodes = new ExprIdentNode[4];
            identNodes[0] = new ExprIdentNodeImpl("Mapped('a')");
            identNodes[1] = new ExprIdentNodeImpl("NestedValue", "Nested");
            identNodes[2] = new ExprIdentNodeImpl("Indexed[1]", "s2");
            identNodes[3] = new ExprIdentNodeImpl("IntPrimitive", "s0");

            streamTypeService = new SupportStreamTypeSvc3Stream(supportEventTypeFactory);
        }

        [Test]
        public void TestValidateInvalid()
        {
            try
            {
                Assert.That(() => identNodes[0].StreamId, Throws.Nothing);
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // expected
            }

            try
            {
                Assert.IsNull(identNodes[0].Forge.ExprEvaluator);
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // expected
            }

            try
            {
                Assert.That(() => identNodes[0].ResolvedStreamName, Throws.Nothing);
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // expected
            }

            try
            {
                Assert.That(() => identNodes[0].ResolvedPropertyName, Throws.Nothing);
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
            identNodes[0].Validate(SupportExprValidationContextFactory.Make(container, streamTypeService));
            Assert.AreEqual(2, identNodes[0].StreamId);
            Assert.AreEqual(typeof(string), identNodes[0].Forge.EvaluationType);
            Assert.AreEqual("Mapped('a')", identNodes[0].ResolvedPropertyName);

            identNodes[1].Validate(SupportExprValidationContextFactory.Make(container, streamTypeService));
            Assert.AreEqual(2, identNodes[1].StreamId);
            Assert.AreEqual(typeof(string), identNodes[1].Forge.EvaluationType);
            Assert.AreEqual("Nested.NestedValue", identNodes[1].ResolvedPropertyName);

            identNodes[2].Validate(SupportExprValidationContextFactory.Make(container, streamTypeService));
            Assert.AreEqual(2, identNodes[2].StreamId);
            Assert.AreEqual(typeof(int?), identNodes[2].Forge.EvaluationType);
            Assert.AreEqual("Indexed[1]", identNodes[2].ResolvedPropertyName);

            identNodes[3].Validate(SupportExprValidationContextFactory.Make(container, streamTypeService));
            Assert.AreEqual(0, identNodes[3].StreamId);
            Assert.AreEqual(typeof(int?), identNodes[3].Forge.EvaluationType);
            Assert.AreEqual("IntPrimitive", identNodes[3].ResolvedPropertyName);

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
            identNodes[0].Validate(SupportExprValidationContextFactory.Make(container, streamTypeService));
            Assert.AreEqual(typeof(string), identNodes[0].Forge.EvaluationType);
        }

        [Test]
        public void TestEvaluate()
        {
            EventBean[] events = new[] { MakeEvent(10) };

            identNodes[3].Validate(SupportExprValidationContextFactory.Make(container, streamTypeService));
            Assert.AreEqual(10, identNodes[3].Forge.ExprEvaluator.Evaluate(events, false, null));
            Assert.IsNull(identNodes[3].Forge.ExprEvaluator.Evaluate(new EventBean[2], false, null));
        }

        [Test]
        public void TestEvaluatePerformance()
        {
            // test performance of evaluate for indexed events
            // fails if the getter is not in place

            EventBean[] events = streamTypeService.SampleEvents;
            identNodes[2].Validate(SupportExprValidationContextFactory.Make(container, streamTypeService));

            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 100000; i++)
            {
                identNodes[2].Forge.ExprEvaluator.Evaluate(events, false, null);
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;
            Log.Info(".testEvaluate delta=" + delta);
            Assert.IsTrue(delta < 500);
        }

        [Test]
        public void TestToExpressionString()
        {
            for (int i = 0; i < identNodes.Length; i++)
            {
                identNodes[i].Validate(SupportExprValidationContextFactory.Make(container, streamTypeService));
            }
            Assert.AreEqual("Mapped('a')", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(identNodes[0]));
            Assert.AreEqual("Nested.NestedValue", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(identNodes[1]));
            Assert.AreEqual("s2.Indexed[1]", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(identNodes[2]));
            Assert.AreEqual("s0.IntPrimitive", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(identNodes[3]));
        }

        [Test]
        public void TestEqualsNode()
        {
            identNodes[0].Validate(SupportExprValidationContextFactory.Make(container, streamTypeService));
            identNodes[2].Validate(SupportExprValidationContextFactory.Make(container, streamTypeService));
            identNodes[3].Validate(SupportExprValidationContextFactory.Make(container, streamTypeService));
            Assert.IsTrue(identNodes[3].EqualsNode(identNodes[3], false));
            Assert.IsFalse(identNodes[0].EqualsNode(identNodes[2], false));
        }

        protected EventBean MakeEvent(int intPrimitive)
        {
            SupportBean theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            return SupportEventBeanFactory.CreateObject(supportEventTypeFactory, theEvent);
        }

        private void TryInvalidValidate(ExprIdentNode identNode)
        {
            try
            {
                identNode.Validate(SupportExprValidationContextFactory.Make(container, streamTypeService));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // expected
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
