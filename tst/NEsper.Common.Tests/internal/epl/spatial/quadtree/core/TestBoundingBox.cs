///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.supportunit.geom;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.core
{
    [TestFixture]
    public class TestBoundingBox : AbstractCommonTest
    {
        private void AssertIntersectsIncludingEnd(
            bool expected,
            Rectangle2D<double> one,
            Rectangle2D<double> two)
        {
            var bbOne = BoundingBox.From(one.X, one.Y, one.Width, one.Height);
            var bbTwo = BoundingBox.From(two.X, two.Y, two.Width, two.Height);

            ClassicAssert.AreEqual(expected, bbOne.IntersectsBoxIncludingEnd(two.X, two.Y, two.Width, two.Height));
            ClassicAssert.AreEqual(expected, bbTwo.IntersectsBoxIncludingEnd(one.X, one.Y, one.Width, one.Height));
        }

        private void RunAssertionQuadrant(
            double x,
            double y,
            BoundingBox bb,
            BoundingBox bbNW,
            BoundingBox bbNE,
            BoundingBox bbSW,
            BoundingBox bbSE,
            QuadrantEnum? expected)
        {
            if (!bb.ContainsPoint(x, y))
            {
                ClassicAssert.IsNull(expected);
                ClassicAssert.IsFalse(bbNW.ContainsPoint(x, y));
                ClassicAssert.IsFalse(bbNE.ContainsPoint(x, y));
                ClassicAssert.IsFalse(bbSW.ContainsPoint(x, y));
                ClassicAssert.IsFalse(bbSE.ContainsPoint(x, y));
                return;
            }

            var received = bb.GetQuadrant(x, y);
            ClassicAssert.AreEqual(expected, received);
            ClassicAssert.AreEqual(expected == QuadrantEnum.NW, bbNW.ContainsPoint(x, y));
            ClassicAssert.AreEqual(expected == QuadrantEnum.NE, bbNE.ContainsPoint(x, y));
            ClassicAssert.AreEqual(expected == QuadrantEnum.SW, bbSW.ContainsPoint(x, y));
            ClassicAssert.AreEqual(expected == QuadrantEnum.SE, bbSE.ContainsPoint(x, y));

            var subdivided = bb.Subdivide();
            ClassicAssert.AreEqual(expected == QuadrantEnum.NW, subdivided[0].ContainsPoint(x, y));
            ClassicAssert.AreEqual(expected == QuadrantEnum.NE, subdivided[1].ContainsPoint(x, y));
            ClassicAssert.AreEqual(expected == QuadrantEnum.SW, subdivided[2].ContainsPoint(x, y));
            ClassicAssert.AreEqual(expected == QuadrantEnum.SE, subdivided[3].ContainsPoint(x, y));
        }

        private void RunAssertionQuadrantAppliesMulti(
            double x,
            double y,
            double width,
            double height,
            BoundingBox bb,
            BoundingBox bbNW,
            BoundingBox bbNE,
            BoundingBox bbSW,
            BoundingBox bbSE,
            bool intersectsNW,
            bool intersectsNE,
            bool intersectsSW,
            bool intersectsSE)
        {
            ClassicAssert.AreEqual(QuadrantAppliesEnum.SOME, bb.GetQuadrantApplies(x, y, width, height));
            ClassicAssert.AreEqual(intersectsNW, bbNW.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.AreEqual(intersectsNE, bbNE.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.AreEqual(intersectsSW, bbSW.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.AreEqual(intersectsSE, bbSE.IntersectsBoxIncludingEnd(x, y, width, height));
        }

        private void RunAssertionQuadrantAppliesNone(
            double x,
            double y,
            double width,
            double height,
            BoundingBox bb,
            BoundingBox bbNW,
            BoundingBox bbNE,
            BoundingBox bbSW,
            BoundingBox bbSE)
        {
            ClassicAssert.AreEqual(QuadrantAppliesEnum.NONE, bb.GetQuadrantApplies(x, y, width, height));
            ClassicAssert.IsFalse(bb.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.IsFalse(bbNW.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.IsFalse(bbNE.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.IsFalse(bbSW.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.IsFalse(bbSE.IntersectsBoxIncludingEnd(x, y, width, height));
        }

        private void RunAssertionQuadrantAppliesAll(
            double x,
            double y,
            double width,
            double height,
            BoundingBox bb,
            BoundingBox bbNW,
            BoundingBox bbNE,
            BoundingBox bbSW,
            BoundingBox bbSE)
        {
            ClassicAssert.AreEqual(QuadrantAppliesEnum.SOME, bb.GetQuadrantApplies(x, y, width, height));
            ClassicAssert.IsTrue(bb.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.IsTrue(bbNW.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.IsTrue(bbNE.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.IsTrue(bbSW.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.IsTrue(bbSE.IntersectsBoxIncludingEnd(x, y, width, height));
        }

        private void RunAssertionQuadrantAppliesOne(
            double x,
            double y,
            double width,
            double height,
            BoundingBox bb,
            BoundingBox bbNW,
            BoundingBox bbNE,
            BoundingBox bbSW,
            BoundingBox bbSE,
            QuadrantAppliesEnum expected)
        {
            ClassicAssert.AreEqual(expected, bb.GetQuadrantApplies(x, y, width, height));
            ClassicAssert.IsTrue(bb.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.AreEqual(expected == QuadrantAppliesEnum.NW, bbNW.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.AreEqual(expected == QuadrantAppliesEnum.NE, bbNE.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.AreEqual(expected == QuadrantAppliesEnum.SW, bbSW.IntersectsBoxIncludingEnd(x, y, width, height));
            ClassicAssert.AreEqual(expected == QuadrantAppliesEnum.SE, bbSE.IntersectsBoxIncludingEnd(x, y, width, height));
        }

        private Rectangle2D<double> Rect(
            double x,
            double y,
            double width,
            double height)
        {
            return new Rectangle2D<double>(x, y, width, height);
        }

        [Test]
        public void TestContainsPoint()
        {
            var bb = new BoundingBox(10, 20, 40, 60);

            ClassicAssert.IsTrue(bb.ContainsPoint(10, 20));
            ClassicAssert.IsTrue(bb.ContainsPoint(39.9999, 59.9999));

            ClassicAssert.IsFalse(bb.ContainsPoint(40, 60));
            ClassicAssert.IsFalse(bb.ContainsPoint(10, 100));
            ClassicAssert.IsFalse(bb.ContainsPoint(100, 10));
        }

        [Test]
        public void TestFrom()
        {
            var bb = BoundingBox.From(10, 20, 4, 15);
            ClassicAssert.AreEqual(10d, bb.MinX);
            ClassicAssert.AreEqual(20d, bb.MinY);
            ClassicAssert.AreEqual(14d, bb.MaxX);
            ClassicAssert.AreEqual(35d, bb.MaxY);
        }

        [Test]
        public void TestIntersectsBoxIncludingEnd()
        {
            var @ref = Rect(1, 2, 4, 6);
            AssertIntersectsIncludingEnd(true, Rect(1, 2, 4, 6), @ref);
            AssertIntersectsIncludingEnd(true, Rect(2, 3, 1, 1), @ref);
            AssertIntersectsIncludingEnd(true, Rect(0, 0, 10, 10), @ref);

            // nw
            AssertIntersectsIncludingEnd(true, Rect(0, 0, 1.00001, 3), @ref);
            AssertIntersectsIncludingEnd(true, Rect(0, 0, 1, 2), @ref);
            AssertIntersectsIncludingEnd(false, Rect(0, 0, 0.99999, 2), @ref);

            // ne
            AssertIntersectsIncludingEnd(true, Rect(4.99999, 0, 1, 3), @ref);
            AssertIntersectsIncludingEnd(true, Rect(5, 0, 1, 3), @ref);
            AssertIntersectsIncludingEnd(false, Rect(5.00001, 0, 1, 3), @ref);

            // sw
            AssertIntersectsIncludingEnd(true, Rect(0, 7.9999, 1.5, 1), @ref);
            AssertIntersectsIncludingEnd(true, Rect(0, 8, 1.5, 1), @ref);
            AssertIntersectsIncludingEnd(false, Rect(0, 8.00001, 1.5, 1), @ref);

            // se
            AssertIntersectsIncludingEnd(true, Rect(0, 0, 3, 2.00001), @ref);
            AssertIntersectsIncludingEnd(true, Rect(0, 0, 3, 2), @ref);
            AssertIntersectsIncludingEnd(false, Rect(0, 0, 3, 1.99999), @ref);
        }

        [Test]
        public void TestQuadrant()
        {
            var bb = new BoundingBox(10, 20, 40, 60);

            var w = (bb.MaxX - bb.MinX) / 2d;
            var h = (bb.MaxY - bb.MinY) / 2d;

            var bbNW = new BoundingBox(bb.MinX, bb.MinY, bb.MinX + w, bb.MinY + h);
            var bbNE = new BoundingBox(bb.MinX + w, bb.MinY, bb.MaxX, bb.MinY + h);
            var bbSW = new BoundingBox(bb.MinX, bb.MinY + h, bb.MinX + w, bb.MaxY);
            var bbSE = new BoundingBox(bb.MinX + w, bb.MinY + h, bb.MaxX, bb.MaxY);

            RunAssertionQuadrant(10, 20, bb, bbNW, bbNE, bbSW, bbSE, QuadrantEnum.NW);
            RunAssertionQuadrant(9, 19, bb, bbNW, bbNE, bbSW, bbSE, null);
            RunAssertionQuadrant(40, 60, bb, bbNW, bbNE, bbSW, bbSE, null);
            RunAssertionQuadrant(39.9999, 59.999999, bb, bbNW, bbNE, bbSW, bbSE, QuadrantEnum.SE);
            RunAssertionQuadrant(39.9999, 20, bb, bbNW, bbNE, bbSW, bbSE, QuadrantEnum.NE);
            RunAssertionQuadrant(10, 59.999999, bb, bbNW, bbNE, bbSW, bbSE, QuadrantEnum.SW);
            RunAssertionQuadrant(24.9999, 39.9999, bb, bbNW, bbNE, bbSW, bbSE, QuadrantEnum.NW);
            RunAssertionQuadrant(25, 40, bb, bbNW, bbNE, bbSW, bbSE, QuadrantEnum.SE);
            RunAssertionQuadrant(24.9999, 40, bb, bbNW, bbNE, bbSW, bbSE, QuadrantEnum.SW);
            RunAssertionQuadrant(25, 39.9999, bb, bbNW, bbNE, bbSW, bbSE, QuadrantEnum.NE);
        }

        [Test]
        public void TestQuadrantIfFits()
        {
            var bb = new BoundingBox(10, 20, 40, 60);

            var w = (bb.MaxX - bb.MinX) / 2d;
            var h = (bb.MaxY - bb.MinY) / 2d;

            var bbNW = new BoundingBox(bb.MinX, bb.MinY, bb.MinX + w, bb.MinY + h);
            var bbNE = new BoundingBox(bb.MinX + w, bb.MinY, bb.MaxX, bb.MinY + h);
            var bbSW = new BoundingBox(bb.MinX, bb.MinY + h, bb.MinX + w, bb.MaxY);
            var bbSE = new BoundingBox(bb.MinX + w, bb.MinY + h, bb.MaxX, bb.MaxY);

            ClassicAssert.AreEqual(QuadrantAppliesEnum.NW, bb.GetQuadrantApplies(10, 20, 1, 1));
            ClassicAssert.AreEqual(QuadrantAppliesEnum.SOME, bb.GetQuadrantApplies(10, 20, 40, 60));
            ClassicAssert.AreEqual(QuadrantAppliesEnum.NONE, bb.GetQuadrantApplies(0, 0, 1, 1));

            // within single
            RunAssertionQuadrantAppliesOne(11, 21, 1, 1, bb, bbNW, bbNE, bbSW, bbSE, QuadrantAppliesEnum.NW);
            RunAssertionQuadrantAppliesOne(26, 21, 1, 1, bb, bbNW, bbNE, bbSW, bbSE, QuadrantAppliesEnum.NE);
            RunAssertionQuadrantAppliesOne(11, 50, 1, 1, bb, bbNW, bbNE, bbSW, bbSE, QuadrantAppliesEnum.SW);
            RunAssertionQuadrantAppliesOne(26, 50, 1, 1, bb, bbNW, bbNE, bbSW, bbSE, QuadrantAppliesEnum.SE);

            // NW approach
            RunAssertionQuadrantAppliesNone(0, 0, 0, 0, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesNone(0, 0, 9.9999, 19.9999, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesNone(0, 0, 10, 19.9999, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesNone(0, 0, 9.9999, 20, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesOne(0, 0, 10, 20, bb, bbNW, bbNE, bbSW, bbSE, QuadrantAppliesEnum.NW);
            RunAssertionQuadrantAppliesOne(0, 0, 10 + 14.999, 20 + 19.999, bb, bbNW, bbNE, bbSW, bbSE, QuadrantAppliesEnum.NW);
            RunAssertionQuadrantAppliesMulti(0, 0, 10 + 15, 20 + 19.9999, bb, bbNW, bbNE, bbSW, bbSE, true, true, false, false);
            RunAssertionQuadrantAppliesMulti(0, 0, 10 + 14.999, 20 + 20, bb, bbNW, bbNE, bbSW, bbSE, true, false, true, false);
            RunAssertionQuadrantAppliesMulti(0, 0, 10 + 15, 20 + 20, bb, bbNW, bbNE, bbSW, bbSE, true, true, true, true);

            // NE approach
            RunAssertionQuadrantAppliesNone(45, 0, 0, 0, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesNone(40.001, 0, 0, 19.9999, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesNone(40.001, 0, 0, 20, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesNone(40, 0, 0, 19.9999, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesOne(40, 0, 0, 20, bb, bbNW, bbNE, bbSW, bbSE, QuadrantAppliesEnum.NE);
            RunAssertionQuadrantAppliesOne(40 - 14.999, 0, 0, 20 + 19.999, bb, bbNW, bbNE, bbSW, bbSE, QuadrantAppliesEnum.NE);
            RunAssertionQuadrantAppliesMulti(40 - 15, 0, 0, 20 + 19.999, bb, bbNW, bbNE, bbSW, bbSE, true, true, false, false);
            RunAssertionQuadrantAppliesMulti(40 - 14.999, 0, 0, 20 + 20, bb, bbNW, bbNE, bbSW, bbSE, false, true, false, true);
            RunAssertionQuadrantAppliesMulti(40 - 15, 0, 0, 20 + 20, bb, bbNW, bbNE, bbSW, bbSE, true, true, true, true);

            // SW approach
            RunAssertionQuadrantAppliesNone(0, 70, 0, 0, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesNone(0, 60.0001, 9.9999, 0, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesNone(0, 60.0001, 10, 0, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesNone(0, 60, 9.9999, 0, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesOne(0, 60, 10, 0, bb, bbNW, bbNE, bbSW, bbSE, QuadrantAppliesEnum.SW);
            RunAssertionQuadrantAppliesOne(0, 40.001, 10 + 14.999, 5, bb, bbNW, bbNE, bbSW, bbSE, QuadrantAppliesEnum.SW);
            RunAssertionQuadrantAppliesMulti(0, 40, 10 + 14.999, 5, bb, bbNW, bbNE, bbSW, bbSE, true, false, true, false);
            RunAssertionQuadrantAppliesMulti(0, 40.001, 10 + 15, 5, bb, bbNW, bbNE, bbSW, bbSE, false, false, true, true);
            RunAssertionQuadrantAppliesMulti(0, 40, 10 + 15, 5, bb, bbNW, bbNE, bbSW, bbSE, true, true, true, true);

            // SE approach
            RunAssertionQuadrantAppliesNone(50, 70, 0, 0, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesNone(40.001, 60.001, 0, 0, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesNone(40, 60.001, 0, 0, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesNone(40.001, 60, 0, 0, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesOne(40, 60, 0, 0, bb, bbNW, bbNE, bbSW, bbSE, QuadrantAppliesEnum.SE);
            RunAssertionQuadrantAppliesOne(40 - 14.9999, 60 - 19.999, 100, 100, bb, bbNW, bbNE, bbSW, bbSE, QuadrantAppliesEnum.SE);
            RunAssertionQuadrantAppliesMulti(40 - 14.9999, 60 - 20, 100, 100, bb, bbNW, bbNE, bbSW, bbSE, false, true, false, true);
            RunAssertionQuadrantAppliesMulti(40 - 15, 60 - 19.999, 100, 100, bb, bbNW, bbNE, bbSW, bbSE, false, false, true, true);
            RunAssertionQuadrantAppliesMulti(40 - 15, 60 - 20, 100, 100, bb, bbNW, bbNE, bbSW, bbSE, true, true, true, true);

            // contains-all
            RunAssertionQuadrantAppliesAll(0, 0, 100, 100, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesAll(10, 20, 40, 60, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesAll(24, 39, 2, 2, bb, bbNW, bbNE, bbSW, bbSE);
            RunAssertionQuadrantAppliesAll(25, 40, 0, 0, bb, bbNW, bbNE, bbSW, bbSE);

            // start-within
            RunAssertionQuadrantAppliesOne(10, 20, 14.9999, 19.999, bb, bbNW, bbNE, bbSW, bbSE, QuadrantAppliesEnum.NW);
            RunAssertionQuadrantAppliesMulti(10, 20, 15, 19.999, bb, bbNW, bbNE, bbSW, bbSE, true, true, false, false);
            RunAssertionQuadrantAppliesMulti(10, 20, 14.9999, 20, bb, bbNW, bbNE, bbSW, bbSE, true, false, true, false);
            RunAssertionQuadrantAppliesMulti(10, 20, 15, 20, bb, bbNW, bbNE, bbSW, bbSE, true, true, true, true);

            // try random
            var random = new Random();
            for (var i = 0; i < 1000000; i++)
            {
                var width = random.NextDouble() * 50;
                var height = random.NextDouble() * 70;
                var x = random.NextDouble() * 50;
                var y = random.NextDouble() * 70 + 10;
                var result = bb.GetQuadrantApplies(x, y, width, height);
                var nw = bbNW.IntersectsBoxIncludingEnd(x, y, width, height);
                var ne = bbNE.IntersectsBoxIncludingEnd(x, y, width, height);
                var sw = bbSW.IntersectsBoxIncludingEnd(x, y, width, height);
                var se = bbSE.IntersectsBoxIncludingEnd(x, y, width, height);
                if (result == QuadrantAppliesEnum.NONE && nw | ne | sw | se)
                {
                    Assert.Fail();
                }
                else if (result == QuadrantAppliesEnum.SOME)
                {
                    ClassicAssert.IsTrue((nw ? 1 : 0) + (ne ? 1 : 0) + (sw ? 1 : 0) + (se ? 1 : 0) > 1);
                }
                else
                {
                    ClassicAssert.AreEqual(result == QuadrantAppliesEnum.NW, nw);
                    ClassicAssert.AreEqual(result == QuadrantAppliesEnum.NE, ne);
                    ClassicAssert.AreEqual(result == QuadrantAppliesEnum.SW, sw);
                    ClassicAssert.AreEqual(result == QuadrantAppliesEnum.SE, se);
                }
            }
        }

        [Test]
        public void TestTreeForDepth()
        {
            var bb = new BoundingBox(0, 0, 100, 100);
            var node = bb.TreeForDepth(3);
            var swNwNw = node.se.nw.nw.bb;
            ClassicAssert.IsTrue(swNwNw.Equals(new BoundingBox(50, 50, 50 + 100 / 2 / 2 / 2.0, 50 + 100 / 2 / 2 / 2.0)));
        }

        [Test]
        public void TestTreeForPath()
        {
            var bb = new BoundingBox(0, 0, 100, 100);
            var node = bb.TreeForPath(new [] { "se","nw","ne","sw" });
            var inner = node.se.nw.ne.sw.bb;
            var tree = bb.TreeForDepth(4);
            ClassicAssert.IsTrue(inner.Equals(tree.se.nw.ne.sw.bb));

            ClassicAssert.AreEqual(node.nw, node.GetQuadrant(QuadrantEnum.NW));
            ClassicAssert.AreEqual(node.ne, node.GetQuadrant(QuadrantEnum.NE));
            ClassicAssert.AreEqual(node.sw, node.GetQuadrant(QuadrantEnum.SW));
            ClassicAssert.AreEqual(node.se, node.GetQuadrant(QuadrantEnum.SE));
        }
    }
} // end of namespace