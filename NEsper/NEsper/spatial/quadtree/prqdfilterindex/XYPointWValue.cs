///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.spatial.quadtree.pointregion;

namespace com.espertech.esper.spatial.quadtree.prqdfilterindex
{
    public class XYPointWValue<TL> : XYPoint
    {
        public XYPointWValue(double x, double y, TL value)
            : base(x, y)
        {
            Value = value;
        }

        public TL Value { get; set; }
    }
} // end of namespace
