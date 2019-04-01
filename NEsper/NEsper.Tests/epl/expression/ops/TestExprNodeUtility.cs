///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
	public class TestExprNodeUtility 
	{
	    private SupportExprNode e1 = new SupportExprNode(1);
	    private SupportExprNode e2 = new SupportExprNode(2);
	    private SupportExprNode e3 = new SupportExprNode(3);
	    private SupportExprNode e4 = new SupportExprNode(4);
	    private SupportExprNode e1Dup = new SupportExprNode(1);
	    private ExprNode[] empty = new ExprNode[0];
	    private ExprNode[] justE1;

        public TestExprNodeUtility()
        {
            justE1 = new ExprNode[] {e1};
        }

        [Test]
	    public void TestDeepEqualsIsSubset() {
	        Assert.IsTrue(ExprNodeUtility.DeepEqualsIsSubset(empty, empty));
	        Assert.IsTrue(ExprNodeUtility.DeepEqualsIsSubset(empty, justE1));
	        Assert.IsTrue(ExprNodeUtility.DeepEqualsIsSubset(empty, new ExprNode[] {e1, e2}));

	        Assert.IsTrue(ExprNodeUtility.DeepEqualsIsSubset(justE1, new ExprNode[] {e1}));
	        Assert.IsTrue(ExprNodeUtility.DeepEqualsIsSubset(justE1, new ExprNode[] {e1, e1}));
	        Assert.IsFalse(ExprNodeUtility.DeepEqualsIsSubset(justE1, new ExprNode[] {e2}));

	        ExprNode[] e1e2 = new ExprNode[] {e1, e2};
	        Assert.IsFalse(ExprNodeUtility.DeepEqualsIsSubset(e1e2, justE1));
	        Assert.IsFalse(ExprNodeUtility.DeepEqualsIsSubset(e1e2, new ExprNode[] {e2}));
	        Assert.IsTrue(ExprNodeUtility.DeepEqualsIsSubset(e1e2, new ExprNode[] {e2, e1}));
	        Assert.IsTrue(ExprNodeUtility.DeepEqualsIsSubset(e1e2, new ExprNode[] {e2, e1, e2, e1}));

	        ExprNode[] e1e2e3 = new ExprNode[] {e1, e2, e3};
	        Assert.IsFalse(ExprNodeUtility.DeepEqualsIsSubset(e1e2e3, justE1));
	        Assert.IsFalse(ExprNodeUtility.DeepEqualsIsSubset(e1e2e3, new ExprNode[] {e2, e3}));
	        Assert.IsTrue(ExprNodeUtility.DeepEqualsIsSubset(e1e2e3, new ExprNode[] {e2, e3, e1}));
	        Assert.IsTrue(ExprNodeUtility.DeepEqualsIsSubset(e1e2e3, e1e2e3));
	    }

        [Test]
	    public void TestDeepEqualsIgnoreOrder() {

	        // compare on set being empty
	        ComparePermutations(true, empty, empty);
	        ComparePermutations(false, new ExprNode[]{e1}, empty);

	        // compare single
	        ComparePermutations(true, justE1, justE1);
	        ComparePermutations(true, justE1, new ExprNode[]{e1Dup});
	        ComparePermutations(false, justE1, new ExprNode[]{e2});
	        ComparePermutations(false, new ExprNode[]{e2}, new ExprNode[]{e3});

	        // compare two (same number of expressions)
	        ComparePermutations(true, new ExprNode[]{e1, e2}, new ExprNode[]{e1, e2});
	        ComparePermutations(true, new ExprNode[]{e1, e2}, new ExprNode[]{e2, e1});
	        ComparePermutations(false, new ExprNode[]{e3, e2}, new ExprNode[]{e2, e1});
	        ComparePermutations(false, new ExprNode[]{e1, e2}, new ExprNode[]{e1, e3});

	        // compare three (same number of expressions)
	        ComparePermutations(true, new ExprNode[]{e1, e2, e3}, new ExprNode[]{e1, e2, e3});
	        ComparePermutations(false, new ExprNode[]{e1, e2, e3}, new ExprNode[]{e1, e2, e4});
	        ComparePermutations(false, new ExprNode[]{e1, e2, e3}, new ExprNode[]{e1, e4, e3});
	        ComparePermutations(false, new ExprNode[]{e1, e2, e3}, new ExprNode[]{e4, e2, e3});

	        // duplicates allowed and ignored
	        ComparePermutations(true, new ExprNode[]{e1}, new ExprNode[]{e1, e1});
	        ComparePermutations(false, new ExprNode[]{e1}, new ExprNode[]{e1, e2});
	        ComparePermutations(true, new ExprNode[]{e1}, new ExprNode[]{e1, e1, e1});
	        ComparePermutations(false, new ExprNode[]{e2}, new ExprNode[]{e2, e2, e1});
	        ComparePermutations(true, new ExprNode[]{e1, e1, e2, e2}, new ExprNode[]{e2, e2, e1});
	        ComparePermutations(false, new ExprNode[]{e1, e1, e2, e2}, new ExprNode[]{e1, e1, e1});
	        ComparePermutations(true, new ExprNode[]{e2, e1, e2}, new ExprNode[]{e2, e1});
	    }

	    private void ComparePermutations(bool expected, ExprNode[] setOne, ExprNode[] setTwo) {
	        if (setTwo.Length == 0) {
	            CompareSingle(expected, setOne, setTwo);
	            return;
	        }
	        var permuter = PermutationEnumerator.Create(setTwo.Length);
            foreach (var permutation in permuter) {
	            var copy = new ExprNode[setTwo.Length];
	            for (int i = 0; i < permutation.Length; i++) {
	                copy[i] = setTwo[permutation[i]];
	            }
	            CompareSingle(expected, setOne, copy);
	        }
	    }

	    private void CompareSingle(bool expected, ExprNode[] setOne, ExprNode[] setTwo) {
	        Assert.AreEqual(expected, ExprNodeUtility.DeepEqualsIgnoreDupAndOrder(setOne, setTwo));
	        Assert.AreEqual(expected, ExprNodeUtility.DeepEqualsIgnoreDupAndOrder(setTwo, setOne));
	    }
	}
} // end of namespace
