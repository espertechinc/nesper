///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportSpatialDualPoint
    {
        public SupportSpatialDualPoint(
            string id,
            double x1,
            double y1,
            double x2,
            double y2)
        {
            Id = id;
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

        public string Id { get; }

        public double X1 { get; }

        public double Y1 { get; }

        public double X2 { get; }

        public double Y2 { get; }
    }
} // end of namespace