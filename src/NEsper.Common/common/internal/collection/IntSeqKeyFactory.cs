///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.collection
{
    public class IntSeqKeyFactory
    {
        public static IntSeqKey From(int[] array)
        {
            if (array.Length == 0) {
                return IntSeqKeyRoot.INSTANCE;
            }

            if (array.Length == 1) {
                return new IntSeqKeyOne(array[0]);
            }

            if (array.Length == 2) {
                return new IntSeqKeyTwo(array[0], array[1]);
            }

            if (array.Length == 3) {
                return new IntSeqKeyThree(array[0], array[1], array[2]);
            }

            if (array.Length == 4) {
                return new IntSeqKeyFour(array[0], array[1], array[2], array[3]);
            }

            if (array.Length == 5) {
                return new IntSeqKeyFive(array[0], array[1], array[2], array[3], array[4]);
            }

            if (array.Length == 6) {
                return new IntSeqKeySix(array[0], array[1], array[2], array[3], array[4], array[5]);
            }

            return new IntSeqKeyMany(array);
        }
    }
} // end of namespace