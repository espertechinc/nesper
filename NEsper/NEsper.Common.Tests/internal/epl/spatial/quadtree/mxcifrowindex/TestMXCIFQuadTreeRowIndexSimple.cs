///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex.SupportMXCIFQuadTreeRowIndexUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex
{
    [TestFixture]
    public class TestMxcifQuadTreeRowIndexSimple : AbstractCommonTest
    {
        private MXCIFQuadTree<object> tree;

        [TearDown]
        public void TearDown()
        {
            tree = null;
        }

        [Test]
        public void TestAddRemoveSimple()
        {
            tree = MXCIFQuadTreeFactory<object>.Make(0, 0, 50, 60);
            AssertFound(tree, 0, 0, 10, 10, "");

            AddNonUnique(tree, 5, 8, 1, 1, "R1");
            AssertFound(tree, 0, 0, 10, 10, "R1");
            AssertFound(tree, 0, 0, 5, 5, "");

            MXCIFQuadTreeRowIndexRemove.Remove(5, 8, 1, 1, "R1", tree);
            AssertFound(tree, 0, 0, 10, 10, "");
        }

        [Test]
        public void TestPoints()
        {
            tree = MXCIFQuadTreeFactory<object>.Make(0, 0, 10, 10);

            AddNonUnique(tree, 8.0, 4.0, 1, 1, "R0");
            AssertFound(tree, 0, 0, 10, 10, "R0");

            AddNonUnique(tree, 8.0, 1.0, 1, 1, "R1");
            AssertFound(tree, 0, 0, 10, 10, "R0,R1");

            AddNonUnique(tree, 8.0, 2.0, 1, 1, "R2");
            AssertFound(tree, 0, 0, 10, 10, "R0,R1,R2");

            AddNonUnique(tree, 4.0, 4.0, 1, 1, "R3");
            AssertFound(tree, 0, 0, 10, 10, "R0,R1,R2,R3");

            AddNonUnique(tree, 1.0, 9.0, 1, 1, "R4");
            AssertFound(tree, 0, 0, 10, 10, "R0,R1,R2,R3,R4");

            AddNonUnique(tree, 8.0, 3.0, 1, 1, "R5");
            AssertFound(tree, 0, 0, 10, 10, "R0,R1,R2,R3,R4,R5");

            AddNonUnique(tree, 0.0, 6.0, 1, 1, "R6");
            AssertFound(tree, 0, 0, 10, 10, "R0,R1,R2,R3,R4,R5,R6");

            AddNonUnique(tree, 5.0, 1.0, 1, 1, "R7");
            AssertFound(tree, 0, 0, 10, 10, "R0,R1,R2,R3,R4,R5,R6,R7");

            AddNonUnique(tree, 5.0, 8.0, 1, 1, "R8");
            AssertFound(tree, 0, 0, 10, 10, "R0,R1,R2,R3,R4,R5,R6,R7,R8");

            AddNonUnique(tree, 7.0, 6.0, 1, 1, "R9");
            AssertFound(tree, 0, 0, 10, 10, "R0,R1,R2,R3,R4,R5,R6,R7,R8,R9");
        }

        [Test]
        public void TestAddRemoveSamePoint()
        {
            tree = MXCIFQuadTreeFactory<object>.Make(0, 0, 100, 100);

            AddNonUnique(tree, 5, 8, 1, 1, "R1");
            AddNonUnique(tree, 5, 8, 1, 1, "R2");
            AssertFound(tree, 0, 0, 10, 10, "R1,R2");

            MXCIFQuadTreeRowIndexRemove.Remove(5, 8, 1, 1, "R1", tree);
            AssertFound(tree, 0, 0, 10, 10, "R2");

            MXCIFQuadTreeRowIndexRemove.Remove(5, 8, 1, 1, "R2", tree);
            AssertFound(tree, 0, 0, 10, 10, "");
        }

        [Test]
        public void TestFewValues()
        {
            tree = MXCIFQuadTreeFactory<object>.Make(0, 0, 100, 100);

            AddNonUnique(tree, 73.32704983331149, 23.46990952575032, 1, 1, "R0");
            AddNonUnique(tree, 53.09747562396894, 17.100976152185034, 1, 1, "R1");
            AddNonUnique(tree, 56.75757294858788, 25.508506696809608, 1, 1, "R2");
            AddNonUnique(tree, 83.66639067675291, 76.53772974832937, 1, 1, "R3");
            AddNonUnique(tree, 51.01654641861326, 43.49009281983866, 1, 1, "R4");

            double beginX = 50.45945198254618;
            double endX = 88.31594559038719;

            double beginY = 4.577595744501329;
            double endY = 22.93393078279351;

            AssertFound(tree, beginX, beginY, endX - beginX, endY - beginY, "R1");
        }

        [Test]
        public void TestAddRemoveScenario()
        {
            tree = MXCIFQuadTreeFactory<object>.Make(0, 0, 100, 100);

            AddUnique(tree, 85.0, 65.0, 0.999, 0.999, "P3");
            AddUnique(tree, 86.0, 50.0, 0.999, 0.999, "P6");
            AddUnique(tree, 17.0, 84.0, 0.999, 0.999, "P0");
            AddUnique(tree, 7.0, 34.0, 0.999, 0.999, "P4");
            AddUnique(tree, 7.0, 69.0, 0.999, 0.999, "P8");
            AddUnique(tree, 36.0, 47.0, 0.999, 0.999, "P9");
            AddUnique(tree, 62.0, 50.0, 0.999, 0.999, "P1");
            AddUnique(tree, 46.0, 17.0, 0.999, 0.999, "P2");
            AddUnique(tree, 43.0, 16.0, 0.999, 0.999, "P5");
            AddUnique(tree, 79.0, 92.0, 0.999, 0.999, "P7");
            Remove(tree, 46.0, 17.0, 0.999, 0.999, "P2");
            AddUnique(tree, 47.0, 17.0, 0.999, 0.999, "P2");
            Remove(tree, 43.0, 16.0, 0.999, 0.999, "P5");
            AddUnique(tree, 44.0, 16.0, 0.999, 0.999, "P5");
            Remove(tree, 62.0, 50.0, 0.999, 0.999, "P1");
            AddUnique(tree, 62.0, 49.0, 0.999, 0.999, "P1");
            Remove(tree, 17.0, 84.0, 0.999, 0.999, "P0");
            AddUnique(tree, 18.0, 84.0, 0.999, 0.999, "P0");
            Remove(tree, 86.0, 50.0, 0.999, 0.999, "P6");
            AddUnique(tree, 86.0, 51.0, 0.999, 0.999, "P6");
            AssertFound(tree, 81.0, 46.0, 10.0, 10.0, "P6");
        }
    }
} // end of namespace
