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
    public class SupportGeneratorPointNonUniqueDouble : SupportQuadTreeUtil.Generator
    {
        public static readonly SupportGeneratorPointNonUniqueDouble INSTANCE = new SupportGeneratorPointNonUniqueDouble();

        private SupportGeneratorPointNonUniqueDouble()
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
            IList<SupportRectangleWithId> result = new List<SupportRectangleWithId>(numPoints);
            for (var i = 0; i < numPoints; i++)
            {
                var px = random.NextDouble() * width + x;
                var py = random.NextDouble() * height + y;
                result.Add(new SupportRectangleWithId("P" + i, px, py, 0, 0));
            }

            return result;
        }
    }
} // end of namespace