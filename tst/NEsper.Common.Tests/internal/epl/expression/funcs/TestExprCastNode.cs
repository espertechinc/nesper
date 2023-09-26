///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.common.@internal.type;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    [TestFixture]
    public class TestExprCastNode : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            castNodes = new ExprCastNode[2];

            castNodes[0] = new ExprCastNode(new ClassDescriptor("long"));
            castNodes[0].AddChildNode(new SupportExprNode(10L, typeof(long?)));

            castNodes[1] = new ExprCastNode(new ClassDescriptor(typeof(int).FullName));
            castNodes[1].AddChildNode(new SupportExprNode(0x10, typeof(byte)));
        }

        private ExprCastNode[] castNodes;

        [Test]
        public void TestEquals()
        {
            Assert.IsFalse(castNodes[0].EqualsNode(new ExprEqualsNodeImpl(true, false), false));
            Assert.IsFalse(castNodes[0].EqualsNode(castNodes[1], false));
            Assert.IsFalse(castNodes[0].EqualsNode(new ExprCastNode(new ClassDescriptor(typeof(int).FullName)), false));
        }

        [Test]
        public void TestEvaluate()
        {
            for (var i = 0; i < castNodes.Length; i++)
            {
                castNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            }

            Assert.AreEqual(10L, castNodes[0].Forge.ExprEvaluator.Evaluate(null, false, null));
            Assert.AreEqual(16, castNodes[1].Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test]
        public void TestGetType()
        {
            for (var i = 0; i < castNodes.Length; i++)
            {
                castNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            }

            Assert.AreEqual(typeof(long?), castNodes[0].TargetType);
            Assert.AreEqual(typeof(int?), castNodes[1].TargetType);
        }

        [Test]
        public void TestToExpressionString()
        {
            castNodes[0].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            Assert.AreEqual("cast(10L,long)", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(castNodes[0]));
        }

        [Test]
        public void TestValidate()
        {
            var castNode = new ExprCastNode(new ClassDescriptor("int"));

            // Test too few nodes under this node
            try
            {
                castNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    }
} // end of namespace
