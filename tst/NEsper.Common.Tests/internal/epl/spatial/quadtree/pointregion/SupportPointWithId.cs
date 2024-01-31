///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.pointregion
{
    public class SupportPointWithId
    {
        public SupportPointWithId(
            string id,
            double x,
            double y)
        {
            Id = id;
            X = x;
            Y = y;
        }

        public string Id { get; }

        public double X { get; set; }

        public double Y { get; set; }
    }
} // end of namespace
