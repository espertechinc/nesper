///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.index.quadtree
{
    public class AdvancedIndexConfigContextPartitionQuadTree : AdvancedIndexConfigContextPartition
    {
        private readonly double _x;
        private readonly double _y;
        private readonly double _width;
        private readonly double _height;
        private readonly int _leafCapacity;
        private readonly int _maxTreeHeight;
    
        public AdvancedIndexConfigContextPartitionQuadTree(double x, double y, double width, double height, int leafCapacity, int maxTreeHeight)
        {
            this._x = x;
            this._y = y;
            this._width = width;
            this._height = height;
            this._leafCapacity = leafCapacity;
            this._maxTreeHeight = maxTreeHeight;
        }

        public double X => _x;

        public double Y => _y;

        public double Width => _width;

        public double Height => _height;

        public int LeafCapacity => _leafCapacity;

        public int MaxTreeHeight => _maxTreeHeight;

        public void ToConfiguration(TextWriter writer)
        {
            writer.Write(_x);
            writer.Write(",");
            writer.Write(_y);
            writer.Write(",");
            writer.Write(_width);
            writer.Write(",");
            writer.Write(_height);
            writer.Write(",");
            writer.Write(_leafCapacity);
            writer.Write(",");
            writer.Write(_maxTreeHeight);
        }
    }
} // end of namespace
