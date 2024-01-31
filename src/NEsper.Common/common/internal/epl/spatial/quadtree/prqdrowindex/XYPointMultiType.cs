///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.spatial.quadtree.prqdrowindex
{
    public class XYPointMultiType : XYPoint
    {
        public XYPointMultiType(
            double x,
            double y,
            object multityped)
            : base(x, y)
        {
            Multityped = multityped;
        }

        public object Multityped { get; set; }

        public int Count()
        {
            if (Multityped is ICollection<object> objectCollection) {
                return objectCollection.Count;
            }

            return 1;
        }

        public void AddSingleValue(object value)
        {
            if (Multityped == null) {
                Multityped = value;
                return;
            }

            if (Multityped is ICollection<object> objectCollection) {
                objectCollection.Add(value);
                return;
            }

            var coll = new LinkedList<object>();
            coll.AddLast(Multityped);
            coll.AddLast(value);
            Multityped = coll;
        }

        public void AddMultiType(XYPointMultiType other)
        {
            if (other.X != X || other.Y != Y) {
                throw new ArgumentException("Coordinate mismatch");
            }

            if (!(other.Multityped is ICollection<object> otherCollection)) {
                AddSingleValue(other.Multityped);
                return;
            }

            if (Multityped is ICollection<object> objectCollection) {
                objectCollection.AddAll(otherCollection);
                return;
            }

            var coll = new LinkedList<object>();
            coll.AddLast(Multityped);
            coll.AddAll(otherCollection);
            Multityped = coll;
        }

        public void CollectInto(ICollection<object> result)
        {
            if (!(Multityped is ICollection<object> objects)) {
                result.Add(Multityped);
                return;
            }

            result.AddAll(objects);
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

            if (Multityped is ICollection<object> objectCollection) {
                return objectCollection.Remove(value);
            }

            return false;
        }

        public bool IsEmpty()
        {
            if (Multityped == null) {
                return true;
            }

            if (Multityped is ICollection<object> objectCollection) {
                return objectCollection.IsEmpty();
            }

            return false;
        }

        public override string ToString()
        {
            return $"XYPointMultiType{{x={X}, y={Y}, numValues={Count()}{'}'}";
        }
    }
} // end of namespace