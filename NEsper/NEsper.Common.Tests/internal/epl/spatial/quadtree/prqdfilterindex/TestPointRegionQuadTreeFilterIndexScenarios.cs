///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion.SupportPointRegionQuadTreeUtil;
using static com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex.SupportPointRegionQuadTreeFilterIndexUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex
{
    [TestFixture]
    public class TestPointRegionQuadTreeFilterIndexScenarios : AbstractCommonTest
    {
        [Test, RunInApplicationDomain]
        public void TestSubdivideAddMany()
        {
            PointRegionQuadTree<object> tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 100, 100, 2, 3);
            Set(tree, 0, 0, "P1");
            Set(tree, 1, 2, "P2");
            Set(tree, 3, 2, "P3");
            Assert.AreEqual(3, NavigateLeaf(tree, "nw,nw").Count);

            Delete(tree, 1, 2);
            Delete(tree, 0, 0);
            Delete(tree, 3, 2);
            AssertCollectAll(tree, "");
        }

        [Test, RunInApplicationDomain]
        public void TestDimension()
        {
            PointRegionQuadTree<object> tree = PointRegionQuadTreeFactory<object>.Make(1000, 100000, 9000, 900000);

            Assert.That(
                () => Set(tree, 10, 90, "P1"),
                Throws.Exception.With.Message.EqualTo(
                    "Point (10.0d,90.0d) not in {MinX=1000.0d, MinY=100000.0d, MaxX=10000.0d, MaxY=1000000.0d}"));

            Assert.That(
                () => Set(tree, 10999999, 90, "P2"),
                Throws.Exception);

            Set(tree, 5000, 800000, "P3");

            AssertCollect(tree, 0, 0, 10000000, 10000000, "P3");
            AssertCollect(tree, 4000, 790000, 1200, 11000, "P3");
            AssertCollect(tree, 4000, 790000, 900, 9000, "");
            AssertCollectAll(tree, "P3");
        }

        [Test, RunInApplicationDomain]
        public void TestSuperslim()
        {
            PointRegionQuadTree<object> tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 100, 100, 1, 100);
            Set(tree, 10, 90, "P1");
            Set(tree, 10, 95, "P2");
            PointRegionQuadTreeNodeLeaf<object> ne = NavigateLeaf(tree, "sw,sw,sw,ne");
            Compare(10, 90, "P1", (XYPointWValue<object>) ne.Points);
            PointRegionQuadTreeNodeLeaf<object> se = NavigateLeaf(tree, "sw,sw,sw,se");
            Compare(10, 95, "P2", (XYPointWValue<object>) se.Points);
        }

        [Test, RunInApplicationDomain]
        public void TestSubdivideMultiChild()
        {
            PointRegionQuadTree<object> tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 100, 100, 4, 3);
            Set(tree, 60, 11, "P1");
            Set(tree, 60, 40, "P2");
            Set(tree, 70, 30, "P3");
            Set(tree, 60, 10, "P4");
            Set(tree, 90, 45, "P5");

            NavigateLeaf(tree, "nw");
            NavigateLeaf(tree, "se");
            NavigateLeaf(tree, "sw");
            PointRegionQuadTreeNodeBranch ne = NavigateBranch(tree, "ne");
            Assert.AreEqual(2, ne.Level);

            PointRegionQuadTreeNodeLeaf<object> nw = NavigateLeaf(ne, "nw");
            IList<XYPointWValue<object>> collection = (IList<XYPointWValue<object>>) nw.Points;
            Compare(60, 11, "P1", collection[0]);
            Compare(60, 10, "P4", collection[1]);
            Assert.AreEqual(2, nw.Count);

            PointRegionQuadTreeNodeLeaf<object> se = NavigateLeaf(ne, "se");
            Compare(90, 45, "P5", (XYPointWValue<object>) se.Points);
            Assert.AreEqual(1, se.Count);

            PointRegionQuadTreeNodeLeaf<object> sw = NavigateLeaf(ne, "sw");
            collection = (IList<XYPointWValue<object>>) sw.Points;
            Compare(60, 40, "P2", collection[0]);
            Compare(70, 30, "P3", collection[1]);
            Assert.AreEqual(2, sw.Count);

            Delete(tree, 60, 11);
            Delete(tree, 60, 40);

            PointRegionQuadTreeNodeLeaf<object> root = NavigateLeaf(tree, "");
            collection = (IList<XYPointWValue<object>>) root.Points;
            Assert.AreEqual(3, root.Count);
            Assert.AreEqual(3, collection.Count);
            Compare(60, 10, "P4", collection[0]);
            Compare(70, 30, "P3", collection[1]);
            Compare(90, 45, "P5", collection[2]);
        }

        [Test, RunInApplicationDomain]
        public void TestRemoveNonExistent()
        {
            PointRegionQuadTree<object> tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 100, 100, 20, 20);
            Delete(tree, 10, 61);
            Set(tree, 10, 60, "P1");
            Delete(tree, 10, 61);
            Set(tree, 10, 80, "P2");
            Set(tree, 20, 70, "P3");
            Set(tree, 10, 80, "P4");
            AssertCollectAll(tree, "P1,P3,P4");

            Assert.AreEqual(3, NavigateLeaf(tree, "").Count);
            Assert.AreEqual(3, NavigateLeaf(tree, "").Count);
            AssertCollectAll(tree, "P1,P3,P4");

            Delete(tree, 10, 61);
            Assert.AreEqual(3, NavigateLeaf(tree, "").Count);
            AssertCollectAll(tree, "P1,P3,P4");

            Delete(tree, 9, 60);
            Delete(tree, 10, 80);
            Assert.AreEqual(2, NavigateLeaf(tree, "").Count);
            AssertCollectAll(tree, "P1,P3");

            Delete(tree, 9, 60);
            Delete(tree, 10, 80);
            Delete(tree, 10, 60);
            Assert.AreEqual(1, NavigateLeaf(tree, "").Count);
            AssertCollectAll(tree, "P3");

            Delete(tree, 20, 70);
            Assert.AreEqual(0, NavigateLeaf(tree, "").Count);
            AssertCollectAll(tree, "");
        }

        [Test, RunInApplicationDomain]
        public void TestSubdivideSingleMerge()
        {
            PointRegionQuadTree<object> tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 100, 100, 3, 2);
            Set(tree, 65, 75, "P1");
            Set(tree, 81, 60, "P2");
            Set(tree, 80, 60, "P3");
            Set(tree, 80, 61, "P4");
            AssertCollect(tree, 60, 60, 21, 21, "P1,P3,P4");
            AssertCollectAll(tree, "P1,P2,P3,P4");

            Assert.IsFalse(tree.Root is PointRegionQuadTreeNodeLeaf<object>);
            Assert.AreEqual(4, NavigateLeaf(tree, "se").Count);
            IList<XYPointWValue<object>> collection = (IList<XYPointWValue<object>>) NavigateLeaf(tree, "se").Points;
            Assert.AreEqual(4, collection.Count);
            Compare(65, 75, "P1", collection[0]);
            Compare(81, 60, "P2", collection[1]);
            Compare(80, 60, "P3", collection[2]);
            Compare(80, 61, "P4", collection[3]);

            Set(tree, 66, 78, "P5");
            Delete(tree, 65, 75);
            Delete(tree, 80, 60);

            Assert.AreEqual(3, NavigateLeaf(tree, "se").Count);
            AssertCollectAll(tree, "P2,P4,P5");
            Assert.AreEqual(3, collection.Count);
            Compare(81, 60, "P2", collection[0]);
            Compare(80, 61, "P4", collection[1]);
            Compare(66, 78, "P5", collection[2]);

            Delete(tree, 66, 78);

            AssertCollectAll(tree, "P2,P4");
            Assert.AreEqual(2, NavigateLeaf(tree, "").Count);
            collection = (IList<XYPointWValue<object>>) NavigateLeaf(tree, "").Points;
            Assert.AreEqual(2, collection.Count);
            Compare(81, 60, "P2", collection[0]);
            Compare(80, 61, "P4", collection[1]);
        }

        [Test, RunInApplicationDomain]
        public void TestSubdivideMerge()
        {
            PointRegionQuadTree<object> tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 100, 100, 3, 2);
            Assert.AreEqual(1, tree.Root.Level);
            Set(tree, 10, 10, "P1");
            Set(tree, 9.9, 10, "P2");
            Set(tree, 10, 9.9, "P3");
            Set(tree, 10, 10, "P4");
            Set(tree, 10, 9.9, "P5");
            Set(tree, 9.9, 10, "P6");
            Assert.IsInstanceOf<PointRegionQuadTreeNodeLeaf<object>>(tree.Root);
            AssertCollect(tree, 9, 10, 1, 1, "P6");
            AssertCollect(tree, 10, 9, 1, 1, "P5");
            AssertCollect(tree, 10, 10, 1, 1, "P4");
            AssertCollect(tree, 9, 9, 2, 2, "P4,P5,P6");
            AssertCollectAll(tree, "P4,P5,P6");

            Set(tree, 10, 10, "P7");
            Assert.IsInstanceOf<PointRegionQuadTreeNodeLeaf<object>>(tree.Root);

            Set(tree, 9.9, 9.9, "P8");

            Assert.IsFalse(tree.Root is PointRegionQuadTreeNodeLeaf<object>);
            Assert.AreEqual(1, tree.Root.Level);
            Assert.AreEqual(4, NavigateLeaf(tree, "nw").Count);
            IList<XYPointWValue<object>> collection = (IList<XYPointWValue<object>>) NavigateLeaf(tree, "nw").Points;
            Assert.AreEqual(4, collection.Count);
            Compare(10, 10, "P7", collection[0]);
            Compare(9.9, 10, "P6", collection[1]);
            Compare(10, 9.9, "P5", collection[2]);
            Compare(9.9, 9.9, "P8", collection[3]);
            AssertCollect(tree, 9, 10, 1, 1, "P6");
            AssertCollect(tree, 10, 9, 1, 1, "P5");
            AssertCollect(tree, 10, 10, 1, 1, "P7");
            AssertCollect(tree, 9, 9, 2, 2, "P5,P6,P7,P8");
            AssertCollectAll(tree, "P5,P6,P7,P8");

            Set(tree, 9.9, 10, "P9");
            Set(tree, 10, 9.9, "P10");
            Set(tree, 10, 10, "P11");
            Set(tree, 10, 10, "P12");
            Set(tree, 10, 10, "P13");

            Assert.AreEqual(4, NavigateLeaf(tree, "nw").Count);
            Assert.AreEqual(2, NavigateLeaf(tree, "nw").Level);
            Assert.AreEqual(4, collection.Count);
            Compare(10, 10, "P13", collection[0]);
            Compare(9.9, 10, "P9", collection[1]);
            Compare(10, 9.9, "P10", collection[2]);
            Compare(9.9, 9.9, "P8", collection[3]);
            AssertCollect(tree, 9, 10, 1, 1, "P9");
            AssertCollect(tree, 10, 9, 1, 1, "P10");
            AssertCollect(tree, 10, 10, 1, 1, "P13");
            AssertCollect(tree, 9, 9, 2, 2, "P8,P9,P10,P13");

            Delete(tree, 9.9, 10);
            Delete(tree, 10, 9.9);
            Delete(tree, 10, 9.9);
            Delete(tree, 10, 9.9);
            AssertCollect(tree, 9, 10, 1, 1, "");
            AssertCollect(tree, 10, 9, 1, 1, "");
            AssertCollect(tree, 10, 10, 1, 1, "P13");
            AssertCollect(tree, 9, 9, 2, 2, "P8,P13");
            AssertCollectAll(tree, "P8,P13");

            Assert.AreEqual(2, NavigateLeaf(tree, "").Count);
            collection = (IList<XYPointWValue<object>>) NavigateLeaf(tree, "").Points;
            Assert.AreEqual(2, collection.Count);
            Compare(10, 10, "P13", collection[0]);
            Compare(9.9, 9.9, "P8", collection[1]);

            Delete(tree, 9.9, 9.9);
            Delete(tree, 10, 10);
            Assert.IsInstanceOf<PointRegionQuadTreeNodeLeaf<object>>(tree.Root);
            Assert.AreEqual(0, NavigateLeaf(tree, "").Count);
            AssertCollectAll(tree, "");
        }
    }
} // end of namespace
