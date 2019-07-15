///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportBean_N
    {
        public SupportBean_N(
            int intPrimitive,
            int? intBoxed,
            double doublePrimitive,
            double? doubleBoxed,
            bool boolPrimitive,
            bool? boolBoxed)
        {
            IntPrimitive = intPrimitive;
            IntBoxed = intBoxed;
            DoublePrimitive = doublePrimitive;
            DoubleBoxed = doubleBoxed;
            IsBoolPrimitive = boolPrimitive;
            BoolBoxed = boolBoxed;
        }

        public SupportBean_N(
            int intPrimitive,
            int? intBoxed)
        {
            IntPrimitive = intPrimitive;
            IntBoxed = intBoxed;
        }

        public int IntPrimitive { get; }

        public int? IntBoxed { get; }

        public double DoublePrimitive { get; }

        public double? DoubleBoxed { get; }

        public bool IsBoolPrimitive { get; }

        public bool? BoolBoxed { get; }

        public override string ToString()
        {
            return "intPrim=" +
                   IntPrimitive +
                   " IntBoxed=" +
                   IntBoxed +
                   " doublePrim=" +
                   DoublePrimitive +
                   " DoubleBoxed=" +
                   DoubleBoxed +
                   " boolPrim=" +
                   IsBoolPrimitive +
                   " BoolBoxed=" +
                   BoolBoxed;
        }

        protected bool Equals(SupportBean_N other)
        {
            return IntPrimitive == other.IntPrimitive &&
                   IntBoxed == other.IntBoxed &&
                   DoublePrimitive.Equals(other.DoublePrimitive) &&
                   DoubleBoxed.Equals(other.DoubleBoxed) &&
                   IsBoolPrimitive == other.IsBoolPrimitive &&
                   BoolBoxed == other.BoolBoxed;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((SupportBean_N) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = IntPrimitive;
                hashCode = (hashCode * 397) ^ IntBoxed.GetHashCode();
                hashCode = (hashCode * 397) ^ DoublePrimitive.GetHashCode();
                hashCode = (hashCode * 397) ^ DoubleBoxed.GetHashCode();
                hashCode = (hashCode * 397) ^ IsBoolPrimitive.GetHashCode();
                hashCode = (hashCode * 397) ^ BoolBoxed.GetHashCode();
                return hashCode;
            }
        }
    }
} // end of namespace