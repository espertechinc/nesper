///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.regressionlib.support.context
{
    public class SupportHashCodeFuncGranularCRC32 : HashCodeFunc
    {
        private readonly int granularity;

        public SupportHashCodeFuncGranularCRC32(int granularity)
        {
            this.granularity = granularity;
        }

        public int CodeFor(string key)
        {
            var codeMod = ComputeCRC32(key) % granularity;
            return (int) codeMod;
        }

        public static long ComputeCRC32(string key)
        {
            return key.GetCrc32();
        }
    }
} // end of namespace