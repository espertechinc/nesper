///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.collection
{
    public sealed class MultiKeyArrayObject : MultiKeyArrayWrap
    {
        public MultiKeyArrayObject(object[] keys)
        {
            Keys = keys;
        }

        public object[] Keys { get; }

        public object Array => Keys;

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (MultiKeyArrayObject) o;

            if (!Keys.DeepEquals(that.Keys)) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return CompatExtensions.Hash(Keys);
        }

        public override string ToString()
        {
            return "MultiKeyObject{" +
                   "keys=" +
                   Keys.RenderAny() +
                   '}';
        }
    }
} // end of namespace