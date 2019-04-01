///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcif
{
    public class SupportRectangleWithId
    {
        public SupportRectangleWithId(string id, double x, double y, double w, double h)
        {
            Id = id;
            X = x;
            Y = y;
            W = w;
            H = h;
        }

        public string Id { get; }

        public double X { get; set; }

        public double Y { get; set; }

        public double W { get; set; }

        public double H { get; set; }
    }
} // end of namespace