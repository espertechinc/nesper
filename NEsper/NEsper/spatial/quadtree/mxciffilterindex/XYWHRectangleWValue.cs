///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using com.espertech.esper.spatial.quadtree.mxcif;

namespace com.espertech.esper.spatial.quadtree.mxciffilterindex
{
    public class XYWHRectangleWValue<L> : XYWHRectangle
    {
        public XYWHRectangleWValue(double x, double y, double w, double h, L value)
            : base(x, y, w, h)
        {
            Value = value;
        }

        internal L Value;
    }
} // end of namespace