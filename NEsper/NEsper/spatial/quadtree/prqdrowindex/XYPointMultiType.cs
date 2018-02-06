///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.spatial.quadtree.pointregion;

namespace com.espertech.esper.spatial.quadtree.prqdrowindex
{
    using Collection = ICollection<object>;

    public class XYPointMultiType : XYPoint
    {
        public XYPointMultiType(double x, double y, object multityped)
            : base(x, y)
        {
            Multityped = multityped;
        }

        public object Multityped { get; set; }

        public int Count()
        {
            if (Multityped is Collection) return ((Collection) Multityped).Count;
            return 1;
        }

        public void AddSingleValue(object value)
        {
            if (Multityped == null)
            {
                Multityped = value;
                return;
            }

            if (Multityped is Collection)
            {
                ((Collection) Multityped).Add(value);
                return;
            }

            var coll = new LinkedList<object>();
            coll.AddLast(Multityped);
            coll.AddLast(value);
            Multityped = coll;
        }

        public void AddMultiType(XYPointMultiType other)
        {
            if (other.X != X || other.Y != Y) throw new ArgumentException("Coordinate mismatch");
            if (!(other.Multityped is Collection))
            {
                AddSingleValue(other.Multityped);
                return;
            }

            var otherCollection = (Collection) other.Multityped;
            if (Multityped is Collection)
            {
                ((Collection) Multityped).AddAll(otherCollection);
                return;
            }

            var coll = new LinkedList<object>();
            coll.AddLast(Multityped);
            coll.AddAll(otherCollection);
            Multityped = coll;
        }

        public void CollectInto(Collection result)
        {
            if (!(Multityped is Collection))
            {
                result.Add(Multityped);
                return;
            }

            result.AddAll((Collection) Multityped);
        }

        public bool Remove(object value)
        {
            if (Multityped == null) return false;
            if (Multityped.Equals(value))
            {
                Multityped = null;
                return true;
            }

            if (Multityped is Collection) return ((Collection) Multityped).Remove(value);
            return false;
        }

        public bool IsEmpty()
        {
            return Multityped == null || Multityped is Collection && ((Collection) Multityped).IsEmpty();
        }

        public override string ToString()
        {
            return "XYPointMultiType{" +
                   "x=" + X +
                   ", y=" + Y +
                   ", numValues=" + Count() +
                   '}';
        }
    }
} // end of namespace