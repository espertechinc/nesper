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
    public class SupportEventRectangleWithOffset
    {
        public SupportEventRectangleWithOffset(
            string id,
            double? xOffset,
            double? yOffset,
            double? x,
            double? y,
            double? width,
            double? height)
        {
            Id = id;
            XOffset = xOffset;
            YOffset = yOffset;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public string Id { get; }

        public double? XOffset { get; }

        public double? YOffset { get; }

        public double? X { get; }

        public double? Y { get; }

        public double? Width { get; }

        public double? Height { get; }
    }
} // end of namespace