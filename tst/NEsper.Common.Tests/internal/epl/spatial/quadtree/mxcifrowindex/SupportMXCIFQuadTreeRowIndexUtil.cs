///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.core;
using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex
{
    public class SupportMXCIFQuadTreeRowIndexUtil
    {
        public static readonly SupportQuadTreeUtil.AdderUnique<MXCIFQuadTree> MXCIF_RI_ADDERUNIQUE = (
            tree,
            value) => AddUnique(tree, value.X, value.Y, value.W, value.H, value.Id);

        public static readonly SupportQuadTreeUtil.Remover<MXCIFQuadTree> MXCIF_RI_REMOVER = (
            tree,
            value) => Remove(tree, value.X, value.Y, value.W, value.H, value.Id);

        public static readonly SupportQuadTreeUtil.Querier<MXCIFQuadTree> MXCIF_RI_QUERIER = QueryWLog;

        public static readonly SupportQuadTreeUtil.AdderNonUnique<MXCIFQuadTree> MXCIF_RI_ADDERNONUNIQUE = (
            tree,
            value) => SupportMXCIFQuadTreeRowIndexUtil.AddNonUnique(tree, value.X, value.Y, value.W, value.H, value.Id);

        internal static ICollection<object> QueryWLog(
            MXCIFQuadTree quadTree,
            double x,
            double y,
            double width,
            double height)
        {
            var values = MXCIFQuadTreeRowIndexQuery.QueryRange(quadTree, x, y, width, height);
            // Comment-me-in: Console.WriteLine("// query(tree, " + x + ", " + y + ", " + width + ", " + height + "); -=> " + values);
            return values;
        }

        internal static void Remove(
            MXCIFQuadTree quadTree,
            double x,
            double y,
            double width,
            double height,
            string value)
        {
            // Comment-me-in: Console.WriteLine("remove(tree, " + x + ", " + y + ", " + width + ", " + height + ", \"" + value + "\");");
            MXCIFQuadTreeRowIndexRemove.Remove(x, y, width, height, value, quadTree);
        }

        internal static bool AddNonUnique(
            MXCIFQuadTree quadTree,
            double x,
            double y,
            double width,
            double height,
            string value)
        {
            // Comment-me-in: Console.WriteLine("addNonUnique(tree, " + x + ", " + y + ", " + width + ", " + height + ", \"" + value + "\");");
            return MXCIFQuadTreeRowIndexAdd.Add(x, y, width, height, value, quadTree, false, "indexNameDummy");
        }

        public static void AddUnique(
            MXCIFQuadTree tree,
            double x,
            double y,
            double width,
            double height,
            string value)
        {
            // Comment-me-in: Console.WriteLine("addUnique(tree, " + x + ", " + y + ", " + width + ", " + height + ", \"" + value + "\");");
            MXCIFQuadTreeRowIndexAdd.Add(x, y, width, height, value, tree, true, "indexNameHere");
        }

        internal static void AssertFound(
            MXCIFQuadTree quadTree,
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
            MXCIFQuadTree quadTree,
            double x,
            double y,
            double width,
            double height,
            object[] ids)
        {
            var values = MXCIFQuadTreeRowIndexQuery.QueryRange(quadTree, x, y, width, height);
            if (ids == null || ids.Length == 0)
            {
                ClassicAssert.IsTrue(values == null);
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
            double width,
            double height,
            string expected,
            XYWHRectangleMultiType rectangle)
        {
            ClassicAssert.AreEqual(x, rectangle.X);
            ClassicAssert.AreEqual(y, rectangle.Y);
            ClassicAssert.AreEqual(width, rectangle.W);
            ClassicAssert.AreEqual(height, rectangle.H);
            ClassicAssert.AreEqual(expected, rectangle.Multityped.RenderAny());
        }
    }
} // end of namespace
