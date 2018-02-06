///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.spatial.quadtree.core
{
    public class BoundingBox
    {
        private readonly double _minX;
        private readonly double _minY;
        private readonly double _maxX;
        private readonly double _maxY;

        public BoundingBox(double minX, double minY, double maxX, double maxY)
        {
            _minX = minX;
            _minY = minY;
            _maxX = maxX;
            _maxY = maxY;
        }

        public double MinX => _minX;

        public double MinY => _minY;

        public double MaxX => _maxX;

        public double MaxY => _maxY;

        public bool ContainsPoint(double x, double y)
        {
            return x >= _minX && y >= _minY && x < _maxX && y < _maxY;
        }

        public bool IntersectsBoxIncludingEnd(double x, double y, double width, double height)
        {
            return IntersectsBoxIncludingEnd(_minX, _minY, _maxX, _maxY, x, y, width, height);
        }

        public static bool IntersectsBoxIncludingEnd(double minX, double minY, double maxX, double maxY, double otherX,
            double otherY, double otherWidth, double otherHeight)
        {
            var otherMaxX = otherX + otherWidth;
            var otherMaxY = otherY + otherHeight;
            if (maxX < otherX) return false; // a is left of b
            if (minX > otherMaxX) return false; // a is right of b
            if (maxY < otherY) return false; // a is above b
            if (minY > otherMaxY) return false; // a is below b
            return true; // boxes overlap
        }

        public static bool ContainsPoint(double x, double y, double width, double height, double px, double py)
        {
            if (px >= x + width) return false;
            if (px < x) return false;
            if (py >= y + height) return false;
            if (py < y) return false;
            return true;
        }

        public override string ToString()
        {
            return "{" +
                   "minX=" + _minX +
                   ", minY=" + _minY +
                   ", maxX=" + _maxX +
                   ", maxY=" + _maxY +
                   '}';
        }

        public QuadrantEnum GetQuadrant(double x, double y)
        {
            var deltaX = x - _minX;
            var deltaY = y - _minY;
            var halfWidth = (_maxX - _minX) / 2;
            var halfHeight = (_maxY - _minY) / 2;
            if (deltaX < halfWidth)
            {
                return deltaY < halfHeight ? QuadrantEnum.NW : QuadrantEnum.SW;
            }

            return deltaY < halfHeight ? QuadrantEnum.NE : QuadrantEnum.SE;
        }

        public QuadrantAppliesEnum GetQuadrantApplies(double x, double y, double w, double h)
        {
            var deltaX = x - _minX;
            var deltaY = y - _minY;
            var halfWidth = (_maxX - _minX) / 2;
            var halfHeight = (_maxY - _minY) / 2;
            var midX = _minX + halfWidth;
            var midY = _minY + halfHeight;
            if (deltaX < halfWidth)
            {
                if (deltaY < halfHeight)
                {
                    // x,y is NW world
                    if (x + w < _minX || y + h < _minY)
                    {
                        return QuadrantAppliesEnum.NONE;
                    }

                    if (x + w >= midX || y + h >= midY)
                    {
                        return QuadrantAppliesEnum.SOME;
                    }

                    return QuadrantAppliesEnum.NW;
                }
                else
                {
                    if (y > _maxY || x + w < _minX)
                    {
                        return QuadrantAppliesEnum.NONE;
                    }

                    if (x + w >= midX || y <= midY)
                    {
                        return QuadrantAppliesEnum.SOME;
                    }

                    return QuadrantAppliesEnum.SW;
                }
            }

            if (deltaY < halfHeight)
            {
                // x,y is NE world
                if (x > _maxX || y + h < _minY)
                {
                    return QuadrantAppliesEnum.NONE;
                }

                if (x <= midX || y + h >= midY)
                {
                    return QuadrantAppliesEnum.SOME;
                }

                return QuadrantAppliesEnum.NE;
            }
            else
            {
                if (x > _maxX || y > _maxY)
                {
                    return QuadrantAppliesEnum.NONE;
                }

                if (x <= midX || y <= midY)
                {
                    return QuadrantAppliesEnum.SOME;
                }

                return QuadrantAppliesEnum.SE;
            }
        }

        public BoundingBox[] Subdivide()
        {
            var w = (_maxX - _minX) / 2d;
            var h = (_maxY - _minY) / 2d;

            var bbNW = new BoundingBox(_minX, _minY, _minX + w, _minY + h);
            var bbNE = new BoundingBox(_minX + w, _minY, _maxX, _minY + h);
            var bbSW = new BoundingBox(_minX, _minY + h, _minX + w, _maxY);
            var bbSE = new BoundingBox(_minX + w, _minY + h, _maxX, _maxY);
            return new BoundingBox[] {bbNW, bbNE, bbSW, bbSE};
        }

        protected bool Equals(BoundingBox other)
        {
            return _minX.Equals(other._minX) && _minY.Equals(other._minY) && _maxX.Equals(other._maxX) &&
                   _maxY.Equals(other._maxY);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BoundingBox) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _minX.GetHashCode();
                hashCode = (hashCode * 397) ^ _minY.GetHashCode();
                hashCode = (hashCode * 397) ^ _maxX.GetHashCode();
                hashCode = (hashCode * 397) ^ _maxY.GetHashCode();
                return hashCode;
            }
        }

        public BoundingBoxNode TreeForDepth(int depth)
        {
            var quadrants = new BoundingBoxNode[4];
            if (depth > 0)
            {
                var subs = Subdivide();
                quadrants[0] = subs[0].TreeForDepth(depth - 1);
                quadrants[1] = subs[1].TreeForDepth(depth - 1);
                quadrants[2] = subs[2].TreeForDepth(depth - 1);
                quadrants[3] = subs[3].TreeForDepth(depth - 1);
            }

            return new BoundingBoxNode(this, quadrants[0], quadrants[1], quadrants[2], quadrants[3]);
        }

        public static BoundingBox From(double x, double y, double width, double height)
        {
            return new BoundingBox(x, y, x + width, y + height);
        }

        public BoundingBoxNode TreeForPath(string[] path)
        {
            return TreeForPath(path, 0);
        }

        private BoundingBoxNode TreeForPath(string[] path, int offset)
        {
            var quadrants = new BoundingBoxNode[4];
            if (offset < path.Length)
            {
                var subs = Subdivide();
                switch (path[offset])
                {
                    case "nw":
                        quadrants[0] = subs[0].TreeForPath(path, offset + 1);
                        break;
                    case "ne":
                        quadrants[1] = subs[1].TreeForPath(path, offset + 1);
                        break;
                    case "sw":
                        quadrants[2] = subs[2].TreeForPath(path, offset + 1);
                        break;
                    case "se":
                        quadrants[3] = subs[3].TreeForPath(path, offset + 1);
                        break;
                }
            }

            return new BoundingBoxNode(this, quadrants[0], quadrants[1], quadrants[2], quadrants[3]);
        }

        public class BoundingBoxNode
        {
            public readonly BoundingBox bb;
            public readonly BoundingBoxNode nw;
            public readonly BoundingBoxNode ne;
            public readonly BoundingBoxNode sw;
            public readonly BoundingBoxNode se;

            public BoundingBoxNode(BoundingBox bb, BoundingBoxNode nw, BoundingBoxNode ne, BoundingBoxNode sw,
                BoundingBoxNode se)
            {
                this.bb = bb;
                this.nw = nw;
                this.ne = ne;
                this.sw = sw;
                this.se = se;
            }

            public BoundingBoxNode GetQuadrant(QuadrantEnum q)
            {
                switch (q)
                {
                    case QuadrantEnum.NW:
                        return nw;
                    case QuadrantEnum.NE:
                        return ne;
                    case QuadrantEnum.SW:
                        return sw;
                    default:
                        return se;
                }
            }
        }
    }
} // end of namespace
