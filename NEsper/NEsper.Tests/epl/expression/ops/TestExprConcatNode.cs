///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
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
    public class TestExprConcatNode 
    {
        private ExprConcatNode _concatNode;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            _concatNode = new ExprConcatNode();
        }
    
        [Test]
        public void TestToExpressionString()
        {
            _concatNode = new ExprConcatNode();
            _concatNode.AddChildNode(new SupportExprNode("a"));
            _concatNode.AddChildNode(new SupportExprNode("b"));
            Assert.AreEqual("\"a\"||\"b\"", _concatNode.ToExpressionStringMinPrecedenceSafe());
            _concatNode.AddChildNode(new SupportExprNode("c"));
            Assert.AreEqual("\"a\"||\"b\"||\"c\"", _concatNode.ToExpressionStringMinPrecedenceSafe());
        }
    
        [Test]
        public void TestValidate()
        {
            // Must have 2 or more String subnodes
            try
            {
                _concatNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
    
            // Must have only string-type subnodes
            _concatNode.AddChildNode(new SupportExprNode(typeof(string)));
            _concatNode.AddChildNode(new SupportExprNode(typeof(int)));
            try
            {
                _concatNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container));
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
            _concatNode.AddChildNode(new SupportExprNode("x"));
            _concatNode.AddChildNode(new SupportExprNode("y"));
            SupportExprNodeUtil.Validate(_concatNode);
            Assert.AreEqual(typeof (string), _concatNode.ExprEvaluator.ReturnType);
            Assert.AreEqual("xy", _concatNode.ExprEvaluator.Evaluate(EvaluateParams.EmptyFalse));
    
            _concatNode.AddChildNode(new SupportExprNode("z"));
            SupportExprNodeUtil.Validate(_concatNode);
            Assert.AreEqual("xyz", _concatNode.ExprEvaluator.Evaluate(EvaluateParams.EmptyFalse));
        }
    
        [Test]
        public void TestEqualsNode()
        {
            Assert.IsTrue(_concatNode.EqualsNode(_concatNode, false));
            Assert.IsFalse(_concatNode.EqualsNode(new ExprMathNode(MathArithTypeEnum.DIVIDE, false, false), false));
        }

        [Test]
        public void TestThreading() 
        {
            runAssertionThreading(ConfigurationEngineDefaults.ThreadingProfile.LARGE);
            runAssertionThreading(ConfigurationEngineDefaults.ThreadingProfile.NORMAL);
        }

        private void runAssertionThreading(ConfigurationEngineDefaults.ThreadingProfile threadingProfile) 
        {
            _concatNode = new ExprConcatNode();
            var textA = "This is the first text";
            var textB = "Second text";
            var textC = "Third text, some more";
            foreach (var text in new[]{ textA, textB, textC }) {
                _concatNode.AddChildNode(new ExprConstantNodeImpl(text));
            }
            _concatNode.Validate(SupportExprValidationContextFactory.MakeEmpty(_container, threadingProfile));

            var numThreads = 4;
            var numLoop = 10000;

            var threads = new List<SupportConcatThread>(numThreads);
            for (var i = 0; i < numThreads; i++) {
                var thread = new SupportConcatThread(_concatNode, numLoop, textA + textB + textC);
                threads.Add(thread);
                thread.Start();
            }

            foreach (var thread in threads) {
                thread.Join();
                Assert.IsFalse(thread.IsFail);
            }
        }

        private class SupportConcatThread
        {
            private readonly ExprConcatNode _node;
            private readonly int _numLoop;
            private readonly string _expectedResult;
            private readonly Thread _thread;

            public SupportConcatThread(ExprConcatNode node, int numLoop, string expectedResult)
            {
                _node = node;
                _numLoop = numLoop;
                _expectedResult = expectedResult;
                _thread = new Thread(Run);
            }

            public void Start()
            {
                _thread.Start();
            }

            public void Join()
            {
                _thread.Join();
            }

            public void Run()
            {
                var eval = _node.ExprEvaluator;
                for(var i = 0; i < _numLoop; i++) {
                    var result = (string) eval.Evaluate(new EvaluateParams(null, true, null));
                    if (!_expectedResult.Equals(result)) {
                        IsFail = true;
                        break;
                    }
                }
            }

            public bool IsFail { get; private set; }
        }
    }
}
