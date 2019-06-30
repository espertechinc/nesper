///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdrowindex.SupportPointRegionQuadTreeRowIndexUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdrowindex
{
    [TestFixture]
    public class TestPointRegionQuadTreeRowIndexSimple : CommonTest
    {
        [TearDown]
        public void TearDown()
        {
            tree = null;
        }

        private PointRegionQuadTree<object> tree;

        [Test]
        public void TestAddRemoveSamePoint()
        {
            tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 100, 100);

            AddNonUnique(tree, 5, 8, "P1");
            AddNonUnique(tree, 5, 8, "P2");
            AssertFound(tree, 0, 0, 10, 10, "P1,P2");

            Remove(tree, 5, 8, "P1");
            AssertFound(tree, 0, 0, 10, 10, "P2");

            Remove(tree, 5, 8, "P2");
            AssertFound(tree, 0, 0, 10, 10, "");
        }

        [Test]
        public void TestAddRemoveSimple()
        {
            tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 50, 60, 4, 20);
            AssertFound(tree, 0, 0, 10, 10, "");

            AddNonUnique(tree, 5, 8, "P1");
            AssertFound(tree, 0, 0, 10, 10, "P1");
            AssertFound(tree, 0, 0, 5, 5, "");

            Remove(tree, 5, 8, "P1");
            AssertFound(tree, 0, 0, 10, 10, "");
        }

        [Test]
        public void TestFewValues()
        {
            tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 100, 100);

            AddNonUnique(tree, 73.32704983331149, 23.46990952575032, "P0");
            AddNonUnique(tree, 53.09747562396894, 17.100976152185034, "P1");
            AddNonUnique(tree, 56.75757294858788, 25.508506696809608, "P2");
            AddNonUnique(tree, 83.66639067675291, 76.53772974832937, "P3");
            AddNonUnique(tree, 51.01654641861326, 43.49009281983866, "P4");

            var beginX = 50.45945198254618;
            var endX = 88.31594559038719;

            var beginY = 4.577595744501329;
            var endY = 22.93393078279351;

            AssertFound(tree, beginX, beginY, endX - beginX, endY - beginY, "P1");
        }

        [Test]
        public void TestPoints()
        {
            tree = PointRegionQuadTreeFactory<object>.Make(0, 0, 10, 10);

            AddNonUnique(tree, 8.0, 4.0, "P0");
            AssertFound(tree, 0, 0, 10, 10, "P0");

            AddNonUnique(tree, 8.0, 1.0, "P1");
            AssertFound(tree, 0, 0, 10, 10, "P0,P1");

            AddNonUnique(tree, 8.0, 2.0, "P2");
            AssertFound(tree, 0, 0, 10, 10, "P0,P1,P2");

            AddNonUnique(tree, 4.0, 4.0, "P3");
            AssertFound(tree, 0, 0, 10, 10, "P0,P1,P2,P3");

            AddNonUnique(tree, 1.0, 9.0, "P4");
            AssertFound(tree, 0, 0, 10, 10, "P0,P1,P2,P3,P4");

            AddNonUnique(tree, 8.0, 3.0, "P5");
            AssertFound(tree, 0, 0, 10, 10, "P0,P1,P2,P3,P4,P5");

            AddNonUnique(tree, 0.0, 6.0, "P6");
            AssertFound(tree, 0, 0, 10, 10, "P0,P1,P2,P3,P4,P5,P6");

            AddNonUnique(tree, 5.0, 1.0, "P7");
            AssertFound(tree, 0, 0, 10, 10, "P0,P1,P2,P3,P4,P5,P6,P7");

            AddNonUnique(tree, 5.0, 8.0, "P8");
            AssertFound(tree, 0, 0, 10, 10, "P0,P1,P2,P3,P4,P5,P6,P7,P8");

            AddNonUnique(tree, 7.0, 6.0, "P9");
            AssertFound(tree, 0, 0, 10, 10, "P0,P1,P2,P3,P4,P5,P6,P7,P8,P9");
        }
    }
} // end of namespace