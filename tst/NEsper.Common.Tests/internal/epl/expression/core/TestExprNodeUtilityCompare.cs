///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.supportunit.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCompare;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    [TestFixture]
    public class TestExprNodeUtilityCompare : AbstractCommonTest
    {
        private readonly SupportExprNode e1 = new SupportExprNode(1);
        private readonly SupportExprNode e2 = new SupportExprNode(2);
        private readonly SupportExprNode e3 = new SupportExprNode(3);
        private readonly SupportExprNode e4 = new SupportExprNode(4);
        private readonly SupportExprNode e1Dup = new SupportExprNode(1);
        private ExprNode[] empty;
        private ExprNode[] justE1;

        [SetUp]
        public void SetupTest()
        {
            empty = new ExprNode[0];
            justE1 = new ExprNode[] {e1};
        }

        private void ComparePermutations(
            bool expected,
            ExprNode[] setOne,
            ExprNode[] setTwo)
        {
            if (setTwo.Length == 0) {
                CompareSingle(expected, setOne, setTwo);
                return;
            }

            var permuter = PermutationEnumerator.Create(setTwo.Length).GetEnumerator();
            for (; permuter.MoveNext();) {
                var permutation = permuter.Current;
                var copy = new ExprNode[setTwo.Length];
                for (var i = 0; i < permutation.Length; i++) {
                    copy[i] = setTwo[permutation[i]];
                }

                CompareSingle(expected, setOne, copy);
            }
        }

        private void CompareSingle(
            bool expected,
            ExprNode[] setOne,
            ExprNode[] setTwo)
        {
            ClassicAssert.AreEqual(expected, DeepEqualsIgnoreDupAndOrder(setOne, setTwo));
            ClassicAssert.AreEqual(expected, DeepEqualsIgnoreDupAndOrder(setTwo, setOne));
        }

        [Test]
        public void TestDeepEquals()
        {
            ClassicAssert.IsFalse(DeepEquals(supportExprNodeFactory.Make2SubNodeAnd(), supportExprNodeFactory.Make3SubNodeAnd(), false));
            ClassicAssert.IsFalse(DeepEquals(supportExprNodeFactory.MakeEqualsNode(), supportExprNodeFactory.MakeMathNode(), false));
            ClassicAssert.IsTrue(DeepEquals(supportExprNodeFactory.MakeMathNode(), supportExprNodeFactory.MakeMathNode(), false));
            ClassicAssert.IsFalse(DeepEquals(supportExprNodeFactory.MakeMathNode(), supportExprNodeFactory.Make2SubNodeAnd(), false));
            ClassicAssert.IsTrue(DeepEquals(supportExprNodeFactory.Make3SubNodeAnd(), supportExprNodeFactory.Make3SubNodeAnd(), false));
        }

        [Test]
        public void TestDeepEqualsIgnoreOrder()
        {
            // compare on set being empty
            ComparePermutations(true, empty, empty);
            ComparePermutations(false, new ExprNode[] {e1}, empty);

            // compare single
            ComparePermutations(true, justE1, justE1);
            ComparePermutations(true, justE1, new ExprNode[] {e1Dup});
            ComparePermutations(false, justE1, new ExprNode[] {e2});
            ComparePermutations(false, new ExprNode[] {e2}, new ExprNode[] {e3});

            // compare two (same number of expressions)
            ComparePermutations(true, new ExprNode[] {e1, e2}, new ExprNode[] {e1, e2});
            ComparePermutations(true, new ExprNode[] {e1, e2}, new ExprNode[] {e2, e1});
            ComparePermutations(false, new ExprNode[] {e3, e2}, new ExprNode[] {e2, e1});
            ComparePermutations(false, new ExprNode[] {e1, e2}, new ExprNode[] {e1, e3});

            // compare three (same number of expressions)
            ComparePermutations(true, new ExprNode[] {e1, e2, e3}, new ExprNode[] {e1, e2, e3});
            ComparePermutations(false, new ExprNode[] {e1, e2, e3}, new ExprNode[] {e1, e2, e4});
            ComparePermutations(false, new ExprNode[] {e1, e2, e3}, new ExprNode[] {e1, e4, e3});
            ComparePermutations(false, new ExprNode[] {e1, e2, e3}, new ExprNode[] {e4, e2, e3});

            // duplicates allowed and ignored
            ComparePermutations(true, new ExprNode[] {e1}, new ExprNode[] {e1, e1});
            ComparePermutations(false, new ExprNode[] {e1}, new ExprNode[] {e1, e2});
            ComparePermutations(true, new ExprNode[] {e1}, new ExprNode[] {e1, e1, e1});
            ComparePermutations(false, new ExprNode[] {e2}, new ExprNode[] {e2, e2, e1});
            ComparePermutations(true, new ExprNode[] {e1, e1, e2, e2}, new ExprNode[] {e2, e2, e1});
            ComparePermutations(false, new ExprNode[] {e1, e1, e2, e2}, new ExprNode[] {e1, e1, e1});
            ComparePermutations(true, new ExprNode[] {e2, e1, e2}, new ExprNode[] {e2, e1});
        }

        [Test]
        public void TestDeepEqualsIsSubset()
        {
            ClassicAssert.IsTrue(DeepEqualsIsSubset(empty, empty));
            ClassicAssert.IsTrue(DeepEqualsIsSubset(empty, justE1));
            ClassicAssert.IsTrue(DeepEqualsIsSubset(empty, new ExprNode[] {e1, e2}));

            ClassicAssert.IsTrue(DeepEqualsIsSubset(justE1, new ExprNode[] {e1}));
            ClassicAssert.IsTrue(DeepEqualsIsSubset(justE1, new ExprNode[] {e1, e1}));
            ClassicAssert.IsFalse(DeepEqualsIsSubset(justE1, new ExprNode[] {e2}));

            ExprNode[] e1e2 = {e1, e2};
            ClassicAssert.IsFalse(DeepEqualsIsSubset(e1e2, justE1));
            ClassicAssert.IsFalse(DeepEqualsIsSubset(e1e2, new ExprNode[] {e2}));
            ClassicAssert.IsTrue(DeepEqualsIsSubset(e1e2, new ExprNode[] {e2, e1}));
            ClassicAssert.IsTrue(DeepEqualsIsSubset(e1e2, new ExprNode[] {e2, e1, e2, e1}));

            ExprNode[] e1e2e3 = {e1, e2, e3};
            ClassicAssert.IsFalse(DeepEqualsIsSubset(e1e2e3, justE1));
            ClassicAssert.IsFalse(DeepEqualsIsSubset(e1e2e3, new ExprNode[] {e2, e3}));
            ClassicAssert.IsTrue(DeepEqualsIsSubset(e1e2e3, new ExprNode[] {e2, e3, e1}));
            ClassicAssert.IsTrue(DeepEqualsIsSubset(e1e2e3, e1e2e3));
        }
    }
} // end of namespace
