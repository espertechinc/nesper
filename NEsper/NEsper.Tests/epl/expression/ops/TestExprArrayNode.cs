///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestExprArrayNode
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            _arrayNodes = new ExprArrayNode[4];
            _arrayNodes[0] = new ExprArrayNode();

            // no coercion array
            _arrayNodes[1] = new ExprArrayNode();
            _arrayNodes[1].AddChildNode(new SupportExprNode(2));
            _arrayNodes[1].AddChildNode(new SupportExprNode(3));

            // coercion
            _arrayNodes[2] = new ExprArrayNode();
            _arrayNodes[2].AddChildNode(new SupportExprNode(1.5D));
            _arrayNodes[2].AddChildNode(new SupportExprNode(1));

            // mixed types
            _arrayNodes[3] = new ExprArrayNode();
            _arrayNodes[3].AddChildNode(new SupportExprNode("a"));
            _arrayNodes[3].AddChildNode(new SupportExprNode(1));

            for (int i = 0; i < _arrayNodes.Length; i++)
            {
                _arrayNodes[i].Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
            }
        }
        
        private ExprArrayNode[] _arrayNodes;

        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(_arrayNodes[0].EqualsNode(_arrayNodes[1], false));
            Assert.IsFalse(_arrayNodes[0].EqualsNode(new SupportExprNode(null), false));
        }

        [Test]
        public void TestEvaluate()
        {
            Object result = _arrayNodes[0].Evaluate(new EvaluateParams(null, true, null));
            Assert.AreEqual(typeof(Object[]), result.GetType());
            Assert.AreEqual(0, ((Object[]) result).Length);

            result = _arrayNodes[1].Evaluate(new EvaluateParams(null, true, null));
            Assert.AreEqual(typeof(int?[]), result.GetType());
            Assert.AreEqual(2, ((int?[])result).Length);
            Assert.AreEqual(2, ((int?[])result)[0]);
            Assert.AreEqual(3, ((int?[])result)[1]);

            result = _arrayNodes[2].Evaluate(new EvaluateParams(null, true, null));
            Assert.AreEqual(typeof(double?[]), result.GetType());
            Assert.AreEqual(2, ((double?[])result).Length);
            Assert.AreEqual(1.5, ((double?[])result)[0]);
            Assert.AreEqual(1.0, ((double?[])result)[1]);

            result = _arrayNodes[3].Evaluate(new EvaluateParams(null, true, null));
            Assert.AreEqual(typeof(Object[]), result.GetType());
            Assert.AreEqual(2, ((Object[]) result).Length);
            Assert.AreEqual("a", ((Object[]) result)[0]);
            Assert.AreEqual(1, ((Object[]) result)[1]);
        }

        [Test]
        public void TestGetType()
        {
            Assert.AreEqual(typeof(Object[]), _arrayNodes[0].ReturnType);
            Assert.AreEqual(typeof(int?[]), _arrayNodes[1].ReturnType);
            Assert.AreEqual(typeof(double?[]), _arrayNodes[2].ReturnType);
            Assert.AreEqual(typeof(Object[]), _arrayNodes[3].ReturnType);
        }

        [Test]
        public void TestToExpressionString()
        {
            Assert.AreEqual("{}", _arrayNodes[0].ToExpressionStringMinPrecedenceSafe());
            Assert.AreEqual("{2,3}", _arrayNodes[1].ToExpressionStringMinPrecedenceSafe());
            Assert.AreEqual("{1.5,1}", _arrayNodes[2].ToExpressionStringMinPrecedenceSafe());
            Assert.AreEqual("{\"a\",1}", _arrayNodes[3].ToExpressionStringMinPrecedenceSafe());
        }
    }
}