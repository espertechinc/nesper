///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    [TestFixture]
    public class TestExprArrayNode : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            arrayNodes = new ExprArrayNode[4];
            arrayNodes[0] = new ExprArrayNode();

            // no coercion array
            arrayNodes[1] = new ExprArrayNode();
            arrayNodes[1].AddChildNode(new SupportExprNode(2));
            arrayNodes[1].AddChildNode(new SupportExprNode(3));

            // coercion
            arrayNodes[2] = new ExprArrayNode();
            arrayNodes[2].AddChildNode(new SupportExprNode(1.5D));
            arrayNodes[2].AddChildNode(new SupportExprNode(1));

            // mixed types
            arrayNodes[3] = new ExprArrayNode();
            arrayNodes[3].AddChildNode(new SupportExprNode("a"));
            arrayNodes[3].AddChildNode(new SupportExprNode(1));

            for (var i = 0; i < arrayNodes.Length; i++)
            {
                arrayNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(container));
            }
        }

        private ExprArrayNode[] arrayNodes;

        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(arrayNodes[0].EqualsNode(arrayNodes[1], false));
            Assert.IsFalse(arrayNodes[0].EqualsNode(new SupportExprNode(null), false));
        }

        [Test]
        public void TestEvaluate()
        {
            var result = arrayNodes[0].Forge.ExprEvaluator.Evaluate(null, true, null);
            Assert.AreEqual(typeof(object[]), result.GetType());
            Assert.AreEqual(0, ((object[]) result).Length);

            result = arrayNodes[1].Forge.ExprEvaluator.Evaluate(null, true, null);
            Assert.AreEqual(typeof(int?[]), result.GetType());
            Assert.AreEqual(2, ((int?[]) result).Length);
            Assert.AreEqual(2, (int) ((int?[]) result)[0]);
            Assert.AreEqual(3, (int) ((int?[]) result)[1]);

            result = arrayNodes[2].Forge.ExprEvaluator.Evaluate(null, true, null);
            Assert.That(result, Is.InstanceOf<double?[]>());
            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result, Has.Member(1.5));
            Assert.That(result, Has.Member(1.0));

            result = arrayNodes[3].Forge.ExprEvaluator.Evaluate(null, true, null);
            Assert.AreEqual(typeof(object[]), result.GetType());
            Assert.AreEqual(2, ((object[]) result).Length);
            Assert.AreEqual("a", ((object[]) result)[0]);
            Assert.AreEqual(1, ((object[]) result)[1]);
        }

        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(object[]), arrayNodes[0].Forge.EvaluationType);
            Assert.AreEqual(typeof(int?[]), arrayNodes[1].Forge.EvaluationType);
            Assert.AreEqual(typeof(double?[]), arrayNodes[2].Forge.EvaluationType);
            Assert.AreEqual(typeof(object[]), arrayNodes[3].Forge.EvaluationType);
        }

        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("{}", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(arrayNodes[0]));
            Assert.AreEqual("{2,3}", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(arrayNodes[1]));
            Assert.AreEqual("{1.5d,1}", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(arrayNodes[2]));
            Assert.AreEqual("{\"a\",1}", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(arrayNodes[3]));
        }
    }
} // end of namespace
