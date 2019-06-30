///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.util
{
    public class MethodExecutableRank
    {
        private readonly bool varargs;

        public MethodExecutableRank(
            int conversionCount,
            bool varargs)
        {
            ConversionCount = conversionCount;
            this.varargs = varargs;
        }

        public int ConversionCount { get; }

        public int CompareTo(
            int conversionCount,
            bool varargs)
        {
            int compareCount = ConversionCount.CompareTo(conversionCount);
            if (compareCount != 0) {
                return compareCount;
            }

            return this.varargs.CompareTo(varargs);
        }

        public int CompareTo(MethodExecutableRank other)
        {
            return CompareTo(other.ConversionCount, other.varargs);
        }

        public bool IsVarargs()
        {
            return varargs;
        }

        public override string ToString()
        {
            return "MethodExecutableRank{" +
                   "conversionCount=" + ConversionCount +
                   ", varargs=" + varargs +
                   '}';
        }
    }
} // end of namespace