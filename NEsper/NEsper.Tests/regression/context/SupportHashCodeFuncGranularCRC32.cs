///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.regression.context
{
    public class SupportHashCodeFuncGranularCRC32 : TestContextHashSegmented.HashCodeFunc
    {
        private readonly int _granularity;
    
        public SupportHashCodeFuncGranularCRC32(int granularity)
        {
            _granularity = granularity;
        }
    
        public int CodeFor(String key) {
            long codeMod = ComputeCRC32(key) % _granularity;
            return (int) codeMod;
        }
    
        public static long ComputeCRC32(String key)
        {
            return key.GetCrc32();
        }
    }
}
