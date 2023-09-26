///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.mxcifrowindex
{
    using Collection = ICollection<object>;

    public class XYWHRectangleMultiType : XYWHRectangle
    {
        public XYWHRectangleMultiType(
            double x,
            double y,
            double w,
            double h,
            object multityped)
            : base(x, y, w, h)
        {
            Multityped = multityped;
        }

        public object Multityped { get; set; }

        public int Count()
        {
            return Multityped is Collection collection ? collection.Count : 1;
        }

        public void AddSingleValue(object value)
        {
            if (Multityped == null) {
                Multityped = value;
                return;
            }

            if (Multityped is Collection collection) {
                collection.Add(value);
                return;
            }

            var coll = new LinkedList<object>();
            coll.AddLast(Multityped);
            coll.AddLast(value);
            Multityped = coll;
        }

        public void AddMultiType(XYWHRectangleMultiType other)
        {
            if (other.X != X || other.Y != Y) {
                throw new ArgumentException("Coordinate mismatch");
            }

            if (!(other.Multityped is Collection otherCollection)) {
                AddSingleValue(other.Multityped);
                return;
            }

            if (Multityped is Collection collection) {
                collection.AddAll(otherCollection);
                return;
            }

            var coll = new LinkedList<object>();
            coll.AddLast(Multityped);
            coll.AddAll(otherCollection);
            Multityped = coll;
        }

        public void CollectInto(Collection result)
        {
            if (Multityped is Collection collection) {
                result.AddAll(collection);
            }
            else {
                result.Add(Multityped);
            }
        }

        public bool Remove(object value)
        {
            if (Multityped == null) {
                return false;
            }

            if (Multityped.Equals(value)) {
                Multityped = null;
                return true;
            }

            if (Multityped is Collection collection) {
                return collection.Remove(value);
            }

            return false;
        }

        public bool IsEmpty()
        {
            return Multityped == null ||
                   (Multityped is Collection collection &&
                    collection.IsEmpty());
        }

        public override string ToString()
        {
            return $"XYWHRectangleMultiType{{X={X}, Y={Y}, W={W}, H={H}, Multityped={Multityped}}}";
        }
    }
} // end of namespace