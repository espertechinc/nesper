///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportSpatialEventRectangle
    {
        public SupportSpatialEventRectangle(
            string id,
            double? x,
            double? y,
            double? width,
            double? height,
            string category)
        {
            Id = id;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Category = category;
        }

        public SupportSpatialEventRectangle(
            string id,
            double x,
            double y,
            double width,
            double height) : this(id, x, y, width, height, null)
        {
        }

        public string Id { get; set; }

        public double? X { get; set; }

        public double? Y { get; set; }

        public double? Width { get; set; }

        public double? Height { get; set; }

        public string Category { get; set; }
    }
} // end of namespace