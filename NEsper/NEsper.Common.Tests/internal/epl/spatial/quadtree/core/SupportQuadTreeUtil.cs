///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.core
{
    public class SupportQuadTreeUtil
    {
        public delegate ICollection<object> Querier<L>(
            L tree,
            double x,
            double y,
            double width,
            double height);

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void AssertIds(
            ICollection<SupportRectangleWithId> rects,
            ICollection<object> received,
            double x,
            double y,
            double width,
            double height,
            bool pointInsideChecking)
        {
            var boundingBox = new BoundingBox(x, y, x + width, y + height);
            IList<string> expected = new List<string>();
            foreach (var p in rects)
            {
                if (pointInsideChecking)
                {
                    if (boundingBox.ContainsPoint(p.X, p.Y))
                    {
                        expected.Add(p.Id);
                    }
                }
                else
                {
                    if (boundingBox.IntersectsBoxIncludingEnd(p.X, p.Y, p.W, p.H))
                    {
                        expected.Add(p.Id);
                    }
                }
            }

            Compare(received, expected);
        }

        private static void Compare(
            ICollection<object> receivedObjects,
            IList<string> expected)
        {
            if (expected.IsEmpty())
            {
                Assert.IsNull(receivedObjects);
                return;
            }

            if (receivedObjects == null)
            {
                Assert.Fail("Did not receive expected " + expected);
            }

            IList<string> received = new List<string>();
            foreach (var item in receivedObjects)
            {
                received.Add(item.ToString());
            }

            received.SortInPlace();
            expected.SortInPlace();
            var receivedText = received.ToString();
            var expectedText = expected.ToString();
            if (!expectedText.Equals(receivedText))
            {
                Log.Info("Expected:" + expectedText);
                Log.Info("Received:" + receivedText);
            }

            Assert.AreEqual(expectedText, receivedText);
        }

        public static void RandomQuery<L>(
            L quadTree,
            IList<SupportRectangleWithId> rectangles,
            Random random,
            double x,
            double y,
            double width,
            double height,
            Querier<L> querier,
            bool pointInsideChecking)
        {
            var bbWidth = random.NextDouble() * width * 1.5;
            var bbHeight = random.NextDouble() * height * 1.5;
            var bbMinX = random.NextDouble() * width + x * 0.8;
            var bbMinY = random.NextDouble() * height + y * 0.8;
            ICollection<object> actual = querier.Invoke(quadTree, bbMinX, bbMinY, bbWidth, bbHeight);
            AssertIds(rectangles, actual, bbMinX, bbMinY, bbWidth, bbHeight, pointInsideChecking);
        }

        public delegate L Factory<L>(SupportQuadTreeConfig config);

        public delegate void AdderUnique<L>(
                L tree,
                SupportRectangleWithId value);

        public delegate void AdderNonUnique<L>(
            L tree,
            SupportRectangleWithId value);

        public delegate void Remover<L>(
            L tree,
            SupportRectangleWithId value);

        public interface Generator
        {
            bool Unique();

            IList<SupportRectangleWithId> Generate(
                Random random,
                int numPoints,
                double x,
                double y,
                double width,
                double height);
        }
    }
} // end of namespace
