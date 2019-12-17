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
    public class SupportGeneratorPointUniqueByXYDouble : SupportQuadTreeUtil.Generator
    {
        public static readonly SupportGeneratorPointUniqueByXYDouble INSTANCE = new SupportGeneratorPointUniqueByXYDouble();

        private SupportGeneratorPointUniqueByXYDouble()
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
            IDictionary<XYPoint, SupportRectangleWithId> points = new Dictionary<XYPoint, SupportRectangleWithId>();
            var pointNum = 0;
            while (points.Count < numPoints)
            {
                var px = x + width * random.NextDouble();
                var py = y + height * random.NextDouble();
                var point = new XYPoint(px, py);
                if (points.ContainsKey(point))
                {
                    continue;
                }

                points.Put(point, new SupportRectangleWithId("P" + pointNum, px, py, 0, 0));
                pointNum++;
            }

            return new List<SupportRectangleWithId>(points.Values);
        }
    }
} // end of namespace
