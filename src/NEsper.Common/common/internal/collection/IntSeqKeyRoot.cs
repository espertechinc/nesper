///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.collection
{
    public class IntSeqKeyRoot : IntSeqKey
    {
        public static readonly IntSeqKeyRoot INSTANCE = new IntSeqKeyRoot();

        private IntSeqKeyRoot()
        {
        }

        public bool IsParentTo(IntSeqKey other)
        {
            return other.Length == 1;
        }

        public IntSeqKey AddToEnd(int num)
        {
            return new IntSeqKeyOne(num);
        }

        public IntSeqKey RemoveFromEnd()
        {
            throw new UnsupportedOperationException("Not applicable to this key");
        }

        public int Length {
            get { return 0; }
        }

        public int Last {
            get { throw new UnsupportedOperationException("Not applicable to this key"); }
        }

        public int[] AsIntArray()
        {
            return new int[0];
        }
    }
} // end of namespace