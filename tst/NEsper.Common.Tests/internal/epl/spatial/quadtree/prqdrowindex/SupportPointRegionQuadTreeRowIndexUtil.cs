///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdrowindex
{
    public class SupportPointRegionQuadTreeRowIndexUtil
    {
        public static readonly SupportQuadTreeUtil.Querier<PointRegionQuadTree<object>> POINTREGION_RI_QUERIER = (
            tree,
            x,
            y,
            width,
            height) => PointRegionQuadTreeRowIndexQuery.QueryRange(tree, x, y, width, height);

        public static readonly SupportQuadTreeUtil.AdderUnique<PointRegionQuadTree<object>> POINTREGION_RI_ADDERUNIQUE = (
            tree,
            value) => AddUnique(tree, value.X, value.Y, value.Id);

        public static readonly SupportQuadTreeUtil.Remover<PointRegionQuadTree<object>> POINTREGION_RI_REMOVER = (
            tree,
            value) => Remove(tree, value.X, value.Y, value.Id);

        public static readonly SupportQuadTreeUtil.AdderNonUnique<PointRegionQuadTree<object>> POINTREGION_RI_ADDERNONUNIQUE = (
            tree,
            value) => AddNonUnique(tree, value.X, value.Y, value.Id);

        internal static void Remove(
            PointRegionQuadTree<object> quadTree,
            double x,
            double y,
            string value)
        {
            PointRegionQuadTreeRowIndexRemove.Remove(x, y, value, quadTree);
        }

        internal static bool AddNonUnique(
            PointRegionQuadTree<object> quadTree,
            double x,
            double y,
            string value)
        {
            return PointRegionQuadTreeRowIndexAdd.Add(x, y, value, quadTree, false, "indexNameDummy");
        }

        internal static bool AddUnique(
            PointRegionQuadTree<object> quadTree,
            double x,
            double y,
            string value)
        {
            return PointRegionQuadTreeRowIndexAdd.Add(x, y, value, quadTree, true, "indexNameDummy");
        }

        internal static void AssertFound(
            PointRegionQuadTree<object> quadTree,
            double x,
            double y,
            double width,
            double height,
            string p1)
        {
            object[] expected = p1.Length == 0 ? null : p1.SplitCsv();
            AssertFound(quadTree, x, y, width, height, expected);
        }

        internal static void AssertFound(
            PointRegionQuadTree<object> quadTree,
            double x,
            double y,
            double width,
            double height,
            object[] ids)
        {
            var values = PointRegionQuadTreeRowIndexQuery.QueryRange(quadTree, x, y, width, height);
            if (ids == null || ids.Length == 0)
            {
                Assert.IsTrue(values == null);
            }
            else
            {
                if (values == null)
                {
                    Assert.Fail("Nothing returned, expected " + Arrays.AsList(ids));
                }

                EPAssertionUtil.AssertEqualsAnyOrder(ids, values.ToArray());
            }
        }

        internal static void Compare(
            double x,
            double y,
            string expected,
            XYPointMultiType point)
        {
            Assert.AreEqual(x, point.X);
            Assert.AreEqual(y, point.Y);
            Assert.AreEqual(expected, point.Multityped.RenderAny());
        }
    }
} // end of namespace
