///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.assemble
{
    [TestFixture]
	public class TestAssemblyStrategyTreeBuilder 
	{
        [Test]
	    public void TestInvalidBuild()
	    {
	        // root stream out of bounds
            TryInvalidBuild(3, Convert(new int[][] { new int[] { 1, 2 }, new int[] { }, new int[]{ } }), new bool[] { true, true, true });
            TryInvalidBuild(-1, Convert(new int[][] { new int[] { 1, 2 }, new int[] { }, new int[] { } }), new bool[] { true, true, true });

	        // not matching outer-inner
            TryInvalidBuild(0, Convert(new int[][] { new int[] { 1, 2 }, new int[] { }, new int[] { } }), new bool[] { true, true });

	        // stream relationships not filled
            TryInvalidBuild(0, Convert(new int[][] { new int[] { 1, 2 } }), new bool[] { true, true, true });

	        // stream relationships duplicates
            TryInvalidBuild(0, Convert(new int[][] { new int[] { 1, 2 }, new int[] { 1 }, new int[] { } }), new bool[] { true, true });
            TryInvalidBuild(0, Convert(new int[][] { new int[] { 1, 2 }, new int[] { }, new int[] { 2 } }), new bool[] { true, true, true });

	        // stream relationships out of range
            TryInvalidBuild(0, Convert(new int[][] { new int[] { 1, 3 }, new int[] { }, new int[] { } }), new bool[] { true, true });

	        // stream relationships missing stream
            TryInvalidBuild(0, Convert(new int[][] { new int[] { 1 }, new int[] { }, new int[] { } }), new bool[] { true, true });
	    }

        [Test]
	    public void TestValidBuildSimpleReqOpt()
	    {
            var nodeFactory = AssemblyStrategyTreeBuilder.Build(2, Convert(new int[][] { new int[] { }, new int[] { 0 }, new int[] { 1 } }), new bool[] { false, true, true });

	        var child1 = (RootRequiredAssemblyNodeFactory) nodeFactory;
	        Assert.AreEqual(2, child1.StreamNum);
	        Assert.AreEqual(1, child1.ChildNodes.Count);
	        Assert.AreEqual(null, child1.ParentNode);

	        var child1_1 = (BranchOptionalAssemblyNodeFactory) child1.ChildNodes[0];
	        Assert.AreEqual(1, child1_1.StreamNum);
	        Assert.AreEqual(1, child1_1.ChildNodes.Count);
	        Assert.AreEqual(child1, child1_1.ParentNode);

            var leaf1_2 = (LeafAssemblyNodeFactory)child1_1.ChildNodes[0];
	        Assert.AreEqual(0, leaf1_2.StreamNum);
	        Assert.AreEqual(0, leaf1_2.ChildNodes.Count);
	        Assert.AreEqual(child1_1, leaf1_2.ParentNode);
	    }

        [Test]
	    public void TestValidBuildSimpleOptReq()
	    {
            var nodeFactory = AssemblyStrategyTreeBuilder.Build(2, Convert(new int[][] { new int[] { }, new int[] { 0 }, new int[] { 1 } }), new bool[] { true, false, true });

	        var child1 = (RootOptionalAssemblyNodeFactory) nodeFactory;
	        Assert.AreEqual(2, child1.StreamNum);
	        Assert.AreEqual(1, child1.ChildNodes.Count);
	        Assert.AreEqual(null, child1.ParentNode);

            var child1_1 = (BranchRequiredAssemblyNodeFactory)child1.ChildNodes[0];
	        Assert.AreEqual(1, child1_1.StreamNum);
	        Assert.AreEqual(1, child1_1.ChildNodes.Count);
	        Assert.AreEqual(child1, child1_1.ParentNode);

            var leaf1_2 = (LeafAssemblyNodeFactory)child1_1.ChildNodes[0];
	        Assert.AreEqual(0, leaf1_2.StreamNum);
	        Assert.AreEqual(0, leaf1_2.ChildNodes.Count);
	        Assert.AreEqual(child1_1, leaf1_2.ParentNode);
	    }

        [Test]
	    public void TestValidBuildCartesian()
	    {
            var nodeFactory = AssemblyStrategyTreeBuilder.Build(1, Convert(new int[][] { new int[] { }, new int[] { 0, 2 }, new int[] { } }), new bool[] { false, true, false });

	        var top = (RootCartProdAssemblyNodeFactory) nodeFactory;
	        Assert.AreEqual(2, top.ChildNodes.Count);

            var leaf1 = (LeafAssemblyNodeFactory)top.ChildNodes[0];
	        Assert.AreEqual(0, leaf1.StreamNum);
	        Assert.AreEqual(0, leaf1.ChildNodes.Count);
	        Assert.AreEqual(top, leaf1.ParentNode);

            var leaf2 = (LeafAssemblyNodeFactory)top.ChildNodes[0];
	        Assert.AreEqual(0, leaf2.StreamNum);
	        Assert.AreEqual(0, leaf2.ChildNodes.Count);
	        Assert.AreEqual(top, leaf2.ParentNode);
	    }

	    private void TryInvalidBuild(int rootStream, IDictionary<int, int[]> joinedPerStream, bool[] isInnerPerStream)
	    {
	        try
	        {
	            AssemblyStrategyTreeBuilder.Build(rootStream, joinedPerStream, isInnerPerStream);
	            Assert.Fail();
	        }
	        catch (ArgumentException ex)
	        {
	            Log.Debug(".tryInvalidBuild expected exception=" + ex);
	            // expected
	        }
	    }

	    private IDictionary<int, int[]> Convert(int[][] array)
	    {
	        IDictionary<int, int[]> result = new Dictionary<int, int[]>();
	        for (var i = 0; i < array.Length; i++)
	        {
	            result.Put(i, array[i]);
	        }
	        return result;
	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
