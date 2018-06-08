///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.regression.context;

using Force.Crc32;

namespace com.espertech.esper.supportregression.context
{
    public class SupportHashCodeFuncGranularCRC32
    {
        private readonly int _granularity;

        public SupportHashCodeFuncGranularCRC32(int granularity)
        {
            _granularity = granularity;
        }

        public int CodeFor(string key)
        {
            long codeMod = ComputeCrc32(key) % _granularity;
            return (int) codeMod;
        }

        public static long ComputeCrc32(string key)
        {
            return (long) Crc32Algorithm.Compute(key.GetUTF8Bytes());
        }
    }
} // end of namespace
