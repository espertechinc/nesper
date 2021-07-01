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
    public class SupportSpatialAABB
    {
        public SupportSpatialAABB(
            string id,
            double x,
            double y,
            double width,
            double height,
            string category)
        {
            Id = id;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Category = category;
        }

        public SupportSpatialAABB(
            string id,
            double x,
            double y,
            double width,
            double height) : this(id, x, y, width, height, null)
        {
        }

        public string Id { get; }

        public double X { get; }

        public double Y { get; }

        public double Width { get; }

        public double Height { get; }

        public string Category { get; }
    }
} // end of namespace