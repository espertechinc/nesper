///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.core
{
    public class SupportGeneratorRectangleUniqueByXYWH : SupportQuadTreeUtil.Generator
    {
        public static readonly SupportGeneratorRectangleUniqueByXYWH INSTANCE = new SupportGeneratorRectangleUniqueByXYWH();

        private SupportGeneratorRectangleUniqueByXYWH()
        {
        }

        public bool Unique()
        {
            return true;
        }

        public IList<SupportRectangleWithId> Generate(
            Random random,
            int numPoints,
            double x,
            double y,
            double width,
            double height)
        {
            IDictionary<XYWHRectangle, SupportRectangleWithId> rectangles = new Dictionary<XYWHRectangle, SupportRectangleWithId>();
            var pointNum = 0;
            while (rectangles.Count < numPoints)
            {
                double rx;
                double ry;
                double rwidth;
                double rheight;
                while (true)
                {
                    rx = x + width * random.NextDouble() - 5;
                    ry = y + height * random.NextDouble() - 5;
                    rwidth = width * random.NextDouble();
                    rheight = height * random.NextDouble();
                    if (BoundingBox.IntersectsBoxIncludingEnd(x, y, x + width, y + height, rx, ry, rwidth, rheight))
                    {
                        break;
                    }
                }

                var rectangle = new XYWHRectangle(rx, ry, rwidth, rheight);
                if (rectangles.ContainsKey(rectangle))
                {
                    continue;
                }

                rectangles.Put(rectangle, new SupportRectangleWithId("P" + pointNum, rectangle.X, rectangle.Y, rectangle.W, rectangle.H));
                pointNum++;
            }

            return new List<SupportRectangleWithId>(rectangles.Values);
        }
    }
} // end of namespace
