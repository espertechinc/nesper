///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    public class TestExprStddevNode : TestExprAggregateNodeAdapter
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            ValidatedNodeToTest = MakeNode(5, typeof(int));
        }
    
        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(double), ValidatedNodeToTest.ReturnType);
        }
    
        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("stddev(5)", ValidatedNodeToTest.ToExpressionStringMinPrecedenceSafe());
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(ValidatedNodeToTest.EqualsNode(ValidatedNodeToTest, false));
            Assert.IsFalse(ValidatedNodeToTest.EqualsNode(new ExprSumNode(false), false));
        }

        [Test]
        public override void TestEvaluate()
        {
            base.TestEvaluate();
        }

        private ExprStddevNode MakeNode(Object value, Type type)
        {
            ExprStddevNode stddevNode = new ExprStddevNode(false);
            stddevNode.AddChildNode(new SupportExprNode(value, type));
            SupportExprNodeFactory.Validate3Stream(stddevNode);
            return stddevNode;
        }
    }
}
