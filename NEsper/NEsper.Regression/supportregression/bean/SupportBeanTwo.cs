///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class SupportBeanTwo
    {
        public SupportBeanTwo()
        {
        }

        public SupportBeanTwo(String stringTwo,
                              int intPrimitiveTwo)
        {
            StringTwo = stringTwo;
            IntPrimitiveTwo = intPrimitiveTwo;
        }

        public string StringTwo { get; set; }

        public bool BoolPrimitiveTwo { get; set; }

        public int IntPrimitiveTwo { get; set; }

        public long LongPrimitiveTwo { get; set; }

        public char CharPrimitiveTwo { get; set; }

        public short ShortPrimitiveTwo { get; set; }

        public byte BytePrimitiveTwo { get; set; }

        public float FloatPrimitiveTwo { get; set; }

        public double DoublePrimitiveTwo { get; set; }

        public bool? BoolBoxedTwo { get; set; }

        public int? IntBoxedTwo { get; set; }

        public long? LongBoxedTwo { get; set; }

        public char? CharBoxedTwo { get; set; }

        public short? ShortBoxedTwo { get; set; }

        public byte? ByteBoxedTwo { get; set; }

        public float? FloatBoxedTwo { get; set; }

        public double? DoubleBoxedTwo { get; set; }

        public SupportEnum EnumValueTwo { get; set; }

        public override String ToString()
        {
            return GetType().Name + "(" + StringTwo + ", " + IntPrimitiveTwo + ")";
        }
    }
}
