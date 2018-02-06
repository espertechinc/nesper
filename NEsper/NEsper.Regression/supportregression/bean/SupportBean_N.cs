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
    public class SupportBean_N
    {
        public SupportBean_N(int intPrimitive, int? intBoxed, double doublePrimitive, double? doubleBoxed,
                             bool boolPrimitive, bool boolBoxed)
        {
            IntPrimitive = intPrimitive;
            IntBoxed = intBoxed;
            DoublePrimitive = doublePrimitive;
            DoubleBoxed = doubleBoxed;
            BoolPrimitive = boolPrimitive;
            BoolBoxed = boolBoxed;
        }

        public SupportBean_N(int intPrimitive, int? intBoxed)
        {
            IntPrimitive = intPrimitive;
            IntBoxed = intBoxed;
        }

        public int IntPrimitive { get; private set; }

        public int? IntBoxed { get; private set; }

        public double DoublePrimitive { get; private set; }

        public double? DoubleBoxed { get; private set; }

        public bool BoolPrimitive { get; private set; }

        public bool BoolBoxed { get; private set; }

        public override String ToString()
        {
            return
                " IntPrimitive=" + IntPrimitive +
                " IntBoxed=" + IntBoxed +
                " DoublePrimitive=" + DoublePrimitive +
                " DoubleBoxed=" + DoubleBoxed +
                " BoolPrimitive=" + BoolPrimitive +
                " BoolBoxed=" + BoolBoxed;
        }
    }
}
