///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    public class SupportMXCIFQuadTreeFilterIndexUtil
    {
        private static readonly QuadTreeCollector<ICollection<object>> COLLECTION_COLLECTOR =
            new ProxyQuadTreeCollector<ICollection<object>>(
                (
                    @event,
                    s,
                    target) => target.Add(s));

        private static readonly QuadTreeCollector<IDictionary<int, string>> MAP_COLLECTOR =
            new ProxyQuadTreeCollector<IDictionary<int, string>>(
                (
                    @event,
                    s,
                    target) => {
                        var asString = (string) s;
                        var num = int.Parse(asString.Substring(1));
                        if (target.ContainsKey(num))
                        {
                            throw new IllegalStateException();
                        }

                        target.Put(num, asString);
                    });

        public static readonly SupportQuadTreeUtil.Querier<MXCIFQuadTree> MXCIF_FI_QUERIER = (
            tree,
            x,
            y,
            width,
            height) => {
                IList<object> received = new List<object>();
                MXCIFQuadTreeFilterIndexCollect<ICollection<object>>
                    .CollectRange(tree, x, y, width, height, null, received, COLLECTION_COLLECTOR);
                // Comment-me-in: System.out.println("// query(tree, " + x + ", " + y + ", " + width + ", " + height + "); -=> " + received);
                return received.IsEmpty() ? null : received;
            };

        public static readonly SupportQuadTreeUtil.AdderUnique<MXCIFQuadTree> MXCIF_FI_ADDER = (
            tree,
            value) => Set(value.X, value.Y, value.W, value.H, value.Id, tree);

        public static readonly SupportQuadTreeUtil.Remover<MXCIFQuadTree> MXCIF_FI_REMOVER = (
            tree,
            value) => MXCIFQuadTreeFilterIndexDelete.Delete(value.X, value.Y, value.W, value.H, tree);

        public static void Set(
            double x,
            double y,
            double width,
            double height,
            String value,
            MXCIFQuadTree tree)
        {
            // Comment-me-in: System.out.println("set(" + x + ", " + y + ", " + width + ", " + height + ", \"" + value + "\", tree);");
            MXCIFQuadTreeFilterIndexSet.Set(x, y, width, height, value, tree);
        }

        internal static void AssertCollectAll(
            MXCIFQuadTree tree,
            String expected)
        {
            BoundingBox bb = tree.Root.Bb;
            AssertCollect(tree, bb.MinX, bb.MinY, bb.MaxX - bb.MinX, bb.MaxY - bb.MinY, expected);
            Assert.AreEqual(expected.Length == 0 ? 0 : expected.SplitCsv().Length, MXCIFQuadTreeFilterIndexCount.Count(tree));
            Assert.AreEqual(expected.Length == 0, MXCIFQuadTreeFilterIndexEmpty.IsEmpty(tree));
        }

        internal static void AssertCollect(
            MXCIFQuadTree tree,
            double x,
            double y,
            double width,
            double height,
            String expected)
        {
            IDictionary<int, string> received = new SortedDictionary<int, string>();
            MXCIFQuadTreeFilterIndexCollect<IDictionary<int, string>>
                .CollectRange(tree, x, y, width, height, null, received, MAP_COLLECTOR);
            AssertCompare(tree, expected, received);
        }

        private static void AssertCompare(
            MXCIFQuadTree tree,
            String expected,
            IDictionary<int, String> received)
        {
            StringJoiner joiner = new StringJoiner(",");
            foreach (string value in received.Values)
            {
                joiner.Add(value);
            }

            Assert.AreEqual(expected, joiner.ToString());
            Assert.IsTrue((expected.Length == 0 ? 0 : expected.SplitCsv().Length) <= MXCIFQuadTreeFilterIndexCount.Count(tree));
        }

        internal static void Compare(
            double x,
            double y,
            double width,
            double height,
            object expected,
            XYWHRectangleWValue rectangle)
        {
            Assert.AreEqual(x, rectangle.X);
            Assert.AreEqual(y, rectangle.Y);
            Assert.AreEqual(width, rectangle.W);
            Assert.AreEqual(height, rectangle.H);
            Assert.AreEqual(expected, rectangle.Value);
        }
    }
} // end of namespace
