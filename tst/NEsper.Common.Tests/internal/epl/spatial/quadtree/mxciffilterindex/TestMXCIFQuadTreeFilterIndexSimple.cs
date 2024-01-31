///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex.SupportMXCIFQuadTreeFilterIndexUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    [TestFixture]
    public class TestMxcifQuadTreeFilterIndexSimple : AbstractCommonTest
    {
        [TearDown]
        public void TearDown()
        {
            tree = null;
        }

        private MXCIFQuadTree tree;

        private static void Delete(
            double x,
            double y,
            double width,
            double height,
            MXCIFQuadTree tree)
        {
            MXCIFQuadTreeFilterIndexDelete.Delete(x, y, width, height, tree);
        }

        private static object Get(
            double x,
            double y,
            double width,
            double height,
            MXCIFQuadTree tree)
        {
            return MXCIFQuadTreeFilterIndexGet.Get(x, y, width, height, tree);
        }

        [Test]
        public void TestFewValues()
        {
            tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100);

            Set(73.32704983331149, 23.46990952575032, 1, 1, "R0", tree);
            Set(53.09747562396894, 17.100976152185034, 1, 1, "R1", tree);
            Set(56.75757294858788, 25.508506696809608, 1, 1, "R2", tree);
            Set(83.66639067675291, 76.53772974832937, 1, 1, "R3", tree);
            Set(51.01654641861326, 43.49009281983866, 1, 1, "R4", tree);

            var beginX = 50.45945198254618;
            var endX = 88.31594559038719;

            var beginY = 4.577595744501329;
            var endY = 22.93393078279351;

            AssertCollect(tree, beginX, beginY, endX - beginX, endY - beginY, "R1");
            AssertCollectAll(tree, "R0,R1,R2,R3,R4");

            ClassicAssert.AreEqual("R0", Get(73.32704983331149, 23.46990952575032, 1, 1, tree));
            ClassicAssert.AreEqual("R1", Get(53.09747562396894, 17.100976152185034, 1, 1, tree));
            ClassicAssert.AreEqual("R2", Get(56.75757294858788, 25.508506696809608, 1, 1, tree));
            ClassicAssert.AreEqual("R3", Get(83.66639067675291, 76.53772974832937, 1, 1, tree));
            ClassicAssert.AreEqual("R4", Get(51.01654641861326, 43.49009281983866, 1, 1, tree));
        }

        [Test]
        public void TestGetSetRemove()
        {
            tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100);
            ClassicAssert.IsNull(Get(10, 20, 30, 40, tree));
            AssertCollectAll(tree, "");

            Set(10, 20, 30, 40, "R0", tree);
            ClassicAssert.AreEqual("R0", Get(10, 20, 30, 40, tree));
            AssertCollectAll(tree, "R0");

            Delete(10, 20, 30, 40, tree);
            ClassicAssert.IsNull(Get(10, 20, 30, 40, tree));
            AssertCollectAll(tree, "");
        }

        [Test]
        public void TestPoints()
        {
            tree = MXCIFQuadTreeFactory.Make(0, 0, 10, 10);

            Set(8.0, 4.0, 1, 1, "R0", tree);
            AssertCollectAll(tree, "R0");

            Set(8.0, 1.0, 1, 1, "R1", tree);
            AssertCollectAll(tree, "R0,R1");

            Set(8.0, 2.0, 1, 1, "R2", tree);
            AssertCollectAll(tree, "R0,R1,R2");

            Set(4.0, 4.0, 1, 1, "R3", tree);
            AssertCollectAll(tree, "R0,R1,R2,R3");

            Set(1.0, 9.0, 1, 1, "R4", tree);
            AssertCollectAll(tree, "R0,R1,R2,R3,R4");

            Set(8.0, 3.0, 1, 1, "R5", tree);
            AssertCollectAll(tree, "R0,R1,R2,R3,R4,R5");

            Set(0.0, 6.0, 1, 1, "R6", tree);
            AssertCollectAll(tree, "R0,R1,R2,R3,R4,R5,R6");

            Set(5.0, 1.0, 1, 1, "R7", tree);
            AssertCollectAll(tree, "R0,R1,R2,R3,R4,R5,R6,R7");

            Set(5.0, 8.0, 1, 1, "R8", tree);
            AssertCollectAll(tree, "R0,R1,R2,R3,R4,R5,R6,R7,R8");

            Set(7.0, 6.0, 1, 1, "R9", tree);
            AssertCollectAll(tree, "R0,R1,R2,R3,R4,R5,R6,R7,R8,R9");
        }

        [Test]
        public void TestSetRemoveTwiceSamePoint()
        {
            tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100);

            Set(5, 8, 1, 2, "R1", tree);
            Set(5, 8, 1, 2, "R2", tree);
            AssertCollectAll(tree, "R2");

            Delete(5, 8, 1, 2, tree);
            AssertCollectAll(tree, "");

            Delete(5, 8, 1, 2, tree);
            AssertCollectAll(tree, "");
        }
    }
} // end of namespace
