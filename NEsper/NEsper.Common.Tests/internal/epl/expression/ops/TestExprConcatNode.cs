///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    [TestFixture]
    public class TestExprConcatNode : AbstractCommonTest
    {
        private ExprConcatNode concatNode;

        [SetUp]
        public void SetUp()
        {
            concatNode = new ExprConcatNode();
        }

        private void RunAssertionThreading(ThreadingProfile threadingProfile)
        {
            concatNode = new ExprConcatNode();
            var textA = "This is the first text";
            var textB = "Second text";
            var textC = "Third text, some more";
            foreach (var text in Arrays.AsList(textA, textB, textC))
            {
                concatNode.AddChildNode(new ExprConstantNodeImpl(text));
            }

            concatNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container, threadingProfile));

            var numThreads = 4;
            var numLoop = 10000;

            var threads = new List<Pair<Thread, SupportConcat>>(numThreads);
            for (var i = 0; i < numThreads; i++)
            {
                var supportConcat = new SupportConcat(concatNode, numLoop, textA + textB + textC);
                var thread = new Thread(supportConcat.Run);
                threads.Add(new Pair<Thread, SupportConcat>(thread, supportConcat));
                thread.Start();
            }

            foreach (var threadPair in threads)
            {
                threadPair.First.Join();
                Assert.IsFalse(threadPair.Second.IsFail);
            }
        }

        private class SupportConcat
        {
            private readonly string expectedResult;
            private readonly ExprConcatNode node;
            private readonly int numLoop;

            public SupportConcat(
                ExprConcatNode node,
                int numLoop,
                string expectedResult)
            {
                this.node = node;
                this.numLoop = numLoop;
                this.expectedResult = expectedResult;
            }

            public bool IsFail { get; private set; }

            public void Run()
            {
                var eval = node.Forge.ExprEvaluator;
                for (var i = 0; i < numLoop; i++)
                {
                    var result = (string) eval.Evaluate(null, true, null);
                    if (!expectedResult.Equals(result))
                    {
                        IsFail = true;
                        break;
                    }
                }
            }
        }

        [Test, RunInApplicationDomain]
        public void TestEqualsNode()
        {
            Assert.IsTrue(concatNode.EqualsNode(concatNode, false));
            Assert.IsFalse(concatNode.EqualsNode(new ExprMathNode(MathArithTypeEnum.DIVIDE, false, false), false));
        }

        [Test, RunInApplicationDomain]
        public void TestEvaluate()
        {
            concatNode.AddChildNode(new SupportExprNode("x"));
            concatNode.AddChildNode(new SupportExprNode("y"));
            SupportExprNodeUtil.Validate(container, concatNode);
            Assert.AreEqual(typeof(string), concatNode.Forge.EvaluationType);
            Assert.AreEqual("xy", concatNode.Forge.ExprEvaluator.Evaluate(null, false, null));

            concatNode.AddChildNode(new SupportExprNode("z"));
            SupportExprNodeUtil.Validate(container, concatNode);
            Assert.AreEqual("xyz", concatNode.Forge.ExprEvaluator.Evaluate(null, false, null));
        }

        [Test, RunInApplicationDomain]
        public void TestThreading()
        {
            RunAssertionThreading(ThreadingProfile.LARGE);
            RunAssertionThreading(ThreadingProfile.NORMAL);
        }

        [Test, RunInApplicationDomain]
        public void TestToExpressionString()
        {
            concatNode = new ExprConcatNode();
            concatNode.AddChildNode(new SupportExprNode("a"));
            concatNode.AddChildNode(new SupportExprNode("b"));
            Assert.AreEqual("\"a\"||\"b\"", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(concatNode));
            concatNode.AddChildNode(new SupportExprNode("c"));
            Assert.AreEqual("\"a\"||\"b\"||\"c\"", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(concatNode));
        }

        [Test, RunInApplicationDomain]
        public void TestValidate()
        {
            // Must have 2 or more String subnodes
            try
            {
                concatNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }

            // Must have only string-type subnodes
            concatNode.AddChildNode(new SupportExprNode(typeof(string)));
            concatNode.AddChildNode(new SupportExprNode(typeof(int?)));
            try
            {
                concatNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
                Assert.Fail();
            }
            catch (ExprValidationException)
            {
                // Expected
            }
        }
    }
} // end of namespace
