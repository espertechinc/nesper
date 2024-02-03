///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class SupportGeneratorPointUniqueByXYInteger : SupportQuadTreeUtil.Generator
    {
        public static readonly SupportGeneratorPointUniqueByXYInteger INSTANCE = new SupportGeneratorPointUniqueByXYInteger();

        private SupportGeneratorPointUniqueByXYInteger()
        {
        }

        public bool Unique()
        {
            return true;
        }

        public IList<SupportRectangleWithId> Generate(Random random, int numPoints, double x, double y, double width, double height)
        {
            IDictionary<XYPoint, SupportRectangleWithId> points = new Dictionary<XYPoint, SupportRectangleWithId>();
            int pointNum = 0;
            while (points.Count < numPoints)
            {
                float fx = random.Next(numPoints);
                float fy = random.Next(numPoints);
                XYPoint p = new XYPoint(fx, fy);
                if (points.ContainsKey(p))
                {
                    continue;
                }
                points.Put(p, new SupportRectangleWithId("P" + pointNum, fx, fy, 0, 0));
                pointNum++;
            }
            return new List<SupportRectangleWithId>(points.Values);
        }
    }
} // end of namespace
