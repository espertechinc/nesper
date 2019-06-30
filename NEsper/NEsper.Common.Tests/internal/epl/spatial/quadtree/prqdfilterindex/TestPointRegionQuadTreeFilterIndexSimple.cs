///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex.SupportPointRegionQuadTreeFilterIndexUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex
{
    [TestFixture]
    public class TestPointRegionQuadTreeFilterIndexSimple : CommonTest
    {
        [TearDown]
        public void TearDown()
        {
            tree = null;
        }

        private PointRegionQuadTree<object> tree;

        [Test]
        public void TestFewValues()
        {
            tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 100, 100);

            Set(tree, 73.32704983331149, 23.46990952575032, "P0");
            Set(tree, 53.09747562396894, 17.100976152185034, "P1");
            Set(tree, 56.75757294858788, 25.508506696809608, "P2");
            Set(tree, 83.66639067675291, 76.53772974832937, "P3");
            Set(tree, 51.01654641861326, 43.49009281983866, "P4");

            var beginX = 50.45945198254618;
            var endX = 88.31594559038719;

            var beginY = 4.577595744501329;
            var endY = 22.93393078279351;

            AssertCollect(tree, beginX, beginY, endX - beginX, endY - beginY, "P1");
            AssertCollectAll(tree, "P0,P1,P2,P3,P4");

            Assert.AreEqual("P0", PointRegionQuadTreeFilterIndexGet<object>.Get(73.32704983331149, 23.46990952575032, tree));
            Assert.AreEqual("P1", PointRegionQuadTreeFilterIndexGet<object>.Get(53.09747562396894, 17.100976152185034, tree));
            Assert.AreEqual("P2", PointRegionQuadTreeFilterIndexGet<object>.Get(56.75757294858788, 25.508506696809608, tree));
            Assert.AreEqual("P3", PointRegionQuadTreeFilterIndexGet<object>.Get(83.66639067675291, 76.53772974832937, tree));
            Assert.AreEqual("P4", PointRegionQuadTreeFilterIndexGet<object>.Get(51.01654641861326, 43.49009281983866, tree));
        }

        [Test]
        public void TestGetSetRemove()
        {
            tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 100, 100);
            Assert.IsNull(PointRegionQuadTreeFilterIndexGet<object>.Get(10, 20, tree));
            AssertCollectAll(tree, "");

            PointRegionQuadTreeFilterIndexSet<object>.Set(10, 20, "P0", tree);
            Assert.AreEqual("P0", PointRegionQuadTreeFilterIndexGet<object>.Get(10, 20, tree));
            AssertCollectAll(tree, "P0");

            PointRegionQuadTreeFilterIndexDelete<object>.Delete(10, 20, tree);
            Assert.IsNull(PointRegionQuadTreeFilterIndexGet<object>.Get(10, 20, tree));
            AssertCollectAll(tree, "");
        }

        [Test]
        public void TestPoints()
        {
            tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 10, 10);

            Set(tree, 8.0, 4.0, "P0");
            AssertCollectAll(tree, "P0");

            Set(tree, 8.0, 1.0, "P1");
            AssertCollectAll(tree, "P0,P1");

            Set(tree, 8.0, 2.0, "P2");
            AssertCollectAll(tree, "P0,P1,P2");

            Set(tree, 4.0, 4.0, "P3");
            AssertCollectAll(tree, "P0,P1,P2,P3");

            Set(tree, 1.0, 9.0, "P4");
            AssertCollectAll(tree, "P0,P1,P2,P3,P4");

            Set(tree, 8.0, 3.0, "P5");
            AssertCollectAll(tree, "P0,P1,P2,P3,P4,P5");

            Set(tree, 0.0, 6.0, "P6");
            AssertCollectAll(tree, "P0,P1,P2,P3,P4,P5,P6");

            Set(tree, 5.0, 1.0, "P7");
            AssertCollectAll(tree, "P0,P1,P2,P3,P4,P5,P6,P7");

            Set(tree, 5.0, 8.0, "P8");
            AssertCollectAll(tree, "P0,P1,P2,P3,P4,P5,P6,P7,P8");

            Set(tree, 7.0, 6.0, "P9");
            AssertCollectAll(tree, "P0,P1,P2,P3,P4,P5,P6,P7,P8,P9");
        }

        [Test]
        public void TestSetRemoveTwiceSamePoint()
        {
            tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 100, 100);

            Set(tree, 5, 8, "P1");
            Set(tree, 5, 8, "P2");
            AssertCollectAll(tree, "P2");

            Delete(tree, 5, 8);
            AssertCollectAll(tree, "");

            Delete(tree, 5, 8);
            AssertCollectAll(tree, "");
        }
    }
} // end of namespace