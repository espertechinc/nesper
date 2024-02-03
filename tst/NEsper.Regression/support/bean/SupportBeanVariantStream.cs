///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;

namespace com.espertech.esper.regressionlib.support.bean
{
    // For testing variant streams to act as a variant of SupportBean
    public class SupportBeanVariantStream
    {
        public SupportBeanVariantStream(string theString)
        {
            TheString = theString;
        }

        public SupportBeanVariantStream(
            string theString,
            bool boolBoxed,
            int intPrimitive,
            int longPrimitive,
            float doublePrimitive,
            SupportEnum enumValue)
        {
            TheString = theString;
            BoolBoxed = boolBoxed;
            IntPrimitive = intPrimitive;
            LongPrimitive = longPrimitive;
            DoublePrimitive = doublePrimitive;
            EnumValue = enumValue;
        }

        public string TheString { get; }

        public bool BoolBoxed { get; }

        public int? IntPrimitive { get; }

        public int LongPrimitive { get; }

        public float DoublePrimitive { get; }

        public SupportEnum EnumValue { get; }

        protected bool Equals(SupportBeanVariantStream other)
        {
            return string.Equals(TheString, other.TheString) &&
                   BoolBoxed == other.BoolBoxed &&
                   IntPrimitive == other.IntPrimitive &&
                   LongPrimitive == other.LongPrimitive &&
                   DoublePrimitive.Equals(other.DoublePrimitive) &&
                   EnumValue == other.EnumValue;
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

            return Equals((SupportBeanVariantStream) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = TheString != null ? TheString.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ BoolBoxed.GetHashCode();
                hashCode = (hashCode * 397) ^ IntPrimitive.GetHashCode();
                hashCode = (hashCode * 397) ^ LongPrimitive;
                hashCode = (hashCode * 397) ^ DoublePrimitive.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) EnumValue;
                return hashCode;
            }
        }
    }
} // end of namespace