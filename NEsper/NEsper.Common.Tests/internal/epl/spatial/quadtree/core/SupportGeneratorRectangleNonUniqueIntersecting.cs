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

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.core
{
    public class SupportGeneratorRectangleNonUniqueIntersecting : SupportQuadTreeUtil.Generator
    {
        public static readonly SupportGeneratorRectangleNonUniqueIntersecting INSTANCE = new SupportGeneratorRectangleNonUniqueIntersecting();

        private SupportGeneratorRectangleNonUniqueIntersecting()
        {
        }

        public bool Unique()
        {
            return false;
        }

        public IList<SupportRectangleWithId> Generate(
            Random random,
            int numPoints,
            double x,
            double y,
            double width,
            double height)
        {
            IList<SupportRectangleWithId> points = new List<SupportRectangleWithId>();
            for (var i = 0; i < numPoints; i++)
            {
                double rx;
                double ry;
                double rwidth;
                double rheight;
                while (true)
                {
                    rx = random.NextDouble() * width + x;
                    ry = random.NextDouble() * height + y;
                    rwidth = random.NextDouble() * 10d;
                    rheight = random.NextDouble() * 10d;
                    if (BoundingBox.IntersectsBoxIncludingEnd(x, y, x + width, y + height, rx, ry, rwidth, rheight))
                    {
                        break;
                    }
                }

                points.Add(new SupportRectangleWithId("P" + i, rx, ry, rwidth, rheight));
            }

            return points;
        }
    }
} // end of namespace