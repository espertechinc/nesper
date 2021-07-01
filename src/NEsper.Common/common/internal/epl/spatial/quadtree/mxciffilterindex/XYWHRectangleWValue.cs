///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.type;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxciffilterindex
{
    public class XYWHRectangleWValue : XYWHRectangle
    {
        public XYWHRectangleWValue(
            double x,
            double y,
            double w,
            double h,
            object value)
            : base(x, y, w, h)
        {
            Value = value;
        }

        public object Value { get; set; }
    }
} // end of namespace