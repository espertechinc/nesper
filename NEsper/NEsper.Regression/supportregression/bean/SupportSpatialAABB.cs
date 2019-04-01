///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class SupportSpatialAABB  {
        private readonly string _id;
        private readonly double _x;
        private readonly double _y;
        private readonly double _width;
        private readonly double _height;
        private readonly string _category;
    
        public SupportSpatialAABB(string id, double x, double y, double width, double height, string category) {
            this._id = id;
            this._x = x;
            this._y = y;
            this._width = width;
            this._height = height;
            this._category = category;
        }
    
        public SupportSpatialAABB(string id, double x, double y, double width, double height)
            : this(id, x, y, width, height, null)
        {
        }

        public string Id => _id;

        public double X => _x;

        public double Y => _y;

        public double Width => _width;

        public double Height => _height;

        public string Category => _category;
    }
} // end of namespace
