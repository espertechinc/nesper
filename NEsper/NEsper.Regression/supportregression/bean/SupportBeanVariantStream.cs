///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

// For testing variant streams to act as a variant of SupportBean

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class SupportBeanVariantStream
    {
        public SupportBeanVariantStream(String str)
        {
            TheString = str;
        }

        public SupportBeanVariantStream(String str, bool boolBoxed, int? intPrimitive, int longPrimitive,
                                        float doublePrimitive, SupportEnum? enumValue)
        {
            TheString = str;
            BoolBoxed = boolBoxed;
            IntPrimitive = intPrimitive;
            LongPrimitive = longPrimitive;
            DoublePrimitive = doublePrimitive;
            EnumValue = enumValue;
        }

        public string TheString { get; private set; }

        public bool BoolBoxed { get; private set; }

        public int? IntPrimitive { get; private set; }

        public int LongPrimitive { get; private set; }

        public float DoublePrimitive { get; private set; }

        public SupportEnum? EnumValue { get; private set; }
    }
}
