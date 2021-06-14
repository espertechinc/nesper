///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdfilterindex
{
    public class XYPointWValue<TL> : XYPointWOpaqueValue
    {
        public XYPointWValue(
            double x,
            double y,
            TL value)
            : base(x, y)
        {
            Value = value;
        }

        public TL Value { get; set; }

        public override object OpaqueValue {
            get => Value;
        }
    }
} // end of namespace