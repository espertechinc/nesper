///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.compat.collections;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif.SupportMXCIFQuadTreeUtil;
using static com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex.SupportMXCIFQuadTreeRowIndexUtil;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex
{
    [TestFixture]
    public class TestMxcifQuadTreeRowIndexScenarios : AbstractCommonTest
    {
        [Test]
        public void TestSubdivideAdd()
        {
            var tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100, 2, 3);
            AddNonUnique(tree, 0, 0, 10, 10, "R1");
            AddNonUnique(tree, 0, 0, 10, 10, "R2");
            AddNonUnique(tree, 0, 0, 10, 10, "R3");
            ClassicAssert.AreEqual(3, NavigateLeaf(tree, "nw,nw").Count);
        }

        [Test]
        public void TestDimension()
        {
            var tree = MXCIFQuadTreeFactory.Make(1000, 100000, 9000, 900000);
            ClassicAssert.IsFalse(AddNonUnique(tree, 10, 90, 1, 1, "R1"));
            ClassicAssert.IsFalse(AddNonUnique(tree, 10999999, 90, 1, 1, "R2"));
            ClassicAssert.IsTrue(AddNonUnique(tree, 5000, 800000, 1, 1, "R3"));

            AssertFound(tree, 0, 0, 10000000, 10000000, "R3");
            AssertFound(tree, 4000, 790000, 1200, 11000, "R3");
            AssertFound(tree, 4000, 790000, 900, 9000, "");
        }

        [Test]
        public void TestSuperslim()
        {
            var tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100, 1, 100);
            AddNonUnique(tree, 10, 90, 1, 1, "R1");
            AddNonUnique(tree, 10, 95, 1, 1, "R2");
            var ne = NavigateLeaf(tree, "sw,sw,sw,ne");
            Compare(10, 90, 1, 1, "\"R1\"", (XYWHRectangleMultiType) ne.Data);
            var se = NavigateLeaf(tree, "sw,sw,sw,se");
            Compare(10, 95, 1, 1, "\"R2\"", (XYWHRectangleMultiType) se.Data);
        }

        [Test]
        public void TestSubdivideMultiChild()
        {
            var tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100, 4, 3);
            AddNonUnique(tree, 60, 10, 1, 1, "R1");
            AddNonUnique(tree, 60, 40, 1, 1, "R2");
            AddNonUnique(tree, 70, 30, 1, 1, "R3");
            AddNonUnique(tree, 60, 10, 1, 1, "R4");
            AddNonUnique(tree, 90, 45, 1, 1, "R5");

            NavigateLeaf(tree, "nw");
            NavigateLeaf(tree, "se");
            NavigateLeaf(tree, "sw");
            var ne = NavigateBranch(tree, "ne");
            ClassicAssert.AreEqual(2, ne.Level);

            var nw = NavigateLeaf(ne, "nw");
            Compare(60, 10, 1, 1, "[\"R1\", \"R4\"]", (XYWHRectangleMultiType) nw.Data);
            ClassicAssert.AreEqual(2, nw.Count);

            var se = NavigateLeaf(ne, "se");
            Compare(90, 45, 1, 1, "\"R5\"", (XYWHRectangleMultiType) se.Data);
            ClassicAssert.AreEqual(1, se.Count);

            var sw = NavigateLeaf(ne, "sw");
            var collection = sw.Data.UnwrapIntoList<XYWHRectangleMultiType>();
            Compare(60, 40, 1, 1, "\"R2\"", collection[0]);
            Compare(70, 30, 1, 1, "\"R3\"", collection[1]);
            ClassicAssert.AreEqual(2, sw.Count);

            Remove(tree, 60, 10, 1, 1, "R1");
            Remove(tree, 60, 40, 1, 1, "R2");

            var root = NavigateLeaf(tree, "");
            collection = root.Data.UnwrapIntoList<XYWHRectangleMultiType>();
            ClassicAssert.AreEqual(3, root.Count);
            ClassicAssert.AreEqual(3, collection.Count);
            Compare(60, 10, 1, 1, "[\"R4\"]", collection[0]);
            Compare(70, 30, 1, 1, "\"R3\"", collection[1]);
            Compare(90, 45, 1, 1, "\"R5\"", collection[2]);
        }

        [Test]
        public void TestRemoveNonExistent()
        {
            var tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100, 20, 20);
            Remove(tree, 10, 61, 1, 1, "R1");
            AddNonUnique(tree, 10, 60, 1, 1, "R1");
            Remove(tree, 10, 61, 1, 1, "R1");
            Remove(tree, 10, 60, 2, 1, "R1");
            Remove(tree, 10, 60, 1, 2, "R1");
            Remove(tree, 11, 60, 1, 1, "R1");
            AddNonUnique(tree, 10, 80, 1, 1, "R2");
            AddNonUnique(tree, 20, 70, 1, 1, "R3");
            AddNonUnique(tree, 10, 80, 1, 1, "R4");
            ClassicAssert.AreEqual(4, NavigateLeaf(tree, "").Count);
            AssertFound(tree, 10, 60, 10000, 10000, "R1,R2,R3,R4");

            Remove(tree, 10, 61, 1, 1, "R1");
            Remove(tree, 9, 60, 1, 1, "R1");
            Remove(tree, 10, 60, 1, 1, "R2");
            Remove(tree, 10, 80, 1, 1, "R1");
            ClassicAssert.AreEqual(4, NavigateLeaf(tree, "").Count);
            AssertFound(tree, 10, 60, 10000, 10000, "R1,R2,R3,R4");

            Remove(tree, 10, 80, 1, 1, "R4");
            ClassicAssert.AreEqual(3, NavigateLeaf(tree, "").Count);
            AssertFound(tree, 10, 60, 10000, 10000, "R1,R2,R3");

            Remove(tree, 10, 80, 1, 1, "R2");
            ClassicAssert.AreEqual(2, NavigateLeaf(tree, "").Count);
            AssertFound(tree, 10, 60, 10000, 10000, "R1,R3");

            Remove(tree, 10, 60, 1, 1, "R1");
            ClassicAssert.AreEqual(1, NavigateLeaf(tree, "").Count);
            AssertFound(tree, 10, 60, 10000, 10000, "R3");

            Remove(tree, 20, 70, 1, 1, "R3");
            ClassicAssert.AreEqual(0, NavigateLeaf(tree, "").Count);
            AssertFound(tree, 10, 60, 10000, 10000, "");
        }

        [Test]
        public void TestSubdivideSingleMerge()
        {
            var tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100, 3, 2);
            AddNonUnique(tree, 65, 75, 1, 1, "P1");
            AddNonUnique(tree, 80, 75, 1, 1, "P2");
            AddNonUnique(tree, 80, 60, 1, 1, "P3");
            AddNonUnique(tree, 80, 60, 1, 1, "P4");
            AssertFound(tree, 60, 60, 21, 21, "P1,P2,P3,P4");

            ClassicAssert.IsFalse(tree.Root is MXCIFQuadTreeNodeLeaf);
            ClassicAssert.AreEqual(4, NavigateLeaf(tree, "se").Count);
            var collection = NavigateLeaf(tree, "se").Data.UnwrapIntoList<XYWHRectangleMultiType>();
            ClassicAssert.AreEqual(3, collection.Count);
            Compare(65, 75, 1, 1, "\"P1\"", collection[0]);
            Compare(80, 75, 1, 1, "\"P2\"", collection[1]);
            Compare(80, 60, 1, 1, "[\"P3\", \"P4\"]", collection[2]);

            AddNonUnique(tree, 66, 78, 1, 1, "P5");
            Remove(tree, 65, 75, 1, 1, "P1");
            Remove(tree, 80, 60, 1, 1, "P3");

            collection = NavigateLeaf(tree, "se").Data.UnwrapIntoList<XYWHRectangleMultiType>();
            ClassicAssert.AreEqual(3, NavigateLeaf(tree, "se").Count);
            AssertFound(tree, 60, 60, 21, 21, "P2,P4,P5");
            ClassicAssert.AreEqual(3, collection.Count);
            Compare(80, 75, 1, 1, "\"P2\"", collection[0]);
            Compare(80, 60, 1, 1, "[\"P4\"]", collection[1]);
            Compare(66, 78, 1, 1, "\"P5\"", collection[2]);

            Remove(tree, 66, 78, 1, 1, "P5");

            AssertFound(tree, 60, 60, 21, 21, "P2,P4");
            ClassicAssert.AreEqual(2, NavigateLeaf(tree, "").Count);
            collection = NavigateLeaf(tree, "").Data.UnwrapIntoList<XYWHRectangleMultiType>();
            ClassicAssert.AreEqual(2, collection.Count);
            Compare(80, 75, 1, 1, "\"P2\"", collection[0]);
            Compare(80, 60, 1, 1, "[\"P4\"]", collection[1]);
        }

        [Test]
        public void TestSubdivideMultitypeMerge()
        {
            var tree = MXCIFQuadTreeFactory.Make(0, 0, 100, 100, 6, 2);
            ClassicAssert.AreEqual(1, tree.Root.Level);
            AddNonUnique(tree, 10, 10, 0, 0, "P1");
            AddNonUnique(tree, 9.9, 10, 0, 0, "P2");
            AddNonUnique(tree, 10, 9.9, 0, 0, "P3");
            AddNonUnique(tree, 10, 10, 0, 0, "P4");
            AddNonUnique(tree, 10, 9.9, 0, 0, "P5");
            AddNonUnique(tree, 9.9, 10, 0, 0, "P6");
            ClassicAssert.IsInstanceOf<MXCIFQuadTreeNodeLeaf>(tree.Root);
            AssertFound(tree, 9, 10, 0.99, 0.99, "P2,P6");
            AssertFound(tree, 10, 9, 0.99, 0.99, "P3,P5");
            AssertFound(tree, 10, 10, 0, 0, "P1,P4");
            AssertFound(tree, 9, 9, 2, 2, "P1,P2,P3,P4,P5,P6");

            AddNonUnique(tree, 10, 10, 0, 0, "P7");

            ClassicAssert.IsNotInstanceOf<MXCIFQuadTreeNodeLeaf>(tree.Root);
            ClassicAssert.AreEqual(1, tree.Root.Level);
            ClassicAssert.AreEqual(7, NavigateLeaf(tree, "nw").Count);
            var collection = NavigateLeaf(tree, "nw").Data.UnwrapIntoList<XYWHRectangleMultiType>();
            ClassicAssert.AreEqual(3, collection.Count);
            Compare(10, 10, 0, 0, "[\"P1\", \"P4\", \"P7\"]", collection[0]);
            Compare(9.9, 10, 0, 0, "[\"P2\", \"P6\"]", collection[1]);
            Compare(10, 9.9, 0, 0, "[\"P3\", \"P5\"]", collection[2]);
            AssertFound(tree, 9, 10, 0.99, 0.99, "P2,P6");
            AssertFound(tree, 10, 9, 0.99, 0.99, "P3,P5");
            AssertFound(tree, 10, 10, 0, 0, "P1,P4,P7");
            AssertFound(tree, 9, 9, 2, 2, "P1,P2,P3,P4,P5,P6,P7");

            AddNonUnique(tree, 9.9, 10, 0, 0, "P8");
            AddNonUnique(tree, 10, 9.9, 0, 0, "P9");
            AddNonUnique(tree, 10, 10, 0, 0, "P10");
            AddNonUnique(tree, 10, 10, 0, 0, "P11");
            AddNonUnique(tree, 10, 10, 0, 0, "P12");

            ClassicAssert.AreEqual(12, NavigateLeaf(tree, "nw").Count);
            ClassicAssert.AreEqual(2, NavigateLeaf(tree, "nw").Level);
            ClassicAssert.AreEqual(3, collection.Count);
            Compare(10, 10, 0, 0, "[\"P1\", \"P4\", \"P7\", \"P10\", \"P11\", \"P12\"]", collection[0]);
            Compare(9.9, 10, 0, 0, "[\"P2\", \"P6\", \"P8\"]", collection[1]);
            Compare(10, 9.9, 0, 0, "[\"P3\", \"P5\", \"P9\"]", collection[2]);
            AssertFound(tree, 9, 10, 0.99, 0.99, "P2,P6,P8");
            AssertFound(tree, 10, 9, 0.99, 0.99, "P3,P5,P9");
            AssertFound(tree, 10, 10, 1, 1, "P1,P4,P7,P10,P11,P12");
            AssertFound(tree, 9, 9, 2, 2, "P1,P2,P3,P4,P5,P6,P7,P8,P9,P10,P11,P12");

            Remove(tree, 9.9, 10, 0, 0, "P8");
            Remove(tree, 10, 9.9, 0, 0, "P3");
            Remove(tree, 10, 9.9, 0, 0, "P5");
            Remove(tree, 10, 9.9, 0, 0, "P9");
            AssertFound(tree, 9, 10, 0.99, 0.99, "P2,P6");
            AssertFound(tree, 10, 9, 0.99, 0.99, "");
            AssertFound(tree, 10, 10, 1, 1, "P1,P4,P7,P10,P11,P12");
            AssertFound(tree, 9, 9, 2, 2, "P1,P2,P4,P6,P7,P10,P11,P12");

            collection = NavigateLeaf(tree, "nw").Data.UnwrapIntoList<XYWHRectangleMultiType>();
            ClassicAssert.AreEqual(8, NavigateLeaf(tree, "nw").Count);
            ClassicAssert.AreEqual(2, collection.Count);
            Compare(10, 10, 0, 0, "[\"P1\", \"P4\", \"P7\", \"P10\", \"P11\", \"P12\"]", collection[0]);
            Compare(9.9, 10, 0, 0, "[\"P2\", \"P6\"]", collection[1]);
            ClassicAssert.IsFalse(tree.Root is MXCIFQuadTreeNodeLeaf);

            Remove(tree, 9.9, 10, 0, 0, "P2");
            Remove(tree, 10, 10, 0, 0, "P1");
            Remove(tree, 10, 10, 0, 0, "P10");
            ClassicAssert.IsInstanceOf<MXCIFQuadTreeNodeLeaf>(tree.Root);
            ClassicAssert.AreEqual(5, NavigateLeaf(tree, "").Count);
            collection = NavigateLeaf(tree, "").Data.UnwrapIntoList<XYWHRectangleMultiType>();
            ClassicAssert.AreEqual(2, collection.Count);
            Compare(10, 10, 0, 0, "[\"P4\", \"P7\", \"P11\", \"P12\"]", collection[0]);
            Compare(9.9, 10, 0, 0, "[\"P6\"]", collection[1]);
            AssertFound(tree, 9, 10, 0.99, 0.99, "P6");
            AssertFound(tree, 10, 9, 0.99, 0.99, "");
            AssertFound(tree, 10, 10, 1, 1, "P4,P7,P11,P12");
            AssertFound(tree, 9, 9, 2, 2, "P4,P6,P7,P11,P12");
        }
    }
} // end of namespace
