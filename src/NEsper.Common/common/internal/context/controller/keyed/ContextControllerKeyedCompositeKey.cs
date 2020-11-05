///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.collection;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedCompositeKey
    {
        public ContextControllerKeyedCompositeKey(
            IntSeqKey path,
            object key)
        {
            Path = path;
            Key = key;
        }

        public IntSeqKey Path { get; }

        public object Key { get; }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (ContextControllerKeyedCompositeKey) o;

            if (!Path.Equals(that.Path)) {
                return false;
            }

            return Key != null ? Key.Equals(that.Key) : that.Key == null;
        }

        public override int GetHashCode()
        {
            var result = Path.GetHashCode();
            result = 31 * result + (Key != null ? Key.GetHashCode() : 0);
            return result;
        }
    }
} // end of namespace