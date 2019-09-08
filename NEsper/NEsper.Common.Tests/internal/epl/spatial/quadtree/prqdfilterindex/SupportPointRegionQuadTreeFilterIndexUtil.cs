///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex
{
    public class SupportPointRegionQuadTreeFilterIndexUtil
    {
        private static readonly QuadTreeCollector<IDictionary<int, string>> MAP_COLLECTOR =
            new ProxyQuadTreeCollector<IDictionary<int, string>>(
                (
                    @event,
                    s,
                    target) => {
                        var num = int.Parse(((string) s).Substring(1));
                        if (target.ContainsKey(num))
                        {
                            throw new IllegalStateException();
                        }

                        target.Put(num, (string) s);
                    });

        private static readonly QuadTreeCollector<ICollection<object>> COLLECTION_COLLECTOR =
            new ProxyQuadTreeCollector<ICollection<object>>(
                (
                    @event,
                    s,
                    target) => target.Add(s));

        public static readonly SupportQuadTreeUtil.Querier<PointRegionQuadTree<object>> POINTREGION_FI_QUERIER =
        (
            tree,
            x,
            y,
            width,
            height) => {
                IList<object> received = new List<object>();
                PointRegionQuadTreeFilterIndexCollect<string, ICollection<object>>
                    .CollectRange(tree, x, y, width, height, null, received, COLLECTION_COLLECTOR);
                return received.IsEmpty() ? null : received;
            };

        public static readonly SupportQuadTreeUtil.AdderUnique<PointRegionQuadTree<object>> POINTREGION_FI_ADDERUNIQUE = (
            tree,
            value) => Set(tree, value.X, value.Y, value.Id);

        public static readonly SupportQuadTreeUtil.Remover<PointRegionQuadTree<object>> POINTREGION_FI_REMOVER = (
            tree,
            value) => Delete(tree, value.X, value.Y);

        internal static void Set(
            PointRegionQuadTree<object> quadTree,
            double x,
            double y,
            string value)
        {
            PointRegionQuadTreeFilterIndexSet<object>.Set(x, y, value, quadTree);
        }

        internal static void Delete(
            PointRegionQuadTree<object> tree,
            double x,
            double y)
        {
            PointRegionQuadTreeFilterIndexDelete<object>.Delete(x, y, tree);
        }

        internal static void AssertCollectAll(
            PointRegionQuadTree<object> tree,
            string expected)
        {
            var bb = tree.Root.Bb;
            AssertCollect(tree, bb.MinX, bb.MinY, bb.MaxX - bb.MinX, bb.MaxY - bb.MinY, expected);
            Assert.AreEqual(expected.Length == 0 ? 0 : expected.SplitCsv().Length, PointRegionQuadTreeFilterIndexCount.Count(tree));
            Assert.AreEqual(expected.Length == 0, PointRegionQuadTreeFilterIndexEmpty.IsEmpty(tree));
        }

        internal static void AssertCollect(
            PointRegionQuadTree<object> tree,
            double x,
            double y,
            double width,
            double height,
            string expected)
        {
            IDictionary<int, string> received = new SortedDictionary<int, string>();
            PointRegionQuadTreeFilterIndexCollect<string, IDictionary<int, string>>
                .CollectRange(tree, x, y, width, height, null, received, MAP_COLLECTOR);
            AssertCompare(tree, expected, received);
        }

        private static void AssertCompare(
            PointRegionQuadTree<object> tree,
            string expected,
            IDictionary<int, string> received)
        {
            StringJoiner joiner = new StringJoiner(",");
            foreach (string value in received.Values)
            {
                joiner.Add(value);
            }

            Assert.AreEqual(expected, joiner.ToString());
            Assert.IsTrue((expected.Length == 0 ? 0 : expected.SplitCsv().Length) <= PointRegionQuadTreeFilterIndexCount.Count(tree));
        }

        internal static void Compare<T>(
            double x,
            double y,
            T expected,
            XYPointWValue<T> point)
        {
            Assert.AreEqual(x, point.X);
            Assert.AreEqual(y, point.Y);
            Assert.AreEqual(expected, point.Value);
        }
    }
} // end of namespace
