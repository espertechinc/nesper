///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif.SupportMXCIFQuadTreeUtil;
using static com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex.SupportMXCIFQuadTreeFilterIndexUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    [TestFixture]
    public class TestMxcifQuadTreeFilterIndexScenarios : AbstractCommonTest
    {
        [Test]
        public void TestDimension()
        {
            var tree = MXCIFQuadTreeFactory.Make(1000, 100000, 9000, 900000);

            try
            {
                Set(10, 90, 1, 1, "R1", tree);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "Rectangle (10.0d,90.0d,1.0d,1.0d) not in {MinX=1000.0d, MinY=100000.0d, MaxX=10000.0d, MaxY=1000000.0d}");
            }

            try
            {
                Set(10999999, 90, 1, 1, "R2", tree);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                // expected
            }

            Set(5000, 800000, 1, 1, "R3", tree);

            AssertCollect(tree, 0, 0, 10000000, 10000000, "R3");
            AssertCollect(tree, 4000, 790000, 1200, 11000, "R3");
            AssertCollect(tree, 4000, 790000, 900, 9000, "");
            AssertCollectAll(tree, "R3");
        }

        [Test]
        public void TestRemoveNonExistent()
        {
            var tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100, 20, 20);
            Delete(10, 61, 1, 1, tree);
            Set(10, 60, 1, 1, "R1", tree);
            Delete(10, 61, 1, 1, tree);
            Set(10, 80, 1, 1, "R2", tree);
            Set(20, 70, 1, 1, "R3", tree);
            Set(10, 80, 1, 1, "R4", tree);
            AssertCollectAll(tree, "R1,R3,R4");

            Assert.AreEqual(3, NavigateLeaf(tree, "").Count);
            Assert.AreEqual(3, NavigateLeaf(tree, "").Count);
            AssertCollectAll(tree, "R1,R3,R4");

            Delete(10, 61, 1, 1, tree);
            Assert.AreEqual(3, NavigateLeaf(tree, "").Count);
            AssertCollectAll(tree, "R1,R3,R4");

            Delete(9, 60, 1, 1, tree);
            Delete(10, 80, 1, 1, tree);
            Assert.AreEqual(2, NavigateLeaf(tree, "").Count);
            AssertCollectAll(tree, "R1,R3");

            Delete(9, 60, 1, 1, tree);
            Delete(10, 80, 1, 1, tree);
            Delete(10, 60, 1, 1, tree);
            Assert.AreEqual(1, NavigateLeaf(tree, "").Count);
            AssertCollectAll(tree, "R3");

            Delete(20, 70, 1, 1, tree);
            Assert.AreEqual(0, NavigateLeaf(tree, "").Count);
            AssertCollectAll(tree, "");
        }

        [Test]
        public void TestSubdivideAddMany()
        {
            var tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100, 2, 3);
            Set(0, 0, 1, 1, "R1", tree);
            Set(1, 2, 1, 1, "R2", tree);
            Set(3, 2, 1, 1, "R3", tree);
            Assert.AreEqual(3, NavigateLeaf(tree, "nw,nw").Count);

            Delete(1, 2, 1, 1, tree);
            Delete(0, 0, 1, 1, tree);
            Delete(3, 2, 1, 1, tree);
            AssertCollectAll(tree, "");
        }

        [Test]
        public void TestSubdivideMerge()
        {
            var tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100, 3, 2);
            Assert.AreEqual(1, tree.Root.Level);
            Set(10, 10, 0.01, 0.01, "R1", tree);
            Set(9.9, 10, 0.01, 0.01, "R2", tree);
            Set(10, 9.9, 0.01, 0.01, "R3", tree);
            Set(10, 10, 0.01, 0.01, "R4", tree);
            Set(10, 9.9, 0.01, 0.01, "R5", tree);
            Set(9.9, 10, 0.01, 0.01, "R6", tree);
            Assert.IsInstanceOf<MXCIFQuadTreeNodeLeaf>(tree.Root);
            AssertCollect(tree, 9, 10, 1, 1, "R4,R6");
            AssertCollect(tree, 10, 9, 1, 1, "R4,R5");
            AssertCollect(tree, 10, 10, 1, 1, "R4");
            AssertCollect(tree, 9, 9, 2, 2, "R4,R5,R6");
            AssertCollectAll(tree, "R4,R5,R6");

            Set(10, 10, 0.01, 0.01, "R7", tree);
            Assert.IsInstanceOf<MXCIFQuadTreeNodeLeaf>(tree.Root);

            Set(9.9, 9.9, 0.01, 0.01, "R8", tree);

            Assert.IsFalse(tree.Root is MXCIFQuadTreeNodeLeaf);
            Assert.AreEqual(1, tree.Root.Level);
            Assert.AreEqual(4, NavigateLeaf(tree, "nw").Count);
            var collection = (IList<XYWHRectangleWValue>) NavigateLeaf(tree, "nw").Data;
            Assert.AreEqual(4, collection.Count);
            Compare(10, 10, 0.01, 0.01, "R7", collection[0]);
            Compare(9.9, 10, 0.01, 0.01, "R6", collection[1]);
            Compare(10, 9.9, 0.01, 0.01, "R5", collection[2]);
            Compare(9.9, 9.9, 0.01, 0.01, "R8", collection[3]);
            AssertCollect(tree, 9, 10, 1, 1, "R6,R7");
            AssertCollect(tree, 10, 9, 1, 1, "R5,R7");
            AssertCollect(tree, 10, 10, 1, 1, "R7");
            AssertCollect(tree, 9, 9, 2, 2, "R5,R6,R7,R8");
            AssertCollectAll(tree, "R5,R6,R7,R8");

            Set(9.9, 10, 0.01, 0.01, "R9", tree);
            Set(10, 9.9, 0.01, 0.01, "R10", tree);
            Set(10, 10, 0.01, 0.01, "R11", tree);
            Set(10, 10, 0.01, 0.01, "R12", tree);
            Set(10, 10, 0.01, 0.01, "R13", tree);

            Assert.AreEqual(4, NavigateLeaf(tree, "nw").Count);
            Assert.AreEqual(2, NavigateLeaf(tree, "nw").Level);
            Assert.AreEqual(4, collection.Count);
            Compare(10, 10, 0.01, 0.01, "R13", collection[0]);
            Compare(9.9, 10, 0.01, 0.01, "R9", collection[1]);
            Compare(10, 9.9, 0.01, 0.01, "R10", collection[2]);
            Compare(9.9, 9.9, 0.01, 0.01, "R8", collection[3]);
            AssertCollect(tree, 9, 10, 1, 1, "R9,R13");
            AssertCollect(tree, 10, 9, 1, 1, "R10,R13");
            AssertCollect(tree, 10, 10, 1, 1, "R13");
            AssertCollect(tree, 9, 9, 2, 2, "R8,R9,R10,R13");

            Delete(9.9, 10, 0.01, 0.01, tree);
            Delete(10, 9.9, 0.01, 0.01, tree);
            Delete(10, 9.9, 0.01, 0.01, tree);
            Delete(10, 9.9, 0.01, 0.01, tree);
            AssertCollect(tree, 9, 10, 1, 1, "R13");
            AssertCollect(tree, 10, 9, 1, 1, "R13");
            AssertCollect(tree, 10, 10, 1, 1, "R13");
            AssertCollect(tree, 9, 9, 2, 2, "R8,R13");
            AssertCollectAll(tree, "R8,R13");

            Assert.AreEqual(2, NavigateLeaf(tree, "").Count);
            collection = (IList<XYWHRectangleWValue>) NavigateLeaf(tree, "").Data;
            Assert.AreEqual(2, collection.Count);
            Compare(10, 10, 0.01, 0.01, "R13", collection[0]);
            Compare(9.9, 9.9, 0.01, 0.01, "R8", collection[1]);

            Delete(9.9, 9.9, 0.01, 0.01, tree);
            Delete(10, 10, 0.01, 0.01, tree);
            Assert.IsInstanceOf<MXCIFQuadTreeNodeLeaf>(tree.Root);
            Assert.AreEqual(0, NavigateLeaf(tree, "").Count);
            AssertCollectAll(tree, "");
        }

        [Test]
        public void TestSubdivideMultiChild()
        {
            var tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100, 4, 3);
            Set(60, 11, 1, 1, "R1", tree);
            Set(60, 40, 1, 1, "R2", tree);
            Set(70, 30, 1, 1, "R3", tree);
            Set(60, 10, 1, 1, "R4", tree);
            Set(90, 45, 1, 1, "R5", tree);

            NavigateLeaf(tree, "nw");
            NavigateLeaf(tree, "se");
            NavigateLeaf(tree, "sw");
            var ne = NavigateBranch(tree, "ne");
            Assert.AreEqual(2, ne.Level);

            var nw = NavigateLeaf(ne, "nw");
            var collection = (IList<XYWHRectangleWValue>) nw.Data;
            Compare(60, 11, 1, 1, "R1", collection[0]);
            Compare(60, 10, 1, 1, "R4", collection[1]);
            Assert.AreEqual(2, nw.Count);

            var se = NavigateLeaf(ne, "se");
            Compare(90, 45, 1, 1, "R5", (XYWHRectangleWValue) se.Data);
            Assert.AreEqual(1, se.Count);

            var sw = NavigateLeaf(ne, "sw");
            collection = (IList<XYWHRectangleWValue>) sw.Data;
            Compare(60, 40, 1, 1, "R2", collection[0]);
            Compare(70, 30, 1, 1, "R3", collection[1]);
            Assert.AreEqual(2, sw.Count);

            Delete(60, 11, 1, 1, tree);
            Delete(60, 40, 1, 1, tree);

            var root = NavigateLeaf(tree, "");
            collection = (IList<XYWHRectangleWValue>) root.Data;
            Assert.AreEqual(3, root.Count);
            Assert.AreEqual(3, collection.Count);
            Compare(60, 10, 1, 1, "R4", collection[0]);
            Compare(70, 30, 1, 1, "R3", collection[1]);
            Compare(90, 45, 1, 1, "R5", collection[2]);
        }

        [Test]
        public void TestSubdivideSingleMerge()
        {
            var tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100, 3, 2);
            Set(65, 75, 1, 1, "R1", tree);
            Set(81, 60, 1, 1, "R2", tree);
            Set(80, 60, 1, 1, "R3", tree);
            Set(80, 61, 1, 1, "R4", tree);
            AssertCollect(tree, 60, 60, 20.5, 20.5, "R1,R3,R4");
            AssertCollectAll(tree, "R1,R2,R3,R4");

            Assert.IsFalse(tree.Root is MXCIFQuadTreeNodeLeaf);
            Assert.AreEqual(4, NavigateLeaf(tree, "se").Count);
            var collection = (IList<XYWHRectangleWValue>) NavigateLeaf(tree, "se").Data;
            Assert.AreEqual(4, collection.Count);
            Compare(65, 75, 1, 1, "R1", collection[0]);
            Compare(81, 60, 1, 1, "R2", collection[1]);
            Compare(80, 60, 1, 1, "R3", collection[2]);
            Compare(80, 61, 1, 1, "R4", collection[3]);

            Set(66, 78, 1, 1, "R5", tree);
            Delete(65, 75, 1, 1, tree);
            Delete(80, 60, 1, 1, tree);

            Assert.AreEqual(3, NavigateLeaf(tree, "se").Count);
            AssertCollectAll(tree, "R2,R4,R5");
            Assert.AreEqual(3, collection.Count);
            Compare(81, 60, 1, 1, "R2", collection[0]);
            Compare(80, 61, 1, 1, "R4", collection[1]);
            Compare(66, 78, 1, 1, "R5", collection[2]);

            Delete(66, 78, 1, 1, tree);

            AssertCollectAll(tree, "R2,R4");
            Assert.AreEqual(2, NavigateLeaf(tree, "").Count);
            collection = (IList<XYWHRectangleWValue>) NavigateLeaf(tree, "").Data;
            Assert.AreEqual(2, collection.Count);
            Compare(81, 60, 1, 1, "R2", collection[0]);
            Compare(80, 61, 1, 1, "R4", collection[1]);
        }

        [Test]
        public void TestSuperslim()
        {
            var tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100, 1, 100);
            Set(10, 90, 0.1, 0.2, "R1", tree);
            Set(10, 95, 0.3, 0.4, "R2", tree);
            var ne = NavigateLeaf(tree, "sw,sw,sw,ne");
            Compare(10, 90, 0.1, 0.2, "R1", (XYWHRectangleWValue) ne.Data);
            var se = NavigateLeaf(tree, "sw,sw,sw,se");
            Compare(10, 95, 0.3, 0.4, "R2", (XYWHRectangleWValue) se.Data);
        }

        private static void Delete(
            double x,
            double y,
            double width,
            double height,
            MXCIFQuadTree tree)
        {
            MXCIFQuadTreeFilterIndexDelete.Delete(x, y, width, height, tree);
        }
    }
} // end of namespace
